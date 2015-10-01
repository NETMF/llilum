//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;

namespace Windows.Foundation
{
    /// <summary>
    /// Supports asynchronous actions and operations. IAsyncInfo is a base interface for IAsyncAction,
    /// IAsyncActionWithProgress{TProgress}, IAsyncOperation{TResult} and IAsyncOperationWithProgress{TResult,TProgress},
    /// each of which support combinations of return type and progress for an asynchronous method.
    /// </summary>
    public interface IAsyncInfo
    {
        /// <summary>
        /// Gets a string that describes an error condition of the asynchronous operation.
        /// </summary>
        /// <value>
        /// The error string.
        /// </value>
        Exception ErrorCode
        {
            get;
        }

        /// <summary>
        /// Gets the handle of the asynchronous operation.
        /// </summary>
        /// <value>
        /// The handle of the asynchronous operation.
        /// </value>
        uint Id
        {
            get;
        }

        /// <summary>
        /// Gets a value that indicates the status of the asynchronous operation.
        /// </summary>
        /// <value>
        /// The status of the operation, as a value of the enumeration. A value of Completed indicates that the method
        /// has returned. The Started value represents a transition state before any of the other 3 final results
        /// (Completed, Error, Canceled) can be determined by the method's invocation.
        /// </value>
        AsyncStatus Status
        {
            get;
        }

        /// <summary>
        /// Cancels the asynchronous operation.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Closes the asynchronous operation.
        /// </summary>
        void Close();
    }
}
