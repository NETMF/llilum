//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Threading;
    
    using RT   = Microsoft.Zelig.Runtime;
    using TS   = Microsoft.Zelig.Runtime.TypeSystem;
    using LLOS = Microsoft.Zelig.LlilumOSAbstraction;


    internal class CmsisRtosSemaphore : CmsisObject
    {
        private readonly AutoResetEvent m_free;
        private          int            m_count;
        
        //--//

        public static CmsisRtosSemaphore Create( int count )
        {
            return new CmsisRtosSemaphore(count);
        }

        private CmsisRtosSemaphore( int count ) : base()
        {
            m_count = count;
            m_free  = new AutoResetEvent( false );
        }

        //--//

        protected override void Dispose( bool fDisposing )
        {
            base.Dispose( fDisposing );

            //////if(m_count == 0)
            //////{
            //////    Release( );
            //////}
        }

        //--//

        //
        // Helper methods
        //

        public int Acquire( int timeout )
        {
            bool fAcquired = false;

            var start = (uint)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTicks();

            do
            {
                while(m_count == 0)
                {
                    if(m_free.WaitOne( timeout, false ) == false)
                    {
                        return -1;
                    }
                }

                using(RT.SmartHandles.InterruptState.DisableAll( )) 
                {
                    //
                    // Some other thread may just have stolen this semaphore, and 
                    // and we may have to sleep again
                    //
                    if(m_count > 0)
                    {
                        --m_count;

                        fAcquired = true;
                    }
                    else
                    {
                        if(timeout >= 0)
                        {
                            timeout -= (int)( ( (uint)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTicks( ) - start ) / 1000 );

                            if(timeout < 0)
                            {
                                return -1;
                            }
                        }
                    }
                }

            } while(fAcquired == false);

            return m_count;
        }
        
        public void Release( )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                ++m_count;

                m_free.Set( );
            }
        }
        
        //--//

        [TS.GenerateUnsafeCast]
        internal extern UIntPtr ToPointer( );
        
        [TS.GenerateUnsafeCast]
        internal extern static CmsisRtosSemaphore ToObject( UIntPtr semaphore );
    }
}
