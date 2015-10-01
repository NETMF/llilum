//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM3
{
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;

    //--//

    // TODO: put right addresses, and fix code generation for LLVM that does not understand the attribute's constants
    //[MemoryMappedPeripheral(Base = 0x40D00000U, Length = 0x000000D0U)]
    public class SysTick
    {
        //////
        //////	From core_cm3.h in mBed CMSIS support: 
        //////	
        //////	/** \brief  Structure type to access the System Timer (SysTick).
        //////	*/
        //////	typedef struct
        //////	{
        //////		__IO uint32_t CTRL;                    /*!< Offset: 0x000 (R/W)  SysTick Control and Status Register */
        //////		__IO uint32_t LOAD;                    /*!< Offset: 0x004 (R/W)  SysTick Reload Value Register       */
        //////		__IO uint32_t VAL;                     /*!< Offset: 0x008 (R/W)  SysTick Current Value Register      */
        //////		__I  uint32_t CALIB;                   /*!< Offset: 0x00C (R/ )  SysTick Calibration Register        */
        //////	} SysTick_Type;
        //////	
        //////	...
        //////	...
        //////	...
        //////	
        //////	#define SysTick             ((SysTick_Type   *)     SysTick_BASE  )   /*!< SysTick configuration struct       */
        //////


        //--//

        public const uint MaxCounterValue = 0x00FFFFFF;

        //
        // SYST_CSR @ address 0xE000E010 
        //
        private const int SYST_CSR__MASK                    = 0x00010007;
        
        private const  int SYST_CSR__ENABLE___SHIFT          =          0;
        private const uint SYST_CSR__ENABLE___MASK           = 0x00000001u << SYST_CSR__ENABLE___SHIFT;
        private const uint SYST_CSR__ENABLE___ENABLED        =          1u << SYST_CSR__ENABLE___SHIFT;
        private const uint SYST_CSR__ENABLE___DISABLED       =          0u << SYST_CSR__ENABLE___SHIFT;

        private const  int SYST_CSR__TICKINT___SHIFT         =          1;
        private const uint SYST_CSR__TICKINT___MASK          = 0x00000001u << SYST_CSR__TICKINT___SHIFT;
        private const uint SYST_CSR__TICKINT___ENABLED       =          1u << SYST_CSR__TICKINT___SHIFT;
        private const uint SYST_CSR__TICKINT___DISABLED      =          0u << SYST_CSR__TICKINT___SHIFT;

        private const  int SYST_CSR__CLKSOURCE___SHIFT       =          2;
        private const uint SYST_CSR__CLKSOURCE___MASK        = 0x00000001u << SYST_CSR__CLKSOURCE___SHIFT;
        private const uint SYST_CSR__CLKSOURCE___PROCESSOR   =          1u << SYST_CSR__CLKSOURCE___SHIFT;
        private const uint SYST_CSR__CLKSOURCE___EXTERNAL    =          0u << SYST_CSR__CLKSOURCE___SHIFT;

        private const  int SYST_CSR__COUNTFLAG___SHIFT       =         16;
        private const uint SYST_CSR__COUNTFLAG___MASK        = 0x00000001u << SYST_CSR__COUNTFLAG___SHIFT;
        private const uint SYST_CSR__COUNTFLAG___COUNTED     =          1u << SYST_CSR__COUNTFLAG___SHIFT;
        private const uint SYST_CSR__COUNTFLAG___DIDNOTCOUNT =          0u << SYST_CSR__COUNTFLAG___SHIFT;

        //--//
        
        private const uint SYST_CSR__STARTED = SYST_CSR__ENABLE___ENABLED | SYST_CSR__TICKINT___ENABLED | SYST_CSR__CLKSOURCE___PROCESSOR;
        private const uint SYST_CSR__STOPPED =                              SYST_CSR__TICKINT___ENABLED | SYST_CSR__CLKSOURCE___PROCESSOR;


        //
        // SYST_CSR @ address 0xE000E014 
        //
        private const int SYST_RVR__MASK                    = 0x00FFFFFF;

        private const int SYST_RVR__RELOAD___MASK           = 0x00FFFFFF;
        private const int SYST_RVR__RELOAD___SHIFT          =          0;

        //
        // SYST_CVR @ address 0xE000E018 
        //
        private const int SYST_CVR__MASK                    = 0x00FFFFFF;

        private const int SYST_CVR__CURRENT___MASK          = 0x00FFFFFF;
        private const int SYST_CVR__CURRENT___SHIFT         =          0;

        //
        // SYST_CALIB @ address 0xE000E01C
        //
        private const uint SYST_CALIB__MASK                  = 0xC0FFFFFF;

        private const  int SYST_CALIB__TENMS___SHIFT         =          0;
        private const uint SYST_CALIB__TENMS___MASK          = 0x00FFFFFFu << SYST_CALIB__TENMS___SHIFT;

        private const  int SYST_CALIB__SKEW__SHIFT           =         30;
        private const uint SYST_CALIB__SKEW__MASK            =         1u << SYST_CALIB__SKEW__SHIFT;
        private const uint SYST_CALIB__SKEW__PRECISE         =         0u << SYST_CALIB__SKEW__SHIFT;
        private const uint SYST_CALIB__SKEW__NOTPRECISE      =         1u << SYST_CALIB__SKEW__SHIFT;

        private const  int SYST_CALIB__NOREF__SHIFT          =         31;
        private const uint SYST_CALIB__NOREF__MASK           =         1u << SYST_CALIB__NOREF__SHIFT;
        private const uint SYST_CALIB__NOREF__HASREF         =         0u << SYST_CALIB__NOREF__SHIFT;
        private const uint SYST_CALIB__NOREF__NOREF          =         1u << SYST_CALIB__NOREF__SHIFT;

        //--//

        public uint Match
        {
            [RT.Inline]
            get
            {
                return CMSIS_STUB_SysTick_GetLOAD( );
            }
            [RT.Inline]
            set
            {
                RT.BugCheck.Assert( value <= 0x00FFFFFF, RT.BugCheck.StopCode.IncorrectArgument ); 

                CMSIS_STUB_SysTick_SetLOAD( value );
            }
        }

        public uint Calibration
        {
            [RT.Inline]
            get
            {
                return CMSIS_STUB_SysTick_GetCALIB( );
            }
        }

        public uint Counter
        {
            [RT.Inline]
            get
            {
                return CMSIS_STUB_SysTick_GetVAL( ); 
            }
            [RT.Inline]
            set
            {
                RT.BugCheck.Assert( value <= 0x00FFFFFF, RT.BugCheck.StopCode.IncorrectArgument );

                //
                // writing any value clear the register to zero, and also clears the count flag
                //
                CMSIS_STUB_SysTick_SetVAL( value );
            }
        }

        public bool HasMatched
        {
            [RT.Inline]
            get
            {
                uint ctrl = CMSIS_STUB_SysTick_GetCTRL( );

                return (( ctrl & SYST_CSR__COUNTFLAG___MASK ) == SYST_CSR__COUNTFLAG___COUNTED);
            }
        }

        [RT.Inline]
        public void ResetAndClear()
        {
            // writing the counter value clear the COUNTFLAG
            this.Counter = 0; 
        }
        

        public bool Enabled
        {
            [RT.Inline]
            set
            {
                if(value == true)
                {
                    // enable SysTick with interrupts from processor clock
                    CMSIS_STUB_SysTick_SetCTRL( SYST_CSR__STARTED );
                }
                else
                {
                    uint ctrl = CMSIS_STUB_SysTick_GetCTRL( );

                    ctrl &= ~SYST_CSR__ENABLE___ENABLED; 
            
                    CMSIS_STUB_SysTick_SetCTRL( ctrl );
                }

            }
        }

        public uint TenMillisecondsCalibrationValue
        {
            get
            {
                return CMSIS_STUB_SysTick_GetCALIB( ) & SYST_CALIB__TENMS___MASK;
            }
        }

        public bool HasRef
        {
            get
            {
                return ((CMSIS_STUB_SysTick_GetCALIB() & SYST_CALIB__NOREF__MASK) == SYST_CALIB__NOREF__HASREF);
            }
        }

        public bool IsPrecise
        {
            get
            {
                return  (CMSIS_STUB_SysTick_GetCALIB() & SYST_CALIB__SKEW__MASK) == SYST_CALIB__SKEW__PRECISE;
            }
        }

        public uint SystemCoreClock
        {
            get
            {
                return CMSIS_STUB_CLOCK__GetSystemCoreClock();
            }
        }

        //
        // Access Methods
        //

        public static extern SysTick Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        //--//

        [DllImport( "C" )]
        private static extern uint CMSIS_STUB_SysTick_GetCTRL( );

        [DllImport( "C" )]
        private static extern uint CMSIS_STUB_SysTick_GetLOAD( );

        [DllImport( "C" )]
        private static extern uint CMSIS_STUB_SysTick_GetVAL( );

        [DllImport( "C" )]
        private static extern uint CMSIS_STUB_SysTick_GetCALIB( );

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_SysTick_SetCTRL( uint value );

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_SysTick_SetLOAD( uint value );

        [DllImport( "C" )]
        private static extern void CMSIS_STUB_SysTick_SetVAL( uint value );

        //--//
        
        [DllImport( "C" )]
        private static extern uint CMSIS_STUB_CLOCK__GetSystemCoreClock( );
    }
}