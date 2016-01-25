//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.Win32.SmartHandles
{
    using System;
    using LLOS = LlilumOSAbstraction;
    using ISA = TargetModel.Win32.InstructionSetVersion;

    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState), PlatformFamilyFilter = ISA.Platform_Family__Win32 )]
    public struct InterruptState : IDisposable
    {
        //
        // State
        //
        private static UIntPtr s_globalMutex = UIntPtr.Zero;
        private const uint c_ActionDisable = 0;
        private const uint c_ActionEnable = 1;

        private uint m_action;
        private uint m_state;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptState( uint basepri )
        {
            m_action = basepri == 0 ? c_ActionDisable : c_ActionEnable;
            m_state = m_action;

            if(s_globalMutex == UIntPtr.Zero)
            {
                InterruptState.Initialize( );
            }

            if(m_action == c_ActionDisable)
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Acquire( s_globalMutex, -1 ), false );
            }
            else
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Release( s_globalMutex ), false );
            }
        }

        //
        // Helper Methods
        //

        public static void Initialize( )
        {
            if(s_globalMutex == UIntPtr.Zero)
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Mutex.LLOS_MUTEX_CreateGlobalLock(ref s_globalMutex), false);
            }
        }

        public static bool AreInterruptsDisabled()
        {
            return ( s_globalMutex == UIntPtr.Zero ) || ( 1 == LLOS.HAL.Mutex.LLOS_MUTEX_CurrentThreadHasLock(s_globalMutex) );
        }

        [Inline]
        public void Dispose()
        {
            if(m_action == m_state)
            {
                if(m_action == c_ActionDisable)
                {
                    LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Release( s_globalMutex ), false );
                }
                else
                {
                    LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Acquire( s_globalMutex, -1 ), false );
                }
            }
        }

        [Inline]
        public void Toggle()
        {
            BugCheck.AssertInterruptsOff( );

            LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Release( s_globalMutex ), false );
            LLOS.HAL.Thread.LLOS_THREAD_Yield( );
            LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Acquire( s_globalMutex, -1 ), false );
        }

        public void SwitchState()
        {
            if(m_state == c_ActionDisable)
            {
                m_state = c_ActionEnable;
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Release( s_globalMutex ), false );
            }
            else
            {
                m_state = c_ActionDisable;
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Mutex.LLOS_MUTEX_Acquire( s_globalMutex, -1 ), false );
            }
        }

        //--//

        [Inline]
        public static InterruptState Disable()
        {
            return new InterruptState( 0 );
        }

        [Inline]
        public static InterruptState DisableAll( )
        {
            return new InterruptState( 0 );
        }

        [Inline]
        public static InterruptState Enable()
        {
            return new InterruptState( 1 );
        }

        [Inline]
        public static InterruptState EnableAll( )
        {
            return new InterruptState( 1 );
        }

        public HardwareException GetCurrentExceptionMode()
        {
            return HardwareException.None;
        }

    }
}
