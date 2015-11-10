//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Runtime.InteropServices;

namespace Windows.Storage.Streams
{
    using Windows.Foundation;

    public enum InputStreamOptions : uint
    {
        None = 0,
        Partial,
        ReadAhead,
    }

    public interface IInputStream : IDisposable
    {
        IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options);
    }
}
