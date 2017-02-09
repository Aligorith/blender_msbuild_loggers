/* Custom MSBuild Logger for compiling Blender using MSVC
 * with cleaner + simpler build outputs.
 *
 * Original Author: Joshua Leung
 * Date: Feb 2017
 */

using System;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;


public class CustomLogger: Logger
{
	/* Logger Config Options ---------------------------------------- */
	/* Show the raw, unrecognised messages? */
	const bool SHOW_RAW_MESSAGES = false;
	
	/* Stop reporting events after first error? */
	const bool STOP_AFTER_FIRST_ERROR = true;
	
	/* Silence all warnings about zero-sized arrays */
	// NOTE: This should ideally be done in the build-scripts/configs,
	// but sometimes when all else fails, a hack here is equally efficient.
	const bool HIDE_ZEROSIZEDARRAY_WARNINGS = true;
	
	/* Logger State ------------------------------------------------- */
	
	/* Counters for keeping track of the number of warnings/errors */
	private uint warnings = 0;
	private uint errors = 0;
	
	/* ILogger ------------------------------------------------------ */
	
	public override void Initialize(IEventSource eventSource)
	{
		/* Only register handlers for the events we actually care about...
		 * If there's something we don't need, don't register a handler
		 * for it, and it won't get added...
		 */
		
		eventSource.MessageRaised += new BuildMessageEventHandler(handleMessageRaised);
		
		eventSource.WarningRaised += new BuildWarningEventHandler(handleWarningRaised);
		eventSource.ErrorRaised   += new BuildErrorEventHandler(handleErrorRaised);
		
		eventSource.BuildFinished += new BuildFinishedEventHandler(handleBuildFinished);
	}
	
	/* Event Handlers - "Message" ------------------------------------ */
	
	/* Entrypoint for all "Message" handling */
	private void handleMessageRaised(object sender, BuildMessageEventArgs e)
	{
		/* We really don't ever care about any of the more detailed stuff, so let's just not bother */
		if (e.Importance == MessageImportance.High) {
			if (e.SenderName == "CL") {
				handleMessage_CL(e);
			}
			else if (e.SenderName == "LIB") {
				handleMessage_LIB(e);
			}
			else if (e.SenderName == "Link") {
				handleMessage_Link(e);
			}
			else if (e.SenderName == "CustomBuild") {
				handleMessage_CustomBuild(e);
			}
			else if (CustomLogger.SHOW_RAW_MESSAGES) {
				Console.WriteLine(String.Format("{0}: [1] -> '{2}'",
				                                e.SenderName, e.Importance, e.Message));
			}
		}
	}
	
	
	/* "CL.exe" events -  A file gets compiled */
	private void handleMessage_CL(BuildMessageEventArgs e)
	{
		/* Detect whether this one is the commandline report, or the name-only */
		bool is_command_line = e.Message.Contains("CL.exe");
		bool is_err_overflow = ((e.Message[0] == ' ') || (e.Message[0] == '\t'));
		
		/* Unless we want detailed logs, skip the one where it reports the commandline being used */
		if (is_command_line == true) {
			if (Verbosity == LoggerVerbosity.Detailed) {
				/* This is the command line - Only show in detailed logs */
				string line = String.Format("[{0}]: {1}", 
				                            e.SenderName, e.Message);
				
				// XXX: Should we shade this?
				WriteShadedLine(line, 
				                ConsoleColor.DarkGray, ConsoleColor.White);
			}
		}
		else if (is_err_overflow) {
			/* This isn't strictly a compile string (i.e. name of the file, from first line of CL output),
			 * but is rather, part of the error/warning strings! So, just shade as if it is one of either.
			 */
			if (!HIDE_ZEROSIZEDARRAY_WARNINGS || !e.Message.Contains("zero-sized array")) {
				WriteShadedLine(e.Message,
				                ConsoleColor.Yellow, ConsoleColor.Black);
			}
		}
		else {
			if (Verbosity != LoggerVerbosity.Detailed) {
				/* This is the name only - Only show when not showing detailed logs */
				// XXX: This may also be part of an error!
				string line = String.Format("  Compiling => \"{0}\"",
				                            e.Message);
				
				//WriteFilledLine(line,
				//                ConsoleColor.Blue, ConsoleColor.White);
				Console.WriteLine(line);
			}
		}
	}
	
	
	/* Helper to extract the libname from the commandline */
	string FindLibNameFromCmdline(string cmd)
	{
		var start_tag = "/OUT:\"";
		var end_tag = ".lib\" /NOLOGO";
		
		/* 1) Find position of the "/OUT:" string */
		var outPos = cmd.IndexOf(start_tag);
		if (outPos == -1) {
			return null;
		}
		
		/* 2) Apply offset to get the start of the path (to the lib) */
		var startPos = outPos + start_tag.Length;
		
		
		/* 3) Find the end of the path (via the endtag) */
		var endPos = cmd.IndexOf(end_tag);
		if (endPos == -1) {
			return null;
		}
		
		/* 4) Grab the path */
		var path = cmd.Substring(startPos, endPos - startPos);
		var libname = System.IO.Path.GetFileName(path);
		
		return libname;
	}
	
	/* "LIB" events - Relinking a module */
	private void handleMessage_LIB(BuildMessageEventArgs e)
	{
		/* Extract out the filename from the "OUT" parameter */
		string libname = FindLibNameFromCmdline(e.Message);
		
		if ((libname != null) && (Verbosity != LoggerVerbosity.Detailed)) {
			string line = String.Format("  Linking Lib => \"{0}\"",
				                            libname);
				
			WriteFilledLine(line,
			                ConsoleColor.DarkBlue, ConsoleColor.White);
		}
		else {
			string line = String.Format("LIB: {0}", e.Message);
			WriteFilledLine(line,
			                ConsoleColor.DarkBlue, ConsoleColor.White);
		}
	}
	
	/* "Linker" events - Same as for CL */
	private void handleMessage_Link(BuildMessageEventArgs e)
	{
		/* Detect whether this one is the commandline report, or the name-only */
		bool is_command_line = e.Message.Contains("link.exe");
		
		/* Unless we want detailed logs, skip the one where it reports the commandline being used */
		if ((is_command_line == true) && (Verbosity == LoggerVerbosity.Detailed)) {
			/* This is the command line - Only show in detailed logs */
			string line = String.Format("[{0}]: {1}", 
			                            e.SenderName, e.Message);
			
			// XXX: Should we shade this?
			WriteShadedLine(line, 
			                ConsoleColor.DarkGray, ConsoleColor.White);
		}
		else if ((is_command_line == false) && (Verbosity != LoggerVerbosity.Detailed)) {
			/* This is the "Creating Library" line - Only show when not showing detailed logs */
			string line = e.Message.Trim();
			
			if (line.StartsWith("Creating library") && line.EndsWith("blender.exp")) {
				line = "Linking \"blender.exe\"...";
			}
			
			WriteFilledLine("  " + line,
			                ConsoleColor.DarkMagenta, ConsoleColor.White);
		}
	}
	
	/* CustomBuild events */
	/* Cases:
	 *  -> Include: "Generate" events
	 *  -> Include: makesdna/rna events - "Running", "Writing", etc.
	 *  -> Skip: "CMake does not need to re-run"...
	 */
	private void handleMessage_CustomBuild(BuildMessageEventArgs e)
	{
		/* Logic: First, just skip the ones we don't want for now, and report everything else */
		if (e.Message.StartsWith("CMake does not need to re-run"))
		{
			return;
		}
		
		
		/* Shade each case differently... */
		ConsoleColor bg_color;
		bool show_mark = false;
		
		if (e.Message.StartsWith("Running")) {
			/* Running makesdna/makesrna - Interesting (as it could fail) */
			bg_color = ConsoleColor.DarkCyan;
			show_mark = true;
		}
		else if (e.Message.StartsWith("Generating") && e.Message.Contains("release/datafiles/blender_icons")) {
			/* Compiling Icon Files - Rare event (interesting) */
			bg_color = ConsoleColor.DarkCyan;
			show_mark = true;
		}
		else if (e.Message.StartsWith("Generating") || e.Message.StartsWith("Writing")) {
			/* Auto-generated files -> Not interesting... */
			bg_color = ConsoleColor.DarkGray;
			show_mark = true;
		}
		else if (e.Message.StartsWith("Checking Build System") || 
		         e.Message.StartsWith("CMake is re-running because"))
		{
			/* Start of proceedings - Start of each CMake action... */
			bg_color = ConsoleColor.DarkYellow;
			show_mark = true;
		}
		else {
			/* XXX: Currently, this will only get used for CMake "prose"... */
			bg_color = ConsoleColor.DarkYellow;
			show_mark = false;
		}
		
		
		/* Create/Format output line */
		string line;
		if ((Verbosity != LoggerVerbosity.Detailed) && 
		    (e.Message.StartsWith("Generating") && e.Message.EndsWith("rna_prototypes_gen.h")) ) 
		{
			/* Use a shortened version in this case */
			line = "> Generating rna_*_gen.c files...";
		}
		else if (show_mark) {
			/* Usually, these are commands/starts of important actions and events */
			line = String.Format("> {0}", e.Message);
		}
		else {
			/* Unimportant line (part of some existing output) */
			line = String.Format(" {0}", e.Message);
		}
		
		/* Write this event */
		WriteFilledLine(line,
		                bg_color, ConsoleColor.White);
	}
	
	
	/* Event Handlers ------------------------------------------------ */
	
	/* Helper to simplify the name of the file (from Warnings/Errors) */
	private string ShortSourcename(string path)
	{
		/* Split path into elements */
		char SEP = System.IO.Path.DirectorySeparatorChar;
		string[] elems = path.Split(SEP);
		
		/* Use grandparent if parent directory is "intern", since there's no useful info there */
		string filename = elems[elems.Length - 1];
		string parname  = elems[elems.Length - 2];
		string gpname   = elems[elems.Length - 3];
		
		string dirname = null;
		
		if (parname == "intern") {
			dirname = gpname;
		}
		else {
			dirname = parname;
		}
		
		/* Construct formatted string for "shortname" */
		//return String.Format(".../{0}/{1}", dirname, filename);
		return String.Format("{0}/{1}", dirname, filename);
	}
	
	
	private void handleWarningRaised(object sender, BuildWarningEventArgs e)
	{
		/* Skip this warning if is one of the ones we don't want to know about */
		if (HIDE_ZEROSIZEDARRAY_WARNINGS && e.Message.Contains("zero-sized array"))
			return;
		
		
		/* Warning is ok, prepare to print it... */
		string filename = ShortSourcename(e.File);
		string line = String.Format("! {0}:{1} - {3}  [{2}]",
		                            filename, e.LineNumber, e.Code, e.Message);
		
		WriteShadedLine(line,
		                ConsoleColor.Yellow, ConsoleColor.Black);
		
		warnings++;
	}
	
	private void handleErrorRaised(object sender, BuildErrorEventArgs e)
	{
		/* Report the error */
		string filename = ShortSourcename(e.File);
		string line = String.Format("ERROR: {0}:{1} - {3}  [{2}]",
		                            filename, e.LineNumber, e.Code, e.Message);
		
		WriteShadedLine(line,
		                ConsoleColor.DarkRed, ConsoleColor.White);
		
		errors++;
		
		/* Try to trigger a stop */
		if (STOP_AFTER_FIRST_ERROR) {
			WriteShadedLine("\nXXX Stopping build now!",
			                ConsoleColor.Red, ConsoleColor.White);
			Environment.Exit(-1);
		}
	}
	
	private void handleBuildFinished(object sender, BuildFinishedEventArgs e)
	{
		Console.WriteLine();
		
		if (errors == 0) {
			WriteShadedLine("# Build succeeded.", ConsoleColor.DarkGreen, ConsoleColor.White);
		}
		else {
			WriteShadedLine("# Build failed.", ConsoleColor.Red, ConsoleColor.White);
		}
		
		var warnings_str = String.Format("  {0} Warning(s)", warnings);
		if (warnings > 0) {
			WriteColoredLine(warnings_str, ConsoleColor.Yellow);
		}
		else {
			Console.WriteLine(warnings_str);
		}
		
		var errors_str   = String.Format("  {0} Error(s)", errors);
		if (errors > 0) {
			WriteColoredLine(errors_str, ConsoleColor.Red);
		}
		else {
			Console.WriteLine(errors_str);
		}
	}
	
	
	/* Helper functions ------------------------------------------- */
	
	/* Write line with colored text only */
	private void WriteColoredLine(string message, ConsoleColor col)
	{
		Console.ForegroundColor = col;
		Console.WriteLine(message);
		Console.ResetColor();
	}
	
	/* Write line with partial color fill */
	private void WriteFilledLine(string message, ConsoleColor bg, ConsoleColor fg)
	{
		Console.BackgroundColor = bg;
		Console.ForegroundColor = fg;
		
		Console.WriteLine(message);
		
		Console.ResetColor();
	}
	
	/* Write line which fills the entire row with a solid block of color */
	private void WriteShadedLine(string message, ConsoleColor bg, ConsoleColor fg)
	{
		Console.BackgroundColor = bg;
		Console.ForegroundColor = fg;
		
		Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
		
		Console.ResetColor();
	}
	
}
