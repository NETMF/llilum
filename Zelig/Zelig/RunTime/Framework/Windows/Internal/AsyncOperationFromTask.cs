//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.Internal
{
    public sealed class AsyncOperationFromTask<T> : IAsyncOperation<T>
    {
        private readonly Task<T> m_task;
        private AsyncOperationCompletedHandler<T> m_completedHandler;

        public AsyncOperationFromTask(Task<T> task)
        {
            m_task = task;
        }

        public AsyncOperationCompletedHandler<T> Completed
        {
            get
            {
                return m_completedHandler;
            }

            set
            {
                if (Interlocked.CompareExchange(ref m_completedHandler, value, null) != null)
                {
                    throw new InvalidOperationException("Completed handler may only be set once.");
                }

                m_task.ContinueWith(task => m_completedHandler(this, Status));
            }
        }

        public Exception ErrorCode
        {
            get
            {
                return m_task.Exception;
            }
        }

        public uint Id => (uint)m_task.Id;

        public AsyncStatus Status
        {
            get
            {
                switch (m_task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        return AsyncStatus.Completed;

                    case TaskStatus.Canceled:
                        return AsyncStatus.Canceled;

                    case TaskStatus.Faulted:
                        return AsyncStatus.Error;

                    default:
                        return AsyncStatus.Started;
                }
            }
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            m_task.Dispose();
        }

        public T GetResults()
        {
            return m_task.Result;
        }

        public Task<T> Task
        {
            get
            {
                return m_task;
            }
        }
    }
}
