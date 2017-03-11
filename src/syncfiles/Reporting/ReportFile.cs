namespace Syncfiles
{
    public class ReportFile: IReportItem
	{
		public string Name {get;}
		public string Hash {get;}

		public ReportFile(string name, string hash)
		{
			this.Name = name;
			this.Hash = hash;
		}
	}
}
