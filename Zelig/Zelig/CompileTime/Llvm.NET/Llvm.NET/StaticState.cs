using System;

namespace Llvm.NET
{
    /// <summary>Target tools to register/enable</summary>
    [Flags]
    public enum TargetRegistration
    {
        /// <summary>Register nothing</summary>
        None = 0x00,
        /// <summary>Register the Target class</summary>
        Target = 0x01,
        /// <summary>Register the Target info for the target</summary>
        TargetInfo = 0x02,
        /// <summary>Register the target machine(s) for a target</summary>
        TargetMachine = 0x04,
        /// <summary>Registers the assembly source code generator for a target</summary>
        AsmPrinter = 0x08,
        /// <summary>Registers the Disassembler for a target</summary>
        Disassembler = 0x10,
        /// <summary>Registers the assembly source parser for a target</summary>
        AsmParser = 0x20,
        /// <summary>Registers all the codegeneration components</summary>
        CodeGen = Target | TargetInfo | TargetMachine,
        /// <summary>Registers all components</summary>
        All = CodeGen | AsmPrinter | Disassembler | AsmParser
    }
    
    /// <summary>Provides support for various LLVM static state initialization and manipulation</summary>
    public static class StaticState
    {
        public static void ParseCommandLineOptions( string[] args, string overview )
        {
            NativeMethods.ParseCommandLineOptions( args.Length, args, overview );
        }

        // basic pattern to follow for any new targets in the future
        //public static void RegisterXXX( TargetRegistration regFlags = TargetRegistration.All )
        //{
        //    if( regFlags.HasFlag( TargetRegistration.Target ) )
        //        LLVMNative.InitializeXXXTarget( );

        //    if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
        //        LLVMNative.InitializeXXXTargetInfo( );

        //    if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
        //        LLVMNative.InitializeXXXTargetMC( );

        //    if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
        //        LLVMNative.InitializeXXXAsmPrinter( );

        //    if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
        //        LLVMNative.InitializeXXXDisassembler( );

        //    if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
        //        LLVMNative.InitializeXXXAsmParser( );
        //}

        /// <summary>Registers components for all available targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterAll( TargetRegistration regFlags = TargetRegistration.All )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeAllTargets( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeAllTargetInfos( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeAllTargetMCs( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeAllAsmPrinters( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeAllDisassemblers( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeAllAsmParsers( );
        }

        /// <summary>Registers components for the target representing the system the calling process is running on</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterNative( TargetRegistration regFlags = TargetRegistration.All )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeNativeTarget( );

            //if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
            //    LLVMNative.InitializeNativeTargetInfo( );

            //if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
            //    LLVMNative.InitializeNativeTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeNativeAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeNativeDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeNativeAsmParser( );
        }

        /// <summary>Registers components for ARM AArch64 target(s)</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterAArch64( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeAArch64Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeAArch64TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeAArch64TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeAArch64AsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeAArch64Disassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeAArch64AsmParser( );
        }

        /// <summary>Registers components for ARM 32bit and 16bit thumb targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterARM( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeARMTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeARMTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeARMTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeARMAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeARMDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeARMAsmParser( );
        }

        /// <summary>Registers components for the Hexagon CPU</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterHexagon( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeHexagonTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeHexagonTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeHexagonTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeHexagonAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeHexagonDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeHexagonAsmParser( );
        }

        /// <summary>Registers components for MIPS targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterMips( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeMipsTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeMipsTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeMipsTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeMipsAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeMipsDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeMipsAsmParser( );
        }

        /// <summary>Registers components for MSP430 targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterMSP430( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeMSP430Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeMSP430TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeMSP430TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeMSP430AsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeMSP430Disassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeMSP430AsmParser( );
        }

        /// <summary>Registers components for the NVPTX targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterNVPTX( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeNVPTXTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeNVPTXTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeNVPTXTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeNVPTXAsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeNVPTXDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeNVPTXAsmParser( );
        }

        /// <summary>Registers components for the PowerPC targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterPowerPC( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializePowerPCTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializePowerPCTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializePowerPCTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializePowerPCAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializePowerPCDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializePowerPCAsmParser( );
        }

        /// <summary>Registers components for AMDGPU targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterAMDGPU( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeAMDGPUTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeAMDGPUTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeAMDGPUTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeAMDGPUAsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeAMDGPUDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeAMDGPUAsmParser( );
        }

        /// <summary>Registers components for SPARC targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterSparc( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeSparcTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeSparcTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeSparcTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeSparcAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeSparcDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeSparcAsmParser( );
        }

        /// <summary>Registers components for SystemZ targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterSystemZ( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeSystemZTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeSystemZTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeSystemZTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeSystemZAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeSystemZDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeSystemZAsmParser( );
        }

        /// <summary>Registers components for X86 targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterX86( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeX86Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeX86TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeX86TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeX86AsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeX86Disassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                NativeMethods.InitializeX86AsmParser( );
        }

        /// <summary>Registers components for XCore targets</summary>
        /// <param name="regFlags">Flags indicating which components to register/enable</param>
        public static void RegisterXCore( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                NativeMethods.InitializeXCoreTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                NativeMethods.InitializeXCoreTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                NativeMethods.InitializeXCoreTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                NativeMethods.InitializeXCoreAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                NativeMethods.InitializeXCoreDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeXCoreAsmParser( );
        }
    }
}
