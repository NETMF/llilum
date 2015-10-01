//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;

using ST = System.Threading;

namespace Windows.System.Threading
{
    public sealed class ThreadPoolTimer
    {
        private readonly TimerElapsedHandler m_elapsedHandler;
        private readonly TimerDestroyedHandler m_destroyedHandler;
        private readonly TimeSpan m_time;
        private readonly bool m_periodic;
        private ST.Timer m_systemTimer;

        private ThreadPoolTimer(
            TimerElapsedHandler elapsedHandler,
            TimerDestroyedHandler destroyedHandler,
            TimeSpan time,
            bool periodic)
        {
            m_elapsedHandler = elapsedHandler;
            m_destroyedHandler = destroyedHandler;
            m_periodic = periodic;
            m_time = time;

            if (periodic)
            {
                m_systemTimer = new ST.Timer(TimerElapsed, null, time, time);
            }
            else
            {
                m_systemTimer = new ST.Timer(TimerElapsed, null, time.Milliseconds, ST.Timeout.Infinite);
            }
        }

        /// <summary>
        /// Gets the timeout value of a single-use timer created with CreateTimer.
        /// </summary>
        /// <value>The timeout value. When the timeout value elapses, the timer expires and its TimerElapsedHandler
        ///     delegate is called.</value>
        /// <remarks>
        /// A timer begins counting down as soon as the timer object is created.
        /// </remarks>
        public TimeSpan Delay
        {
            get
            {
                return m_periodic ? default(TimeSpan) : m_time;
            }
        }

        /// <summary>
        /// Gets the timeout value of a periodic timer created with CreatePeriodicTimer.
        /// </summary>
        /// <value>The timeout value. When the timeout value elapses, the timer expires, its
        ///     TimerElapsedHandler delegate is called, and the timer reactivates. This behavior continues until the
        ///     timer is canceled.</value>
        /// <remarks>
        /// A periodic timer begins counting down as soon as the timer object is created. When the timer expires, it is
        /// reactivated and begins counting down again.
        /// </remarks>
        public TimeSpan Period
        {
            get
            {
                return m_periodic ? m_time : default(TimeSpan);
            }
        }

        /// <summary>
        /// Creates a periodic timer.
        /// </summary>
        /// <param name="handler">The method to call when the timer expires.</param>
        /// <param name="period">The amount of time until the timer expires. The timer reactivates each time the period
        ///     elapses, until the timer is canceled.
        ///     <para>Note: A TimeSpan value of zero (or any value less than 1 millisecond) will cause the periodic
        ///          timer to behave as a single-shot timer.</para></param>
        /// <returns>An instance of a periodic timer.</returns>
        public static ThreadPoolTimer CreatePeriodicTimer(TimerElapsedHandler handler, TimeSpan period)
        {
            return CreatePeriodicTimer(handler, period, null);
        }

        /// <summary>
        /// Creates a periodic timer and specifies a method to call after the periodic timer is complete. The periodic
        /// timer is complete when the timer has expired without being reactivated, and the final call to handler has
        /// finished.
        /// </summary>
        /// <param name="handler">The method to call when the timer expires.</param>
        /// <param name="period">The amount of time until the timer expires. The timer reactivates each time the period
        ///     elapses, until the timer is canceled.
        ///     <para>Note: A TimeSpan value of zero (or any value less than 1 millisecond) will cause the periodic
        ///          timer to behave as a single-shot timer.</para></param>
        /// <param name="destroyed">The method to call after the periodic timer is complete.</param>
        /// <returns>An instance of a periodic timer.</returns>
        public static ThreadPoolTimer CreatePeriodicTimer(
            TimerElapsedHandler handler,
            TimeSpan period,
            TimerDestroyedHandler destroyed)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return new ThreadPoolTimer(handler, destroyed, period, periodic: true);
        }

        /// <summary>
        /// Creates a single-use timer.
        /// </summary>
        /// <param name="handler">The method to call when the timer expires.</param>
        /// <param name="delay">The amount of time until the timer expires.</param>
        /// <returns>An instance of a single-use timer.</returns>
        public static ThreadPoolTimer CreateTimer(
            TimerElapsedHandler handler,
            TimeSpan delay)
        {
            return CreateTimer(handler, delay, null);
        }

        /// <summary>
        /// Creates a single-use timer and specifies a method to call after the timer is complete. The timer is complete
        /// when the timer has expired and the final call to <paramref name="handler"/> has finished.
        /// </summary>
        /// <param name="handler">The method to call when the timer expires.</param>
        /// <param name="delay">The amount of time until the timer expires.</param>
        /// <param name="destroyed">The method to call after the timer is complete.</param>
        /// <returns>An instance of a single-use timer.</returns>
        public static ThreadPoolTimer CreateTimer(
            TimerElapsedHandler handler,
            TimeSpan delay,
            TimerDestroyedHandler destroyed)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return new ThreadPoolTimer(handler, destroyed, delay, periodic: false);
        }

        /// <summary>
        /// Cancels a timer.
        /// </summary>
        /// <remarks>
        /// When a timer is canceled, pending TimerElapsedHandler delegates are also canceled. TimerElapsedHandler
        /// delegates that are already running are allowed to finish.
        /// </remarks>
        public void Cancel()
        {
            var systemTimer = ST.Interlocked.Exchange(ref m_systemTimer, null);
            if (systemTimer != null)
            {
                m_destroyedHandler?.Invoke(this);
                systemTimer.Dispose();
            }
        }

        private void TimerElapsed(object state)
        {
            // Note: We don't try to enforce causality here, so this handler may be called multiple times concurrently
            // for very fast timers. As the timer is stateless and only supports read-only operations, this should be
            // acceptable for our purposes.
            m_elapsedHandler(this);

            // If this is a one-shot timer, immediately force cancellation so the completed handler gets invoked.
            if (!m_periodic)
            {
                Cancel();
            }
        }
    }
}
