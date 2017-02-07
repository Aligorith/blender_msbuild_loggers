using System;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
 
public class CustomLogger: Logger
{
	private int warnings = 0;
	private int errors = 0;
	
	public override void Initialize(IEventSource eventSource)
	{
		/* Only register handlers for the events we actually care about...
		 * If there's something we don't need, don't register a handler
		 * for it, and it won't get added...
		 */
		
		//eventSource.TaskStarted += new TaskStartedEventHandler(handleTaskStarted);
		 
		eventSource.MessageRaised += new BuildMessageEventHandler(handleMessageRaised);
		
		eventSource.WarningRaised += new BuildWarningEventHandler(handleWarningRaised);
		eventSource.ErrorRaised   += new BuildErrorEventHandler(handleErrorRaised);
		
		eventSource.BuildFinished += new BuildFinishedEventHandler(handleBuildFinished);
	}
	
	/* Event Handlers - "Message" ------------------------------------ */
	
	/* Entrypoint for all "Message" handling */
	private void handleMessageRaised(object sender, BuildMessageEventArgs e)
	{
		// XXX: We almost NEVER want the really detailed shit here... MSVC can be too bloody noisy (producing lots of garbage) if left unchecked
		if ( (e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)) ||
			 (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)) ||
			 (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)) )
		{
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
			else {
				//Console.WriteLine(String.Format("{0}: [1] -> '{2}'",
				//                                e.SenderName, e.Importance, e.Message));
			}
		}
	}
	
	
	/* "CL.exe" events -  A file gets compiled */
	// XXX: CL events can also be second-line output from errors...
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
			// XXX: Color needs work...
			WriteShadedLine(e.Message,
			                ConsoleColor.Yellow, ConsoleColor.Black);
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
		
		if (libname != null) {
			string line = String.Format("  Linking Lib => \"{0}\"",
				                            e.Message);
				
			//WriteFilledLine(line,
			//                ConsoleColor.DarkBlue, ConsoleColor.White);
			Console.WriteLine(line);
		}
		else {
			Console.WriteLine(String.Format("LIB: {0}", e.Message));
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
			WriteFilledLine(e.Message,
			                ConsoleColor.DarkMagenta, ConsoleColor.White);
		}
	}
	
	/* CustomBuild events */
	private void handleMessage_CustomBuild(BuildMessageEventArgs e)
	{
		/* Cases:
		 *  -> Include: "Generate" events
		 *  -> Include: makesdna/rna events - "Running", "Writing", etc.
		 *  -> Skip: "CMake does not need to re-run"...
		 */
		
		/* Logic: Just skip the ones we don't want for now, and report everything else */
		// TODO: Shade the different cases differently?
		bool unwanted = (e.Message.StartsWith("CMake does not need to re-run"));
		
		if (unwanted == false) {
			string line = String.Format("> {0}", e.Message);
			WriteFilledLine(line,
			                ConsoleColor.DarkYellow, ConsoleColor.White);
		}
	}
	
	
	/* Event Handlers ------------------------------------------------ */
	
	private void handleTaskStarted(object sender, TaskStartedEventArgs e)
	{
		//Console.WriteLine(e.Message);
		Console.WriteLine(String.Format("Task: '{0}' from '{1}' for {2}",
		                                e.TaskName, e.TaskFile, e.ProjectFile));
	}
	
	
	private void handleWarningRaised(object sender, BuildWarningEventArgs e)
	{
		WriteShadedLine("! " + FormatWarningEvent(e),
		                ConsoleColor.Yellow, ConsoleColor.Black);
		warnings++;
	}
	
	private void handleErrorRaised(object sender, BuildErrorEventArgs e)
	{
		WriteShadedLine("!!! " + FormatErrorEvent(e),
		                ConsoleColor.DarkRed, ConsoleColor.White);
		
		errors++;
	}
	
	private void handleBuildFinished(object sender, BuildFinishedEventArgs e)
	{
		if (errors == 0) {
			WriteShadedLine("Build succeeded.", ConsoleColor.DarkGreen, ConsoleColor.White);
		}
		else {
			WriteShadedLine("Build failed.", ConsoleColor.Red, ConsoleColor.Black);
		}
		Console.WriteLine( String.Format( "  {0} Warning(s)", warnings ) );
		Console.WriteLine( String.Format( "  {0} Error(s)", errors ) );
	}
	
	
	/* Helper functions ------------------------------------------- */
	
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
