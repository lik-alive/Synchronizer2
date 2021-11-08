using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronizer.Processing
{
    public class AsyncBase
    {
        public enum AsyncStatus
        {
            RUNNING,
            FINISHED,
            ABORTED
        }

        public AsyncStatus Status { get; protected set; } = AsyncStatus.FINISHED;

        public Boolean IsRunning { get { return Status == AsyncStatus.RUNNING; } }
        public Boolean IsAborted { get { return Status == AsyncStatus.ABORTED; } }

        public Double Progress { get; protected set; }

        protected Thread execThread = null;

        protected Boolean forceStop = false;

        private Thread monitorThread = null;

        public AsyncBase()
        {
        }

        public void Start()
        {
            Status = AsyncStatus.RUNNING;
            Progress = 0;
            forceStop = false;
            monitorThread = new Thread(() =>
            {
                execThread.Start();
                execThread.Join();

                if (forceStop)
                    Status = AsyncStatus.ABORTED;
                else
                {
                    Status = AsyncStatus.FINISHED;
                    Progress = 100;
                }


                Finished?.Invoke();
            });
            monitorThread.Start();
        }

        public void Join()
        {
            monitorThread.Join();
        }

        public void Abort()
        {
            if (!IsRunning || execThread == null) return;

            forceStop = true;
            monitorThread.Join();
        }

        public event Action Finished;
    }
}

