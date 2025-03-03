using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Web.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace HamsterStudio.Web.Services
{
    public partial class HamsterTaskManager : ObservableObject, IHamsterTaskManager
    {
        [ObservableProperty]
        private ObservableCollection<IHamsterTask> _DownloadingTasks = [];

        [ObservableProperty]
        private ObservableCollection<IHamsterTask> _FinishedTasks = [];

        [ObservableProperty]
        private ObservableCollection<IHamsterTask> _FailedTasks = [];

        private readonly List<BackgroundWorker> DownloadBackgroundWorkers_ = [];

        [ObservableProperty]
        private int _EnqueueTaskCount = 0;

        public void EnqueueTask(IHamsterTask task)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ++EnqueueTaskCount;
                DownloadingTasks.Add(task);
            });
        }

        public HamsterTaskManager()
        {
            for (int i = 0; i < 5; i++)
            {
                BackgroundWorker worker = new()
                {
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += DownloadBackgroundWorker_DoWork;
                worker.RunWorkerAsync();
                DownloadBackgroundWorkers_.Add(worker);
            }
        }

        private void DownloadBackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                try
                {
                    if (DownloadingTasks.Count == 0)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    IHamsterTask? task;
                    lock (DownloadingTasks)
                    {
                        task = DownloadingTasks.Where(x => x.State == HamsterTaskState.Waitting).FirstOrDefault();
                        // Marshal the removal of the task to the UI thread
                        if (task != null)
                        {
                            task.State = HamsterTaskState.Running;
                            task.TaskComplete += (s, t) => Application.Current.Dispatcher.Invoke(() =>
                            {
                                lock (DownloadingTasks)
                                    DownloadingTasks.Remove(task);
                                switch (task.State)
                                {
                                    case HamsterTaskState.Succeed: FinishedTasks.Add(task); ; break;
                                    case HamsterTaskState.Failed: FailedTasks.Add(task); break;
                                    default: throw new NotImplementedException();
                                }
                            });
                        }
                    }

                    if (task != null)
                    {
                        task.Run();
                        task.NotifySuccess.WaitOne();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceWarning($"DownloadBackgroundWorker_DoWork : {ex.Message}");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"DownloadBackgroundWorker_DoWork : {ex.Message}");
                }
            }
        }

    }
}
