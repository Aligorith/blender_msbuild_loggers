using System;
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
			else if (e.SenderName == "Link") {
				handleMessage_Link(e);
			}
			else {
				//Console.WriteLine(String.Format("{0}: [1] -> '{2}'",
				//                                e.SenderName, e.Importance, e.Message));
			}
		}
	}
	
	
	/* "CL.exe" events */
	private void handleMessage_CL(BuildMessageEventArgs e)
	{
		/* Detect whether this one is the commandline report, or the name-only */
		bool is_command_line = e.Message.Contains("CL.exe");
		
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
			/* This is the name only - Only show when not showing detailed logs */
			string line = String.Format("   Compiling => \"{0}\"",
			                            e.Message);
			
			//WriteFilledLine(line,
			//                ConsoleColor.Blue, ConsoleColor.White);
			Console.WriteLine(line);
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
