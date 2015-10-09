// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace System.Threading
{
    /// <summary>
    /// Represents a callback delegate that has been registered with a <see cref="T:System.Threading.CancellationToken">CancellationToken</see>.
    /// </summary>
    /// <remarks>
    /// To unregister a callback, dispose the corresponding Registration instance.
    /// </remarks>
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable
    {
        private readonly CancellationCallbackInfo m_callbackInfo;

        internal CancellationTokenRegistration(CancellationCallbackInfo callbackInfo)
        {
            m_callbackInfo = callbackInfo;
        }

        /// <summary>
        /// Attempts to deregister the item. If it's already being run, this may fail. Entails a full memory fence.
        /// </summary>
        /// <returns>True if the callback was found and deregistered, false otherwise.</returns>
        [FriendAccessAllowed]
        internal bool TryDeregister()
        {
            if (m_callbackInfo?.CancellationTokenSource == null)  //can be null for dummy registrations.
            {
                return false;
            }

            return m_callbackInfo.CancellationTokenSource.TryDeregisterCallback(m_callbackInfo);
        }

        /// <summary>
        /// Disposes of the registration and unregisters the target callback from the associated
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>. If the target callback is
        /// currently executing this method will wait until it completes, except in the degenerate cases where a
        /// callback method deregisters itself.
        /// </summary>
        public void Dispose()
        {
            // Remove this callback registration from the invocation list. If that fails, wait for the callback to complete.
            if (!TryDeregister())
            {
                m_callbackInfo?.CancellationTokenSource?.WaitForCallbackToComplete(m_callbackInfo);
            }
        }

        /// <summary>
        /// Determines whether two <see
        /// cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see>
        /// instances are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True if the instances are equal; otherwise, false.</returns>
        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True if the instances are not equal; otherwise, false.</returns>
        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instance is equal to the 
        /// specified <see cref="T:System.Object"/>.
        /// </summary> 
        /// <param name="obj">The other object to which to compare this instance.</param>
        /// <returns>True, if both this and <paramref name="obj"/> are equal. False, otherwise.
        /// Two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are equal if
        /// they both refer to the output of a single call to the same Register method of a 
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>. 
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((obj is CancellationTokenRegistration) && Equals((CancellationTokenRegistration)obj));
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instance is equal to the 
        /// specified <see cref="T:System.Object"/>.
        /// </summary> 
        /// <param name="other">The other <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> to which to compare this instance.</param>
        /// <returns>True, if both this and <paramref name="other"/> are equal. False, otherwise.
        /// Two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are equal if
        /// they both refer to the output of a single call to the same Register method of a 
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>. 
        /// </returns>
        public bool Equals(CancellationTokenRegistration other)
        {
            return m_callbackInfo == other.m_callbackInfo;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration.</see>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instance.</returns>
        public override int GetHashCode()
        {
            if (m_callbackInfo == null)
            {
                return 0;
            }

            return m_callbackInfo.GetHashCode();
        }
    }
}
