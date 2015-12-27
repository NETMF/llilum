//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using System.Threading;
    using Microsoft.Zelig.Runtime.TypeSystem;


    internal class CmsisRtosMutex : CmsisObject
    {
        private readonly object m_sync;

        //--//

        public static CmsisRtosMutex Create( )
        {
            return new CmsisRtosMutex();
        }

        private CmsisRtosMutex( ) : base()
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

        //--//

        [GenerateUnsafeCast]
        internal extern UIntPtr ToPointer( );


        [GenerateUnsafeCast]
        internal extern static CmsisRtosMutex ToObject( UIntPtr mutex );
    }
}
