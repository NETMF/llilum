//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Threading.WaitHandle))]
    public abstract class WaitHandleImpl : MarshalByRefObject
    {
        //
        // State
        //

        protected Synchronization.WaitableObject m_handle;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation]
        protected WaitHandleImpl()
        {
        }

        //
        // Helper Methods
        //

        public virtual bool WaitOne( int  millisecondsTimeout ,
                                     bool exitContext         )
        {
            return WaitOne( (SchedulerTime)millisecondsTimeout, exitContext );
        }

        public virtual bool WaitOne( TimeSpan timeout     ,
                                     bool     exitContext )
        {
            return WaitOne( (SchedulerTime)timeout, exitContext );
        }

        public static bool WaitAll( WaitHandleImpl[] waitHandles         ,
                                    int              millisecondsTimeout ,
                                    bool             exitContext         )
        {
            throw new NotImplementedException();
        }

        public static int WaitAny( WaitHandleImpl[] waitHandles         ,
                                   int              millisecondsTimeout ,
                                   bool             exitContext         )
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose( bool explicitDisposing )
        {
            Synchronization.WaitableObject handle = System.Threading.Interlocked.Exchange( ref m_handle, null );

            if(handle != null)
            {
                handle.Dispose();
            }
        }

        //--//

        public bool WaitOne( SchedulerTime timeout     ,
                             bool          exitContext )
        {
            return m_handle.Acquire( timeout );
        }
    }
}

