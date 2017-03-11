using Xunit;

namespace Syncfiles.Tests
{
    public class WhenGettingMediaFileInfo
    {
    	private MediaFileInfo fileInfo;
    	
		public WhenGettingMediaFileInfo()
		{
			this.fileInfo = new MediaFileInfo(".\\files\\dir\\IMG_6220.JPG");
		}

        [Fact]
        public void ShouldRetrieveFileHash()
        {
			Assert.Equal("12fe13c3351f2db4531a9c00755c9676", fileInfo.Hash);
        }

        [Fact]
        public void ShouldRetrieveFileName()
		{
			Assert.True(fileInfo.FilePath.EndsWith("files\\dir\\IMG_6220.JPG"));
		}
    }

}
