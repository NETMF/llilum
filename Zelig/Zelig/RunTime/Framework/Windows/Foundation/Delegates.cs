//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Foundation
{
    /// <summary>
    /// Represents a method that handles the completed event of an asynchronous action.
    /// </summary>
    /// <param name="asyncInfo">The asynchronous action.</param>
    /// <param name="asyncStatus">One of the enumeration values.</param>
    public delegate void AsyncActionCompletedHandler(IAsyncAction asyncInfo, AsyncStatus asyncStatus);

    /// <summary>
    /// Represents a method that handles the completed event of an asynchronous operation.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="asyncInfo"></param>
    /// <param name="asyncStatus"></param>
    public delegate void AsyncOperationCompletedHandler<TResult>(IAsyncOperation<TResult> asyncInfo, AsyncStatus asyncStatus);

    /// <summary>Represents a method that handles progress update events of an asynchronous operation that provides progress updates.</summary>
	/// <typeparam name="TResult">The result.</typeparam>
	/// <typeparam name="TProgress">The progress information.</typeparam>
	/// <param name="asyncInfo">The asynchronous operation.</param>
	/// <param name="progressInfo">The progress information.</param>
    public delegate void AsyncOperationProgressHandler<TResult, TProgress>(IAsyncOperationWithProgress<TResult, TProgress> asyncInfo, TProgress progressInfo);

    /// <summary>
    /// Represents a method that handles the completed event of an asynchronous operation with a completion handler.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TProgress"></typeparam>
    /// <param name="asyncInfo"></param>
    /// <param name="asyncStatus"></param>
    public delegate void AsyncOperationWithProgressCompletedHandler<TResult, TProgress>(IAsyncOperationWithProgress<TResult, TProgress> asyncInfo, AsyncStatus asyncStatus);

    /// <summary>
    /// Represents a method that handles general events.
    /// </summary>
    /// <typeparam name="TSender">The event source.</typeparam>
    /// <typeparam name="TResult">The event data. If there is no event data, this parameter will be null.</typeparam>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}
