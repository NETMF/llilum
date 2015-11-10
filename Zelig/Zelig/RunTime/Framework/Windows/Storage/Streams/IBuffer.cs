//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Windows.Storage.Streams
{
    public interface IBuffer
    {
        uint Capacity
        {
            get;
        }

        uint Length
        {
            get;
            set;
        }
    }
}
