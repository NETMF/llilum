//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.LLVM
{
    using System;
    using System.Runtime.InteropServices;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public abstract partial class ProcessorLLVM : Processor
    {
        //--//

        //
        // Interrupts management
        //
        // In the simplest case, we will use the PRIMASK against a BASEPRI of 0x00000000, which is the reset value 
        public const uint c_InterruptsOff = 0x00000001;
        public const uint c_InterruptsOn  = 0x00000000;
        public const uint c_FaultsOff     = 0x00000001;
        public const uint c_FaultsOn      = 0x00000000;

        //--//

        [TS.WellKnownType( "Microsoft_Zelig_LLVM_MethodWrapper" )]
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
            // For ARMv7-M ISA, we will rely on the fact that the BASEPRI is reset to 0x00000000 on reset
            //
        }


        public override Processor.Context AllocateProcessorContext( )
        {
            return null;
        }

        //--//

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            throw new Exception( "GetCacheableAddress not implemented" );
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            throw new Exception( "GetUncacheableAddress not implemented" );
        }

        //TODO: [TS.WellKnownMethod( "ProcessorARM_SetPriMaskRegister" )] 
        //
        // For ARMv7-M ISA, use CMSIS, e.g.: from file come_cmFunc.h: 
        //
        ///** \brief  Set Priority Mask

        //    This function assigns the given value to the Priority Mask Register.

        //    \param [in]    priMask  Priority Mask
        //*/
        //__attribute__( ( always_inline ) ) __STATIC_INLINE void __set_PRIMASK(uint32_t priMask)
        //{
        //  __ASM volatile ("MSR primask, %0" : : "r" (priMask) : "memory");
        //}
        [DllImport( "C" )]
        public static extern void SetPriMaskRegister( uint primask );

        //TODO: [TS.WellKnownMethod( "ProcessorARM_GetPriMaskRegister" )]
        [DllImport( "C" )]
        public static extern uint GetPriMaskRegister( );

        //TODO: [TS.WellKnownMethod( "ProcessorARM_SetFaultMaskRegister" )] 
        [DllImport( "C" )]
        public static extern void SetFaultMaskRegister( uint primask );

        //TODO: [TS.WellKnownMethod( "ProcessorARM_GetFaultMaskRegister" )]
        [DllImport( "C" )]
        public static extern uint GetFaultMaskRegister( );

        public override void FlushCacheLine( UIntPtr target )
        {
            throw new Exception( "FlushCacheLine not implemented" );
        }

        //--//--//

        [Inline]
        public override bool AreInterruptsDisabled( )
        {
            uint primask = GetPriMaskRegister( );

            return ( primask & c_InterruptsOff ) == c_InterruptsOff;
        }

        [Inline]
        public override bool AreFaultsDisabled( )
        {
            uint faultmask = GetFaultMaskRegister( );

            return ( faultmask & c_FaultsOff ) == c_FaultsOff;
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( ) && AreFaultsDisabled( ); 
        }

        //--//--//

        internal static uint DisableInterrupts( )
        {
            uint primask = GetPriMaskRegister( );

            SetPriMaskRegister( c_InterruptsOff ); 

            return primask;
        }

        internal static uint DisableFaults( )
        {
            uint faultmask = GetFaultMaskRegister( );

            SetFaultMaskRegister( c_FaultsOff );

            return faultmask;
        }

        //internal static uint DisableAllInterrupts( )
        //{
        //    return DisableInterrupts( ) && DisableFaults( );
        //}

        internal static uint EnableInterrupts( )
        {
            uint primask = GetPriMaskRegister( );

            SetPriMaskRegister( c_InterruptsOn );

            return primask; 
        }

        //internal static uint EnableAllInterrupts( )
        //{
        //    //
        //    // TODO: implement this as for ARMv7-M ISA processor, but use CMSIS layer from mbed distro
        //    //
        //    throw new NotImplementedException( );
        //}

        //--//

        public override void Breakpoint( )
        {

        }

        //--//

        internal static void SetMode( uint primask )
        {
            SetPriMaskRegister( primask );
        }

        internal static void SetFaultMode( uint faultmask )
        {
            SetFaultMaskRegister( faultmask );
        }

        //--//

        internal static void Nop( )
        {
            //
            // TODO: implement this as for ARMv7-M ISA using CMSIS layer from ARM distro
            //
            throw new NotImplementedException( );
        }
    }
}
