using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Task;
using HamsterStudio.Web.DataModels.Boundhub;
using HamsterStudio.Web.Interfaces;
using System.Diagnostics;

namespace HamsterStudio.Web.Tasks
{
    internal partial class BoundhubVideoTask(VideoDescriptor descriptor) :ObservableObject, IHamsterTask
    {
        //private readonly ObservableDownloader _ObservableDownloader = new();
        private readonly AutoResetEvent _NotifySuccess = new(false);

        [ObservableProperty]
        public long _MaxProgress;

        public event EventHandler<IHamsterTask>? TaskComplete;

        [ObservableProperty]
        private string _Name = descriptor.Uri.Split("?").First().Split("/").Last().Split("\\").First();

        public string Description => descriptor.Title;

        [ObservableProperty]
        private long _ProgressValue = 0;

        [ObservableProperty]
        private long _ProgressMaximum = 0;

        [ObservableProperty]
        private double _DownloadSpeed = 0;

        public AutoResetEvent NotifySuccess => _NotifySuccess;

        [ObservableProperty]
        private HamsterTaskState _State = HamsterTaskState.Waitting;

        public void Run()
        {
            try
            {
                State = HamsterTaskState.Running;
                //long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //long bytes = 0;
                //_ObservableDownloader.ProgressChanged += delegate (object? sender, MyDownloadProgressChangedEventArgs e)
                //{
                //    ProgressValue = e.BytesReceived;
                //    ProgressMaximum = e.TotalBytes;

                //    progressReport?.Invoke(e.BytesReceived * 100 / e.TotalBytes);
                //    long passed_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start;
                //    if (passed_ms > 500)
                //    {
                //        double updated_k = (e.BytesReceived - bytes) / 1024;

                //        DownloadSpeed = updated_k * 1000 / passed_ms;
                //        Trace.TraceInformation($"{Name} update {e.BytesReceived}(+{updated_k}k)/{e.TotalBytes} Speed : {DownloadSpeed:F2} kB/s");

                //        bytes = e.BytesReceived;
                //        start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //    }
                //};
                //_ObservableDownloader.DownloadFileAsync(descriptor.Uri, Name, descriptor.OriginUrl, descriptor.Cookie);
            }
            catch (Exception ex)
            {
                State = HamsterTaskState.Failed;
                Trace.TraceError("BoundhubVideoTask::Run : " + ex.Message);
            }
            State = HamsterTaskState.Succeed;
        }

        void Suspend()
        {
            throw new NotImplementedException();
        }
    }

    public class MyDownloadProgressChangedEventArgs : EventArgs
    {
        public long BytesReceived { get; }
        public long TotalBytes { get; }

        public MyDownloadProgressChangedEventArgs(long bytesReceived, long totalBytes)
        {
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }
    }
}
