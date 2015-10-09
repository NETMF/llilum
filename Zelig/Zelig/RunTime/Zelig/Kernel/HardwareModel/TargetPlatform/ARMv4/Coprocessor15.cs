//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4
{
    using System;

    using TS     = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;


    public static class Coprocessor15
    {
        public const uint c_ControlRegister__Vector         = (1U << 13); // Exception Vector Relocation (V).
                                                                          // 0 = Base address of exception vectors is 0x0000,0000
                                                                          // 1 = Base address of exception vectors is 0xFFFF,0000
                                                                          //
        public const uint c_ControlRegister__ICache         = (1U << 12); // Instruction Cache Enable/Disable (I)
                                                                          // 0 = Disabled
                                                                          // 1 = Enabled
                                                                          // 
        public const uint c_ControlRegister__BTB            = (1U << 11); // Branch Target Buffer Enable (Z)
                                                                          // 0 = Disabled
                                                                          // 1 = Enabled
                                                                          // 
        public const uint c_ControlRegister__RomProt        = (1U <<  9); // ROM Protection (R)
                                                                          // This selects the access checks performed by the memory
                                                                          // management unit. See the ARM Architecture Reference
                                                                          // Manual for more information.
                                                                          // 
        public const uint c_ControlRegister__SysProt        = (1U <<  8); // System Protection (S)
                                                                          // This selects the access checks performed by the memory
                                                                          // management unit. See the ARM Architecture Reference
                                                                          // Manual for more information.
                                                                          // 
        public const uint c_ControlRegister__BigEndian      = (1U <<  7); // Big/Little Endian (B)
                                                                          // 0 = Little-endian operation
                                                                          // 1 = Big-endian operation
                                                                          // 
        public const uint c_ControlRegister__DCache         = (1U <<  2); // Data cache enable/disable (C)
                                                                          // 0 = Disabled
                                                                          // 1 = Enabled
                                                                          // 
        public const uint c_ControlRegister__AlignmentFault = (1U <<  1); // Alignment fault enable/disable (A)
                                                                          // 0 = Disabled
                                                                          // 1 = Enabled
                                                                          // 
        public const uint c_ControlRegister__MMU            = (1U <<  0); // Memory management unit enable/disable (M)
                                                                          // 0 = Disabled
                                                                          // 1 = Enabled
                                                                          // 
        //
        // Helper Methods
        //

        [Inline]
        public static unsafe void InvalidateCaches()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 7, 0, 0 );
        }

        [Inline]
        public static unsafe void InvalidateICache()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 5, 0, 0 );
        }

        [Inline]
        public static unsafe void InvalidateICache( UIntPtr target )
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 5, 1, target.ToUInt32() );
        }

        [Inline]
        public static unsafe void InvalidateDCache()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 6, 0, 0 );
        }

        [Inline]
        public static unsafe void InvalidateDCache( UIntPtr target )
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 6, 1, target.ToUInt32() );
        }

        [Inline]
        public static unsafe void CleanDCache( UIntPtr target )
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 10, 1, target.ToUInt32() );
        }

        [Inline]
        public static unsafe void CleanAndInvalidateDCache( UIntPtr target )
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 14, 1, target.ToUInt32() );
        }

        //--//

        [Inline]
        public static unsafe void TestAndCleanDCache()
        {
            while((ProcessorARMv4.MoveFromCoprocessor( 15, 0, 7, 10, 3 ) & (1u << 30)) == 0)
            {
            }
        }

        [Inline]
        public static unsafe void TestCleanAndInvalidateDCache()
        {
            while((ProcessorARMv4.MoveFromCoprocessor( 15, 0, 7, 14, 3 ) & (1u << 30)) == 0)
            {
            }
        }

        [Inline]
        public static unsafe void DrainWriteBuffer()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 10, 4, 0 );
        }

        [Inline]
        public static unsafe void WaitForInterrupt()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 0, 4, 0 );
        }

        [Inline]
        public static unsafe void InvalidateMMUTLBs()
        {
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 8, 7, 0, 0 );
        }

        [Inline]
        public static unsafe void SetDomainAccessControl( uint val )
        {
            //
            // Domain access control.
            //
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 3, 0, 0, val );
        }

        [Inline]
        public static unsafe void SetTranslationTableBaseRegister( uint* ptr )
        {
            //
            // Write the Translation Table Base Register.
            //
            ProcessorARMv4.MoveToCoprocessor( 15, 0, 2, 0, 0, new UIntPtr( ptr ).ToUInt32() );
        }

        //--//--//

        [Inline]
        public static void ResetControlRegisterBits( uint mask )
        {
            uint reg = ProcessorARMv4.MoveFromCoprocessor( 15, 0, 1, 0, 0 ); // MRC p15, 0, <Rd>, c1, c0, 0 ; read control register

            reg &= ~mask;

            ProcessorARMv4.MoveToCoprocessor( 15, 0, 1, 0, 0, reg );
        }

        [Inline]
        public static void SetControlRegisterBits( uint mask )
        {
            uint reg = ProcessorARMv4.MoveFromCoprocessor( 15, 0, 1, 0, 0 ); // MRC p15, 0, <Rd>, c1, c0, 0 ; read control register

            reg |= mask;

            ProcessorARMv4.MoveToCoprocessor( 15, 0, 1, 0, 0, reg );
        }
    }
}
