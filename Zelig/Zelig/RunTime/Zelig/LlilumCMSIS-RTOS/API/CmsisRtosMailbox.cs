//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Runtime.TypeSystem;
    
    

    internal class CmsisRtosMailbox : IDisposable
    {
        private static ArrayList    s_mailboxes = new ArrayList();
        private static object       s_sync      = new object();

        //--//

        private KernelCircularBuffer<UIntPtr> m_buffer;

        //--//

        public static CmsisRtosMailbox Create(int queueSize)
        {
            var mailbox = new CmsisRtosMailbox(queueSize);

            lock(s_sync)
            {
                s_mailboxes.Add(mailbox);
            }
            return mailbox;
        }
        
        private CmsisRtosMailbox(int queueSize)
        {
            m_buffer = new KernelCircularBuffer<UIntPtr>(queueSize);
        }

        public unsafe bool TryGetMessage(int msTimeout, out UIntPtr message)
        {
            if(m_buffer.DequeueBlocking(msTimeout, out message))
            {
                return true;
            }
            return false;
        }

        public unsafe bool TryPutMessage(UIntPtr message, int msTimeout)
        {
            if(m_buffer.EnqueueBlocking(msTimeout, message))
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            lock(s_sync)
            {
                s_mailboxes.Remove(this);
            }
        }

        [GenerateUnsafeCast]
        public extern UIntPtr ToPointer();
        
        [GenerateUnsafeCast]
        public extern static CmsisRtosMailbox ToObject(UIntPtr mutex);
    }
}
