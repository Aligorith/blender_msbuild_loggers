using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
 
public class CustomLogger: Logger
{
	public override void Initialize(IEventSource eventSource)
	{
		/* Project Finished Events - Only report the ones we care about... */
		eventSource.ProjectFinished += new ProjectFinishedEventHandler(handleProjectFinished);
	}
	
	
	/* Our custom handler for "Project Finished" events */
	private void handleProjectFinished(object sender, ProjectFinishedEventArgs e)
	{
		// Default Handling - Test
		Console.BackgroundColor = ConsoleColor.DarkBlue;
		
		//Console.WriteLine(e.Message);
		//Console.WriteLine("ProjectEnd: " + e.ProjectFile + ", [" + e.Succeeded + "]");
		
		/* Filter out all the "ZERO_CHECK ones */
		if (e.ProjectFile.Contains("ZERO_CHECK.vcxproj") == false &&
		    e.ProjectFile.Contains("ALL_BUILD.vcxproj") == false)
		{
			Console.WriteLine("ProjectEnd: " + e.ProjectFile);
		}
		
		Console.ResetColor();
	}
}
