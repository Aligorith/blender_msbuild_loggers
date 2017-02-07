/* Test app for a function that truncates a source-file path down to just the filename and the parent directory (or its parent)
 * In Blender's codebase, we rarely (if ever) need to see the full path. Just knowing the filename and a parent directory is enough
 * to quickly pick up any errors you caused.
 */
using System;
using System.IO;

public class SourceShortnameApp
{
	static void Main()
	{
		Test(@"c:\blenderdev\master2\blender\source\blender\makesdna\DNA_curve_types.h");
		Test(@"C:\blenderdev\master2\blender\source\blender\depsgraph\intern\builder\deg_builder_nodes.cc");
	}
	
	static void Test(string path)
	{
		Console.WriteLine(String.Format("{0} <- {1}",
		                                ShortSourcename(path), path));
	}
	
	/* ---------------------------- */
	
	
	static string ShortSourcename(string path)
	{
		char SEP = System.IO.Path.DirectorySeparatorChar;
		string[] elems = path.Split(SEP);
		
		/* Use grandparent if parent directory is "intern", since there's no useful info there */
		if (elems[elems.Length - 2] == "intern") {
			return elems[elems.Length - 3] + SEP + elems[elems.Length - 1];
		}
		else {
			return elems[elems.Length - 2] + SEP + elems[elems.Length - 1];
		}
	}
}
