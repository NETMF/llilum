//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using RT = Microsoft.Zelig.Runtime;


    public static class CmsisRtos
    {
        //
        // !!! keep in sync with same enums and structures in cmsis_os.h
        //
        public enum osStatus
        {
            osOK                    = 0,       ///< function completed; no error or event occurred.
            osEventSignal           = 0x08,       ///< function completed; signal event occurred.
            osEventMessage          = 0x10,       ///< function completed; message event occurred.
            osEventMail             = 0x20,       ///< function completed; mail event occurred.
            osEventTimeout          = 0x40,       ///< function completed; timeout occurred.
            osErrorParameter        = 0x80,       ///< parameter error: a mandatory parameter was missing or specified an incorrect object.
            osErrorResource         = 0x81,       ///< resource not available: a specified resource was not available.
            osErrorTimeoutResource  = 0xC1,       ///< resource not available within given time: a specified resource was not available within the timeout period.
            osErrorISR              = 0x82,       ///< not allowed in ISR context: the function cannot be called from interrupt service routines.
            osErrorISRRecursive     = 0x83,       ///< function called multiple times from ISR with same object.
            osErrorPriority         = 0x84,       ///< system cannot determine priority or thread has illegal priority.
            osErrorNoMemory         = 0x85,       ///< system is out of memory: it was impossible to allocate or reserve memory for the operation.
            osErrorValue            = 0x86,       ///< value of a parameter is out of range.
            osErrorOS               = 0xFF,       ///< unspecified RTOS error: run-time error but no other error message fits.
            os_status_reserved      = 0x7FFFFFFF  ///< prevent from enum down-size compiler optimization.
        }

        //--//

        //[StructLayout( LayoutKind.Sequential, Pack = 4 )]
        //public struct osEvent__value
        //{
        //    public uint    v;
        //    public UIntPtr p;
        //    public int     signals;
        //}

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct osEvent
        {
            public osStatus         status;
            public UIntPtr          value;
            public UIntPtr          def;

            public osEvent( osStatus status )
            {
                this.status         = status;
                this.value          = UIntPtr.Zero;
                this.def            = UIntPtr.Zero;
            }

            public static osEvent ErrorParameter
            {
                get
                {
                    return new osEvent(osStatus.osErrorParameter);
                }
            }

            public static osEvent ErrorValue
            {
                get
                {
                    return new osEvent(osStatus.osErrorValue);
                }
            }

            public static osEvent OK
            {
                get
                {
                    return new osEvent(osStatus.osOK);
                }
            }

            public static osEvent Timeout
            {
                get
                {
                    return new osEvent(osStatus.osEventTimeout);
                }
            }

            public static osEvent ErrorISR
            {
                get
                {
                    return new osEvent(osStatus.osErrorISR);
                }
            }

            public static osEvent Message
            {
                get
                {
                    return new osEvent(osStatus.osEventMessage);
                }
            }

            internal osEvent With(UIntPtr message)
            {
                this.value = message;

                return this;
            }
        }

        //--//

        public enum OsPriority
        {
            osPriorityIdle          = -3,          ///< priority: idle (lowest)
            osPriorityLow           = -2,          ///< priority: low
            osPriorityBelowNormal   = -1,          ///< priority: below normal
            osPriorityNormal        = 0,          ///< priority: normal (default)
            osPriorityAboveNormal   = +1,          ///< priority: above normal
            osPriorityHigh          = +2,          ///< priority: high
            osPriorityRealtime      = +3,          ///< priority: realtime (highest)
            osPriorityError         = 0x84        ///< system cannot determine priority or thread has illegal priority
        };

        internal static ThreadPriority ConvertPriority( OsPriority pri )
        {
            switch(pri)
            {
                case OsPriority.osPriorityIdle          : return ThreadPriority.Lowest;
                case OsPriority.osPriorityLow           : return ThreadPriority.Lowest;
                case OsPriority.osPriorityBelowNormal   : return ThreadPriority.BelowNormal;
                case OsPriority.osPriorityNormal        : return ThreadPriority.Normal;
                case OsPriority.osPriorityAboveNormal   : return ThreadPriority.Normal;
                case OsPriority.osPriorityHigh          : return ThreadPriority.AboveNormal;
                case OsPriority.osPriorityRealtime      : return ThreadPriority.Highest;
                case OsPriority.osPriorityError         :
                default:
                    throw new ArgumentException( );
            }
        }

        //--//

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct osMessageQDef_t
        {
            public uint    queue_sz;
            public uint    item_sz;
            public UIntPtr pool;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct os_messageQ_cb
        {

        }

        //--//

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct osMutexDef_t
        {
            public uint dummy;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct os_mutex_cb
        {
            //public uint dummy;
        }

        //--//

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct oPoolQDef_t
        {
            public uint    pool_sz;
            public uint    item_sz;
            public UIntPtr pool;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct os_pool_cb
        {
            //public UIntPtr dummy;
        }

        //--//

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct osSemaphoreDef_t
        {
            public uint dummy;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct os_semaphore_cb
        {
            //public uint dummy;
        }

        //--//

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct osThreadDef_t
        {
            public UIntPtr      pthread;    // this is the code pointer for a delegate (function pointer) type 'typedef void(* os_pthread)(void const *argument)'
            public OsPriority   tpriority;
            public uint         instances;
            public uint         stackSize;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 4 )]
        public struct os_thread_cb
        {
            //UIntPtr dummy;
        }

        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //

        //
        // Wait/Delay
        // 

        [RT.ExportedMethod]
        public static osStatus osDelay( uint millisec )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( Runtime.BugCheck.StopCode.IllegalMode );
                return osStatus.osErrorISR;
            }

            RT.TargetPlatform.ARMv7.ProcessorARMv7M.DelayMicroseconds( millisec * 1000 );

            return osStatus.osEventTimeout;
        }

        [RT.ExportedMethod]
        public static osStatus osWaitEx( uint millisec, ref osEvent ev )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                ev.status = osStatus.osErrorParameter;

                return osStatus.osErrorParameter;
            }

            RT.TargetPlatform.ARMv7.ProcessorARMv7M.DelayMicroseconds( millisec * 1000 );

            return osStatus.osEventTimeout;
        }

        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //

        //
        // Message Queue
        //

        [RT.ExportedMethod]
        public static unsafe os_messageQ_cb* osMessageCreate( osMessageQDef_t* queue_def, os_thread_cb* thread_id )
        {
            var msgQueue = CmsisRtosMessageQueue.Create( queue_def->queue_sz );

            return (os_messageQ_cb*)msgQueue.ToPointer( );
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osMessageGetEx( os_messageQ_cb* queue_id, uint millisec, ref osEvent ev )
        {
            if(queue_id == null)
            {
                ev.status = osStatus.osErrorParameter;

                return osStatus.osErrorParameter;
            }

            var msgQueue = CmsisRtosMessageQueue.ToObject( (UIntPtr)queue_id );

            if(msgQueue == null)
            {
                ev.status = osStatus.osErrorParameter;

                return osStatus.osErrorParameter;
            }

            UIntPtr message;
            if(msgQueue.TryGetMessage( (int)millisec, out message ) == false)
            {
                if(millisec == 0)
                {
                    ev.status = osStatus.osOK;

                    return osStatus.osOK;
                }
                
                ev.status = osStatus.osEventTimeout;

                return osStatus.osEventTimeout;
            }
            
            ev.status  = osStatus.osEventMessage;
            ev.value   = message;

            return osStatus.osEventMessage;
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osMessagePut( os_messageQ_cb* queue_id, uint info, uint millisec )
        {
            if(queue_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var msgQueue = CmsisRtosMessageQueue.ToObject( (UIntPtr)queue_id );

            if(msgQueue == null)
            {
                return osStatus.osErrorParameter;
            }

            if(msgQueue.TryPutMessage( new UIntPtr( info ), (int)millisec ) == false)
            {
                if(millisec == 0)
                {
                    return osStatus.osOK;
                }

                return osStatus.osErrorTimeoutResource;
            }

            return osStatus.osOK;
        }
        
        [RT.ExportedMethod]
        public static unsafe osStatus osMessageDelete( os_messageQ_cb* queue_id )
        {
            if(queue_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var msgQueue = CmsisRtosMessageQueue.ToObject( (UIntPtr)queue_id );

            if(msgQueue == null)
            {
                return osStatus.osErrorParameter;
            }

            msgQueue.Dispose( ); 

            return osStatus.osOK;
        }

        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //

        //
        // Mutex
        //

        [RT.ExportedMethod]
        public static unsafe os_mutex_cb* osMutexCreate( osMutexDef_t* mutex_def )
        {
            var mutex = CmsisRtosMutex.Create();

            return (os_mutex_cb*)mutex.ToPointer( );
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osMutexWait( os_mutex_cb* mutex_id, uint millisec )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( Runtime.BugCheck.StopCode.IllegalMode );
                return osStatus.osErrorISR;
            }

            if(mutex_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var mutex = CmsisRtosMutex.ToObject( (UIntPtr)mutex_id );

            if(mutex == null)
            {
                return osStatus.osErrorParameter;
            }

            var locked = mutex.Lock( (int)millisec );

            if(locked)
            {
                return osStatus.osOK;
            }

            if(millisec == 0)
            {
                return osStatus.osErrorResource;
            }

            return osStatus.osErrorTimeoutResource;
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osMutexRelease( os_mutex_cb* mutex_id )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( Runtime.BugCheck.StopCode.IllegalMode );
                return osStatus.osErrorISR;
            }

            if(mutex_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var mutex = CmsisRtosMutex.ToObject( (UIntPtr)mutex_id );

            if(mutex == null)
            {
                return osStatus.osErrorParameter;
            }

            mutex.Unlock( );

            return osStatus.osOK;
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osMutexDelete( os_mutex_cb* mutex_id )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( Runtime.BugCheck.StopCode.IllegalMode );
                return osStatus.osErrorISR;
            }

            if(mutex_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var mutex = CmsisRtosMutex.ToObject( (UIntPtr)mutex_id );

            if(mutex == null)
            {
                return osStatus.osErrorParameter;
            }

            mutex.Dispose( );

            return osStatus.osOK;
        }

        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //

        //
        // Semaphore
        //

        [RT.ExportedMethod]
        public static unsafe os_semaphore_cb* osSemaphoreCreate( osSemaphoreDef_t* semaphore_def, int count )
        {
            var sem = CmsisRtosSemaphore.Create( count );

            return (os_semaphore_cb*)sem.ToPointer( );
        }

        [RT.ExportedMethod]
        public static unsafe int osSemaphoreWaitEx( os_semaphore_cb* semaphore_id, uint millisec )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( Runtime.BugCheck.StopCode.IllegalMode );
            }

            if(semaphore_id == null)
            {
                return 0;
            }

            var sem = CmsisRtosSemaphore.ToObject( (UIntPtr)semaphore_id );

            if(sem == null)
            {
                return 0;
            }

            return sem.Acquire( (int)millisec );
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osSemaphoreRelease( os_semaphore_cb* semaphore_id )
        {
            if(semaphore_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var sem = CmsisRtosSemaphore.ToObject( (UIntPtr)semaphore_id );

            if(sem == null)
            {
                return osStatus.osErrorParameter;
            }

            //////if(Microsoft.Zelig.Runtime.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            //////{
            //////    using(RT.SmartHandles.InterruptState.Disable( ))
            //////    {
            //////        using(RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallInterruptThread( ))
            //////        {
            //////            sem.Release( );
            //////        }
            //////    }
            //////}
            //////else
            //////{
                sem.Release( );
            //////}

            return osStatus.osOK;
        }

        [RT.ExportedMethod]
        public static unsafe osStatus osSemaphoreDelete( os_semaphore_cb* semaphore_id )
        {
            if(RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive( ))
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.IllegalMode ); 
                return osStatus.osErrorISR;
            }

            if(semaphore_id == null)
            {
                return osStatus.osErrorParameter;
            }

            var sem = CmsisRtosSemaphore.ToObject( (UIntPtr)semaphore_id );

            if(sem == null)
            {
                return osStatus.osErrorParameter;
            }

            sem.Dispose( );

            return osStatus.osOK;
        }

        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //
        // o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o //

        //
        // Thread
        //

        [RT.ExportedMethod]
        public static unsafe os_thread_cb* osThreadCreate( osThreadDef_t* thread_def, UIntPtr arg )
        {
            var th = CmsisRtosThread.Create(                    thread_def->pthread    , 
                                            ConvertPriority(    thread_def->tpriority ),
                                                                thread_def->stackSize  ,
                                                                arg );

            return (os_thread_cb*)th.ToPointer( );
        }

        [RT.ExportedMethod]
        public static unsafe os_thread_cb* osThreadGetId( )
        {
            return (os_thread_cb*)CmsisRtosThread.GetId( Thread.CurrentThread );
        }
    }
}

