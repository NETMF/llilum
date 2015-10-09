// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an abstract scheduler for tasks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> acts as the extension point for all 
    /// pluggable scheduling logic.  This includes mechanisms such as how to schedule a task for execution, and
    /// how scheduled tasks should be exposed to debuggers.
    /// </para>
    /// <para>
    /// All members of the abstract <see cref="TaskScheduler"/> type are thread-safe
    /// and may be used from multiple threads concurrently.
    /// </para>
    /// </remarks>
    [DebuggerDisplay("Id={Id}")]
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    ////[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public abstract class TaskScheduler
    {
        ////////////////////////////////////////////////////////////
        //
        // User Provided Methods and Properties
        //

        /// <summary>
        /// Queues a <see cref="T:System.Threading.Tasks.Task">Task</see> to the scheduler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A class derived from <see cref="T:System.Threading.Tasks.TaskScheduler">TaskScheduler</see>  
        /// implements this method to accept tasks being scheduled on the scheduler.
        /// A typical implementation would store the task in an internal data structure, which would
        /// be serviced by threads that would execute those tasks at some time in the future.
        /// </para>
        /// <para>
        /// This method is only meant to be called by the .NET Framework and
        /// should not be called directly by the derived class. This is necessary 
        /// for maintaining the consistency of the system.
        /// </para>
        /// </remarks>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task">Task</see> to be queued.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is null.</exception>
        [SecurityCritical]
        protected internal abstract void QueueTask(Task task);

        /// <summary>
        /// Determines whether the provided <see cref="T:System.Threading.Tasks.Task">Task</see>
        /// can be executed synchronously in this call, and if it can, executes it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A class derived from <see cref="TaskScheduler">TaskScheduler</see> implements this function to
        /// support inline execution of a task on a thread that initiates a wait on that task object. Inline
        /// execution is optional, and the request may be rejected by returning false. However, better
        /// scalability typically results the more tasks that can be inlined, and in fact a scheduler that
        /// inlines too little may be prone to deadlocks. A proper implementation should ensure that a
        /// request executing under the policies guaranteed by the scheduler can successfully inline. For
        /// example, if a scheduler uses a dedicated thread to execute tasks, any inlining requests from that
        /// thread should succeed.
        /// </para>
        /// <para>
        /// If a scheduler decides to perform the inline execution, it should do so by calling to the base
        /// TaskScheduler's
        /// <see cref="TryExecuteTask">TryExecuteTask</see> method with the provided task object, propagating
        /// the return value. It may also be appropriate for the scheduler to remove an inlined task from its
        /// internal data structures if it decides to honor the inlining request. Note, however, that under
        /// some circumstances a scheduler may be asked to inline a task that was not previously provided to
        /// it with the <see cref="QueueTask"/> method.
        /// </para>
        /// <para>
        /// The derived scheduler is responsible for making sure that the calling thread is suitable for
        /// executing the given task as far as its own scheduling and execution policies are concerned.
        /// </para>
        /// </remarks>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task">Task</see> to be
        /// executed.</param>
        /// <param name="taskWasPreviouslyQueued">A Boolean denoting whether or not task has previously been
        /// queued. If this parameter is True, then the task may have been previously queued (scheduled); if
        /// False, then the task is known not to have been queued, and this call is being made in order to
        /// execute the task inline without queueing it.</param>
        /// <returns>A Boolean value indicating whether the task was executed inline.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is
        /// null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <paramref name="task"/> was already
        /// executed.</exception>
        [SecurityCritical]
        protected abstract bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued);

        /// <summary>
        /// Generates an enumerable of <see cref="T:System.Threading.Tasks.Task">Task</see> instances
        /// currently queued to the scheduler waiting to be executed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A class derived from <see cref="TaskScheduler"/> implements this method in order to support
        /// integration with debuggers. This method will only be invoked by the .NET Framework when the
        /// debugger requests access to the data. The enumerable returned will be traversed by debugging
        /// utilities to access the tasks currently queued to this scheduler, enabling the debugger to
        /// provide a representation of this information in the user interface.
        /// </para>
        /// <para>
        /// It is important to note that, when this method is called, all other threads in the process will
        /// be frozen. Therefore, it's important to avoid synchronization with other threads that may lead to
        /// blocking. If synchronization is necessary, the method should prefer to throw a <see
        /// cref="System.NotSupportedException"/>
        /// than to block, which could cause a debugger to experience delays. Additionally, this method and
        /// the enumerable returned must not modify any globally visible state.
        /// </para>
        /// <para>
        /// The returned enumerable should never be null. If there are currently no queued tasks, an empty
        /// enumerable should be returned instead.
        /// </para>
        /// <para>
        /// For developers implementing a custom debugger, this method shouldn't be called directly, but
        /// rather this functionality should be accessed through the internal wrapper method
        /// GetScheduledTasksForDebugger:
        /// <c>internal Task[] GetScheduledTasksForDebugger()</c>. This method returns an array of tasks,
        /// rather than an enumerable. In order to retrieve a list of active schedulers, a debugger may use
        /// another internal method: <c>internal static TaskScheduler[] GetTaskSchedulersForDebugger()</c>.
        /// This static method returns an array of all active TaskScheduler instances.
        /// GetScheduledTasksForDebugger then may be used on each of these scheduler instances to retrieve
        /// the list of scheduled tasks for each.
        /// </para>
        /// </remarks>
        /// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This scheduler is unable to generate a list of queued tasks at this time.
        /// </exception>
        [SecurityCritical]
        protected abstract IEnumerable<Task> GetScheduledTasks();

        /// <summary>
        /// Indicates the maximum concurrency level this 
        /// <see cref="TaskScheduler"/>  is able to support.
        /// </summary>
        public virtual Int32 MaximumConcurrencyLevel
        {
            get
            {
                return Int32.MaxValue;
            }
        }

        /// <summary>
        /// Attempts to dequeue a <see cref="T:System.Threading.Tasks.Task">Task</see> that was previously queued to
        /// this scheduler.
        /// </summary>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task">Task</see> to be dequeued.</param>
        /// <returns>A Boolean denoting whether the <paramref name="task"/> argument was successfully dequeued.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is null.</exception>
        [SecurityCritical]
        protected internal virtual bool TryDequeue(Task task)
        {
            return false;
        }

        /// <summary>
        /// Notifies the scheduler that a work item has made progress.
        /// </summary>
        internal virtual void NotifyWorkItemProgress()
        {
        }

        /// <summary>
        /// Indicates whether this is a custom scheduler, in which case the safe code paths will be taken upon task entry
        /// using a CAS to transition from queued state to executing.
        /// </summary>
        internal virtual bool RequiresAtomicStartTransition
        {
            get { return true; }
        }

        ////////////////////////////////////////////////////////////
        //
        // Constructors and public properties
        //

        /// <summary>
        /// Initializes the <see cref="System.Threading.Tasks.TaskScheduler"/>.
        /// </summary>
        protected TaskScheduler()
        {
        }

        /// <summary>
        /// Gets the default <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see> instance.
        /// </summary>
        public static TaskScheduler Default
        {
            get
            {
#if DISABLED_FOR_LLILUM
                return s_defaultTaskScheduler;
#else // DISABLED_FOR_LLILUM
                return null;
#endif // DISABLED_FOR_LLILUM
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// associated with the currently executing task.
        /// </summary>
        /// <remarks>
        /// When not called from within a task, <see cref="Current"/> will return the <see cref="Default"/> scheduler.
        /// </remarks>
        public static TaskScheduler Current
        {
            get
            {
#if DISABLED_FOR_LLILUM
                TaskScheduler current = InternalCurrent;
                return current ?? TaskScheduler.Default;
#else // DISABLED_FOR_LLILUM
                return null;
#endif // DISABLED_FOR_LLILUM
            }
        }

        /// <summary>
        /// Creates a <see cref="TaskScheduler"/>
        /// associated with the current <see cref="T:System.Threading.SynchronizationContext"/>.
        /// </summary>
        /// <remarks>
        /// All <see cref="System.Threading.Tasks.Task">Task</see> instances queued to 
        /// the returned scheduler will be executed through a call to the
        /// <see cref="System.Threading.SynchronizationContext.Post">Post</see> method
        /// on that context.
        /// </remarks>
        /// <returns>
        /// A <see cref="TaskScheduler"/> associated with 
        /// the current <see cref="T:System.Threading.SynchronizationContext">SynchronizationContext</see>, as
        /// determined by <see cref="System.Threading.SynchronizationContext.Current">SynchronizationContext.Current</see>.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// The current SynchronizationContext may not be used as a TaskScheduler.
        /// </exception>
        public static TaskScheduler FromCurrentSynchronizationContext()
        {
#if DISABLED_FOR_LLILUM
            return new SynchronizationContextTaskScheduler();
#else // DISABLED_FOR_LLILUM
            throw new NotImplementedException();
#endif // DISABLED_FOR_LLILUM
        }

        /// <summary>
        /// Gets the unique ID for this <see cref="TaskScheduler"/>.
        /// </summary>
        public Int32 Id
        {
            get
            {
#if DISABLED_FOR_LLILUM
                if (m_taskSchedulerId == 0)
                {
                    int newId = 0;

                    // We need to repeat if Interlocked.Increment wraps around and returns 0.
                    // Otherwise next time this scheduler's Id is queried it will get a new value
                    do
                    {
                        newId = Interlocked.Increment(ref s_taskSchedulerIdCounter);
                    } while (newId == 0);

                    Interlocked.CompareExchange(ref m_taskSchedulerId, newId, 0);
                }

                return m_taskSchedulerId;
#else // DISABLED_FOR_LLILUM
                throw new NotImplementedException();
#endif // DISABLED_FOR_LLILUM
            }
        }

        /// <summary>
        /// Attempts to execute the provided <see cref="T:System.Threading.Tasks.Task">Task</see>
        /// on this scheduler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Scheduler implementations are provided with <see cref="T:System.Threading.Tasks.Task">Task</see>
        /// instances to be executed through either the <see cref="QueueTask"/> method or the
        /// <see cref="TryExecuteTaskInline"/> method. When the scheduler deems it appropriate to run the
        /// provided task, <see cref="TryExecuteTask"/> should be used to do so. TryExecuteTask handles all
        /// aspects of executing a task, including action invocation, exception handling, state management,
        /// and lifecycle control.
        /// </para>
        /// <para>
        /// <see cref="TryExecuteTask"/> must only be used for tasks provided to this scheduler by the .NET
        /// Framework infrastructure. It should not be used to execute arbitrary tasks obtained through
        /// custom mechanisms.
        /// </para>
        /// </remarks>
        /// <param name="task">
        /// A <see cref="T:System.Threading.Tasks.Task">Task</see> object to be executed.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <paramref name="task"/> is not associated with this scheduler.
        /// </exception>
        /// <returns>A Boolean that is true if <paramref name="task"/> was successfully executed, false if it
        /// was not. A common reason for execution failure is that the task had previously been executed or
        /// is in the process of being executed by another thread.</returns>
        [SecurityCritical]
        protected bool TryExecuteTask(Task task)
        {
#if DISABLED_FOR_LLILUM
            if (task.ExecutingTaskScheduler != this)
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskScheduler_ExecuteTask_WrongTaskScheduler"));
            }

            return task.ExecuteEntry(true);
#else // DISABLED_FOR_LLILUM
            throw new NotImplementedException();
#endif // DISABLED_FOR_LLILUM
        }

        ////////////////////////////////////////////////////////////
        //
        // Events
        //

        private static EventHandler<UnobservedTaskExceptionEventArgs> _unobservedTaskException;
        private static readonly object _unobservedTaskExceptionLockObject = new object();

        /// <summary>
        /// Occurs when a faulted <see cref="System.Threading.Tasks.Task"/>'s unobserved exception is about to trigger exception escalation
        /// policy, which, by default, would terminate the process.
        /// </summary>
        /// <remarks>
        /// This AppDomain-wide event provides a mechanism to prevent exception
        /// escalation policy (which, by default, terminates the process) from triggering. 
        /// Each handler is passed a <see cref="T:System.Threading.Tasks.UnobservedTaskExceptionEventArgs"/>
        /// instance, which may be used to examine the exception and to mark it as observed.
        /// </remarks>
        public static event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException
        {
            [System.Security.SecurityCritical]
            add
            {
                if (value != null)
                {
#if DISABLED_FOR_LLILUM
                    RuntimeHelpers.PrepareContractedDelegate(value);
#endif // DISABLED_FOR_LLILUM
                    lock (_unobservedTaskExceptionLockObject) _unobservedTaskException += value;
                }
            }

            [System.Security.SecurityCritical]
            remove
            {
                lock (_unobservedTaskExceptionLockObject) _unobservedTaskException -= value;
            }
        }

        ////////////////////////////////////////////////////////////
        //
        // Internal methods
        //

        // This is called by the TaskExceptionHolder finalizer.
        internal static void PublishUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs ueea)
        {
            // Lock this logic to prevent just-unregistered handlers from being called.
            lock (_unobservedTaskExceptionLockObject)
            {
                // Since we are under lock, it is technically no longer necessary
                // to make a copy.  It is done here for convenience.
                EventHandler<UnobservedTaskExceptionEventArgs> handler = _unobservedTaskException;
                if (handler != null)
                {
                    handler(sender, ueea);
                }
            }
        }

        /// <summary>
        /// Provides an array of all queued <see cref="System.Threading.Tasks.Task">Task</see> instances
        /// for the debugger.
        /// </summary>
        /// <remarks>
        /// The returned array is populated through a call to <see cref="GetScheduledTasks"/>.
        /// Note that this function is only meant to be invoked by a debugger remotely. 
        /// It should not be called by any other codepaths.
        /// </remarks>
        /// <returns>An array of <see cref="System.Threading.Tasks.Task">Task</see> instances.</returns> 
        /// <exception cref="T:System.NotSupportedException">
        /// This scheduler is unable to generate a list of queued tasks at this time.
        /// </exception>
        [SecurityCritical]
        internal Task[] GetScheduledTasksForDebugger()
        {
            // this can throw InvalidOperationException indicating that they are unable to provide the info
            // at the moment. We should let the debugger receive that exception so that it can indicate it in the UI
            IEnumerable<Task> activeTasksSource = GetScheduledTasks();

            if (activeTasksSource == null)
                return null;

            // If it can be cast to an array, use it directly
            Task[] activeTasksArray = activeTasksSource as Task[];
            if (activeTasksArray == null)
            {
                activeTasksArray = (new List<Task>(activeTasksSource)).ToArray();
            }

            // touch all Task.Id fields so that the debugger doesn't need to do a lot of cross-proc calls to generate them
            foreach (Task t in activeTasksArray)
            {
                int tmp = t.Id;
            }

            return activeTasksArray;
        }
    }

    /// <summary>
    /// Provides data for the event that is raised when a faulted <see cref="System.Threading.Tasks.Task"/>'s
    /// exception goes unobserved.
    /// </summary>
    /// <remarks>
    /// The Exception property is used to examine the exception without marking it
    /// as observed, whereas the <see cref="SetObserved"/> method is used to mark the exception
    /// as observed.  Marking the exception as observed prevents it from triggering exception escalation policy
    /// which, by default, terminates the process.
    /// </remarks>
    public class UnobservedTaskExceptionEventArgs : EventArgs
    {
        private AggregateException m_exception;
        internal bool m_observed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnobservedTaskExceptionEventArgs"/> class
        /// with the unobserved exception.
        /// </summary>
        /// <param name="exception">The Exception that has gone unobserved.</param>
        public UnobservedTaskExceptionEventArgs(AggregateException exception)
        { m_exception = exception; }

        /// <summary>
        /// Marks the <see cref="Exception"/> as "observed," thus preventing it
        /// from triggering exception escalation policy which, by default, terminates the process.
        /// </summary>
        public void SetObserved()
        { m_observed = true; }

        /// <summary>
        /// Gets whether this exception has been marked as "observed."
        /// </summary>
        public bool Observed
        { get { return m_observed; } }

        /// <summary>
        /// The Exception that went unobserved.
        /// </summary>
        public AggregateException Exception
        { get { return m_exception; } }
    }
}