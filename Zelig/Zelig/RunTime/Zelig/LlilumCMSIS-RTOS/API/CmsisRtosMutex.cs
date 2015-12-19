//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using System.Threading;
    using Microsoft.Zelig.Runtime.TypeSystem;


    internal class CmsisRtosMutex : IDisposable
    {
        private static ArrayList s_mutexes = new ArrayList();
        private static object    s_sync    = new object();

        //--//

        private object m_sync;

        //--//

        public static CmsisRtosMutex Create( )
        {
            var mutex = new CmsisRtosMutex();

            lock (s_sync)
            {
                s_mutexes.Add( mutex );
            }

            return mutex;
        }

        private CmsisRtosMutex( )
        {
            m_sync = new object( );
        }

        //--//

        public uint Lock( int millisec )
        {
            if(millisec < 0)
            {
                Monitor.Enter( m_sync );
            }
            else
            {
                if(Monitor.TryEnter( m_sync, millisec ))
                {
                    return 0;
                }

                return 1;
            }

            return 0;
        }

        public uint Unlock( )
        {
            Monitor.Exit( m_sync );

            return 0;
        }

        public void Dispose( )
        {
            lock(s_sync)
            {
                s_mutexes.Remove( this );
            }
        }

        [GenerateUnsafeCast]
        public extern UIntPtr ToPointer( );


        [GenerateUnsafeCast]
        public extern static CmsisRtosMutex ToObject( UIntPtr mutex );
    }
}
