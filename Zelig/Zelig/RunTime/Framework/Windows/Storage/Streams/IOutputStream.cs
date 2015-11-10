//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;

namespace Windows.Storage.Streams
{
    using Windows.Foundation;

    public interface IOutputStream : IDisposable
    {
        IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer);

        IAsyncOperation<bool> FlushAsync();
    }
}
