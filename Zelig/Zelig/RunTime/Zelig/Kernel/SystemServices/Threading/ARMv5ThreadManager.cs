//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [SingletonFactoryPlatformFilter("ARM")]
    public abstract class ARMv5ThreadManager : ThreadManager
    {
        //
        // State 
        //
    
        protected ThreadImpl    m_interruptThread;
        protected ThreadImpl    m_fastInterruptThread;
        protected ThreadImpl    m_abortThread;
        protected ThreadImpl    m_undefThread;

        //--//

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            base.InitializeAfterStaticConstructors( systemStack );
            
            m_interruptThread     = new ThreadImpl( null, new uint[512] );
            m_fastInterruptThread = new ThreadImpl( null, new uint[512] );
            m_abortThread         = new ThreadImpl( null, new uint[128] );
            m_undefThread         = new ThreadImpl( null, new uint[128] );
            
            //
            // These threads are never started, so we have to manually register them, to enable the debugger to see them.
            //
            RegisterThread( m_interruptThread     );
            RegisterThread( m_fastInterruptThread );
            RegisterThread( m_abortThread         );
            RegisterThread( m_undefThread         );

            //--//
            
            m_interruptThread    .SetupForExceptionHandling( TargetPlatform.ARMv4.ProcessorARMv4.c_psr_mode_IRQ   );
            m_fastInterruptThread.SetupForExceptionHandling( TargetPlatform.ARMv4.ProcessorARMv4.c_psr_mode_FIQ   );
            m_abortThread        .SetupForExceptionHandling( TargetPlatform.ARMv4.ProcessorARMv4.c_psr_mode_ABORT );
            m_undefThread        .SetupForExceptionHandling( TargetPlatform.ARMv4.ProcessorARMv4.c_psr_mode_UNDEF );
        }
        
        public override ThreadImpl InterruptThread
        {
            get
            {
                return m_interruptThread;
            }
        }

        public override ThreadImpl FastInterruptThread
        {
            get
            {
                return m_fastInterruptThread;
            }
        }

        public override ThreadImpl AbortThread
        {
            get
            {
                return m_abortThread;
            }
        }
    }
}

