//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
// Compiler-targeted types that build tasks for use as the return types of asynchronous methods.
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

// In the desktop and phone CLR, the system caches lightweight tasks for common default values to reduce allocation
// overhead. In the future, we may want to enable limited caching if performance/memory analysis indicates that it would
// be beneficial. For now, we're generating these results on demand to limit static memory overhead.
//#define ENABLE_CACHED_TASKS

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Provides a builder for asynchronous methods that return void.
    /// This type is intended for compiler use only.
    /// </summary>
    ////[HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct AsyncVoidMethodBuilder
    {
        /// <summary>State related to the IAsyncStateMachine.</summary>
        private AsyncMethodBuilderCore m_coreState; // mutable struct: must not be readonly
        /// <summary>Task used for debugging and logging purposes only.  Lazily initialized.</summary>
        private Task m_task;

        /// <summary>Initializes a new <see cref="AsyncVoidMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncVoidMethodBuilder"/>.</returns>
        public static AsyncVoidMethodBuilder Create()
        {
            return new AsyncVoidMethodBuilder();
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).</exception>
        [SecuritySafeCritical]
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            AsyncMethodBuilderCore.Start(stateMachine);
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            m_coreState.SetStateMachine(stateMachine); // argument validation handled by AsyncMethodBuilderCore
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                awaiter.OnCompleted(m_coreState.GetCompletionAction(stateMachine));
            }
            catch (Exception exc)
            {
                // Prevent exceptions from leaking to the call site, which could then allow multiple flows of execution
                // through the same async method if the awaiter had already scheduled the continuation by the time the
                // exception was thrown. We propagate the exception on the ThreadPool because we can trust it to not
                // throw, unlike if we were to go to a user-supplied SynchronizationContext, whose Post method could
                // easily throw.
                AsyncMethodBuilderCore.ThrowAsync(exc);
            }
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                awaiter.UnsafeOnCompleted(m_coreState.GetCompletionAction(stateMachine));
            }
            catch (Exception e)
            {
                AsyncMethodBuilderCore.ThrowAsync(e);
            }
        }

        /// <summary>Completes the method builder successfully.</summary>
        public void SetResult()
        {
        }

        /// <summary>Faults the method builder with an exception.</summary>
        /// <param name="exception">The exception that is the cause of this fault.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="exception"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is not initialized.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

#if ENABLE_CONTRACTS
            Contract.EndContractBlock();
#endif // ENABLE_CONTRACTS

            // Otherwise, queue the exception to be thrown on the ThreadPool.
            AsyncMethodBuilderCore.ThrowAsync(exception);
        }

        // This property lazily instantiates the Task in a non-thread-safe manner.
        private Task Task
        {
            get
            {
                if (m_task == null)
                {
                    m_task = new Task();
                }

                return m_task;
            }
        }
    }

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="System.Threading.Tasks.Task"/>.
    /// This type is intended for compiler use only.
    /// </summary>
    /// <remarks>
    /// AsyncTaskMethodBuilder is a value type, and thus it is copied by value.
    /// Prior to being copied, one of its Task, SetResult, or SetException members must be accessed,
    /// or else the copies may end up building distinct Task instances.
    /// </remarks>
    ////[HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct AsyncTaskMethodBuilder
    {
#if ENABLE_CACHED_TASKS // We cache VoidTaskResult regardless of our overall cache policy, since it's extremely common.
        /// <summary>A cached VoidTaskResult task used for builders that complete synchronously.</summary>
        private readonly static Task<VoidTaskResult> s_cachedCompleted = AsyncTaskMethodBuilder<VoidTaskResult>.s_defaultResultTask;
#else // ENABLE_CACHED_TASKS
        /// <summary>A cached VoidTaskResult task used for builders that complete synchronously.</summary>
        private readonly static Task<VoidTaskResult> s_cachedCompleted = new Task<VoidTaskResult>(default(VoidTaskResult));
#endif // ENABLE_CACHED_TASKS

        /// <summary>The generic builder object to which this non-generic instance delegates.</summary>
        private AsyncTaskMethodBuilder<VoidTaskResult> m_builder; // mutable struct: must not be readonly

        /// <summary>Initializes a new <see cref="AsyncTaskMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncTaskMethodBuilder"/>.</returns>
        public static AsyncTaskMethodBuilder Create()
        {
            return default(AsyncTaskMethodBuilder);
            // Note: If ATMB<T>.Create is modified to do any initialization, this
            //       method needs to be updated to do m_builder = ATMB<T>.Create().
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [SecuritySafeCritical]
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            AsyncMethodBuilderCore.Start(stateMachine);
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            m_builder.SetStateMachine(stateMachine); // argument validation handled by AsyncMethodBuilderCore
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            m_builder.AwaitOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            m_builder.AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }

        /// <summary>Gets the <see cref="System.Threading.Tasks.Task"/> for this builder.</summary>
        /// <returns>The <see cref="System.Threading.Tasks.Task"/> representing the builder's asynchronous operation.</returns>
        /// <exception cref="System.InvalidOperationException">The builder is not initialized.</exception>
        public Task Task => m_builder.Task;

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task"/> in the
        /// <see cref="System.Threading.Tasks.TaskStatus">RanToCompletion</see> state.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        public void SetResult()
        {
            // Accessing AsyncTaskMethodBuilder.s_cachedCompleted is faster than
            // accessing AsyncTaskMethodBuilder<T>.s_defaultResultTask.
            m_builder.SetResult(s_cachedCompleted);
        }

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task"/> in the
        /// <see cref="System.Threading.Tasks.TaskStatus">Faulted</see> state with the specified exception.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> to use to fault the task.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="exception"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
#if DISABLED_FOR_LLILUM
            m_builder.SetException(exception);
#else // DISABLED_FOR_LLILUM
            // We don't yet enable catching of exceptions, so this path should never run.
            throw new NotImplementedException();
#endif // DISABLED_FOR_LLILUM
        }
    }

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="System.Threading.Tasks.Task{TResult}"/>.
    /// This type is intended for compiler use only.
    /// </summary>
    /// <remarks>
    /// AsyncTaskMethodBuilder{TResult} is a value type, and thus it is copied by value.
    /// Prior to being copied, one of its Task, SetResult, or SetException members must be accessed,
    /// or else the copies may end up building distinct Task instances.
    /// </remarks>
    ////[HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct AsyncTaskMethodBuilder<TResult>
    {
#if ENABLE_CACHED_TASKS
        /// <summary>A cached task for default(TResult).</summary>
        internal readonly static Task<TResult> s_defaultResultTask = AsyncTaskCache.CreateCacheableTask(default(TResult));
#endif // ENABLE_CACHED_TASKS

        private static readonly string s_transitionToFinalAlreadyCompleted = "An attempt was made to transition a task to a final state when it had already completed.";

        // WARNING: For performance reasons, the m_task field is lazily initialized.
        //          For correct results, the struct AsyncTaskMethodBuilder<TResult> must
        //          always be used from the same location/copy, at least until m_task is
        //          initialized.  If that guarantee is broken, the field could end up being
        //          initialized on the wrong copy.

        /// <summary>State related to the IAsyncStateMachine.</summary>
        private AsyncMethodBuilderCore m_coreState; // mutable struct: must not be readonly
        /// <summary>The lazily-initialized built task.</summary>
        private Task<TResult> m_task; // lazily-initialized: must not be readonly

        /// <summary>Initializes a new <see cref="AsyncTaskMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncTaskMethodBuilder"/>.</returns>
        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            return default(AsyncTaskMethodBuilder<TResult>);
            // NOTE:  If this method is ever updated to perform more initialization,
            //        ATMB.Create must also be updated to call this Create method.
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [SecuritySafeCritical]
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            AsyncMethodBuilderCore.Start(stateMachine);
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            m_coreState.SetStateMachine(stateMachine); // argument validation handled by AsyncMethodBuilderCore
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                awaiter.OnCompleted(m_coreState.GetCompletionAction(stateMachine));
            }
            catch (Exception e)
            {
                AsyncMethodBuilderCore.ThrowAsync(e);
            }
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                awaiter.UnsafeOnCompleted(m_coreState.GetCompletionAction(stateMachine));
            }
            catch (Exception e)
            {
                AsyncMethodBuilderCore.ThrowAsync(e);
            }
        }

        /// <summary>Gets the <see cref="System.Threading.Tasks.Task{TResult}"/> for this builder.</summary>
        /// <returns>The <see cref="System.Threading.Tasks.Task{TResult}"/> representing the builder's asynchronous operation.</returns>
        public Task<TResult> Task
        {
            get
            {
                // Get and return the task. If there isn't one, first create one and store it.
                if (m_task == null)
                {
                    m_task = new Task<TResult>();
                }

                return m_task;
            }
        }

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task{TResult}"/> in the
        /// <see cref="System.Threading.Tasks.TaskStatus">RanToCompletion</see> state with the specified result.
        /// </summary>
        /// <param name="result">The result to use to complete the task.</param>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        public void SetResult(TResult result)
        {
            // Get the currently stored task, which will be non-null if get_Task has already been accessed.
            // If there isn't one, get a task and store it.
            var task = m_task;
            if (task == null)
            {
                m_task = GetTaskForResult(result);
#if ENABLE_CONTRACTS
                Contract.Assert(m_task != null, "GetTaskForResult should never return null");
#endif // ENABLE_CONTRACTS
            }
            // Slow path: complete the existing task.
            else if (!task.TrySetResult(result))
            {
                throw new InvalidOperationException(s_transitionToFinalAlreadyCompleted);
            }
        }

        /// <summary>
        /// Completes the builder by using either the supplied completed task, or by completing
        /// the builder's previously accessed task using default(TResult).
        /// </summary>
        /// <param name="completedTask">A task already completed with the value default(TResult).</param>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        internal void SetResult(Task<TResult> completedTask)
        {
#if ENABLE_CONTRACTS
            Contract.Requires(completedTask != null, "Expected non-null task");
            Contract.Requires(completedTask.Status == TaskStatus.RanToCompletion, "Expected a successfully completed task");
#endif // ENABLE_CONTRACTS

            // Get the currently stored task, which will be non-null if get_Task has already been accessed.
            // If there isn't one, store the supplied completed task.
            var task = m_task;
            if (task == null)
            {
                m_task = completedTask;
            }
            else
            {
                // Otherwise, complete the task that's there.
                SetResult(default(TResult));
            }
        }

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task{TResult}"/> in the
        /// <see cref="System.Threading.Tasks.TaskStatus">Faulted</see> state with the specified exception.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> to use to fault the task.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="exception"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

#if ENABLE_CONTRACTS
            Contract.EndContractBlock();
#endif // ENABLE_CONTRACTS

#if DISABLED_FOR_LLILUM
            var task = m_task;
            if (task == null)
            {
                // Get the task, forcing initialization if it hasn't already been initialized.
                task = this.Task;
            }

            // If the exception represents cancellation, cancel the task.  Otherwise, fault the task.
            var oce = exception as OperationCanceledException;
            bool successfullySet = oce != null ?
                task.TrySetCanceled(oce.CancellationToken, oce) :
                task.TrySetException(exception);

            // Unlike with TaskCompletionSource, we do not need to spin here until m_task is completed,
            // since AsyncTaskMethodBuilder.SetException should not be immediately followed by any code
            // that depends on the task having completely completed.  Moreover, with correct usage,
            // SetResult or SetException should only be called once, so the Try* methods should always
            // return true, so no spinning would be necessary anyway (the spinning in TCS is only relevant
            // if another thread completes the task first).

            if (!successfullySet)
            {
                throw new InvalidOperationException(s_transitionToFinalAlreadyCompleted);
            }
#else // DISABLED_FOR_LLILUM
            throw new NotImplementedException();
#endif // DISABLED_FOR_LLILUM
        }

        /// <summary>
        /// Gets a task for the specified result.  This will either
        /// be a cached or new task, never null.
        /// </summary>
        /// <param name="result">The result for which we need a task.</param>
        /// <returns>The completed task containing the result.</returns>
        [SecuritySafeCritical] // for JitHelpers.UnsafeCast
        private Task<TResult> GetTaskForResult(TResult result)
        {
#if ENABLE_CONTRACTS
            Contract.Ensures(
                EqualityComparer<TResult>.Default.Equals(result, Contract.Result<Task<TResult>>().Result),
                "The returned task's Result must return the same value as the specified result value.");
#endif // ENABLE_CONTRACTS

#if ENABLE_CACHED_TASKS
            // The goal of this function is to give back a cached task if possible, or to otherwise give back a new task.
            // To give back a cached task, we need to be able to evaluate the incoming result value, and we need to
            // avoid as much overhead as possible when doing so, as this function is invoked as part of the return path
            // from every async method. Most tasks won't be cached, and thus we need the checks for those that are to be
            // as close to free as possible. This requires some trickiness given the lack of generic specialization.

            // Note: For ahead-of-time compilation, this is far less sensitive than the CoreCLR version. We have
            // therefore simplified it to remove special JIT-related optimizations.

            // Avoid value-type branches for ref types.
            if (null != default(TResult))
            {
                Task<TResult> defaultTask;
                AsyncTaskCache.GetCachedTask(result, out defaultTask);
                if (defaultTask != null)
                {
                    return defaultTask;
                }
            }
            else if (result == null) // optimized away for value types
            {
                return s_defaultResultTask;
            }
#endif // ENABLE_CACHED_TASKS

            // No cached task is available.  Manufacture a new one for this result.
            return new Task<TResult>(result);
        }
    }

#if ENABLE_CACHED_TASKS
    /// <summary>Provides a cache of closed generic tasks for async methods.</summary>
    internal static class AsyncTaskCache
    {
        // All static members are initialized inline to ensure type is beforefieldinit

        /// <summary>A cached Task{Boolean}.Result == true.</summary>
        internal readonly static Task<bool> TrueTask = CreateCacheableTask(true);
        /// <summary>A cached Task{Boolean}.Result == false.</summary>
        internal readonly static Task<bool> FalseTask = CreateCacheableTask(false);

        // Tasks for defaults of various known value types.
        internal readonly static Task<Byte> ByteTask = CreateCacheableTask(default(Byte));
        internal readonly static Task<SByte> SByteTask = CreateCacheableTask(default(SByte));
        internal readonly static Task<Char> CharTask = CreateCacheableTask(default(Char));
        internal readonly static Task<Int16> Int16Task = CreateCacheableTask(default(Int16));
        internal readonly static Task<UInt16> UInt16Task = CreateCacheableTask(default(UInt16));
        internal readonly static Task<Int32> Int32Task = CreateCacheableTask(default(Int32));
        internal readonly static Task<UInt32> UInt32Task = CreateCacheableTask(default(UInt32));
        internal readonly static Task<Int64> Int64Task = CreateCacheableTask(default(Int64));
        internal readonly static Task<UInt64> UInt64Task = CreateCacheableTask(default(UInt64));
        internal readonly static Task<IntPtr> IntPtrTask = CreateCacheableTask(default(IntPtr));
        internal readonly static Task<UIntPtr> UIntPtrTask = CreateCacheableTask(default(UIntPtr));

        // Generic default task accessors for the known value types.
        internal static void GetCachedTask(bool value,      out Task<bool> task)    { task = value ? TrueTask : FalseTask; }
        internal static void GetCachedTask(Byte value,      out Task<Byte> task)    { task = (value == default(Byte)) ? AsyncTaskMethodBuilder<Byte>.s_defaultResultTask : null; }
        internal static void GetCachedTask(SByte value,     out Task<SByte> task)   { task = (value == default(SByte)) ? AsyncTaskMethodBuilder<SByte>.s_defaultResultTask : null; }
        internal static void GetCachedTask(Char value,      out Task<Char> task)    { task = (value == default(Char)) ? AsyncTaskMethodBuilder<Char>.s_defaultResultTask : null; }
        internal static void GetCachedTask(Int16 value,     out Task<Int16> task)   { task = (value == default(Int16)) ? AsyncTaskMethodBuilder<Int16>.s_defaultResultTask : null; }
        internal static void GetCachedTask(UInt16 value,    out Task<UInt16> task)  { task = (value == default(UInt16)) ? AsyncTaskMethodBuilder<UInt16>.s_defaultResultTask : null; }
        internal static void GetCachedTask(Int32 value,     out Task<Int32> task)   { task = (value == default(Int32)) ? AsyncTaskMethodBuilder<Int32>.s_defaultResultTask : null; }
        internal static void GetCachedTask(UInt32 value,    out Task<UInt32> task)  { task = (value == default(UInt32)) ? AsyncTaskMethodBuilder<UInt32>.s_defaultResultTask : null; }
        internal static void GetCachedTask(Int64 value,     out Task<Int64> task)   { task = (value == default(Int64)) ? AsyncTaskMethodBuilder<Int64>.s_defaultResultTask : null; }
        internal static void GetCachedTask(UInt64 value,    out Task<UInt64> task)  { task = (value == default(UInt64)) ? AsyncTaskMethodBuilder<UInt64>.s_defaultResultTask : null; }
        internal static void GetCachedTask(IntPtr value,    out Task<IntPtr> task)  { task = (value == default(IntPtr)) ? AsyncTaskMethodBuilder<IntPtr>.s_defaultResultTask : null; }
        internal static void GetCachedTask(UIntPtr value,   out Task<UIntPtr> task) { task = (value == default(UIntPtr)) ? AsyncTaskMethodBuilder<UIntPtr>.s_defaultResultTask : null; }
        internal static void GetCachedTask<TResult>(TResult value, out Task<TResult> task) { task = null; }

        /// <summary>Creates a non-disposable task.</summary>
        /// <typeparam name="TResult">Specifies the result type.</typeparam>
        /// <param name="result">The result for the task.</param>
        /// <returns>The cacheable task.</returns>
        internal static Task<TResult> CreateCacheableTask<TResult>(TResult result)
        {
            return new Task<TResult>(result);
        }
    }
#endif // ENABLE_CACHED_TASKS

    /// <summary>Holds state related to the builder's IAsyncStateMachine.</summary>
    /// <remarks>This is a mutable struct.  Be very delicate with it.</remarks>
    internal struct AsyncMethodBuilderCore
    {
        /// <summary>A reference to the heap-allocated state machine object associated with this builder.</summary>
        internal IAsyncStateMachine m_stateMachine;
        /// <summary>A cached Action delegate used when dealing with a default ExecutionContext.</summary>
        internal Action m_defaultContextAction;

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument is null (Nothing in Visual Basic).</exception>
        [SecuritySafeCritical]
        [DebuggerStepThrough]
        internal static void Start(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

#if ENABLE_CONTRACTS
            Contract.EndContractBlock();
#endif // ENABLE_CONTRACTS

            RuntimeHelpers.PrepareConstrainedRegions();
            stateMachine.MoveNext();
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

#if ENABLE_CONTRACTS
            Contract.EndContractBlock();
#endif // ENABLE_CONTRACTS

            if (m_stateMachine != null)
            {
                throw new InvalidOperationException();
            }

            m_stateMachine = stateMachine;
        }

        /// <summary>
        /// Gets the Action to use with an awaiter's OnCompleted or UnsafeOnCompleted method.
        /// On first invocation, the supplied state machine will be boxed.
        /// </summary>
        /// <typeparam name="TMethodBuilder">Specifies the type of the method builder used.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine used.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="stateMachine">The state machine.</param>
        /// <returns>An Action to provide to the awaiter.</returns>
        [SecuritySafeCritical]
        internal Action GetCompletionAction(IAsyncStateMachine stateMachine)
        {
#if ENABLE_CONTRACTS
            Contract.Assert(m_defaultContextAction == null || m_stateMachine != null,
                "Expected non-null m_stateMachine on non-null m_defaultContextAction");
#endif // ENABLE_CONTRACTS

            // If this is our first await, such that we've not yet boxed the state machine, do so now.
            if (m_stateMachine == null)
            {
                // Box the state machine, then tell the boxed instance to call back into its own builder,
                // so we can cache the boxed reference.  NOTE: The language compiler may choose to use
                // a class instead of a struct for the state machine for debugging purposes; in such cases,
                // the stateMachine will already be an object.
                m_stateMachine = stateMachine;
                m_stateMachine.SetStateMachine(m_stateMachine);
            }

            // BUGBUG: Initializing delegates with interface methods doesn't work yet. Restore this line to the
            // following once they're fixed:
            return new Action(m_stateMachine.MoveNext);
        }

        /// <summary>Throws the exception on the ThreadPool.</summary>
        /// <param name="exception">The exception to propagate.</param>
        /// <param name="targetContext">The target context on which to propagate the exception.  Null to use the ThreadPool.</param>
        internal static void ThrowAsync(Exception exception)
        {
            // Propagate the exception(s) on the ThreadPool.
            ThreadPool.QueueUserWorkItem(e => { throw (Exception)e; }, exception);
        }
    }
}
