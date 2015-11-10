//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Foundation
{
    public interface IAsyncOperationWithProgress<TResult, TProgress> : IAsyncInfo
    {
        AsyncOperationProgressHandler<TResult, TProgress> Progress
        {
            get;
            set;
        }

        /// <summary>Gets or sets the method that handles the operation completed notification.</summary>
        /// <returns>The method that handles the notification.</returns>
        AsyncOperationWithProgressCompletedHandler<TResult, TProgress> Completed
        {
            get;
            set;
        }

        /// <summary>Returns the results of the operation.</summary>
        /// <returns>The results of the operation.</returns>
        TResult GetResults();
    }
}
