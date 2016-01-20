//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal class CmsisRtosThread : CmsisObject
    {        
        private RT.ThreadImpl   m_thread;
        private UIntPtr         m_codePtr;
        private UIntPtr         m_arg;

        //--//
        
        public static unsafe CmsisRtosThread Create( UIntPtr codePtr, ThreadPriority priority, uint stackSize, UIntPtr arg )
        {
            return new CmsisRtosThread( codePtr, priority, stackSize, arg );
        }

        private unsafe CmsisRtosThread( UIntPtr codePtr, ThreadPriority priority, uint stackSize, UIntPtr arg ) : base()
        {    
            m_arg               = arg;
            m_codePtr           = codePtr;
            m_thread            = new RT.ThreadImpl( ThreadProcedure, new uint[ stackSize ] );
            m_thread.Priority   = priority;

            m_thread.Start( );
        }

        //--//

        protected override void Dispose( bool fDisposing )
        {
            base.Dispose( fDisposing );

            //////m_thread.Detach( );

            //////if(m_thread != RT.ThreadImpl.CurrentThread)
            //////{
            //////    m_thread.Join( );
            //////}
        }

        //--//

        //
        // Helper methods
        //

        public void Terminate()
        {
            Dispose( ); 
        }

        public static UIntPtr GetId( Thread thread )
        {
            var th = CmsisObject.FindObject<CmsisRtosThread>( thread );

            if(th == null)
            {
                return UIntPtr.Zero;
            }

            return ( (CmsisRtosThread)th ).Id;
        }

        public UIntPtr Id
        {
            get
            {
                return ToPointer( );
            }
        }

        //--//
        
        protected override bool SameObject( object cmp )
        {
            return m_thread == cmp;
        }

        //--//

        private unsafe void ThreadProcedure()
        {
            LLOS_lwIPTaskRun( m_codePtr.ToPointer(), m_arg.ToPointer() );
        }

        //--//
        
        [DllImport("C")]
        private static unsafe extern void LLOS_lwIPTaskRun( void* codePtr, void* arg );

        //--//

        [TS.GenerateUnsafeCast]
        internal extern UIntPtr ToPointer();
        
        [TS.GenerateUnsafeCast]
        internal extern static CmsisRtosThread ToObject( UIntPtr thread );
    }

}
