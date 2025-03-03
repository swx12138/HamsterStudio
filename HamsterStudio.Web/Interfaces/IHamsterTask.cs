
namespace HamsterStudio.Web.Interfaces
{
    public delegate void ProgressReport(long n);

    //public struct TaskInfo
    //{
    //    public string Name;
    //    public string Desc;
    //    public string Url;
    //    public ProgressReport? progressReport;

    //    public TaskInfo(string name, string desc, string url, ProgressReport? pr)
    //    {
    //        Name = name;
    //        Desc = desc;
    //        Url = url;
    //        progressReport = pr;
    //    }
    //}

    public enum HamsterTaskState
    {
        Waitting, Running, Succeed, Failed
    }

    public interface IHamsterTask
    {
        public string Name { get; }

        public string Description { get; }

        public long ProgressValue { get; }

        public long ProgressMaximum { get; }

        public AutoResetEvent NotifySuccess { get; }

        public HamsterTaskState State { get; set; }

        public event EventHandler<IHamsterTask>? TaskComplete;

        void Run();
    }

}
