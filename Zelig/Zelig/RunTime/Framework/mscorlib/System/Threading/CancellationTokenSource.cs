// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#pragma warning disable 0420

//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics.Contracts;

namespace System.Threading
{
    /// <summary>
    /// Signals to a <see cref="System.Threading.CancellationToken"/> that it should be canceled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="T:System.Threading.CancellationTokenSource"/> is used to instantiate a <see
    /// cref="T:System.Threading.CancellationToken"/>
    /// (via the source's <see cref="System.Threading.CancellationTokenSource.Token">Token</see> property)
    /// that can be handed to operations that wish to be notified of cancellation or that can be used to
    /// register asynchronous operations for cancellation. That token may have cancellation requested by
    /// calling to the source's <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see>
    /// method.
    /// </para>
    /// <para>
    /// All members of this class, except <see cref="Dispose">Dispose</see>, are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </para>
    /// </remarks>
    [ComVisible(false)]
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public class CancellationTokenSource : IDisposable
    {
        // Legal values for m_state
        private const int NOT_CANCELED = 0;
        private const int NOTIFYING = 1;
        private const int NOTIFYING_COMPLETE = 2;
        private const int CANNOT_BE_CANCELED = 3;

        // Static sources that can be used as the backing source for 'fixed' CancellationTokens that never change state.
        private static readonly CancellationTokenSource s_preCanceled = new CancellationTokenSource(true);
        private static readonly CancellationTokenSource s_notCancelable = new CancellationTokenSource(false);

        private static readonly TimerCallback s_timerCallback = new TimerCallback(TimerCallbackLogic);
        private static readonly object s_callbackSentinel = new object();

        private volatile int m_state;
        private object m_callbacks; // This will be either s_callbackSentinel or a List<CancellationCallbackInfo>.

        private bool m_disposed;

        private volatile Timer m_timer;
        private volatile ManualResetEvent m_kernelEvent;
        private volatile Thread m_currentCallbackThread;
        private volatile CancellationCallbackInfo m_executingCallback;

#if DISABLED_FOR_LLILUM
        private CancellationTokenRegistration[] m_linkingRegistrations; //lazily initialized if required.
        private static readonly Action<object> s_LinkedTokenCancelDelegate = new Action<object>(LinkedTokenCancelDelegate);
#endif // DISABLED_FOR_LLILUM

#if DISABLED_FOR_LLILUM
        private static void LinkedTokenCancelDelegate(object source)
        {
            CancellationTokenSource cts = source as CancellationTokenSource;
            Contract.Assert(source != null);
            cts.Cancel();
        }
#endif // DISABLED_FOR_LLILUM

        /// <summary>
        /// Initializes the <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        public CancellationTokenSource()
        {
        }

        /// <summary>
        /// Constructs a <see cref="T:System.Threading.CancellationTokenSource"/> that will be canceled after a specified time span.
        /// </summary>
        /// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource"/></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception that is thrown when <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the delay starts during the call to the constructor.  When the delay expires, 
        /// the constructed <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the delay for the constructed 
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public CancellationTokenSource(TimeSpan delay)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if ((totalMilliseconds < -1) || (totalMilliseconds > int.MaxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            m_timer = new Timer(s_timerCallback, this, (int)totalMilliseconds, -1);
        }

        /// <summary>
        /// Constructs a <see cref="T:System.Threading.CancellationTokenSource"/> that will be canceled after a specified time span.
        /// </summary>
        /// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource"/></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception that is thrown when <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the millisecondsDelay starts during the call to the constructor.  When the millisecondsDelay expires, 
        /// the constructed <see cref="T:System.Threading.CancellationTokenSource"/> is canceled (if it has
        /// not been canceled already).
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the millisecondsDelay for the constructed 
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public CancellationTokenSource(int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            }

            m_timer = new Timer(s_timerCallback, this, millisecondsDelay, -1);
        }

        /// <summary>
        /// Initializes static source with a preset status.
        /// </summary>
        /// <param name="canceled">Whether the source should be pre-canceled.</param>
        private CancellationTokenSource(bool canceled)
        {
            m_state = canceled ? NOTIFYING_COMPLETE : CANNOT_BE_CANCELED;
        }

        /// <summary>
        /// Gets whether cancellation has been requested for this <see
        /// cref="System.Threading.CancellationTokenSource">CancellationTokenSource</see>.
        /// </summary>
        /// <value>Whether cancellation has been requested for this <see
        /// cref="System.Threading.CancellationTokenSource">CancellationTokenSource</see>.</value>
        /// <remarks>
        /// <para>
        /// This property indicates whether cancellation has been requested for this token source, such as
        /// due to a call to its
        /// <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see> method.
        /// </para>
        /// <para>
        /// If this property returns true, it only guarantees that cancellation has been requested. It does not
        /// guarantee that every handler registered with the corresponding token has finished executing, nor
        /// that cancellation requests have finished propagating to all registered handlers. Additional
        /// synchronization may be required, particularly in situations where related objects are being
        /// canceled concurrently.
        /// </para>
        /// </remarks>
        public bool IsCancellationRequested
        {
            get
            {
                int state = m_state;
                return (state == NOTIFYING) || (state == NOTIFYING_COMPLETE);
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// associated with this <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <value>The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// associated with this <see cref="CancellationTokenSource"/>.</value>
        /// <exception cref="T:System.ObjectDisposedException">The token source has been
        /// disposed.</exception>
        public CancellationToken Token
        {
            get
            {
                ThrowIfDisposed();
                return new CancellationToken(this);
            }
        }

        internal bool CanBeCanceled
        {
            get
            {
                return m_state != CANNOT_BE_CANCELED;
            }
        }

        internal WaitHandle WaitHandle
        {
            get
            {
                ThrowIfDisposed();

                // fast path if already allocated.
                if (m_kernelEvent != null)
                {
                    return m_kernelEvent;
                }

                // lazy-init the mre.
                ManualResetEvent mre = new ManualResetEvent(false);
                if (Interlocked.CompareExchange(ref m_kernelEvent, mre, null) != null)
                {
                    ((IDisposable)mre).Dispose();
                }

                // There is a race condition between checking IsCancellationRequested and setting the event.
                // However, at this point, the kernel object definitely exists and the cases are:
                //   1. if IsCancellationRequested = true, then we will call Set()
                //   2. if IsCancellationRequested = false, then Cancel will see that the event exists, and will call Set().
                if (IsCancellationRequested)
                {
                    m_kernelEvent.Set();
                }

                return m_kernelEvent;
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The associated <see cref="T:System.Threading.CancellationToken" /> will be
        /// notified of the cancellation and will transition to a state where 
        /// <see cref="System.Threading.CancellationToken.IsCancellationRequested">IsCancellationRequested</see> returns true. 
        /// Any callbacks or cancelable operations
        /// registered with the <see cref="T:System.Threading.CancellationToken"/>  will be executed.
        /// </para>
        /// <para>
        /// Cancelable operations and callbacks registered with the token should not throw exceptions.
        /// However, this overload of Cancel will aggregate any exceptions thrown into a <see cref="System.AggregateException"/>,
        /// such that one callback throwing an exception will not prevent other registered callbacks from being executed.
        /// </para>
        /// <para>
        /// The <see cref="T:System.Threading.ExecutionContext"/> that was captured when each callback was registered
        /// will be reestablished when the callback is invoked.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown
        /// by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken"/>.</exception>
        /// <exception cref="T:System.ObjectDisposedException">This <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.</exception> 
        public void Cancel()
        {
            Cancel(false);
        }

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The associated <see cref="T:System.Threading.CancellationToken" /> will be
        /// notified of the cancellation and will transition to a state where 
        /// <see cref="System.Threading.CancellationToken.IsCancellationRequested">IsCancellationRequested</see> returns true. 
        /// Any callbacks or cancelable operations
        /// registered with the <see cref="T:System.Threading.CancellationToken"/>  will be executed.
        /// </para>
        /// <para>
        /// Cancelable operations and callbacks registered with the token should not throw exceptions. 
        /// If <paramref name="throwOnFirstException"/> is true, an exception will immediately propagate out of the
        /// call to Cancel, preventing the remaining callbacks and cancelable operations from being processed.
        /// If <paramref name="throwOnFirstException"/> is false, this overload will aggregate any 
        /// exceptions thrown into a <see cref="System.AggregateException"/>,
        /// such that one callback throwing an exception will not prevent other registered callbacks from being executed.
        /// </para>
        /// <para>
        /// The <see cref="T:System.Threading.ExecutionContext"/> that was captured when each callback was registered
        /// will be reestablished when the callback is invoked.
        /// </para>
        /// </remarks>
        /// <param name="throwOnFirstException">Specifies whether exceptions should immediately propagate.</param>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown
        /// by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken"/>.</exception>
        /// <exception cref="T:System.ObjectDisposedException">This <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.</exception> 
        public void Cancel(bool throwOnFirstException)
        {
            ThrowIfDisposed();

            // Do nothing if we've already been canceled.
            if (IsCancellationRequested)
            {
                return;
            }

            // If we're the first to signal cancellation, do the main extra work.
            if (NOT_CANCELED == Interlocked.CompareExchange(ref m_state, NOTIFYING, NOT_CANCELED))
            {
                // Dispose of the timer, if any.
                m_timer?.Dispose();

                // If the kernel event is null at this point, it will be set during lazy construction.
                m_kernelEvent?.Set();

                m_currentCallbackThread = Thread.CurrentThread;

                // - late enlisters to the Canceled event will have their callbacks called immediately in the Register() methods.
                // - Callbacks are not called inside a lock.
                // - After transition, no more delegates will be added to the 
                // - list of handlers, and hence it can be consumed and cleared at leisure by ExecuteCallbackHandlers.
                ExecuteCallbackHandlers(throwOnFirstException);
#if ENABLE_CONTRACTS
                Contract.Assert(IsCancellationCompleted, "Expected cancellation to have finished.");
#endif // ENABLE_CONTRACTS
            }
        }

        /// <summary>
        /// Schedules a Cancel operation on this <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        /// <param name="delay">The time span to wait before canceling this <see
        /// cref="T:System.Threading.CancellationTokenSource"/>.
        /// </param>
        /// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception thrown when <paramref name="delay"/> is less than -1 or 
        /// greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the delay starts during this call.  When the delay expires, 
        /// this <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the delay for this  
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public void CancelAfter(TimeSpan delay)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if ((totalMilliseconds < -1) || (totalMilliseconds > int.MaxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            CancelAfter((int)totalMilliseconds);
        }

        /// <summary>
        /// Schedules a Cancel operation on this <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        /// <param name="millisecondsDelay">The time span to wait before canceling this <see
        /// cref="T:System.Threading.CancellationTokenSource"/>.
        /// </param>
        /// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception thrown when <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the millisecondsDelay starts during this call.  When the millisecondsDelay expires, 
        /// this <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the millisecondsDelay for this  
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public void CancelAfter(int millisecondsDelay)
        {
            ThrowIfDisposed();

            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            }

            if (IsCancellationRequested)
            {
                return;
            }

            throw new NotImplementedException();
#if DISABLED_FOR_LLILUM

            // There is a race condition here as a Cancel could occur between the check of
            // IsCancellationRequested and the creation of the timer.  This is benign; in the 
            // worst case, a timer will be created that has no effect when it expires.

            // Also, if Dispose() is called right here (after ThrowIfDisposed(), before timer
            // creation), it would result in a leaked Timer object (at least until the timer
            // expired and Disposed itself).  But this would be considered bad behavior, as
            // Dispose() is not thread-safe and should not be called concurrently with CancelAfter().

            if (m_timer == null)
            {
                // Lazily initialize the timer in a thread-safe fashion.
                // Initially set to "never go off" because we don't want to take a
                // chance on a timer "losing" the initialization and then
                // cancelling the token before it (the timer) can be disposed.
                Timer newTimer = new Timer(s_timerCallback, this, -1, -1);
                if (Interlocked.CompareExchange(ref m_timer, newTimer, null) != null)
                {
                    // We did not initialize the timer.  Dispose the new timer.
                    newTimer.Dispose();
                }
            }


            // It is possible that m_timer has already been disposed, so we must do
            // the following in a try/catch block.
            try
            {
                m_timer.Change(millisecondsDelay, -1);
            }
            catch (ObjectDisposedException)
            {
                // Just eat the exception.  There is no other way to tell that
                // the timer has been disposed, and even if there were, there
                // would not be a good way to deal with the observe/dispose
                // race condition.
            }
#endif // DISABLED_FOR_LLILUM
        }

        /// <summary>
        /// Creates a <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> that will be in the canceled state
        /// when any of the source tokens are in the canceled state.
        /// </summary>
        /// <param name="token1">The first <see cref="T:System.Threading.CancellationToken">CancellationToken</see> to observe.</param>
        /// <param name="token2">The second <see cref="T:System.Threading.CancellationToken">CancellationToken</see> to observe.</param>
        /// <returns>A <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> that is linked 
        /// to the source tokens.</returns>
        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            throw new NotImplementedException();
#if DISABLED_FOR_LLILUM
            CancellationTokenSource linkedTokenSource = new CancellationTokenSource();

            bool token2CanBeCanceled = token2.CanBeCanceled;

            if (token1.CanBeCanceled)
            {
                linkedTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[token2CanBeCanceled ? 2 : 1]; // there will be at least 1 and at most 2 linkings
                linkedTokenSource.m_linkingRegistrations[0] = token1.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, linkedTokenSource);
            }

            if (token2CanBeCanceled)
            {
                int index = 1;
                if (linkedTokenSource.m_linkingRegistrations == null)
                {
                    linkedTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[1]; // this will be the only linking
                    index = 0;
                }
                linkedTokenSource.m_linkingRegistrations[index] = token2.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, linkedTokenSource);
            }

            return linkedTokenSource;
#endif // DISABLED_FOR_LLILUM
        }

        /// <summary>
        /// Creates a <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> that will be in the canceled state
        /// when any of the source tokens are in the canceled state.
        /// </summary>
        /// <param name="tokens">The <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instances to observe.</param>
        /// <returns>A <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> that is linked 
        /// to the source tokens.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="tokens"/> is null.</exception>
        public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            throw new NotImplementedException();
#if DISABLED_FOR_LLILUM
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (tokens.Length == 0)
            {
                throw new ArgumentException("No tokens were supplied.");
            }

            // a defensive copy is not required as the array has value-items that have only a single IntPtr field,
            // hence each item cannot be null itself, and reads of the payloads cannot be torn.
            Contract.EndContractBlock();

            CancellationTokenSource linkedTokenSource = new CancellationTokenSource();
            linkedTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].CanBeCanceled)
                {
                    linkedTokenSource.m_linkingRegistrations[i] = tokens[i].InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, linkedTokenSource);
                }

                // Empty slots in the array will be default(CancellationTokenRegistration), which are nops to Dispose.
                // Based on usage patterns, such occurrences should also be rare, such that it's not worth resizing
                // the array and incurring the related costs.
            }

            return linkedTokenSource;
#endif // DISABLED_FOR_LLILUM
        }

        /// <summary>
        /// Releases the resources used by this <see cref="T:System.Threading.CancellationTokenSource" />.
        /// </summary>
        /// <remarks>
        /// This method is not thread-safe for any other concurrent calls.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Threading.CancellationTokenSource" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // NOTE: This is intentionally exposed as "protected" rather than "internal" to match CoreCLR's interface.

            if (disposing)
            {
                // We specifically tolerate that a callback can be deregistered after the CTS has been disposed and/or
                // concurrently with cts.Dispose(). This is safe without locks because the reg.Dispose() only mutates a
                // sparseArrayFragment and then reads from properties of the CTS that are not invalidated by cts.Dispose().
                //
                // We also tolerate that a callback can be registered after the CTS has been disposed. This is safe
                // without locks because InternalRegister is tolerant of m_registeredCallbacksLists becoming null during
                // its execution. However, we run the acceptable risk of m_registeredCallbacksLists getting reinitialized
                // to non-null if there is a race between Dispose and Register, in which case this instance may
                // unnecessarily hold onto a registered callback. But that's no worse than if Dispose wasn't safe to use
                // concurrently, as Dispose would never be called, and thus no handlers would be dropped.

                if (m_disposed)
                {
                    return;
                }

                m_timer?.Dispose();

#if DISABLED_FOR_LLILUM
                var linkingRegistrations = m_linkingRegistrations;
                if (linkingRegistrations != null)
                {
                    m_linkingRegistrations = null; // free for GC once we're done enumerating
                    for (int i = 0; i < linkingRegistrations.Length; i++)
                    {
                        linkingRegistrations[i].Dispose();
                    }
                }

                // Registered callbacks are now either complete or will never run, due to guarantees made by ctr.Dispose()
                // so we can now perform main disposal work without risk of linking callbacks trying to use this CTS.

                m_registeredCallbacksLists = null; // free for GC.
#endif // DISABLED_FOR_LLILUM

                if (m_kernelEvent != null)
                {
                    m_kernelEvent.Close(); // the critical cleanup to release an OS handle
                    m_kernelEvent = null; // free for GC.
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Throws an exception if the source has been disposed.
        /// </summary>
        internal void ThrowIfDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(null, "The CancellationTokenSource has been disposed.");
            }
        }

        internal CancellationTokenRegistration RegisterCallback(Action<object> callback, object stateForCallback)
        {
            CancellationCallbackInfo callbackInfo = TryRegisterCallback(callback, stateForCallback);
            if (callbackInfo == null)
            {
                callback(stateForCallback);
                return new CancellationTokenRegistration();
            }

            return new CancellationTokenRegistration(callbackInfo);
        }

        /// <summary>
        /// Registers a callback object and returns the wrapped callback info.
        /// </summary>
        /// <returns>Callback information if successful; otherwise null.</returns>
        internal CancellationCallbackInfo TryRegisterCallback(Action<object> callback, object stateForCallback)
        {
            ThrowIfDisposed();

#if ENABLE_CONTRACTS
            // The CancellationToken has already checked that the token is cancelable before calling this method.
            Contract.Assert(CanBeCanceled, "Cannot register for uncancelable token src");
#endif // ENABLE_CONTRACTS

            // If this source has already been canceled, we'll run the callback synchronously and skip the registration.
            if (IsCancellationRequested)
            {
                return null;
            }

            // Ensure that the callbacks list exists. In the event that we hit a very narrow race, we'll temporarily
            // allocate an extra list and drop it. This is acceptable overhead to keep the implementation light.
            if (m_callbacks == null)
            {
                var newCallbacks = new List<CancellationCallbackInfo>(1);
                Interlocked.CompareExchange(ref m_callbacks, newCallbacks, null);
            }

            // Resample m_callbacks in case there was a race above. This is guaranteed to be either a list or the
            // s_callbackSentinel token.
            var list = m_callbacks as List<CancellationCallbackInfo>;

            // If the list is null, we must already be executing callbacks, so execute this one synchronously.
            if (list == null)
            {
                return null;
            }

            lock (list)
            {
                // It's possible for invocation to begin right after we snap the list, so check the sentinel again.
                if (m_callbacks == s_callbackSentinel)
                {
                    return null;
                }

                CancellationCallbackInfo callbackInfo = new CancellationCallbackInfo(callback, stateForCallback, this);
                list.Add(callbackInfo);
                return callbackInfo;
            }
        }

        internal bool TryDeregisterCallback(CancellationCallbackInfo callbackInfo)
        {
            var callbacks = m_callbacks as List<CancellationCallbackInfo>;
            if (callbacks == null)
            {
                return false;
            }

            lock (callbacks)
            {
                // It's possible for invocation to begin right after we snap the list, so check the sentinel again.
                if (m_callbacks == s_callbackSentinel)
                {
                    return false;
                }

                return callbacks.Remove(callbackInfo);
            }
        }

        /// <summary>
        /// Invoke the Canceled event.
        /// </summary>
        /// <remarks>
        /// The handlers are invoked synchronously in LIFO order.
        /// </remarks>
        private void ExecuteCallbackHandlers(bool throwOnFirstException)
        {
#if ENABLE_CONTRACTS
            Contract.Assert(IsCancellationRequested, "ExecuteCallbackHandlers should only be called after setting IsCancellationRequested->true");
#endif // ENABLE_CONTRACTS

            // Design decision: call the delegates in LIFO order so that callbacks fire 'deepest first'. This is
            // intended to help with nesting scenarios so that child enlisters cancel before their parents.

            object callbackObject = Interlocked.Exchange(ref m_callbacks, s_callbackSentinel);

            // Trivial case: If we have no callbacks to call, we're done.
            if (callbackObject == null)
            {
                m_state = NOTIFYING_COMPLETE;
                return;
            }

            List<Exception> exceptionList = null;

            try
            {
#if ENABLE_CONTRACTS
                Contract.Assert(callbackObject != s_callbackSentinel, "ExecuteCallbackHandlers should only be called once.");
#endif // ENABLE_CONTRACTS

                // We don't want to invoke these callbacks under a lock, so create a copy.
                CancellationCallbackInfo[] callbacks;
                lock (callbackObject)
                {
                    callbacks = ((List<CancellationCallbackInfo>)callbackObject).ToArray();
                }

                for (int i = callbacks.Length - 1; i >= 0; --i)
                {
                    try
                    {
                        m_executingCallback = callbacks[i];
                        m_executingCallback.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (throwOnFirstException)
                        {
                            throw;
                        }

                        if (exceptionList == null)
                        {
                            exceptionList = new List<Exception>();
                        }

                        exceptionList.Add(ex);
                    }
                }
            }
            finally
            {
                m_state = NOTIFYING_COMPLETE;
                m_executingCallback = null;
                m_currentCallbackThread = null;
            }

            if (exceptionList != null)
            {
#if ENABLE_CONTRACTS
                Contract.Assert(exceptionList.Count > 0, "Expected exception count > 0");
#endif // ENABLE_CONTRACTS
                throw new AggregateException(exceptionList);
            }
        }

        /// <summary>
        /// InternalGetStaticSource()
        /// </summary>
        /// <param name="canceled">Whether the source should be set.</param>
        /// <returns>A static source to be shared among multiple tokens.</returns>
        internal static CancellationTokenSource InternalGetStaticSource(bool canceled)
        {
            return canceled ? s_preCanceled : s_notCancelable;
        }

        /// <summary>
        /// Wait for a singlre callback to complete. It's OK to call this method after the callback has already finished,
        /// but calling this before the target callback has started is not allowed.
        /// </summary>
        /// <param name="callbackInfo">The callback to wait for.</param>
        internal void WaitForCallbackToComplete(CancellationCallbackInfo callbackInfo)
        {
            // Design decision: In order to avoid deadlocks, we won't block if the callback is being executed on the
            // current thread. It's generally poor form to do this, but it does happen in consumer code.
            if ((m_state == NOTIFYING) && (m_currentCallbackThread != Thread.CurrentThread))
            {
                // It's OK to spin here since callbacks are expected to be relatively fast and calling this wait method
                // should be relatively rare.
                SpinWait spinWait = new SpinWait();
                while (m_executingCallback == callbackInfo)
                {
                    spinWait.SpinOnce();
                }
            }
        }

        private static void TimerCallbackLogic(object obj)
        {
            CancellationTokenSource cts = (CancellationTokenSource)obj;

            // Cancel the source; handle a race condition with cts.Dispose()
            if (!cts.m_disposed)
            {
                // There is a small window for a race condition where a cts.Dispose can sneak
                // in right here.  I'll wrap the cts.Cancel() in a try/catch to proof us
                // against this race condition.
                try
                {
                    cts.Cancel(); // will take care of disposing of m_timer
                }
                catch (ObjectDisposedException)
                {
                    // If the ODE was not due to the target cts being disposed, then propagate the ODE.
                    if (!cts.m_disposed)
                    {
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A helper class for collating the various bits of information required to execute 
    /// cancellation callbacks.
    /// </summary>
    internal class CancellationCallbackInfo
    {
        internal readonly Action<object> Callback;
        internal readonly object StateForCallback;
        internal readonly CancellationTokenSource CancellationTokenSource;

        internal CancellationCallbackInfo(
            Action<object> callback,
            object stateForCallback,
            CancellationTokenSource cancellationTokenSource)
        {
            Callback = callback;
            StateForCallback = stateForCallback;
            CancellationTokenSource = cancellationTokenSource;
        }

        [SecuritySafeCritical]
        internal void Invoke()
        {
            Callback(StateForCallback);
        }
    }
}
