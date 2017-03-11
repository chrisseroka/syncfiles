using Xunit;

namespace Syncfiles.Tests 
{
	public class WhenGettingReportFromFilesFolder 
	{
		[Fact]
		public void ShouldGenerateReportCorrectly()
		{
			var expectedReport = new Report(
						new ReportFile("files\\IMG_6153.JPG", "d6d2ea3e769584404d0905a4325d7e16"),
						new ReportFolder("files\\dir",
							new ReportFile("files\\dir\\IMG_6220.JPG", "12fe13c3351f2db4531a9c00755c9676")
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
