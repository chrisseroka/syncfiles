using System;

namespace Syncfiles
{
    public class ReportFile: IReportItem
	{
		public string Name {get;}
		public string Hash {get;}
		public DateTime CreationTime {get;}

		public ReportFile(string name, string hash, DateTime creationTime)
		{
			this.Name = name;
			this.Hash = hash;
			this.CreationTime = creationTime;
		}
	}
}
