//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define SPIN_ON_SLEEP

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using RT = Microsoft.Zelig.Runtime;


    public abstract partial class ProcessorARMv7M : RT.Processor
    {
        public enum IRQn_Type : int
        {
            //  Cortex-M Processor IRQ number, from cmsis implementation
            Reset_IRQn              = -42,      /*!< Exception#: 1 Reset (not actually defined as an IRQn_Type)   */
            NonMaskableInt_IRQn     = -14,      /*!< Exception#: 2 Non Maskable Interrupt                         */
            HardFault_IRQn          = -13,      /*!< Exception#: 3 Non Maskable Interrupt                         */
            MemoryManagement_IRQn   = -12,      /*!< Exception#: 4 Cortex-M3/4 Memory Management Interrupt        */
            BusFault_IRQn           = -11,      /*!< Exception#: 5 Cortex-M3/4 Bus Fault Interrupt                */
            UsageFault_IRQn         = -10,      /*!< Exception#: 6 Cortex-M3/4 Usage Fault Interrupt              */
            Reserved_IRQn9          = -9,       /*!< Exception#: 7 Reserved                                       */
            Reserved_IRQn8          = -8,       /*!< Exception#: 8 Reserved                                       */
            Reserved_IRQn7          = -7,       /*!< Exception#: 9 Reserved                                       */
            Reserved_IRQn6          = -6,       /*!< Exception#: 10 Reserved                                      */
            SVCall_IRQn             = -5,       /*!< Exception#: 11 Cortex-M3/4 SV Call Interrupt                 */
            DebugMonitor_IRQn       = -4,       /*!< Exception#: 12 Cortex-M3/4 Debug Monitor Interrupt           */
            Reserved_IRQn3          = -3,       /*!< Exception#: 13 Reserved                                      */
            PendSV_IRQn             = -2,       /*!< Exception#: 14 Cortex-M3/4 Pend SV Interrupt                 */
            SysTick_IRQn            = -1,       /*!< Exception#: 15 Cortex-M3/4 System Tick Interrupt             */
            //--//
            AnyInterrupt16          =  0,
            
            //--//

            Invalid                 = 0xFFFF,
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
            SupervisorCall__LongJump = 0x11,
            SupervisorCall__StartThreads = 0x12,
            SupervisorCall__RetireThread = 0x13,
            SupervisorCall__SnapshotProcessModeRegisters = 0x14,
        }


        //
        // Exception priorities
        //

        public const uint c_Priority__MASK                  = 0x000000FFu;
        public const uint c_Priority__NeverDisabled         = 0x00000000u;
        public const uint c_Priority__Highest               = 0x00000001u;
        public const uint c_Priority__Lowest                = 0x000000FFu;
        public const uint c_Priority__HigherThanAnyWeOwn    = 0x00000004u;
        public const uint c_Priority__SVCCall               = 0x00000005u;
        public const uint c_Priority__Default               = 0x00000007u;
        public const uint c_Priority__SystemTimer           = c_Priority__Default;
        public const uint c_Priority__SysTick               = c_Priority__Default;
        public const uint c_Priority__GenericPeripherals    = c_Priority__Default + 1;
        public const uint c_Priority__PendSV                = 0x0000000Eu;

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

        public const uint c_CCR_STD_CONFIG_4            = (c_CCR__STKALIGN__4 | c_CCR__DIV_0_TRP__TRAP | c_CCR__UNALIGN_TRP__NOTRAP | c_CCR__NONBASETHRDENA__ANY) & c_CCR__MASK;
        public const uint c_CCR_STD_CONFIG_8            = (c_CCR__STKALIGN__8 | c_CCR__DIV_0_TRP__TRAP | c_CCR__UNALIGN_TRP__NOTRAP | c_CCR__NONBASETHRDENA__ANY) & c_CCR__MASK;

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
        public const uint c_SCB_SHCSR_USGFAULTENA__MASK  = 1u << c_SCB_SHCSR_USGFAULTENA__SHIFT;
        public const uint c_SCB_SHCSR_USGFAULTENA__SET   = 1u << c_SCB_SHCSR_USGFAULTENA__SHIFT;

        public const int  c_SCB_SHCSR_BUSFAULTENA__SHIFT = 17;
        public const uint c_SCB_SHCSR_BUSFAULTENA__MASK  = 1u << c_SCB_SHCSR_BUSFAULTENA__SHIFT;
        public const uint c_SCB_SHCSR_BUSFAULTENA__SET   = 1u << c_SCB_SHCSR_BUSFAULTENA__SHIFT;

        public const int  c_SCB_SHCSR_MEMFAULTENA__SHIFT = 16;
        public const uint c_SCB_SHCSR_MEMFAULTENA__SET   = 1u << c_SCB_SHCSR_MEMFAULTENA__SHIFT;

        //--//

        public const int  c_SCB_HFSR_FORCED_SHIFT        = 30;
        public const uint c_SCB_HFSR_FORCED_MASK         = 1u << c_SCB_HFSR_FORCED_SHIFT;
        public const uint c_SCB_HFSR_FORCED_FORCED       = 1u << c_SCB_HFSR_FORCED_SHIFT;  // escalated hard fault

        public const int  c_SCB_HFSR_VECTTBL_SHIFT       = 1;
        public const uint c_SCB_HFSR_VECTTBL_MASK        = 1u << c_SCB_HFSR_VECTTBL_SHIFT;
        public const uint c_SCB_HFSR_VECTTBL_READ        = 1u << c_SCB_HFSR_VECTTBL_SHIFT; // fault on vector table read

        //--//

        public const int  c_SCB_CFSR_MEMFAULT_SHIFT      = 0;
        public const uint c_SCB_CFSR_MEMFAULT_MASK       = 0xFFu << c_SCB_CFSR_MEMFAULT_SHIFT;
        public const uint c_SCB_CFSR_MEMFAULT_IACCVIOL   = 0x01u << c_SCB_CFSR_MEMFAULT_SHIFT; // instruction access violation
        public const uint c_SCB_CFSR_MEMFAULT_DACCVIOL   = 0x02u << c_SCB_CFSR_MEMFAULT_SHIFT; // load or store at invalid address
        public const uint c_SCB_CFSR_MEMFAULT_MUNSTKERR  = 0x08u << c_SCB_CFSR_MEMFAULT_SHIFT; // memory access fault on unstacking 
        public const uint c_SCB_CFSR_MEMFAULT_MSTKERR    = 0x10u << c_SCB_CFSR_MEMFAULT_SHIFT; // memory access fault on stacking 
        public const uint c_SCB_CFSR_MEMFAULT_MLSPERR    = 0x20u << c_SCB_CFSR_MEMFAULT_SHIFT; // fault on lazy state preservation
        public const uint c_SCB_CFSR_MEMFAULT_MMFARVALID = 0x80u << c_SCB_CFSR_MEMFAULT_SHIFT;

        public const int  c_SCB_CFSR_BUSFAULT_SHIFT      = 8;
        public const uint c_SCB_CFSR_BUSFAULT_MASK       = 0xFFu << c_SCB_CFSR_BUSFAULT_SHIFT;
        public const uint c_SCB_CFSR_BUSFAULT_IBUSERR    = 0x01u << c_SCB_CFSR_BUSFAULT_SHIFT; // bus error on instruction fetch and attempted execution
        public const uint c_SCB_CFSR_BUSFAULT_PRECISERR  = 0x02u << c_SCB_CFSR_BUSFAULT_SHIFT; // stacked PC points to actual location of fault
        public const uint c_SCB_CFSR_BUSFAULT_IMPRCISERR = 0x04u << c_SCB_CFSR_BUSFAULT_SHIFT; // unknow fault location 
        public const uint c_SCB_CFSR_BUSFAULT_UNSTKERR   = 0x08u << c_SCB_CFSR_BUSFAULT_SHIFT; // bus fault on unstacking 
        public const uint c_SCB_CFSR_BUSFAULT_STKERR     = 0x10u << c_SCB_CFSR_BUSFAULT_SHIFT; // bus fault on stacking 
        public const uint c_SCB_CFSR_MEMFAULT_LSPERR     = 0x20u << c_SCB_CFSR_MEMFAULT_SHIFT; // fault on lazy state preservation
        public const uint c_SCB_CFSR_BUSFAULT_BFARVALID  = 0x80u << c_SCB_CFSR_BUSFAULT_SHIFT;

        public const int  c_SCB_CFSR_USGFAULT_SHIFT      = 16;
        public const uint c_SCB_CFSR_USGFAULT_MASK       = 0xFFFFu << c_SCB_CFSR_USGFAULT_SHIFT;
        public const uint c_SCB_CFSR_USGFAULT_UNDEFINSTR = 0x0001u << c_SCB_CFSR_USGFAULT_SHIFT; // undefined instruction
        public const uint c_SCB_CFSR_USGFAULT_INVSTATE   = 0x0002u << c_SCB_CFSR_USGFAULT_SHIFT; // instruction makes illegal use of the EPSR
        public const uint c_SCB_CFSR_USGFAULT_INVPC      = 0x0004u << c_SCB_CFSR_USGFAULT_SHIFT; // illegal load of EXC_RETURN to the PC, as a result of an invalid context, or an invalid EXC_RETURN value
        public const uint c_SCB_CFSR_USGFAULT_NOPC       = 0x0008u << c_SCB_CFSR_USGFAULT_SHIFT; // coprocessor access 
        public const uint c_SCB_CFSR_USGFAULT_UNALIGNED  = 0x0100u << c_SCB_CFSR_USGFAULT_SHIFT; // unaligned access
        public const uint c_SCB_CFSR_USGFAULT_DIVBYZERO  = 0x0200u << c_SCB_CFSR_USGFAULT_SHIFT; // divide by zero 

        public const uint c_COREDEBUG_DHCSR_CONNECTED    = 0x00000001;

        #endregion

        //--//

        //
        // Helper methods
        //

        public override void InitializeProcessor( )
        {
            //
            // We want to run ISRs in privileged Handler mode using the Main Stack Pointer and all the other tasks 
            // in privileged Thread mode using the Process Stack Pointer. 
            //
            // We will assume that native context switching is possible when processor initialization is carried out in
            // Handler/Privileged mode. Of course that is not a complete guarantee. After carrying out the initialization of the 
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
            // Ensure privileged Handler mode to boot
            //

            if(!VerifyHandlerMode( ))
            {
                RT.BugCheck.Log( "Cannot bootstrap in Thread mode" );
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.FailedBootstrap );
            }

            //
            // Enforce reset behavior: 
            // - only enabled interrupts or events can wakeup the processor, disabled interrupts are excluded
            // - sleep by turning off clock to main proc (not deep) 
            // - do not sleep when returning to thread mode
            //
            SetSystemControlRegister( c_SCR__SLEEPONEXIT__NO_SLEEP    |
                                        c_SCR__SLEEPDEEP__SLEEP         |
                                        c_SCR__SEVONPEND__ENONLY );

            // Enforce 8 bytes alignment
            //
            SetCCR( c_CCR_STD_CONFIG_4 );

            //
            // Enable system exceptions we intercept
            //
            EnableSystemHandler( IRQn_Type.HardFault_IRQn );
            EnableSystemHandler( IRQn_Type.BusFault_IRQn );
            EnableSystemHandler( IRQn_Type.MemoryManagement_IRQn );
            EnableSystemHandler( IRQn_Type.UsageFault_IRQn );
        }

        //--//

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
            return (GetBasePriRegister( ) <= c_Priority__Highest);
        }

        [Inline]
        public override bool AreFaultsDisabled( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK( );

            return (faultmask & c_FAULTMASK__FaultsOff) == c_FAULTMASK__FaultsOff;
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( ) && AreFaultsDisabled( );
        }

        public override void Breakpoint( )
        {
            Breakpoint( 0xa5a5a5a5 );
        }

        //--//

        public static uint EnableInterrupts( )
        {
            return DisableInterruptsWithPriorityLevelLowerOrEqualTo( c_Priority__Lowest );
        }

        public static uint DisableInterrupts( )
        {
            return DisableInterruptsWithPriorityLevelLowerOrEqualTo( c_Priority__Highest );
        }

        public static uint EnableFaults( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK();

            CMSIS_STUB_SCB__Enable_Fault_Irq( );

            return faultmask << 1;
        }

        public static uint DisableFaults( )
        {
            uint faultmask = CMSIS_STUB_SCB__get_FAULTMASK( );

            CMSIS_STUB_SCB__Disable_Fault_Irq( );

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

        public static uint DisableInterruptsWithPriorityLevelLowerOrEqualTo( uint basepri )
        {
            return SetBasePriRegister( basepri );
        }

        public static bool IsDebuggerConnected
        {
            get
            {
                return CUSTOM_STUB_DebuggerConnected( ) == c_COREDEBUG_DHCSR_CONNECTED;
            }
        }

        //--//

        [Inline]
        internal static uint GetBasePriRegister( )
        {
            return CMSIS_STUB_SCB__get_BASEPRI( );
        }

        [Inline]
        internal static uint SetBasePriRegister( uint basepri )
        {
            return CMSIS_STUB_SCB__set_BASEPRI( basepri );
        }

        internal static void RaiseSystemHandler( uint mask )
        {
            BugCheck.Assert( ((mask & ~(c_ICSR__ALLOWED_MASK)) == 0), BugCheck.StopCode.IncorrectArgument );

            CUSTOM_STUB_SCB_ICSR_RaiseSystemException( mask );
        }

        public static unsafe void RaiseSupervisorCall( SVC_Code code )
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
                case SVC_Code.SupervisorCall__SnapshotProcessModeRegisters:                   
                    //
                    // Cause a SVC call to transition to Handler mode and 
                    // snapshot the Processmode registers
                    //
                    CUSTOM_STUB_RaiseSupervisorCallForSnapshotProcessModeRegisters( );                        
                    break;
                default:
                    throw new ArgumentException( "Request SVC does not exists or is not supported" );
            }


        }

        public static bool IsAnyExceptionActive( )
        {
            return (((ISR_NUMBER)CUSTOM_STUB_SCB_IPSR_GetCurrentISRNumber( )) != ISR_NUMBER.ThreadMode);
        }

        public static void InitiateContextSwitch( )
        {
            RaiseSystemHandler( c_ICSR__PENDSTSET__SET );
        }

        public static void CompleteContextSwitch( )
        {
            RaiseSystemHandler( c_ICSR__PENDSVSET__SET );
        }

        public static void WaitForEvent( )
        {
#if SPIN_ON_SLEEP
            while(true) { }
#else
            CMSIS_STUB_POWER_WaitForEvent( );
#endif
        }

        public static void SendEvent( )
        {
            CMSIS_STUB_POWER_SendEvent( );
        }

        public static void WaitForInterrupt( )
        {
#if SPIN_ON_SLEEP
            while(true) { }
#else
            CMSIS_STUB_POWER_WaitForInterrupt( );
#endif
        }

        protected abstract void RemapInterrupt( IRQn_Type IRQn, Action isr );

        protected void SetSystemControlRegister( uint flags )
        {
            CUSTOM_STUB_SCB_SCR_SetSystemControlRegister( flags );
        }

        protected static bool VerifyHandlerMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            return ((control & c_CONTROL__SPSEL_MASK) == c_CONTROL__SPSEL_MSP);
        }
        
        protected static void EnableSystemHandler( IRQn_Type ex )
        {
            BugCheck.Assert( (ex < 0), BugCheck.StopCode.IncorrectArgument );

            uint mask = 0;

            switch(ex)
            {
                case IRQn_Type.DebugMonitor_IRQn:  
                    // Not implemented, we should never do this
                    BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
                    break;
                case IRQn_Type.PendSV_IRQn:
                case IRQn_Type.SysTick_IRQn:
                    // Should use NVIC for this ones
                    BugCheck.Assert( false, BugCheck.StopCode.IncorrectArgument );
                    break;
                case IRQn_Type.Reset_IRQn         :
                case IRQn_Type.NonMaskableInt_IRQn:  
                case IRQn_Type.SVCall_IRQn        :
                case IRQn_Type.HardFault_IRQn     :
                    // always enabled 
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

        protected static void DisableSystemHandler( IRQn_Type ex )
        {
            BugCheck.Assert( (ex < 0), BugCheck.StopCode.IncorrectArgument );

            uint mask = 0;

            switch(ex)
            {
                case IRQn_Type.Reset_IRQn:
                case IRQn_Type.NonMaskableInt_IRQn:
                case IRQn_Type.HardFault_IRQn:
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

        //--//

        private static void SetCCR( uint val )
        {
            // NOTE: LWIP uses memory functions on addresses which are not 8 byte aligned
            // This prevents faults from occurring from those memory accesses

            CUSTOM_STUB_SCB_set_CCR( val );
        }

        #region Native methods helpers

        //
        // We will implement the intrernal methods below with CMSIS-Core or custom stubs
        //      

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_DebuggerConnected( );

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_GetProgramCounter( );

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_SCB__get_HFSR( );

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_SCB__get_CFSR( );

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_SCB__get_MMFAR( );

        [DllImport( "C" )]
        internal static extern uint CUSTOM_STUB_SCB__get_BFAR( );

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
        internal static unsafe extern void* CMSIS_STUB_SCB__get_MSP_ResetValue( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_MSP_StackSize( );

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
        // !!! Cortex M3/4 only !!!
        //

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Enable_Fault_Irq( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_SCB__Disable_Fault_Irq( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__get_BASEPRI( );

        [DllImport( "C" )]
        internal static extern uint CMSIS_STUB_SCB__set_BASEPRI( uint basePri );

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
        internal static extern void CUSTOM_STUB_RaiseSupervisorCallForSnapshotProcessModeRegisters( );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SetExcReturn( uint ret );

        [DllImport( "C" )]
        internal static extern int CUSTOM_STUB_SCB_IPSR_GetCurrentISRNumber( );

        [DllImport( "C" )]
        internal static extern int CUSTOM_STUB_SCB_SCR_GetSystemControlRegister( );

        [DllImport( "C" )]
        internal static extern void CUSTOM_STUB_SCB_SCR_SetSystemControlRegister( uint scr );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_POWER_WaitForEvent( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_POWER_SendEvent( );

        [DllImport( "C" )]
        internal static extern void CMSIS_STUB_POWER_WaitForInterrupt( );

        //--//

        [DllImport( "C" )]
        internal static extern unsafe uint* CUSTOM_STUB_FetchSoftwareFrameSnapshot( );

        //--//
        //--//
        //--//

        //
        // Utility methods 
        //

        [TS.WellKnownMethod( "ProcessorARMv7_Breakpoint" )]

        [DllImport( "C" )]
        public static extern void Breakpoint( uint value );

        #endregion
    }
}
