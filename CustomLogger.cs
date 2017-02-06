using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
 
public class CustomLogger: Logger
{
	private int warnings = 0;
	private int errors = 0;
 
	public override void Initialize(IEventSource eventSource)
	{
		eventSource.WarningRaised += ( s, e ) => ++warnings;
		eventSource.ErrorRaised += ( s, e ) => ++errors;
		eventSource.BuildFinished += ( s, e ) =>
		{
			Console.WriteLine( errors == 0 ? "Build succeeded." : "Build failed." );
			Console.WriteLine( String.Format( "{0} Warning(s)", warnings ) );
			Console.WriteLine( String.Format( "{0} Error(s)", errors ) );
		};
	}
}
