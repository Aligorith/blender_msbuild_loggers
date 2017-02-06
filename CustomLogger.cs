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
	
	private void handleMessageRaised(object sender, BuildMessageEventArgs e)
	{
		// XXX: We almost NEVER want the really detailed shit here... MSVC can be too bloody noisy (producing lots of garbage) if left unchecked
		if ( (e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)) ||
			 (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)) ||
			 (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)) )
		{
			//Console.WriteLine(String.Format("{0}: [1] -> '{2}'",
			//                                e.SenderName, e.Importance, e.Message));
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
	
	/* Write line which fills the entire row with a solid block of color */
	private void WriteShadedLine(string message, ConsoleColor bg, ConsoleColor fg)
	{
		Console.BackgroundColor = bg;
		Console.ForegroundColor = fg;
		
		Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
		
		Console.ResetColor();
	}
	
}
