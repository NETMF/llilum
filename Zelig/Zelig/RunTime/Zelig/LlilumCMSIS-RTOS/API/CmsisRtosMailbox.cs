//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Runtime.TypeSystem;
    
    

    internal class CmsisRtosMailbox : CmsisObject
    {
        private readonly KernelCircularBuffer<UIntPtr> m_buffer;

        //--//

        public static CmsisRtosMailbox Create( uint queueSize )
        {
            return new CmsisRtosMailbox( queueSize );
        }
        
        private CmsisRtosMailbox( uint queueSize ) : base()
        {
            m_buffer = new KernelCircularBuffer<UIntPtr>( (int)queueSize );
        }

        public unsafe bool TryGetMessage( int millisec, out UIntPtr message ) 
        {
            if(m_buffer.DequeueBlocking( millisec, out message ))
            {
                return true; 
            }

            return false;
        }

        public unsafe bool TryPutMessage( UIntPtr message, int millisec )
        {
            if(m_buffer.EnqueueBlocking( millisec, message ))
            {
                return true;
            }

            return false;
        }

        //--//

        [GenerateUnsafeCast]
        internal extern UIntPtr ToPointer();
        
        [GenerateUnsafeCast]
        internal extern static CmsisRtosMailbox ToObject(UIntPtr mutex);
    }
}
