//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Zelig.Runtime;

    using CMSISRTOS = Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos;


    public static class CmsisRtos
    {
        private static List<Thread> m_threads = new List<Thread>();

        //
        // Mailbox
        //

        [ExportedMethod]
        public static unsafe UIntPtr LLOS_osMessageCreate(void* mbox, int queue_sz)
        {
            var mailbox = CMSISRTOS.CmsisRtosMailbox.Create(queue_sz);

            return mailbox.ToPointer();
        }

        [ExportedMethod]
        public static unsafe UIntPtr LLOS_osMessageGet(UIntPtr mbox, int msTimeout)
        {
            var mailbox = CmsisRtosMailbox.ToObject(mbox);
            UIntPtr message;

            if(mailbox.TryGetMessage(msTimeout, out message))
            {
                return message;
            }
            return UIntPtr.Zero;
        }

        [ExportedMethod]
        public static unsafe uint LLOS_osMessagePut(UIntPtr mbox, UIntPtr msg, int msTimeout)
        {
            var mailbox = CmsisRtosMailbox.ToObject(mbox);
            if (mailbox.TryPutMessage(msg, msTimeout))
            {
                return 0;
            }
            return 1;
        }

        //
        // Mutex
        //

        [ExportedMethod]
        public static unsafe UIntPtr LLOS_osMutexCreate()
        {
            var mutex = CmsisRtosMutex.Create();

            return mutex.ToPointer();
        }

        [ExportedMethod]
        public static unsafe uint LLOS_osMutexWait(UIntPtr mutex, int msTimeout)
        {
            var mutexObj = CmsisRtosMutex.ToObject(mutex);

            return mutexObj.Lock(msTimeout);
        }

        [ExportedMethod]
        public static unsafe uint LLOS_osMutexRelease(UIntPtr mutex)
        {
            var mutexObj = CmsisRtosMutex.ToObject(mutex);

            return mutexObj.Unlock();
        }

        [ExportedMethod]
        public static unsafe void LLOS_osMutexFree(UIntPtr mutex)
        {
            var mutexObj = CmsisRtosMutex.ToObject(mutex);
            mutexObj.Dispose();
        }

        //
        // Semaphore
        //

        [ExportedMethod]
        public static UIntPtr LLOS_osSemaphoreCreate(IntPtr semaphore_def, uint count)
        {
            var sem = CmsisRtosSemaphore.Create((int)count);

            return sem.ToPointer();
        }

        [ExportedMethod]
        public static int LLOS_osSemaphoreWait(UIntPtr semaphore_id, uint millisec)
        {
            var sem = CmsisRtosSemaphore.ToObject(semaphore_id);

            return sem.Acquire((int)millisec);

        }

        [ExportedMethod]
        public static uint LLOS_osSemaphoreRelease(UIntPtr semaphore_id)
        {
            var sem = CmsisRtosSemaphore.ToObject(semaphore_id);

            sem.Release();

            return 0;
        }

        [ExportedMethod]
        public static uint LLOS_osSemaphoreDelete(UIntPtr semaphore_id)
        {
            var sem = CmsisRtosSemaphore.ToObject(semaphore_id);

            sem.Dispose();

            return 0;
        }

        //
        // Thread
        //

        [ExportedMethod]
        public static UIntPtr LLOS_osThreadCreate(UIntPtr nativeThread)
        {
            // This thread calls into a native method LLOS_lwIPTaskRun with a pointer
            // to the thread, which has the function to run and args
            Thread thread = new Thread(delegate ()
            {
                LLOS_lwIPTaskRun(nativeThread);
            });

            m_threads.Add(thread);

            thread.Start();

            return nativeThread;
        }


        [DllImport("C")]
        public static unsafe extern void LLOS_lwIPTaskRun(UIntPtr thread);
    }
}
