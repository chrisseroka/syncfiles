namespace Syncfiles
{
    public class Report 
	{
		public IReportItem[] Items {get;}

		public Report(params IReportItem[] items) 
		{
			this.Items = items;
		}
	}
}
