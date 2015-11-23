//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.LWIP
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Zelig.Runtime.TypeSystem;
    using LLOS = Microsoft.Zelig.LlilumOSAbstraction;

    internal class LwipSemaphore : IDisposable
    {
        private static ArrayList s_semaphores = new ArrayList();
        private static object s_sync = new object();

        //--//

        private object m_sync;
        private int m_count;
        private int m_ceiling;
        private AutoResetEvent m_free;

        //--//

        //
        // err_t sys_sem_new( sys_sem_t *sem, u8_t count )
        //
        public static LwipSemaphore Create(int count)
        {
            var sem = new LwipSemaphore(count);

            lock (s_sync)
            {
                s_semaphores.Add(sem);
            }

            return sem;
        }

        private LwipSemaphore(int count)
        {
            m_sync = new object();
            m_count = count;
            m_ceiling = count;
            m_free = new AutoResetEvent(false);
        }

        //
        // u32_t sys_arch_sem_wait( sys_sem_t *sem, u32_t timeout )
        //
        public int Acquire(int timeout)
        {
            bool fAcquired = false;

            var start = (uint)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTicks();

            do
            {
                while (m_count == 0)
                {
                    if (m_free.WaitOne(timeout, false) == false)
                    {
                        return -1;
                    }
                }

                lock (m_sync)
                {
                    //
                    // Some other thread may just have stolen this semaphore, and 
                    // and we may have to sleep again
                    //
                    if (m_count > 0)
                    {
                        --m_count;

                        fAcquired = true;
                    }
                    else
                    {
                        if(timeout >= 0)
                        {
                            timeout -= (int)(((uint)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTicks() - start)/1000);
                            if(timeout < 0)
                            {
                                return -1;
                            }
                        }
                    }
                }

            } while (fAcquired == false);

            return m_count;
        }

        //
        // void sys_sem_signal( sys_sem_t *sem )
        // 
        public void Release()
        {
            lock (m_sync)
            {
                ++m_count;

                m_free.Set();
            }
        }

        //
        // void sys_sem_free( sys_sem_t *sem )
        //
        public void Dispose()
        {
            lock (s_sync)
            {
                s_semaphores.Remove(this);
            }
        }

        //--//

        [GenerateUnsafeCast]
        public extern UIntPtr ToPointer();


        [GenerateUnsafeCast]
        public extern static LwipSemaphore ToObject(UIntPtr semaphore);
    }

}
