//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Runtime.TypeSystem;
    
    

    internal class CmsisRtosMessageQueue : CmsisObject
    {
        private readonly KernelCircularBuffer<UIntPtr> m_messages;

        //--//

        public static CmsisRtosMessageQueue Create( uint queueSize )
        {
            return new CmsisRtosMessageQueue( queueSize );
        }
        
        private CmsisRtosMessageQueue( uint queueSize ) : base()
        {
            m_messages = new KernelCircularBuffer<UIntPtr>( (int)queueSize );
        }
        
        //--//

        protected override void Dispose( bool fDisposing )
        {
            base.Dispose( fDisposing );

            //////UIntPtr message;
            //////while(TryGetMessage( 0, out message ))
            //////{

            //////}
        }

        //--//

        //
        // Helper methods
        //

        public unsafe bool TryGetMessage( int millisec, out UIntPtr message ) 
        {
            if(m_messages.DequeueBlocking( millisec, out message ))
            {
                return true; 
            }

            return false;
        }

        public unsafe bool TryPutMessage( UIntPtr message, int millisec )
        {
            if(m_messages.EnqueueBlocking( millisec, message ))
            {
                return true;
            }

            return false;
        }

        //--//

        [GenerateUnsafeCast]
        internal extern UIntPtr ToPointer();
        
        [GenerateUnsafeCast]
        internal extern static CmsisRtosMessageQueue ToObject(UIntPtr mutex);
    }
}
