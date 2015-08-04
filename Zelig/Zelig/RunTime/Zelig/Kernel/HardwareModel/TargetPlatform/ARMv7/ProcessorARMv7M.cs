//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public abstract partial class ProcessorARMv7M : Runtime.Processor
    {
        //--//

        //
        // Interrupts management
        //
        // In the simplest case, we will use the PRIMASK against a BASEPRI of 0x00000000, which is the reset value 
        public const uint c_PRIMASK_InterruptsOff = 0x00000001;
        public const uint c_PRIMASK_InterruptsOn  = 0x00000000;
        public const uint c_FAULTMASK_FaultsOff   = 0x00000001;
        public const uint c_FAULTMASKFaultsOn     = 0x00000000;

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
            SetPriMaskRegister  ( c_PRIMASK_InterruptsOff );
            SetFaultMaskRegister( c_FAULTMASK_FaultsOff   ); 
        }

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
        
        //
        // We will implement this methods with CMSIS-Core
        //

        //TODO: [TS.WellKnownMethod( "ProcessorARM_SetPriMaskRegister" )] 
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

        //--//--//

        [Inline]
        public override bool AreInterruptsDisabled( )
        {
            uint primask = GetPriMaskRegister( );

            return ( primask & c_PRIMASK_InterruptsOff ) == c_PRIMASK_InterruptsOff;
        }

        [Inline]
        public override bool AreFaultsDisabled( )
        {
            uint faultmask = GetFaultMaskRegister( );

            return ( faultmask & c_FAULTMASK_FaultsOff ) == c_FAULTMASK_FaultsOff;
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( ) && AreFaultsDisabled( ); 
        }

        //--//--//

        internal static uint DisableInterrupts( )
        {
            uint primask = GetPriMaskRegister( );

            SetPriMaskRegister( c_PRIMASK_InterruptsOff ); 

            return primask;
        }

        internal static uint DisableFaults( )
        {
            uint faultmask = GetFaultMaskRegister( );

            SetFaultMaskRegister( c_FAULTMASK_FaultsOff );

            return faultmask;
        }

        internal static uint EnableInterrupts( )
        {
            uint primask = GetPriMaskRegister( );

            SetPriMaskRegister( c_PRIMASK_InterruptsOn );

            return primask; 
        }

        internal static uint EnableFaults( )
        {
            uint primask = GetPriMaskRegister( );

            SetPriMaskRegister( c_FAULTMASKFaultsOn );

            return primask; 
        }

        internal static uint DisableAllInterrupts( )
        {
            return DisableInterrupts( ) & DisableFaults( );
        }

        internal static uint EnableAllInterrupts( )
        {
            return EnableInterrupts( ) & EnableFaults( );
        }

        //--//
        
        public override void Breakpoint( )
        {
            Breakpoint( 0xa5a5a5a5 ); 
        }

        //--//
        
        //[TS.WellKnownMethod( "ProcessorARM_Breakpoint" )]
        [DllImport( "C" )]
        public static extern void Breakpoint( uint value );

        [DllImport( "C" )]
        public static extern void Nop( );

        internal static void SetMode( uint primask )
        {
            SetPriMaskRegister( primask );
        }

        internal static void SetFaultMode( uint faultmask )
        {
            SetFaultMaskRegister( faultmask );
        }
    }
}
