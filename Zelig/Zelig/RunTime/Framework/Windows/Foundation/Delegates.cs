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
    /// Represents a method that handles general events.
    /// </summary>
    /// <typeparam name="TSender">The event source.</typeparam>
    /// <typeparam name="TResult">The event data. If there is no event data, this parameter will be null.</typeparam>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}
