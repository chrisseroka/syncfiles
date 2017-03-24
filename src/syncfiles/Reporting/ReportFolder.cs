using System;

namespace Syncfiles
{
	public class ReportFolder: IReportItem
	{
		public string Name {get;}
		public IReportItem[] Items {get;}

		public ReportFolder(string name, params IReportItem[] items)
		{
			this.Name = name;
			this.Items = items;
		}

	}
}
