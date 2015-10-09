//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// Types for awaiting Task and Task<T>. These types are emitted from Task{<T>}.GetAwaiter
// and Task{<T>}.ConfigureAwait.  They are meant to be used only by the compiler, e.g.
//
//   await nonGenericTask;
//   =====================
//       var $awaiter = nonGenericTask.GetAwaiter();
//       if (!$awaiter.IsCompleted)
//       {
//           SPILL:
//           $builder.AwaitUnsafeOnCompleted(ref $awaiter, ref this);
//           return;
//           Label:
//           UNSPILL;
//       }
//       $awaiter.GetResult();
//
//   result += await genericTask.ConfigureAwait(false);
//   ===================================================================================
//       var $awaiter = genericTask.ConfigureAwait(false).GetAwaiter();
//       if (!$awaiter.IsCompleted)
//       {
//           SPILL;
//           $builder.AwaitUnsafeOnCompleted(ref $awaiter, ref this);
//           return;
//           Label:
//           UNSPILL;
//       }
//       result += $awaiter.GetResult();
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Diagnostics.Contracts;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

// NOTE: For performance reasons, initialization is not verified.  If a developer
//       incorrectly initializes a task awaiter, which should only be done by the compiler,
//       NullReferenceExceptions may be generated (the alternative would be for us to detect
//       this case and then throw a different exception instead).  This is the same tradeoff
//       that's made with other compiler-focused value types like List<T>.Enumerator.

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides an awaiter for awaiting a <see cref="System.Threading.Tasks.Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    ////[HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>The task being awaited.</summary>
        private readonly Task m_task;

        /// <summary>Initializes the <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to be awaited.</param>
        internal TaskAwaiter(Task task)
        {
#if ENABLE_CONTRACTS
            Contract.Requires(task != null, "Constructing an awaiter requires a task to await.");
#endif // ENABLE_CONTRACTS
            m_task = task;
        }

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted => m_task.IsCompleted;

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecuritySafeCritical]
        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
        }

        /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task"/>.</summary>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
        public void GetResult()
        {
            ValidateEnd(m_task);
        }

        /// <summary>
        /// Fast checks for the end of an await operation to determine whether more needs to be done
        /// prior to completing the await.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        internal static void ValidateEnd(Task task)
        {
            // Synchronously wait for the task to complete. When used by the compiler, the task will already be complete.
            // This code exists only for direct GetResult use, for cases where the same exception propagation semantics
            // used by "await" are desired, but where for one reason or another synchronous rather than asynchronous
            // waiting is needed.
            if (!task.IsCompleted)
            {
                bool taskCompleted = task.InternalWait(Timeout.Infinite, default(CancellationToken));
#if ENABLE_CONTRACTS
                Contract.Assert(taskCompleted, "With an infinite timeout, the task should have always completed.");
#endif // ENABLE_CONTRACTS
            }

            // And throw an exception if the task is faulted or canceled.
            switch (task.Status)
            {
            case TaskStatus.RanToCompletion:
                break;

            // If the task completed in a canceled state, throw an OperationCanceledException.
            // This will either be the OCE that actually caused the task to cancel, or it will be a new
            // TaskCanceledException. TCE derives from OCE, and by throwing it we automatically pick up the
            // completed task's CancellationToken if it has one, including that CT in the OCE.
            case TaskStatus.Canceled:
                throw new TaskCanceledException(task);

            // If the task faulted, throw its first exception,
            // even if it contained more than one.
            case TaskStatus.Faulted:
                throw task.Exception;
            }
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The task being awaited.</param>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
        /// <param name="flowExecutionContext">Whether to flow ExecutionContext across the await.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecurityCritical]
        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext, bool flowExecutionContext)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            // Set the continuation onto the awaited task.
            // BUGBUG: We don't currently support capturing context, so this will always run on the thread pool.
            task.SetContinuationForAwait(continuation);
        }
    }

    /// <summary>Provides an awaiter for awaiting a <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    ////[HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        /// <summary>The task being awaited.</summary>
        private readonly Task<TResult> m_task;

        /// <summary>Initializes the <see cref="TaskAwaiter{TResult}"/>.</summary>
        /// <param name="task">The <see cref="System.Threading.Tasks.Task{TResult}"/> to be awaited.</param>
        internal TaskAwaiter(Task<TResult> task)
        {
#if ENABLE_CONTRACTS
            Contract.Requires(task != null, "Constructing an awaiter requires a task to await.");
#endif // ENABLE_CONTRACTS
            m_task = task;
        }

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted => m_task.IsCompleted;

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecuritySafeCritical]
        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
        }

        /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
        /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
        public TResult GetResult()
        {
            TaskAwaiter.ValidateEnd(m_task);
            return m_task.Result;
        }
    }

    /// <summary>Provides an awaitable object that allows for configured awaits on <see cref="System.Threading.Tasks.Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct ConfiguredTaskAwaitable
    {
        /// <summary>The task being awaited.</summary>
        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        /// <summary>Initializes the <see cref="ConfiguredTaskAwaitable"/>.</summary>
        /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        internal ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext)
        {
#if ENABLE_CONTRACTS
            Contract.Requires(task != null, "Constructing an awaitable requires a task to await.");
#endif // ENABLE_CONTRACTS
            m_configuredTaskAwaiter = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        /// <summary>Gets an awaiter for this awaitable.</summary>
        /// <returns>The awaiter.</returns>
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return m_configuredTaskAwaiter;
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskAwaitable"/>.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        ////[HostProtection(Synchronization = true, ExternalThreading = true)]
        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
        {
            /// <summary>The task being awaited.</summary>
            private readonly Task m_task;
            /// <summary>Whether to attempt marshaling back to the original context.</summary>
            private readonly bool m_continueOnCapturedContext;

            /// <summary>Initializes the <see cref="ConfiguredTaskAwaiter"/>.</summary>
            /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to await.</param>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the original context captured
            /// when BeginAwait is called; otherwise, false.
            /// </param>
            internal ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext)
            {
#if ENABLE_CONTRACTS
                Contract.Requires(task != null, "Constructing an awaiter requires a task to await.");
#endif // ENABLE_CONTRACTS
                m_task = task;
                m_continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets whether the task being awaited is completed.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            public bool IsCompleted => m_task.IsCompleted;

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecuritySafeCritical]
            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
            }

            /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task"/>.</summary>
            /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public void GetResult()
            {
                TaskAwaiter.ValidateEnd(m_task);
            }
        }
    }

    /// <summary>Provides an awaitable object that allows for configured awaits on <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct ConfiguredTaskAwaitable<TResult>
    {
        /// <summary>The underlying awaitable on whose logic this awaitable relies.</summary>
        private readonly ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        /// <summary>Initializes the <see cref="ConfiguredTaskAwaitable{TResult}"/>.</summary>
        /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
        {
            m_configuredTaskAwaiter = new ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        /// <summary>Gets an awaiter for this awaitable.</summary>
        /// <returns>The awaiter.</returns>
        public ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return m_configuredTaskAwaiter;
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskAwaitable{TResult}"/>.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        [HostProtection(Synchronization = true, ExternalThreading = true)]
        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
        {
            /// <summary>The task being awaited.</summary>
            private readonly Task<TResult> m_task;
            /// <summary>Whether to attempt marshaling back to the original context.</summary>
            private readonly bool m_continueOnCapturedContext;

            /// <summary>Initializes the <see cref="ConfiguredTaskAwaiter"/>.</summary>
            /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
            /// </param>
            internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
            {
#if ENABLE_CONTRACTS
                Contract.Requires(task != null, "Constructing an awaiter requires a task to await.");
#endif // ENABLE_CONTRACTS
                m_task = task;
                m_continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets whether the task being awaited is completed.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            public bool IsCompleted => m_task.IsCompleted;

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecuritySafeCritical]
            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
            }

            /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
            /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public TResult GetResult()
            {
                TaskAwaiter.ValidateEnd(m_task);
                return m_task.Result;
            }
        }
    }
}
