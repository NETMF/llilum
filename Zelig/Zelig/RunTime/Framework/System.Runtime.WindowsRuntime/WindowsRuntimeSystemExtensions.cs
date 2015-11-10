//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Internal;

namespace System.Runtime
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
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsAsyncAction(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous action.
        /// </summary>
        /// <param name="source">The asynchronous action.</param>
        /// <returns>A task that represents the asynchronous action.</returns>
        public static Task AsTask(this IAsyncAction source)
        {
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsTask(source);
        }

        /// <summary>
        /// Returns a Windows Runtime asynchronous operation that represents a started task.
        /// </summary>
        /// <param name="source">The started task.</param>
        /// <returns>A Windows.Foundation.IAsyncOperation instance that represents the started task.</returns>
        public static IAsyncOperation<T> AsAsyncOperation<T>(this Task<T> source)
        {
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsAsyncOperation(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous operation.
        /// </summary>
        /// <param name="source">The asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<T> AsTask<T>(this IAsyncOperation<T> source)
        {
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsTask(source);
        }

        /// <summary>
        /// Returns a Windows Runtime asynchronous operation that represents a started task.
        /// </summary>
        /// <param name="source">The started task.</param>
        /// <returns>A Windows.Foundation.IAsyncOperation instance that represents the started task.</returns>
        public static IAsyncOperationWithProgress<T, P> AsAsyncOperationWithProgress<T, P>(this Task<T> source)
        {
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsAsyncOperationWithProgress<T, P>(source);
        }

        /// <summary>
        /// Returns a task that represents a Windows Runtime asynchronous operation.
        /// </summary>
        /// <param name="source">The asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<T> AsTask<T, P>(this IAsyncOperationWithProgress<T, P> source)
        {
            return Windows.Internal.WindowsRuntimeSystemExtensions.AsTask(source);
        }
    }
}

