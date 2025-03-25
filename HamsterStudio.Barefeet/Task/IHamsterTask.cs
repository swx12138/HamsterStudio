namespace HamsterStudio.Barefeet.Task;

public delegate void ProgressReport(long n);

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
