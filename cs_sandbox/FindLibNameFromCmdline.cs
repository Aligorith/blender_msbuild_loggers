using System;
using System.IO;
 
public class MyApp
{
	static void Main()
	{
		var input = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\bin\x86_amd64\Lib.exe /OUT:""C:\blenderdev\master2\build\lib\Release\bf_rna.lib"" /NOLOGO";
		var result = FindLibNameFromCmdline(input);
		Console.WriteLine(String.Format("\n\nFrom '{0}''\n  -> '{1}'", input, result));
	}
	
	/* Function to extract the library name from a "Lib.exe" commandline */
	static string FindLibNameFromCmdline(string command)
	{
		var start_tag = "/OUT:\"";
		var end_tag = ".lib\" /NOLOGO";
		
		/* 1) Find position of the "/OUT:" string */
		var outPos = command.IndexOf(start_tag);
		if (outPos == -1) {
			return null;
		}
		
		Console.WriteLine("$$$ First index = " + outPos);
		
		
		/* 2) Apply offset to get the start of the path (to the lib) */
		var startPos = outPos + start_tag.Length;
		Console.WriteLine("$$$ startPos = " + startPos);
		
		/* 3) Find the end of the path (via the endtag) */
		var endPos = command.IndexOf(end_tag);
		Console.WriteLine("$$$ endPos = " + endPos);
		if (endPos == -1) {
			return null;
		}
		
		/* 4) Grab the path */
		var path = command.Substring(startPos, endPos - startPos);
		Console.WriteLine(String.Format("$$$ Command stripped down to {0} - {1} -> {2}", path, startPos, endPos));
		var libname = System.IO.Path.GetFileName(path); //  XXX: does this work without an extension?
		Console.WriteLine(String.Format("$$$ libname = {0}", libname));
		
		return libname;
	}
	
}
