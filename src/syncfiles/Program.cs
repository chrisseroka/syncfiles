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

			var outputFileName = args[0];
			var inputFolder = args[1];
        	var report = FileScanner.Scan(inputFolder);
			var output = report.ToJson();
			System.IO.File.WriteAllText(outputFileName, output);

			var count = FileScanner.GetFilesCount(args[0]);
			System.Console.WriteLine($"Files found {count}");
        }
    }
}
