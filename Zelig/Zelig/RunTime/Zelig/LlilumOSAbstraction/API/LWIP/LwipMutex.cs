//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.LWIP
{
    using System;
    using System.Collections;
    using System.Threading;
    using Microsoft.Zelig.Runtime.TypeSystem;

    internal class LwipMutex : IDisposable
    {
        private static ArrayList s_mutexes = new ArrayList();
        private static object s_sync = new object();

        private object m_sync;

        public static LwipMutex Create()
        {
            var mutex = new LwipMutex();

            lock(s_sync)
            {
                s_mutexes.Add(mutex);
            }

            return mutex;
        }

        private LwipMutex()
        {
            m_sync = new object();
        }

        public uint Lock(int msTimeout)
        {
            if (msTimeout < 0)
            {
                Monitor.Enter(m_sync);
                return 0;
            }
            else
            {
                if (Monitor.TryEnter(m_sync, msTimeout))
                {
                    return 0;
                }
                return 1;
            }
        }

        public uint Unlock()
        {
            Monitor.Exit(m_sync);
            return 0;
        }

        public void Dispose()
        {
            lock (s_sync)
            {
                s_mutexes.Remove(this);
            }
        }

        [GenerateUnsafeCast]
        public extern UIntPtr ToPointer();


        [GenerateUnsafeCast]
        public extern static LwipMutex ToObject(UIntPtr mutex);
    }
}
