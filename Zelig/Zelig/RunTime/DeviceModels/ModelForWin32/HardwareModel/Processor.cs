//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Win32
{
    using System;
    using System.Threading;
    using LLOS = Zelig.LlilumOSAbstraction;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using SH = Microsoft.Zelig.Runtime.TargetPlatform.Win32.SmartHandles;
    
    [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.Win32Product")]
    public sealed class Processor : RT.Processor
    {
        public new class Context : RT.Processor.Context
        {
            private UIntPtr m_threadHandle;

            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            public override UIntPtr BaseStackPointer
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public override uint ExcReturn
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr ProgramCounter
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override uint ScratchedIntegerRegisters
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr StackPointer
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr GetRegisterByIndex( uint idx )
            {
                throw new NotImplementedException( );
            }

            public override void Populate( )
            {
                throw new NotImplementedException( );
            }

            public override void Populate( RT.Processor.Context context )
            {
                throw new NotImplementedException( );
            }

            public override void PopulateFromDelegate( Delegate dlg, uint[] stack )
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_CreateThread(dlg, m_owner, ref m_threadHandle), false);
            }

            public override void SetRegisterByIndex( uint idx, UIntPtr value )
            {
                throw new NotImplementedException( );
            }

            public override void SetupForExceptionHandling( uint mode )
            {
                throw new NotImplementedException( );
            }

            public override void SwitchTo( )
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_SetPriority(m_threadHandle, LLOS.HAL.Thread.ThreadPriority.Highest), false);
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_Yield(), false);

                // For Win32 builds we need to keep the entry-point thread alive since it is the main process thread.
                while(true)
                {
                }
            }

            public override bool Unwind( )
            {
                throw new NotImplementedException( );
            }

            //--//

            public void WaitForEvent(int timeoutMs)
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_Wait(m_threadHandle, timeoutMs), false);
            }

            public void SetEvent()
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_Signal(m_threadHandle), false);
            }

            public void Yield()
            {
                LLOS.HAL.Thread.LLOS_THREAD_Yield( );
            }

            public void Start()
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Thread.LLOS_THREAD_Start( m_threadHandle ), false );
            }

            public void Retire()
            {
                LLOS.HAL.Thread.LLOS_THREAD_DeleteThread(m_threadHandle);
            }
        }

        [TS.WellKnownType( "Microsoft_Zelig_Win32_MethodWrapper" )]
        public sealed class MethodWrapper : RT.AbstractMethodWrapper
        {

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public override void Prologue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Prologue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  RT.HardwareException he )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public override void Epilogue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Epilogue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  RT.HardwareException he )
            {

            }

        }

        //
        // Access methods
        //

        public override RT.Processor.Context AllocateProcessorContext( RT.ThreadImpl owner )
        {
            return new Context( owner );
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( );
        }

        public override bool AreInterruptsDisabled( )
        {
            return SH.InterruptState.AreInterruptsDisabled();
        }

        public override bool AreInterruptsEnabled()
        {
            // Always return true because we use a global mutex instead of interrupts, so cannot guarantee that a
            // thread holding the mutex will not be swapped out by the Windows scheduler.  The only thing that uses
            // this function is the AssertInterruptsOn assert which is not valid in the Win32 port.
            return true;
        }

        public override void Breakpoint( )
        {
            throw new NotImplementedException( );
        }

        public override void FlushCacheLine( UIntPtr target )
        {
            throw new NotImplementedException( );
        }

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            throw new NotImplementedException( );
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            throw new NotImplementedException( );
        }

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            RT.SmartHandles.InterruptState.DisableAll( );
        }

        [TS.GenerateUnsafeCast()]
        internal extern static RT.ThreadImpl CastAsThreadImpl(UIntPtr ptr);


        public RT.ThreadImpl CurrentThread
        {
            get
            {
                UIntPtr threadHandle = UIntPtr.Zero;

                if(LLOS.LlilumErrors.Succeeded( LLOS.HAL.Thread.LLOS_THREAD_GetCurrentThread( ref threadHandle ) ))
                {
                    return CastAsThreadImpl(threadHandle);
                }

                return null;
            }
        }
    }
}
