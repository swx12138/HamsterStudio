using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Web.Interfaces;
using System.ComponentModel;

namespace HamsterStudio.Web.Tasks
{
    partial class TimerTask : ObservableObject, IHamsterTask
    {
        public string Name => $"timer_{_TimerId}";
        public string Description => $"timer_{_TimerId}_desc";
        public long ProgressValue => _Progress;
        public long ProgressMaximum => _ProgressMaximum;
        public AutoResetEvent NotifySuccess => _success;

        private readonly int _TimerId;
        private readonly int _ProgressMaximum;
        private int _Progress = 0;
        private readonly BackgroundWorker _worker = new();
        private readonly AutoResetEvent _success = new(false);

        [ObservableProperty]
        private HamsterTaskState _State = HamsterTaskState.Waitting;

        public event EventHandler<IHamsterTask>? TaskComplete;

        public TimerTask(int timerId, int progressMaximum = 100)
        {
            _TimerId = timerId;
            _worker.DoWork += RunTask;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            _ProgressMaximum = progressMaximum;
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            TaskComplete?.Invoke(sender, this);
        }

        private void RunTask(object? sender, DoWorkEventArgs e)
        {
            while (ProgressValue < ProgressMaximum)
            {
                SetProperty(ref _Progress, _Progress + 1, nameof(ProgressValue));
                Thread.Sleep(100);
            }
            
            State = _ProgressMaximum > 30 ?
                HamsterTaskState.Succeed :
                HamsterTaskState.Failed;
            
            _success.Set();
        }

        public void Run()
        {
            State = HamsterTaskState.Running;
            _worker.RunWorkerAsync();
        }
    }
}
