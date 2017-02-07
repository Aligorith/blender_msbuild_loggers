/* Test app for demonstrating all the colors possible */
using System;
//using System.Collections.Generic;
using System.Linq;

/* From: http://stackoverflow.com/a/972323/6531515 */
// public static class EnumUtil
// {
// 	public static IEnumerable<T> GetValues<T>()
// 	{
//  		return Enum.GetValues(typeof(T)).Cast<T>();
// 	}
// }

public static class InUtils
{
	/* From: http://stackoverflow.com/a/8228973/6531515 */
	public static bool In<T>(this T item, params T[] list)
	{
		return list.Contains(item);
	}
}

public class ConsoleColorsApp
{
	static void Main()
	{
		//var values = EnumUtil.GetValues<ConsoleColor>();
		var values = Enum.GetValues(typeof(ConsoleColor));
		
		WriteShadedLine("Background Colors:", ConsoleColor.White, ConsoleColor.Black);
		foreach (ConsoleColor c in values) {
			ConsoleColor fg;
			if (c.In(ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow, ConsoleColor.White)) {
				fg = ConsoleColor.Black;
			}
			else {
				fg = ConsoleColor.White;
			}
			
			WriteFilledLine(c.ToString(), c, fg);
		}
		
		Console.Write("\n\n");
		
		WriteShadedLine("Background Colors:", ConsoleColor.White, ConsoleColor.Black);
		foreach (ConsoleColor c in values) {
			ConsoleColor bg;
			if (c.In(ConsoleColor.Black, ConsoleColor.DarkBlue, ConsoleColor.DarkRed, ConsoleColor.DarkMagenta, 
			         ConsoleColor.DarkGreen, ConsoleColor.DarkYellow, ConsoleColor.DarkCyan, ConsoleColor.Blue)) 
			{
				bg = ConsoleColor.Gray;
			}
			else {
				bg = ConsoleColor.Black;
			}
			
			WriteFilledLine(c.ToString(), bg, c);
		}
	}
	
	
	/* Write line with partial color fill */
	static void WriteFilledLine(string line, ConsoleColor bg, ConsoleColor fg)
	{
		Console.BackgroundColor = bg;
		Console.ForegroundColor = fg;
		
		Console.WriteLine(line);
		
		Console.ResetColor();
	}
	
	/* Write line which fills the entire row with a solid block of color */
	static void WriteShadedLine(string line, ConsoleColor bg, ConsoleColor fg)
	{
		Console.BackgroundColor = bg;
		Console.ForegroundColor = fg;
		
		Console.WriteLine(line.PadRight(Console.WindowWidth - 1));
		
		Console.ResetColor();
	}	
}
