using System.Collections.ObjectModel;

namespace HamsterStudio.Web.Interfaces
{
    public interface IHamsterTaskManager
    {
        ObservableCollection<IHamsterTask> DownloadingTasks { get; }

        ObservableCollection<IHamsterTask> FinishedTasks { get; }

        ObservableCollection<IHamsterTask> FailedTasks { get; }

        int EnqueueTaskCount { get; }

        void EnqueueTask(IHamsterTask task);

    }
}
