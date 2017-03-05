using Xunit;

namespace Syncfiles.Tests 
{
	public class WhenGettingReportFromFilesFolder 
	{
		[Fact]
		public void ShouldGenerateReportCorrectly()
		{
			var expectedReport = new Report(
						new ReportFile("IMG_6153.JPG", ""),
						new ReportFolder("dir",
							new ReportFile("IMG_6220.JPG", "")
							)
					);

			var report = FileScanner.Scan("files");

			Assert.Equal(SerializeReport(expectedReport), SerializeReport(report));
		}

		private string SerializeReport(Report report)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(report);
		}
	}
}
