// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*============================================================
**
**
**
** Purpose: Exception for cancelled IO requests.
**
**
===========================================================*/

using System;
using System.Runtime.Serialization;
using System.Threading;

namespace System
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class OperationCanceledException : SystemException
    {
        [NonSerialized]
        private CancellationToken _cancellationToken;

        public CancellationToken CancellationToken
        {
            get
            {
                return _cancellationToken;
            }

            private set
            {
                _cancellationToken = value;
            }
        }

#if EXCEPTION_STRINGS
        public OperationCanceledException() : base(Environment.GetResourceString("OperationCanceled"))
#else // EXCEPTION_STRINGS
        public OperationCanceledException()
#endif // EXCEPTION_STRINGS
        {
        }

        public OperationCanceledException(string message) :
            base(message)
        {
        }

        public OperationCanceledException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public OperationCanceledException(CancellationToken token) :
            this()
        {
            CancellationToken = token;
        }

        public OperationCanceledException(string message, CancellationToken token) :
            this(message)
        {
            CancellationToken = token;
        }

        public OperationCanceledException(string message, Exception innerException, CancellationToken token) :
            this(message, innerException)
        {
            CancellationToken = token;
        }

#if DISABLED_FOR_LLILUM
        protected OperationCanceledException(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
#endif // DISABLED_FOR_LLILUM
    }
}
