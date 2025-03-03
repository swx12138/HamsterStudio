using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace HamsterStudio.Web.DataModels
{
    internal class DownloadorModel
    {
        private readonly List<BackgroundWorker> _ThreadPool;
        private readonly AutoResetEvent _NewTaskEnqueue = new(false);
        private readonly ConcurrentQueue<Action> _ActionQueue = new();

        public DownloadorModel(int threads)
        {
            _ThreadPool = CreateWorkers(threads).ToList();
        }

        ~DownloadorModel()
        {
            foreach (var worker in _ThreadPool)
            {
                worker.CancelAsync();
            }
        }

        private IEnumerable<BackgroundWorker> CreateWorkers(int threads)
        {
            for (int i = 0; i < threads; i++)
            {
                BackgroundWorker worker = new()
                {
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerAsync();
                yield return worker;
            }
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                try
                {
                    if (!_NewTaskEnqueue.WaitOne(100))
                    {
                        continue;
                    }

                    if (!_ActionQueue.TryDequeue(out Action? action))
                    {
                        continue;
                    }

                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("DownloadorModel::Worker_DoWork : " + ex.Message);
                }
            }
        }

        public void EnqueueTask(Action action)
        {
            _ActionQueue.Enqueue(action);
        }

    }
}
