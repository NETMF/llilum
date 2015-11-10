//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;

namespace Windows.Foundation
{
    /// <summary>Represents an asynchronous operation, which returns a result upon completion. This is the return type for many Windows Runtime asynchronous methods that have results but don't report progress.</summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IAsyncOperation<TResult> : IAsyncInfo
    {
        AsyncOperationCompletedHandler<TResult> Completed
        {
            get;
            set;
        }

        /// <summary>Returns the results of the operation.</summary>
        /// <returns>The results of the operation.</returns>
        TResult GetResults();
    }
}
