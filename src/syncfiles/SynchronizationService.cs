using System;
using System.Collections.Generic;

namespace Syncfiles
{
   public class SynchronizationService 
   {
      public string[] GenerateMoveFilesReport(string inputFolder, string outputFolder) 
      {
         var result = new List<string>();
         var files = System.IO.Directory.EnumerateFiles(inputFolder, "*", System.IO.SearchOption.AllDirectories);
         var outputFolderPath = new System.IO.DirectoryInfo(outputFolder).FullName;
         foreach(var fileName in files)
         {
            var creationDate = System.IO.File.GetCreationTime(fileName);
            var relativePath = fileName.Replace(inputFolder + "\\", "");
            var creationYear = creationDate.Year.ToString();
            var creationMonth = creationDate.Month.ToString("D2");
            var outputFilepath = System.IO.Path.Combine(outputFolderPath, creationYear, creationMonth, relativePath);
            var reportLine = BuildReportLine(fileName, outputFilepath);

            result.Add(reportLine);
         }

         return result.ToArray();
      }

      public MoveFilesReport LoadMoveFilesReport(string path)
      {
          return MoveFilesReport.Load(path);
      }

      public MoveFilesSummary MoveFiles(MoveFilesReport report)
      {
         throw new NotImplementedException();
      }

      private string BuildReportLine(string inputFilePath, string outputFilePath)
      {
         return $"{inputFilePath}          ///-->///           {outputFilePath}";
      }
   }


   public class MoveFilesSummary 
   {

   }
}
