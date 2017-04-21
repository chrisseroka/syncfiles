using System.IO;
using System;
using Xunit;
using System.Globalization;

namespace Syncfiles.Tests
{
   public class SynchronizationServiceTests
   {
      private const string testFilesDirectory = "syncFilesTestDir";
      private SynchronizationService service;
      private string outputFolder;
      private string inputFolder;

      public SynchronizationServiceTests()
      {
         this.inputFolder = PrepareFolder("input");
         this.outputFolder = PrepareFolder("output");

         this.service = new SynchronizationService();
      }

      [Fact]
      public void ShouldGenerateMoveFilesReport()
      {
         //ARRANGE
         TouchFile(inputFolder, @"download1.JPG", "2015-01-23");
         TouchFile(inputFolder, @"My vacations\DCIM\IMG_001.JPG", "2017-03-01");
         TouchFile(inputFolder, @"Personal\Unsorted\My Photos\2016_07_10.JPG", "2016-07-10");

         //ACT
         var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

         //ASSERT
         var expectedReport = new[] {
               ReportLine(inputFolder, @"download1.JPG", outputFolder, @"2015\01\download1.JPG"),
               ReportLine(inputFolder, @"My vacations\DCIM\IMG_001.JPG", outputFolder, @"2017\03\My vacations\DCIM\IMG_001.JPG"),
               ReportLine(inputFolder, @"Personal\Unsorted\My Photos\2016_07_10.JPG", outputFolder, @"2016\07\Personal\Unsorted\My Photos\2016_07_10.JPG"),
         };
         Assert.Equal(expectedReport[0], report[0]);
         Assert.Equal(expectedReport[1], report[1]);
         Assert.Equal(expectedReport[2], report[2]);
      }

      [Fact]
      public void ShouldReturnEmptyFileForEmptyInput()
      {
         //ARRANGE
         //Leaving empty...

         //ACT
         var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

         //ASSERT
         Assert.Equal(Array.Empty<string>(), report);
      }

      public string ReportLine(string inputFolder, string inputFile, string outputFolder, string outputFile)
      {
            var inputFilePath = Path.Combine(inputFolder, inputFile);
            var outputFilePath = Path.Combine(outputFolder, outputFile);
            return $"{inputFilePath}          ///-->///           {outputFilePath}";
      }

      private string PrepareFolder(string name)
      {
         var path = Path.Combine(Directory.GetCurrentDirectory(), testFilesDirectory, name);
         if (Directory.Exists(path)) 
         {
            Directory.Delete(path, true);
         }
         Directory.CreateDirectory(path);
         return path;
      }

      private void TouchFile(string inputFolder, string relativePath, string creationDate)
      {
         var path = Path.Combine(inputFolder, relativePath);
         Directory.CreateDirectory(Path.GetDirectoryName(path));
            
         File.Create(path).Dispose();
         File.SetCreationTime(path, DateTime.ParseExact(creationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
      }
    }
}
