//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;


    public abstract partial class ProcessorARMv7M_VFP : ProcessorARMv7M
    {

        //
        // Floating point context
        //
        public const  int c_CONTROL__FPCA_SHIFT       = 2;
        public const uint c_CONTROL__FPCA_MASK        = 0x1u << c_CONTROL__FPCA_SHIFT;
        public const uint c_CONTROL__FPCA_INACTIVE    = 0x0u << c_CONTROL__FPCA_SHIFT;
        public const uint c_CONTROL__FPCA_ACTIVE      = 0x1u << c_CONTROL__FPCA_SHIFT;

        //
        // FP control
        //       
        public const  int c_FPCCR__ASPEN_SHIFT        = 31;
        public const uint c_FPCCR__ASPEN_MASK         = 0x1u << c_FPCCR__ASPEN_SHIFT;
        public const uint c_FPCCR__ASPEN_ENABLED      = 0x1u << c_FPCCR__ASPEN_SHIFT;

        public const  int c_FPCCR__LSPEN_SHIFT        = 30;
        public const uint c_FPCCR__LSPEN_MASK         = 0x1u << c_FPCCR__LSPEN_SHIFT;
        public const uint c_FPCCR__LSPEN_ENABLED      = 0x1u << c_FPCCR__LSPEN_SHIFT;

        //
        // EXC_RETURN 
        //
        public const uint c_MODE_RETURN__HANDLER_MSP_VFP = 0xFFFFFFE1; // handler will return in handler mode using the MSP
        public const uint c_MODE_RETURN__THREAD_MSP_VFP  = 0xFFFFFFE9; // handler will return in thread mode using the MSP
        public const uint c_MODE_RETURN__THREAD_PSP_VFP  = 0xFFFFFFED; // handler will return in thread mode using the PSP

        //
        // Helper Methods
        //
        public override void InitializeProcessor( )
        {
            base.InitializeProcessor( );

            DisableLazyStacking( );
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
        
        protected void EnableLazyStacking()
        {
            uint value = CUSTOM_STUB_SCB__get_FPCCR( );

            value |= c_FPCCR__LSPEN_ENABLED;

            CUSTOM_STUB_SCB__set_FPCCR( value ); 
        }

        protected void DisableLazyStacking()
        {
            uint value = CUSTOM_STUB_SCB__get_FPCCR( );

            value &= ~c_FPCCR__LSPEN_ENABLED;

            CUSTOM_STUB_SCB__set_FPCCR( value ); 
        }

        protected bool IsFPContextActiveOnCurrentMode()
        {
            return ( ( CMSIS_STUB_SCB__get_CONTROL( ) & c_CONTROL__FPCA_ACTIVE ) == c_CONTROL__FPCA_ACTIVE ); 
        }

        //--//
        
        [DllImport( "C" )]
        private static extern uint CUSTOM_STUB_SCB__get_FPCCR( ); 

        [DllImport( "C" )]
        private static extern void CUSTOM_STUB_SCB__set_FPCCR( uint value ); 

        [DllImport( "C" )]
        private static extern bool CUSTOM_STUB_IsFPContextActiveOnCurrentMode( ); 
    }
}
