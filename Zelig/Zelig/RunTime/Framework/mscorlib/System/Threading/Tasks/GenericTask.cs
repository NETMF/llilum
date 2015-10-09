// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// A task that produces a value.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Security.Permissions;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

// Disable the "reference to volatile field not treated as volatile" error.
#pragma warning disable 0420

namespace System.Threading.Tasks
{
    // Special internal struct that we use to signify that we are not interested in a Task<VoidTaskResult>'s result.
    internal struct VoidTaskResult
    {
    }

    /// <summary>
    /// Represents an asynchronous operation that produces a result at some time in the future.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the result produced by this <see cref="Task{TResult}"/>.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <see cref="Task{TResult}"/> instances may be created in a variety of ways. The most common approach is by
    /// using the task's <see cref="Factory"/> property to retrieve a <see
    /// cref="System.Threading.Tasks.TaskFactory{TResult}"/> instance that can be used to create tasks for several
    /// purposes. For example, to create a <see cref="Task{TResult}"/> that runs a function, the factory's StartNew
    /// method may be used:
    /// <code>
    /// // C#
    /// var t = Task&lt;int&gt;.Factory.StartNew(() => GenerateResult());
    /// - or -
    /// var t = Task.Factory.StartNew(() => GenerateResult());
    ///
    /// ' Visual Basic
    /// Dim t = Task&lt;int&gt;.Factory.StartNew(Function() GenerateResult())
    /// - or -
    /// Dim t = Task.Factory.StartNew(Function() GenerateResult())
    /// </code>
    /// </para>
    /// <para>
    /// The <see cref="Task{TResult}"/> class also provides constructors that initialize the task but that do not
    /// schedule it for execution. For performance reasons, the StartNew method should be the
    /// preferred mechanism for creating and scheduling computational tasks, but for scenarios where creation
    /// and scheduling must be separated, the constructors may be used, and the task's
    /// <see cref="System.Threading.Tasks.Task.Start()">Start</see>
    /// method may then be used to schedule the task for execution at a later time.
    /// </para>
    /// <para>
    /// All members of <see cref="Task{TResult}"/>, except for
    /// <see cref="System.Threading.Tasks.Task.Dispose()">Dispose</see>, are thread-safe
    /// and may be used from multiple threads concurrently.
    /// </para>
    /// </remarks>
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    [DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}, Result = {DebuggerDisplayResultDescription}")]
    public class Task<TResult> : Task
#if SUPPORT_IOBSERVABLE
        , IObservable<TResult>
#endif // SUPPORT_IOBSERVABLE
    {
        internal TResult m_result; // The value itself, if set.

        // Construct a promise-style task without any options.
        internal Task() : base()
        {
        }

        // Construct a pre-completed Task<TResult>
        internal Task(TResult result) : base(false, default(CancellationToken))
        {
            m_result = result;
        }

        /// <summary>
        /// Create a pre-canceled or pre-completed task.
        /// </summary>
        /// <param name="canceled">Whether the task is pre-canceled. If so, the task will be initialized to Canceled
        ///     state. Otherwise, the task will be initialized in the RanToCompletion state.</param>
        /// <param name="result">If not canceled, the final result of the task.</param>
        /// <param name="cancellationToken">If canceled, the token that triggered cancellation.</param>
        internal Task(bool canceled, TResult result, CancellationToken cancellationToken) :
            base(true, cancellationToken)
        {
            m_result = result;
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        public Task(Func<TResult> function) :
            base(function, null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to this task.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func<TResult> function, CancellationToken cancellationToken) :
            base(function, null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and creation options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Func<TResult> function, TaskCreationOptions creationOptions) :
            base(function, null, default(CancellationToken), creationOptions, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and creation options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions) :
            base(function, null, cancellationToken, creationOptions, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and state.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        public Task(Func<object, TResult> function, object state) :
            base(function, state, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to the new task.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken) :
            base(function, state, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Func<object, TResult> function, object state, TaskCreationOptions creationOptions) :
            base(function, state, default(CancellationToken), creationOptions, InternalTaskOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) :
            base(function, state, cancellationToken, creationOptions, InternalTaskOptions.None)
        {
        }

        internal Task(Delegate function, object state, CancellationToken cancellationToken, TaskCreationOptions options, InternalTaskOptions internalOptions) :
            base(function, state, cancellationToken, options, internalOptions)
        {
        }

        /// <summary>
        /// Gets the result value of this <see cref="Task{TResult}"/>.
        /// </summary>
        /// <remarks>
        /// The get accessor for this property ensures that the asynchronous operation is complete before
        /// returning. Once the result of the computation is available, it is stored and will be returned
        /// immediately on later calls to <see cref="Result"/>.
        /// </remarks>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public TResult Result
        {
            get
            {
                if (!IsRanToCompletion)
                {
                    // If the result has not been calculated yet, wait for it.
                    if (!IsCompleted)
                    {
                        InternalWait(Timeout.Infinite, default(CancellationToken));
                    }

#if DISABLED_FOR_LLILUM
                    // Throw an exception if appropriate.
                    if (!IsRanToCompletion)
                    {
                        ThrowIfExceptional(includeTaskCanceledExceptions: true);
                    }
#endif // DISABLED_FOR_LLILUM
                }

#if ENABLE_CONTRACTS
                // We shouldn't be here if the result has not been set.
                Contract.Assert(IsRanToCompletion, "Task<T>.Result getter: Expected result to have been set.");
#endif // ENABLE_CONTRACTS

                return m_result;
            }
        }

#if DISABLED_FOR_LLILUM
        /// <summary>
        /// Provides access to factory methods for creating <see cref="Task{TResult}"/> instances.
        /// </summary>
        /// <remarks>
        /// The factory returned from <see cref="Factory"/> is a default instance
        /// of <see cref="System.Threading.Tasks.TaskFactory{TResult}"/>, as would result from using
        /// the default constructor on the factory type.
        /// </remarks>
        public new static TaskFactory<TResult> Factory
        {
            get
            {
                return s_Factory;
            }
        }
#endif // DISABLED_FOR_LLILUM

        #region Await Support

        /// <summary>Gets an awaiter used to await this <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public new TaskAwaiter<TResult> GetAwaiter()
        {
            return new TaskAwaiter<TResult>(this);
        }

        /// <summary>Configures an awaiter used to await this <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        /// <returns>An object used to await this task.</returns>
        public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredTaskAwaitable<TResult>(this, continueOnCapturedContext);
        }

        #endregion Await Support

        #region Continuation methods

        #region Action<Task<TResult>> continuations

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
            return ContinueWith(continuationAction, default(CancellationToken), TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken)
        {
            return ContinueWith(continuationAction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, default(CancellationToken), TaskContinuationOptions.None, scheduler);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed. If the continuation criteria specified through the <paramref
        /// name="continuationOptions"/> parameter are not met, the continuation task will be canceled
        /// instead of scheduled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationAction, default(CancellationToken), continuationOptions, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its
        /// execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed. If the criteria specified through the <paramref name="continuationOptions"/> parameter
        /// are not met, the continuation task will be canceled instead of scheduled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task ContinueWith(
            Action<Task<TResult>> continuationAction,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions,
            TaskScheduler scheduler)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            if (scheduler != TaskScheduler.Default)
            {
                throw new NotImplementedException();
            }

            TaskCreationOptions creationOptions = CreationOptionsFromContinuationOptions(continuationOptions);
            var continuationTask = new ContinuationTaskFromResultTask<TResult>(this, continuationAction, null, cancellationToken, creationOptions);

            ContinueWithCore(continuationTask, continuationOptions);
            return continuationTask;
        }

        #endregion Action<Task<TResult>> continuations

        #region Action<Task<TResult>, object> continuations

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation action.</param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state)
        {
            return ContinueWith(continuationAction, state, default(CancellationToken), TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task ContinueWith(
            Action<Task<TResult>, object> continuationAction,
            object state,
            CancellationToken cancellationToken)
        {
            return ContinueWith(continuationAction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation action.</param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, state, default(CancellationToken), TaskContinuationOptions.None, scheduler);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation action.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed. If the continuation criteria specified through the <paramref
        /// name="continuationOptions"/> parameter are not met, the continuation task will be canceled
        /// instead of scheduled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        public Task ContinueWith(
            Action<Task<TResult>, object> continuationAction,
            object state,
            TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationAction, state, default(CancellationToken), continuationOptions, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its
        /// execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed. If the criteria specified through the <paramref name="continuationOptions"/> parameter
        /// are not met, the continuation task will be canceled instead of scheduled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task ContinueWith(
            Action<Task<TResult>, object> continuationAction,
            object state,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions,
            TaskScheduler scheduler)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            if (scheduler != TaskScheduler.Default)
            {
                throw new NotImplementedException();
            }

            TaskCreationOptions creationOptions = CreationOptionsFromContinuationOptions(continuationOptions);
            var continuationTask = new ContinuationTaskFromResultTask<TResult>(this, continuationAction, state, cancellationToken, creationOptions);

            ContinueWithCore(continuationTask, continuationOptions);
            return continuationTask;
        }

        #endregion Action<Task<TResult>, object> continuations

        #region Func<Task<TResult>,TNewResult> continuations

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
        {
            return ContinueWith(continuationFunction, default(CancellationToken), TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken)
        {
            return ContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes.  When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, TNewResult> continuationFunction,
            TaskScheduler scheduler)
        {
            return ContinueWith<TNewResult>(continuationFunction, default(CancellationToken), TaskContinuationOptions.None, scheduler);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// <para>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </para>
        /// <para>
        /// The <paramref name="continuationFunction"/>, when executed, should return a <see
        /// cref="Task{TNewResult}"/>. This task's completion state will be transferred to the task returned
        /// from the ContinueWith call.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, TNewResult> continuationFunction,
            TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationFunction, default(CancellationToken), continuationOptions, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be passed as
        /// an argument this completed task.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its
        /// execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// <para>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </para>
        /// <para>
        /// The <paramref name="continuationFunction"/>, when executed, should return a <see cref="Task{TNewResult}"/>.
        /// This task's completion state will be transferred to the task returned from the
        /// ContinueWith call.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, TNewResult> continuationFunction,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions,
            TaskScheduler scheduler)
        {
            if (continuationFunction == null)
            {
                throw new ArgumentNullException(nameof(continuationFunction));
            }

            if (scheduler != TaskScheduler.Default)
            {
                throw new NotImplementedException();
            }

            TaskCreationOptions creationOptions = CreationOptionsFromContinuationOptions(continuationOptions);
            var continuationTask = new ContinuationResultTaskFromResultTask<TResult, TNewResult>(this, continuationFunction, null, cancellationToken, creationOptions);

            ContinueWithCore(continuationTask, continuationOptions);
            return continuationTask;
        }

        #endregion Func<Task<TResult>,TNewResult> continuations

        #region Func<Task<TResult>, object,TNewResult> continuations

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation function.</param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, object, TNewResult> continuationFunction,
            object state)
        {
            return ContinueWith(continuationFunction, state, default(CancellationToken), TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, object, TNewResult>
            continuationFunction,
            object state,
            CancellationToken cancellationToken)
        {
            return ContinueWith(continuationFunction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes.  When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation function.</param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, object, TNewResult> continuationFunction,
            object state,
            TaskScheduler scheduler)
        {
            return ContinueWith(continuationFunction, state, default(CancellationToken), TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation function.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// <para>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current
        /// task has completed, whether it completes due to running to completion successfully, faulting due
        /// to an unhandled exception, or exiting out early due to being canceled.
        /// </para>
        /// <para>
        /// The <paramref name="continuationFunction"/>, when executed, should return a <see
        /// cref="Task{TNewResult}"/>. This task's completion state will be transferred to the task returned
        /// from the ContinueWith call.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, object, TNewResult> continuationFunction,
            object state,
            TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationFunction, state, default(CancellationToken), continuationOptions, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
        /// </summary>
        /// <typeparam name="TNewResult">
        /// The type of the result produced by the continuation.
        /// </typeparam>
        /// <param name="continuationFunction">
        /// A function to run when the <see cref="Task{TResult}"/> completes. When run, the delegate will be
        /// passed the completed task and the caller-supplied state object as arguments.
        /// </param>
        /// <param name="state">An object representing data to be used by the continuation function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="continuationOptions">
        /// Options for when the continuation is scheduled and how it behaves. This includes criteria, such
        /// as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled">OnlyOnCanceled</see>, as
        /// well as execution options, such as <see
        /// cref="System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously">ExecuteSynchronously</see>.
        /// </param>
        /// <param name="scheduler">
        /// The <see cref="TaskScheduler"/> to associate with the continuation task and to use for its
        /// execution.
        /// </param>
        /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
        /// <remarks>
        /// <para>
        /// The returned <see cref="Task{TNewResult}"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </para>
        /// <para>
        /// The <paramref name="continuationFunction"/>, when executed, should return a <see cref="Task{TNewResult}"/>.
        /// This task's completion state will be transferred to the task returned from the
        /// ContinueWith call.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationFunction"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="continuationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskContinuationOptions">TaskContinuationOptions</see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="scheduler"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, object, TNewResult> continuationFunction,
            object state,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions,
            TaskScheduler scheduler)
        {
            if (continuationFunction == null)
            {
                throw new ArgumentNullException(nameof(continuationFunction));
            }

            if (scheduler != TaskScheduler.Default)
            {
                throw new NotImplementedException();
            }

            TaskCreationOptions creationOptions = CreationOptionsFromContinuationOptions(continuationOptions);
            var continuationTask = new ContinuationResultTaskFromResultTask<TResult, TNewResult>(this, continuationFunction, state, cancellationToken, creationOptions);

            ContinueWithCore(continuationTask, continuationOptions);
            return continuationTask;
        }

        #endregion Func<Task<TResult>, object,TNewResult> continuations

        #endregion Continuation methods

        /// <summary>
        /// Subscribes an <see cref="IObserver{TResult}"/> to receive notification of the final state of this <see cref="Task{TResult}"/>.
        /// </summary>
        /// <param name="observer">
        /// The <see cref="IObserver{TResult}"/> to call on task completion. If this Task throws an exception,
        /// observer.OnError is called with this Task's AggregateException. If this Task RanToCompletion,
        /// observer.OnNext is called with this Task's result, followed by a call to observer.OnCompleted.
        /// If this Task is Canceled,  observer.OnError is called with a TaskCanceledException
        /// containing this Task's CancellationToken
        /// </param>
        /// <returns>An IDisposable object <see cref="Task"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="observer"/> argument is null.
        /// </exception>
#if SUPPORT_IOBSERVABLE
        IDisposable IObservable<TResult>.Subscribe(IObserver<TResult> observer)
        {
            if (observer == null)
                throw new System.ArgumentNullException(nameof(observer));


            var continuationTask =
                this.ContinueWith(delegate(Task<TResult> observedTask, object taskObserverObject)
                {
                    IObserver<TResult> taskObserver = (IObserver<TResult>)taskObserverObject;
                    if (observedTask.IsFaulted)
                        taskObserver.OnError(observedTask.Exception);
                    else if (observedTask.IsCanceled)
                        taskObserver.OnError(new TaskCanceledException(observedTask));
                    else
                    {
                        taskObserver.OnNext(observedTask.Result);
                        taskObserver.OnCompleted();
                    }

                }, observer, TaskScheduler.Default);

            return new DisposableSubscription(this, continuationTask);
        }
#endif // SUPPORT_IOBSERVABLE

        internal override void Invoke()
        {
            var function = m_action as Func<TResult>;
            if (function != null)
            {
                m_result = function();
                return;
            }

            var functionWithState = m_action as Func<object, TResult>;
            if (functionWithState != null)
            {
                m_result = functionWithState(m_stateObject);
                return;
            }

            // This task's m_action isn't set, which means it must be a special Promise-type task.
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Helper for setting a result for promise-style tasks.
        /// </summary>
        /// <param name="result"></param>
        /// <returns>Returns true if successful, false if not.</returns>
        internal bool TrySetResult(TResult result)
        {
            if (IsCompleted)
            {
                return false;
            }

#if ENABLE_CONTRACTS
            Contract.Assert(m_action == null, "Task<T>.TrySetResult(): non-null m_action");
#endif // ENABLE_CONTRACTS

            // Immediately transition to RanToCompletion before setting the result. Though this does introduce a narrow
            // race if someone is rapidly polling the completion status, this pattern should not be used in the wild.
            if (!AtomicStateUpdate(TaskStatus.RanToCompletion))
            {
                return false;
            }

            m_result = result;
            FinishContinuations();
            return true;
        }

        /// <summary>
        /// Helper for canceling promise-style tasks.
        /// </summary>
        /// <param name="tokenToRecord">Token to transfer to this task.</param>
        /// <returns>Returns true if successful, false if not.</returns>
        internal bool TrySetCanceled(CancellationToken tokenToRecord)
        {
            // Immediately transition to Canceled state before setting the result. Though this does introduce a narrow
            // race if someone is rapidly polling the completion status, this pattern should not be used in the wild.
            if (!AtomicStateUpdate(TaskStatus.Canceled))
            {
                return false;
            }

            RecordInternalCancellationRequest(tokenToRecord);
            FinishContinuations();
            return true;
        }
    }

#if SUPPORT_IOBSERVABLE
    // Class that calls RemoveContinuation if Dispose() is called before task completion
    internal class DisposableSubscription : IDisposable
    {
        private Task _notifyObserverContinuationTask;
        private Task _observedTask;

        internal DisposableSubscription(Task observedTask, Task notifyObserverContinuationTask)
        {
            _observedTask = observedTask;
            _notifyObserverContinuationTask = notifyObserverContinuationTask;
        }
        void IDisposable.Dispose()
        {
            Task localObservedTask = _observedTask;
            Task localNotifyingContinuationTask = _notifyObserverContinuationTask;
            if (localObservedTask != null && localNotifyingContinuationTask != null && !localObservedTask.IsCompleted)
            {
                localObservedTask.RemoveContinuation(localNotifyingContinuationTask);
            }
            _observedTask = null;
            _notifyObserverContinuationTask = null;
        }
    }
#endif // SUPPORT_IOBSERVABLE
}
