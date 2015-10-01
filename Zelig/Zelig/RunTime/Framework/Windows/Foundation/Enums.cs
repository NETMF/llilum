//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Foundation
{
    /// <summary>
    /// Specifies the status of an asynchronous operation.
    /// </summary>
    public enum AsyncStatus
    {
        /// <summary>
        /// The operation has started.
        /// </summary>
        Started = 0,

        /// <summary>
        /// The operation has completed.
        /// </summary>
        Completed,

        /// <summary>
        /// The operation was canceled.
        /// </summary>
        Canceled,

        /// <summary>
        /// The operation has encountered an error.
        /// </summary>
        Error,
    }
}
