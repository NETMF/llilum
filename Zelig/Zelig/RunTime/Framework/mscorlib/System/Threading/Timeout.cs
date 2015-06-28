// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Threading
{
    using System;
    using System.Threading;

    // A constant used by methods that take a timeout (Object.Wait, Thread.Sleep
    // etc) to indicate that no timeout should occur.
    //
    // <TODO>@todo: this should become an enum.</TODO>
    //This class has only static members and does not require serialization.
    public static class Timeout
    {
        public const int Infinite = -1;
    }
}
