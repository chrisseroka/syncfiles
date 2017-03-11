using System;

namespace Syncfiles
{
    class Program
    {
        static void Main(string[] args)
        {
        	foreach(var arg in args)
			{
				Console.WriteLine($"arg: {arg}");
			}

        	var report = FileScanner.Scan(args[0]);
			report.ToText();
        }
    }
}
