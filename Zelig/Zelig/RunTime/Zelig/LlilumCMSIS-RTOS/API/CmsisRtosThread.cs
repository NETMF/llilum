//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Runtime.TypeSystem;
    using LLOS = Microsoft.Zelig.LlilumOSAbstraction;


    internal class CmsisRtosThread : CmsisObject
    {        
        private Thread  m_thread;
        private UIntPtr m_codePtr;
        private UIntPtr m_arg;

        //--//

        //
        // err_t sys_sem_new( sys_sem_t *sem, u8_t count )
        //
        public static unsafe CmsisRtosThread Create( UIntPtr codePtr, UIntPtr arg )
        {
            return new CmsisRtosThread( codePtr, arg );
        }

        private unsafe CmsisRtosThread( UIntPtr codePtr, UIntPtr arg ) : base()
        {
            //var code = new CodePointer();
            //code.Target = (IntPtr)threadStart.ToPointer();

            //DelegateImpl dlgImpl = new DelegateImpl( this, code );
            
            m_arg     = arg;
            m_codePtr = codePtr;
            m_thread  = new Thread( ThreadProcedure );

            m_thread.Start( );
        }

        public void Terminate()
        {
            m_thread.Abort( ); 
            m_thread.Join ( ); 

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

        protected void Start()
        {
            m_thread.Start( ); 
        }

        //--//

        private unsafe void ThreadProcedure()
        {
            LLOS_lwIPTaskRun( m_codePtr.ToPointer(), m_arg.ToPointer() );
        }

        //--//

        // This thread calls into a native method LLOS_lwIPTaskRun with a pointer
        // to the thread, which has the function to run and args
        [DllImport("C")]
        private static unsafe extern void LLOS_lwIPTaskRun( void* codePtr, void* arg );

        //--//

        protected override bool SameObject( object cmp )
        {
            return m_thread == cmp;
        }

        [GenerateUnsafeCast]
        internal extern UIntPtr ToPointer();
        
        [GenerateUnsafeCast]
        internal extern static CmsisRtosThread ToObject( UIntPtr thread );

    }

}
