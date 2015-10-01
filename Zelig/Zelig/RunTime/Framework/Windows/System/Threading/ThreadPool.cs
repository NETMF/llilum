//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Threading.Tasks;
using Windows.Foundation;

using ST = System.Threading;

namespace Windows.System.Threading
{
    /// <summary>
    /// Allows access to the thread pool.
    /// </summary>
    public static class ThreadPool
    {
        /// <summary>
        /// Creates a work item.
        /// </summary>
        /// <param name="handler">The method to call when a thread becomes available to run the work item.</param>
        /// <returns>An IAsyncAction interface that provides access to the work item.</returns>
        public static IAsyncAction RunAsync(WorkItemHandler handler)
        {
            return RunAsync(handler, WorkItemPriority.Normal, WorkItemOptions.None);
        }

        /// <summary>
        /// Creates a work item and specifies its priority relative to other work items in the thread pool.
        /// </summary>
        /// <param name="handler">The method to call when a thread becomes available to run the work item.</param>
        /// <param name="priority">The priority of the work item relative to other work items in the thread pool.</param>
        /// <returns>An IAsyncAction interface that provides access to the work item.</returns>
        public static IAsyncAction RunAsync(WorkItemHandler handler, WorkItemPriority priority)
        {
            return RunAsync(handler, priority, WorkItemOptions.None);
        }

        /// <summary>
        /// Creates a work item, specifies its priority relative to other work items in the thread pool, and specifies
        /// how long-running work items should be run.
        /// </summary>
        /// <param name="handler">The method to call when a thread becomes available to run the work item.</param>
        /// <param name="priority">The priority of the work item relative to other work items in the thread pool.</param>
        /// <param name="options">If this parameter is TimeSliced, the work item runs simultaneously with other time-
        ///     sliced work items with each work item receiving a share of processor time. If this parameter is None,
        ///     the work item runs when a worker thread becomes available.</param>
        /// <returns>An IAsyncAction interface that provides access to the work item.</returns>
        public static IAsyncAction RunAsync(WorkItemHandler handler, WorkItemPriority priority, WorkItemOptions options)
        {
            return new AsyncActionFromWorkItem(handler);
        }

        /// <summary>
        /// This is a helper class specifically for RunAsync. It helps us break the circular dependency between Task,
        /// WorkItemHandler, and IAsyncAction. We could similarly implement this with a continuation, but that approach
        /// has two drawbacks: higher heap impact and creation of a dependency on the System.Runtime.WindowsRuntime
        /// assembly.
        /// </summary>
        private class AsyncActionFromWorkItem : IAsyncAction
        {
            private readonly Task m_task;
            private AsyncActionCompletedHandler m_completedHandler;

            public AsyncActionFromWorkItem(WorkItemHandler handler)
            {
                m_task = Task.Run(() => handler(this));
            }

            public AsyncActionCompletedHandler Completed
            {
                get
                {
                    return m_completedHandler;
                }

                set
                {
                    if (ST.Interlocked.CompareExchange(ref m_completedHandler, value, null) != null)
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

            public void GetResults()
            {
            }
        }
    }
}
