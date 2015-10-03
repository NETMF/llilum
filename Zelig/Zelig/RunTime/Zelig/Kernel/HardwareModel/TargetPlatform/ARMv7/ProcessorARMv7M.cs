//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public abstract partial class ProcessorARMv7M : Runtime.Processor
    {
        public enum IRQn_Type : int
        {
            //  Cortex-M Processor IRQ number, from cmsis implementation
            Reset_IRQn              = -42,      /*!< Exception#: 1 Reset (not actually defined as an IRQn_Type)   */
            NonMaskableInt_IRQn     = -14,      /*!< Exception#: 2 Non Maskable Interrupt                         */
            HardFault_IRQn          = -13,      /*!< Exception#: 3 Non Maskable Interrupt                         */
            MemoryManagement_IRQn   = -12,      /*!< Exception#: 4 Cortex-M3 Memory Management Interrupt          */
            BusFault_IRQn           = -11,      /*!< Exception#: 5 Cortex-M3 Bus Fault Interrupt                  */
            UsageFault_IRQn         = -10,      /*!< Exception#: 6 Cortex-M3 Usage Fault Interrupt                */
            Reservedr_IRQn9         = -9,       /*!< Exception#: 7 Reserved                                       */
            Reservedr_IRQn8         = -8,       /*!< Exception#: 8 Reserved                                       */
            Reservedr_IRQn7         = -7,       /*!< Exception#: 9 Reserved                                       */
            Reservedr_IRQn6         = -6,       /*!< Exception#: 10 Reserved                                      */
            SVCall_IRQn             = -5,       /*!< Exception#: 11 Cortex-M3 SV Call Interrupt                   */
            DebugMonitor_IRQn       = -4,       /*!< Exception#: 12 Cortex-M3 Debug Monitor Interrupt             */
            Reservedr_IRQn3         = -3,       /*!< Exception#: 13 Reserved                                      */
            PendSV_IRQn             = -2,       /*!< Exception#: 14 Cortex-M3 Pend SV Interrupt                   */
            SysTick_IRQn            = -1,       /*!< Exception#: 15 Cortex-M3 System Tick Interrupt               */
            //--//
            AnyInterrupt16          =  0, 
        }
       
        public enum ISR_NUMBER : uint
        {
            //  Cortex-M Processor exception Numbers, as reported by the IPSR
            ThreadMode          =   0,
            Reset               =   1,
            NMI                 =   2,
            HardFault           =   3,
            MemManage           =   4,
            BusFault            =   5,
            UsageFault          =   6,
            Reserved7           =   7,
            Reserved8           =   8,
            Reserved9           =   9,
            Reserved10          =  10,
            SVCall              =  11,
            ReservedForDebug    =  12,
            Reserved13          =  13,
            PendSV              =  14,
            SysTick             =  15,
            //--//
            Peripheral          =  16, 
            Last                = 240, 
        }
        
        public enum SVC_Code : byte
        {
            SupervisorCall__LongJump        = 0x11,
            SupervisorCall__StartThreads    = 0x12,
            SupervisorCall__RetireThread    = 0x13,
        }

        //
        // Exception priorities
        //
        
        public const byte c_Priority__MASK                  = 0x000000FF;
        public const byte c_Priority__Highest               = 0x00000001;
        public const byte c_Priority__Lowest                = 0x000000FF;
        public const byte c_Priority__HigherThanAnyWeOwn    = 0x00000004;
        public const byte c_Priority__SVCCall               = 0x00000007;
        public const byte c_Priority__SystemTimer           = 0x00000007;
        public const byte c_Priority__SysTick               = 0x00000007;
        public const byte c_Priority__PendSV                = 0x0000000E;

        //--//

        #region Registers and Fields Definitions

        //
        // Interrupts management
        // In the simplest case, we will use the PRIMASK against a BASEPRI of 0x00000000, which is the reset value 
        //
        public const uint c_PRIMASK__InterruptsOff      = 0x00000001;
        public const uint c_PRIMASK__InterruptsOn       = 0x00000000;
        public const uint c_FAULTMASK__FaultsOff        = 0x00000001;
        public const uint c_FAULTMASK__FaultsOn         = 0x00000000;
        public const uint c_FAULTMASK__AreFaultsOff     = c_FAULTMASK__FaultsOff << 1;

        //
        // PSR register
        //

        public const uint c_psr_InitialValue            = 0x01000000; // just Thumb state set

        //
        // CONTROL register
        //
        
        public const uint c_CONTROL__MASK               = 0x00000003;
        public const uint c_CONTROL__MASK_PRIVILEGE     = 0x00000001;

        //
        // stack pointer
        //
        public const  int c_CONTROL__SPSEL_SHIFT        = 1;
        public const uint c_CONTROL__SPSEL_MASK         = 0x1u << c_CONTROL__SPSEL_SHIFT;
        public const uint c_CONTROL__SPSEL_MSP          = 0x0u << c_CONTROL__SPSEL_SHIFT;
        public const uint c_CONTROL__SPSEL_PSP          = 0x1u << c_CONTROL__SPSEL_SHIFT;

        //
        // privilege
        //
        public const  int c_CONTROL__nPRIV_SHIFT        = 0;
        public const uint c_CONTROL__nPRIV_MASK         = 0x1u << c_CONTROL__nPRIV_SHIFT;
        public const uint c_CONTROL__nPRIV_PRIV         = 0x0u << c_CONTROL__nPRIV_SHIFT;
        public const uint c_CONTROL__nPRIV_UNPRIV       = 0x1u << c_CONTROL__nPRIV_SHIFT;
        
        //
        // Modes
        // 
        public const uint c_CONTROL__MODE__HNDLR        = c_CONTROL__SPSEL_MSP | c_CONTROL__nPRIV_PRIV;
        public const uint c_CONTROL__MODE__THRD_PRIV    = c_CONTROL__SPSEL_PSP | c_CONTROL__nPRIV_PRIV;
        public const uint c_CONTROL__MODE__THRD_UNPRIV  = c_CONTROL__SPSEL_PSP | c_CONTROL__nPRIV_UNPRIV;

        //
        // EXC_RETURN 
        //
        public const uint c_MODE_RETURN__HANDLER_MSP    = 0xFFFFFFF1; // handler will return in handler mode using the MSP
        public const uint c_MODE_RETURN__THREAD_MSP     = 0xFFFFFFF9; // handler will return in thread mode using the MSP
        public const uint c_MODE_RETURN__THREAD_PSP     = 0xFFFFFFFD; // handler will return in thread mode using the PSP
        
        //
        // SCR
        //
        public const uint c_SCR__MASK                   = 0x0000001F;

        public const  int c_SCR__SLEEPONEXIT__SHIFT     = 1; 
        public const uint c_SCR__SLEEPONEXIT__MASK      = 1u << c_SCR__SLEEPONEXIT__SHIFT;
        public const uint c_SCR__SLEEPONEXIT__NO_SLEEP  = 0u << c_SCR__SLEEPONEXIT__SHIFT; 
        public const uint c_SCR__SLEEPONEXIT__SLEEP     = 1u << c_SCR__SLEEPONEXIT__SHIFT;

        public const  int c_SCR__SLEEPDEEP__SHIFT       = 2; 
        public const uint c_SCR__SLEEPDEEP__MASK        = 1u << c_SCR__SLEEPDEEP__SHIFT;
        public const uint c_SCR__SLEEPDEEP__SLEEP       = 0u << c_SCR__SLEEPDEEP__SHIFT; 
        public const uint c_SCR__SLEEPDEEP__DEEP        = 1u << c_SCR__SLEEPDEEP__SHIFT;  

        public const  int c_SCR__SEVONPEND__SHIFT       = 4; 
        public const uint c_SCR__SEVONPEND__MASK        = 1u << c_SCR__SEVONPEND__SHIFT;
        public const uint c_SCR__SEVONPEND__ENONLY      = 0u << c_SCR__SEVONPEND__SHIFT; 
        public const uint c_SCR__SEVONPEND__ALL         = 1u << c_SCR__SEVONPEND__SHIFT; 

        //
        // CCR
        //
        public const uint c_CCR__MASK                   = 0x000003FF;
        
        public const  int c_CCR__NONBASETHRDENA__SHIFT  = 0; 
        public const uint c_CCR__NONBASETHRDENA__MASK   = 1u << c_CCR__NONBASETHRDENA__SHIFT;
        public const uint c_CCR__NONBASETHRDENA__NO_EX  = 0u << c_CCR__NONBASETHRDENA__SHIFT; 
        public const uint c_CCR__NONBASETHRDENA__ANY    = 1u << c_CCR__NONBASETHRDENA__SHIFT;

        public const  int c_CCR__UNALIGN_TRP__SHIFT     = 3; 
        public const uint c_CCR__UNALIGN_TRP__MASK      = 1u << c_CCR__UNALIGN_TRP__SHIFT;
        public const uint c_CCR__UNALIGN_TRP__NOTRAP    = 0u << c_CCR__UNALIGN_TRP__SHIFT; 
        public const uint c_CCR__UNALIGN_TRP__TRAP      = 1u << c_CCR__UNALIGN_TRP__SHIFT;

        public const  int c_CCR__DIV_0_TRP__SHIFT       = 4; 
        public const uint c_CCR__DIV_0_TRP__MASK        = 1u << c_CCR__DIV_0_TRP__SHIFT;
        public const uint c_CCR__DIV_0_TRP__NOTRAP      = 0u << c_CCR__DIV_0_TRP__SHIFT; 
        public const uint c_CCR__DIV_0_TRP__TRAP        = 1u << c_CCR__DIV_0_TRP__SHIFT;

        public const  int c_CCR__STKALIGN__SHIFT        = 9; 
        public const uint c_CCR__STKALIGN__MASK         = 1u << c_CCR__STKALIGN__SHIFT;
        public const uint c_CCR__STKALIGN__4            = 0u << c_CCR__STKALIGN__SHIFT; 
        public const uint c_CCR__STKALIGN__8            = 1u << c_CCR__STKALIGN__SHIFT;
        
        public const uint c_CCR_STD_CONFIG_4            = (                     c_CCR__DIV_0_TRP__TRAP | c_CCR__UNALIGN_TRP__TRAP | c_CCR__NONBASETHRDENA__ANY) & c_CCR__MASK; 
        public const uint c_CCR_STD_CONFIG_8            = (c_CCR__STKALIGN__8 | c_CCR__DIV_0_TRP__TRAP | c_CCR__UNALIGN_TRP__TRAP | c_CCR__NONBASETHRDENA__ANY) & c_CCR__MASK; 

        //
        // ICSR
        //
        public const uint c_ICSR__MASK                  = 0xFFFFFFFF;
        public const uint c_ICSR__ALLOWED_MASK          = 0x9E400000;
        
        public const  int c_ICSR__PENDSTCLR__SHIFT      = 25;
        public const uint c_ICSR__PENDSTCLR__MASK       = 1u << c_ICSR__PENDSTCLR__SHIFT;
        public const uint c_ICSR__PENDSTCLR__SET        = 1u << c_ICSR__PENDSTCLR__SHIFT;
    
        public const  int c_ICSR__PENDSTSET__SHIFT      = 26;
        public const uint c_ICSR__PENDSTSET__MASK       = 1u << c_ICSR__PENDSTSET__SHIFT;
        public const uint c_ICSR__PENDSTSET__SET        = 1u << c_ICSR__PENDSTSET__SHIFT;
        
        public const  int c_ICSR__PENDSVCLR__SHIFT      = 27;
        public const uint c_ICSR__PENDSVCLR__MASK       = 1u << c_ICSR__PENDSVCLR__SHIFT;
        public const uint c_ICSR__PENDSVCLR__SET        = 1u << c_ICSR__PENDSVCLR__SHIFT;

        public const  int c_ICSR__PENDSVSET__SHIFT      = 28;
        public const uint c_ICSR__PENDSVSET__MASK       = 1u << c_ICSR__PENDSVSET__SHIFT;
        public const uint c_ICSR__PENDSVSET__SET        = 1u << c_ICSR__PENDSVSET__SHIFT;

        public const  int c_ICSR__NMIPENDSET__SHIFT     = 31;
        public const uint c_ICSR__NMIPENDSET__MASK      = 1u << c_ICSR__NMIPENDSET__SHIFT;
        public const uint c_ICSR__NMIPENDSET__SET       = 1u << c_ICSR__NMIPENDSET__SHIFT;

        //
        // SHCSR
        //
        public const uint c_SHCSR__MASK                 = 0xFFF7FD8B;
        
        public const  int c_SHCSR__MEMFAULTENA__SHIFT   = 16;
        public const uint c_SHCSR__MEMFAULTENA__MASK    = 1u << c_SHCSR__MEMFAULTENA__SHIFT;
        public const uint c_SHCSR__MEMFAULTENA__ENABLE  = 1u << c_SHCSR__MEMFAULTENA__SHIFT;
        public const uint c_SHCSR__MEMFAULTENA__DISABLE = 0u << c_SHCSR__MEMFAULTENA__SHIFT;

        public const  int c_SHCSR__BUSFAULTENA__SHIFT   = 17;
        public const uint c_SHCSR__BUSFAULTENA__MASK    = 1u << c_SHCSR__BUSFAULTENA__SHIFT;
        public const uint c_SHCSR__BUSFAULTENA__ENABLE  = 1u << c_SHCSR__BUSFAULTENA__SHIFT;
        public const uint c_SHCSR__BUSFAULTENA__DISABLE = 0u << c_SHCSR__BUSFAULTENA__SHIFT;
        
        public const  int c_SHCSR__USGFAULTENA__SHIFT   = 18;
        public const uint c_SHCSR__USGFAULTENA__MASK    = 1u << c_SHCSR__USGFAULTENA__SHIFT;
        public const uint c_SHCSR__USGFAULTENA__ENABLE  = 1u << c_SHCSR__USGFAULTENA__SHIFT;
        public const uint c_SHCSR__USGFAULTENA__DISABLE = 0u << c_SHCSR__USGFAULTENA__SHIFT;
        
        //--//
                    
        public const int  c_SCB_SHCSR_USGFAULTENA__SHIFT = 18;
        public const uint c_SCB_SHCSR_USGFAULTENA__SET   = 1u << c_SCB_SHCSR_USGFAULTENA__SHIFT;

        public const int  c_SCB_SHCSR_BUSFAULTENA__SHIFT = 17;
        public const uint c_SCB_SHCSR_BUSFAULTENA__SET   = 1u << c_SCB_SHCSR_BUSFAULTENA__SHIFT;

        public const int  c_SCB_SHCSR_MEMFAULTENA__SHIFT = 16;
        public const uint c_SCB_SHCSR_MEMFAULTENA__SET   = 1u << c_SCB_SHCSR_MEMFAULTENA__SHIFT;
        
        #endregion 

        //--//

        [TS.WellKnownType( "Microsoft_Zelig_ARMv7_MethodWrapper" )]
        public sealed class MethodWrapper : AbstractMethodWrapper
        {

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public override void Prologue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Prologue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  HardwareException he )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public override void Epilogue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Epilogue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  HardwareException he )
            {

            }

        }

        //
        // Helper Methods
        //

        public override void InitializeProcessor( )
        {
            //
            // We want to run ISRs in privilged Handler mode using the Main Stack Pointer and all the other tasks 
            // in privileged Thread mode using the Process Stack Pointer. 
            //
            // We will assume that native context switching is possible when processor initialization is carried out in
            // Handler/Privileged mode. Of course that is not a complete guarantee. After carrying out the initailization of the 
            // idle thread task, we will let the initialization thread return to thread mode upon first context switch as per classic 
            // technique mentioned below, from ARM reference manual. As switching the mode is carried out naturally by 
            // setting the appropriate flag, there is nothing else we need to do at initialization time. See context switch code 
            // in thread manager as well.

            //
            // From ARMv-7M Architecture Reference Manual. 
            // 
            // 
            // By default, Thread mode uses the MSP. To switch the stack pointer used in Thread mode to the
            // PSP, either:
            // - use the MSR instruction to set the Active stack pointer bit to 1
            // - perform an exception return to Thread mode with the appropriate EXC_RETURN value
            //

            ////// 
            //////  
            ////// 
            //////  Table 2-17 Exception return behavior
            //////
            //////  EXC_RETURN      Description
            //////  =========================================================================
            //////  0xFFFFFF[F|E]1  Return to Handler mode.
            //////                  Exception return gets state [and FP state] from the main 
            //////                  stack (MSP).
            //////                  Execution uses MSP after return.
            ////// 
            //////  0xFFFFFF[F|E]9  Return to Thread mode.
            //////                  Exception Return get state [and FP state] from the main 
            //////                  stack (MSP).
            //////                  Execution uses MSP after return.
            ////// 
            //////  0xFFFFFF[F|E]D  Return to Thread mode.
            //////                  Exception return gets state [and FP state] from the process 
            //////                  stack (PSP).
            //////                  Execution uses PSP after return.
            ////// 
            //////  All other values Reserved.
            ////// 
            
            //
            // Disable interrupts, but not faults 
            //
            DisableInterrupts( );

            //
            // Enforce reset behavior: 
            // - only enabled interrupts or events can wakeup the processor, disabled interrupts are excluded
            // - sleep by turning off clock to main proc (not deep) 
            // - do not sleep when returning to thread mode
            //
            SetSystemControlRegister(   c_SCR__SLEEPONEXIT__NO_SLEEP    |  
                                        c_SCR__SLEEPDEEP__SLEEP         | 
                                        c_SCR__SEVONPEND__ENONLY        ); 
            
            // Enforce 8 bytes alignment
            //
            Set_8_BytesAlignment( );
        }
                
        internal static void Set_8_BytesAlignment( )
        {
            CUSTOM_STUB_SCB_set_CCR( c_CCR_STD_CONFIG_8 );
        }

        internal static void RaiseSystemHandler( uint mask )
        {
            BugCheck.Assert( ((mask & ~(c_ICSR__ALLOWED_MASK)) == 0), BugCheck.StopCode.IncorrectArgument );

            CUSTOM_STUB_SCB_ICSR_RaiseSystemException( mask );
        }

        public static void RaiseSupervisorCall( SVC_Code code )
        {
            switch(code)
            {
                case SVC_Code.SupervisorCall__LongJump:
                    CUSTOM_STUB_RaiseSupervisorCallForLongJump( );
                    break;
                case SVC_Code.SupervisorCall__StartThreads:
                    CUSTOM_STUB_RaiseSupervisorCallForStartThreads( ); 
                    break;
                 case SVC_Code.SupervisorCall__RetireThread:
                     CUSTOM_STUB_RaiseSupervisorCallForRetireThread( );
                    break;
                default:
                    throw new ArgumentException( "Request SVC does not exists or is not supported" ); 
            }
        }
        
        public static bool IsAnyExceptionActive( )
        {
            return ( ( (ISR_NUMBER)CUSTOM_STUB_SCB_IPSR_GetCurrentISRNumber( ) ) != ISR_NUMBER.ThreadMode ); 
        }
        
        public static void InitiateContextSwitch( )
        {
            RaiseSystemHandler( c_ICSR__PENDSTSET__SET ) ;
        }

        public static void CompleteContextSwitch( )
        {
            RaiseSystemHandler( c_ICSR__PENDSVSET__SET ) ;
        }

        internal void SetSystemControlRegister( uint flags )
        {
            CUSTOM_STUB_SCB_SCR_SetSystemControlRegister( flags );
        }

        public static void WaitForEvent( )
        {
            CMSIS_STUB_POWER_WaitForEvent( ); 
        }

        public static void SendEvent( )
        {
            CMSIS_STUB_POWER_SendEvent( ); 
        }

        public static void WaitForInterrupt( )
        {
            CMSIS_STUB_POWER_WaitForInterrupt( ); 
        }

        //--//


        private static void EnableSystemHandler( IRQn_Type ex )
        {
            BugCheck.Assert( (ex < 0), BugCheck.StopCode.IncorrectArgument );

            uint mask = 0;

            switch(ex)
            {
                case IRQn_Type.Reset_IRQn         :
                case IRQn_Type.NonMaskableInt_IRQn:
                case IRQn_Type.HardFault_IRQn     :  
                    // Cannot enable or disable NMI, Reset or HardFault
                    //BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
                    break;
                case IRQn_Type.SVCall_IRQn:
                case IRQn_Type.DebugMonitor_IRQn:  
                    // Not implemented, we should never do this
                    //BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
                    break;
                case IRQn_Type.PendSV_IRQn:
                case IRQn_Type.SysTick_IRQn:
                    // NOT IMPLEMENTED: call NVIC
                    //BugCheck.Assert( false, BugCheck.StopCode.IncorrectArgument );
                    break;
                case IRQn_Type.MemoryManagement_IRQn:
                    mask = c_SHCSR__MEMFAULTENA__ENABLE;
                    break;
                case IRQn_Type.BusFault_IRQn:
                    mask = c_SHCSR__BUSFAULTENA__ENABLE;
                    break;
                case IRQn_Type.UsageFault_IRQn:
                    mask = c_SHCSR__USGFAULTENA__ENABLE;
                    break;
                default:
                    BugCheck.Assert( false, BugCheck.StopCode.IncorrectArgument );
                    break;
            }

            CUSTOM_STUB_SCB_SHCRS_EnableSystemHandler( mask );
        }
        
        private static void DisableSystemHandler( IRQn_Type ex )
        {
            BugCheck.Assert( (ex < 0), BugCheck.StopCode.IncorrectArgument );
            
            uint mask = 0;

            switch(ex)
            {
                case IRQn_Type.Reset_IRQn         :
                case IRQn_Type.NonMaskableInt_IRQn:
                case IRQn_Type.HardFault_IRQn     :  
                    // Cannot enable or disable NMI, Reset or HardFault
                    //BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
                    break;
                case IRQn_Type.SVCall_IRQn:
                case IRQn_Type.DebugMonitor_IRQn:  
                    // Not implemented, we should never do this
                    //BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
                    break;
                case IRQn_Type.PendSV_IRQn:
                case IRQn_Type.SysTick_IRQn:
                    // NOT IMPLEMENTED: call NVIC
                    //BugCheck.Assert( false, BugCheck.StopCode.IncorrectArgument );
                    break;
                case IRQn_Type.MemoryManagement_IRQn:
                    mask = c_SHCSR__MEMFAULTENA__DISABLE;
                    break;
                case IRQn_Type.BusFault_IRQn:
                    mask = c_SHCSR__BUSFAULTENA__DISABLE;
                    break;
                case IRQn_Type.UsageFault_IRQn:
                    mask = c_SHCSR__USGFAULTENA__DISABLE;
                    break;
                default:
                    BugCheck.Assert( false, BugCheck.StopCode.IncorrectArgument );
                    break;
            }

            CUSTOM_STUB_SCB_SHCRS_DisableSystemHandler( mask );
        }
        
        //
        // Cache
        // 

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            // Cortex-M7 actually has a cache, so override the method
            return ptr;
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            // Cortex-M7 actually has a cache, so override the method
            return ptr;
        }    
   
        public override void FlushCacheLine( UIntPtr target )
        {
        }

        //--//
        
        [Inline]
        public override bool AreInterruptsDisabled( )
        {
            uint primask = CMSIS_STUB_SCB__get_PRIMASK( );

            bool disabledByPrimask = ( primask & c_PRIMASK__InterruptsOff ) == c_PRIMASK__InterruptsOff;

            bool disabledByPriority = (GetBasePriRegister() <= c_Priority__Highest);

            return disabledByPrimask | disabledByPriority;

            //return disabledByPriority;
        }

        [Inline]
        public override bool AreFaultsDisabled( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK( );

            return ( faultmask & c_FAULTMASK__FaultsOff ) == c_FAULTMASK__FaultsOff;
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( ) && AreFaultsDisabled( ); 
        }
        
        public override void Breakpoint( )
        {
            Breakpoint( 0xa5a5a5a5 ); 
        }
        
        //--//--//

        public static uint EnableInterrupts( )
        {
            uint basepri = GetBasePriRegister( );

            DisableInterruptsWithPriorityLevelHigherOrEqualTo( c_Priority__Lowest );

            return basepri; 
        }
        
        public static uint DisableInterrupts( )
        {
            uint basepri = GetBasePriRegister( );

            DisableInterruptsWithPriorityLevelHigherOrEqualTo( c_Priority__Highest );

            return basepri; 
        }
        
        public static void DisableInterruptsByPrimask()
        {
            CMSIS_STUB_SCB__set_PRIMASK( c_PRIMASK__InterruptsOff ); 
        }
        public static void EnableInterruptsByPrimask()
        {
            CMSIS_STUB_SCB__set_PRIMASK( c_PRIMASK__InterruptsOn ); 
        }

        public static uint EnableFaults( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK();
            
            CMSIS_STUB_SCB__Enable_Fault_Irq();

            return faultmask << 1; 
        }

        public static uint DisableFaults( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK( );
            
            CMSIS_STUB_SCB__Disable_Fault_Irq(); 

            // since faults and interrupts have the same mask we need to differentiate 
            return faultmask << 1;
        }
        
        public static uint EnableAllInterrupts( )
        {
            return EnableInterrupts( ) | EnableFaults( );
        }

        public static uint DisableAllInterrupts( )
        {
            return DisableInterrupts( ) | DisableFaults( );
        }

        public static void DisableInterruptsWithPriorityLevelHigherOrEqualTo( uint basepri )
        {
            SetBasePriRegister( basepri );
        }

        public static bool VerifyHandlerMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );
            
            return ((control & c_CONTROL__SPSEL_MASK) == c_CONTROL__SPSEL_MSP);
        }
        
        //--//
        
        [Inline]
        internal static uint GetBasePriRegister( )
        {
            return CMSIS_STUB_SCB__get_BASEPRI( );
        }

        [Inline]
        internal static void SetBasePriRegister( uint basepri )
        {
            CMSIS_STUB_SCB__set_BASEPRI( basepri );
        }

        //--//

        [Inline]
        protected static UIntPtr GetMainStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_MSP() );
        }
            
        [Inline]
        protected static void SetMainStackPointer( UIntPtr topOfMainStack )
        {
            CMSIS_STUB_SCB__set_MSP( topOfMainStack.ToUInt32() );
        }
            
        [Inline]
        protected static UIntPtr GetProcessStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_PSP( ) );
        }
            
        [Inline]
        protected static void SetProcessStackPointer( UIntPtr topOfProcessStack )
        {
            CMSIS_STUB_SCB__set_PSP( topOfProcessStack.ToUInt32() );
        }
        
        protected static void SwitchToHandlerPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );
            
            control &= ~c_CONTROL__MASK; 
            control |=  c_CONTROL__MODE__HNDLR;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }
        
        protected static void SwitchToThreadUnprivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );
            
            control &= ~c_CONTROL__MASK; 
            control |= c_CONTROL__MODE__THRD_UNPRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }
        
        protected static void SwitchToThreadPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );
            
            control &= ~c_CONTROL__MASK; 
            control |= c_CONTROL__MODE__THRD_PRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }
        
        protected static void SwitchToPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK_PRIVILEGE; 
            control |=  c_CONTROL__nPRIV_PRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }
        
        protected static void SwitchToUnprivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );
            
            control &= ~c_CONTROL__MASK_PRIVILEGE; 
            control |=  c_CONTROL__nPRIV_UNPRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        protected static void SetExcReturn( uint ret )
        {
            CUSTOM_STUB_SetExcReturn( ret );
        }
        
        //--//
         
        //
        // We will implement the intrernal methods below with CMSIS-Core
        //

        // TODO: do we need well-known methods?
        
        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_CONTROL( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_CONTROL( uint control );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_IPSR( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_APSR( );


        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_xPSR( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_PSP( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_PSP( uint topOfProcStack );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_MSP( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_MSP( uint topOfMainStack );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_PRIMASK( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_PRIMASK( uint priMask );
        
        
        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Enable_Irq( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Disable_Irq( );

        //
        // !!! Cortex M3 only !!!
        //
        
        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Enable_Fault_Irq( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Disable_Fault_Irq( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_BASEPRI( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_BASEPRI( uint basePri );
        
        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_FAULTMASK( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__set_FAULTMASK( uint faultMask );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_set_CCR( uint value );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_SHCRS_EnableSystemHandler( uint ex );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_SHCRS_DisableSystemHandler( uint ex );
        
        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_ICSR_RaiseSystemException( uint ex );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_RaiseSupervisorCallForLongJump( );
 
        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_RaiseSupervisorCallForStartThreads( );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_RaiseSupervisorCallForRetireThread( );
        
        [DllImport( "C" )]
        private static extern void CUSTOM_STUB_SetExcReturn( uint ret );
        
        [DllImport( "C" )]
        internal static extern int CUSTOM_STUB_SCB_IPSR_GetCurrentISRNumber( );

        [DllImport( "C" )]
        internal static extern int CUSTOM_STUB_SCB_SCR_GetSystemControlRegister( );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_SCR_SetSystemControlRegister( uint scr ); 

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_POWER_WaitForEvent( ); 

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_POWER_SendEvent( ); 

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_POWER_WaitForInterrupt( ); 

        //--//
        //--//
        //--//
        
        //
        // Utility methods 
        //

        //[TS.WellKnownMethod( "ProcessorARM_Breakpoint" )]

        [DllImport( "C" )]
        public static extern void Breakpoint( uint value );

        [DllImport( "C" )]
        public static extern void Nop( );
    }
}
