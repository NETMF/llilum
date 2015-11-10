//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;

namespace Windows.Internal
{
    using Windows.Foundation;

    public sealed class SynchronousOperation<T> : IAsyncOperation<T>
    {
        internal readonly T m_result;
        private AsyncOperationCompletedHandler<T> m_completedHandler;

        public SynchronousOperation(T createdObject)
        {
            m_result = createdObject;
            Id = AsyncInfoHelper.GetNextActionId();
        }

        public SynchronousOperation(Exception error)
        {
            ErrorCode = error;
            Id = AsyncInfoHelper.GetNextActionId();
        }

        public AsyncOperationCompletedHandler<T> Completed
        {
            get
            {
                return m_completedHandler;
            }

            set
            {
                if(m_completedHandler != null)
                {
                    throw new InvalidOperationException();
                }
                m_completedHandler = value;
            }
        }

        public Exception ErrorCode
        {
            get;
        }

        public uint Id
        {
            get;
        }

        public AsyncStatus Status
        {
            get
            {
                if (ErrorCode == null)
                {
                    return AsyncStatus.Completed;
                }
                else
                {
                    return AsyncStatus.Error;
                }
            }
        }

        public void Cancel()
        {
            // NOP
        }

        public void Close()
        {
            if (m_completedHandler != null)
            {
                m_completedHandler.Invoke(this, Status);
            }

            IDisposable currentObject = m_result as IDisposable;
            if(currentObject != null)
            {
                currentObject.Dispose();
            }
        }

        public T GetResults()
        {
            return m_result;
        }
    }

    public sealed class SynchronousOperationWithProgress<T, P> : IAsyncOperationWithProgress<T, P>
    {
        private readonly T m_result;
        private AsyncOperationWithProgressCompletedHandler<T, P> m_completedHandler;
        private AsyncOperationProgressHandler<T, P> m_progressHandler;

        public SynchronousOperationWithProgress(T createdObject)
        {
            m_result = createdObject;
            Id = AsyncInfoHelper.GetNextActionId();
        }

        public SynchronousOperationWithProgress(Exception error)
        {
            ErrorCode = error;
            Id = AsyncInfoHelper.GetNextActionId();
        }

        public Exception ErrorCode
        {
            get;
        }

        public uint Id
        {
            get;
        }

        public AsyncStatus Status
        {
            get
            {
                if (ErrorCode == null)
                {
                    return AsyncStatus.Completed;
                }
                else
                {
                    return AsyncStatus.Error;
                }
            }
        }

        public AsyncOperationProgressHandler<T, P> Progress
        {
            get
            {
                return m_progressHandler;
            }

            set
            {
                m_progressHandler = value;
            }
        }

        AsyncOperationWithProgressCompletedHandler<T, P> IAsyncOperationWithProgress<T, P>.Completed
        {
            get
            {
                return m_completedHandler;
            }

            set
            {
                if (m_completedHandler != null)
                {
                    throw new InvalidOperationException();
                }
                m_completedHandler = value;
            }
        }

        public void Cancel()
        {
            // NOP
        }

        public T GetResults()
        {
            return m_result;
        }

        public void Close()
        {
            if (m_completedHandler != null)
            {
                m_completedHandler.Invoke(this, Status);
            }

            IDisposable currentObject = m_result as IDisposable;
            if (currentObject != null)
            {
                currentObject.Dispose();
            }
        }
    }
}
