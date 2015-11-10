//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.Internal
{
    /// <summary>
    /// Provides extension methods for converting between tasks and Windows Runtime asynchronous actions and operations.
    /// </summary>
    [CLSCompliant(false)]
    public static class WindowsRuntimeSystemExtensions
    {
        /// <summary>
        /// Returns a Windows Runtime asynchronous action that represents a started task.
        /// </summary>
        /// <param name="source">The started task.</param>
        /// <returns>A Windows.Foundation.IAsyncAction instance that represents the started task.</returns>
        public static IAsyncAction AsAsyncAction(this Task source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Status == TaskStatus.Created)
            {
                throw new InvalidOperationException("Task has not been started.");
            }

            return new AsyncActionFromTask(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous action.
        /// </summary>
        /// <param name="source">The asynchronous action.</param>
        /// <returns>A task that represents the asynchronous action.</returns>
        public static Task AsTask(this IAsyncAction source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Short-circuit round-tripped tasks and just return the original.
            var asyncActionFromTask = source as AsyncActionFromTask;
            if (asyncActionFromTask != null)
            {
                return asyncActionFromTask.Task;
            }

            // BUGBUG: We need to implement TaskCompletionSource before we can implement this properly.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a Windows Runtime asynchronous operation that represents a started task.
        /// </summary>
        /// <param name="source">The started task.</param>
        /// <returns>A Windows.Foundation.IAsyncOperation instance that represents the started task.</returns>
        public static IAsyncOperation<T> AsAsyncOperation<T>(this Task<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Status == TaskStatus.Created)
            {
                throw new InvalidOperationException("Task has not been started.");
            }

            return new AsyncOperationFromTask<T>(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous operation.
        /// </summary>
        /// <param name="source">The asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<T> AsTask<T>(this IAsyncOperation<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Short-circuit round-tripped tasks and just return the original.
            var asyncOperationFromTask = source as AsyncOperationFromTask<T>;
            if (asyncOperationFromTask != null)
            {
                return asyncOperationFromTask.Task;
            }

            // BUGBUG: We need to implement TaskCompletionSource before we can implement this properly.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a Windows Runtime asynchronous operation that represents a started task.
        /// </summary>
        /// <param name="source">The started task.</param>
        /// <returns>A Windows.Foundation.IAsyncOperation instance that represents the started task.</returns>
        public static IAsyncOperationWithProgress<T, P> AsAsyncOperationWithProgress<T, P>(this Task<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Status == TaskStatus.Created)
            {
                throw new InvalidOperationException("Task has not been started.");
            }

            return new AsyncOperationWithProgressFromTask<T, P>(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous operation.
        /// </summary>
        /// <param name="source">The asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<T> AsTask<T, P>(this IAsyncOperationWithProgress<T, P> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Short-circuit round-tripped tasks and just return the original.
            var asyncOperationFromTask = source as AsyncOperationWithProgressFromTask<T, P>;
            if (asyncOperationFromTask != null)
            {
                return asyncOperationFromTask.Task;
            }

            // BUGBUG: We need to implement TaskCompletionSource before we can implement this properly.
            throw new NotImplementedException();
        }
    }
}

