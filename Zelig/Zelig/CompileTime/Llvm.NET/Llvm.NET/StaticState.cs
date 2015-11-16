using System;

namespace Llvm.NET
{
    /// <summary>Target tools to register/enable</summary>
    [Flags]
    public enum TargetRegistrations
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
        //public static void RegisterXXX( TargetRegistrations registrations = TargetRegistration.All )
        //{
        //    if( registrations.HasFlag( TargetRegistrations.Target ) )
        //        LLVMNative.InitializeXXXTarget( );

        //    if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
        //        LLVMNative.InitializeXXXTargetInfo( );

        //    if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
        //        LLVMNative.InitializeXXXTargetMC( );

        //    if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
        //        LLVMNative.InitializeXXXAsmPrinter( );

        //    if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
        //        LLVMNative.InitializeXXXDisassembler( );

        //    if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
        //        LLVMNative.InitializeXXXAsmParser( );
        //}

        /// <summary>Registers components for all available targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterAll( TargetRegistrations registrations = TargetRegistrations.All )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeAllTargets( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeAllTargetInfos( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeAllTargetMCs( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeAllAsmPrinters( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeAllDisassemblers( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeAllAsmParsers( );
        }

        /// <summary>Registers components for the target representing the system the calling process is running on</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterNative( TargetRegistrations registrations = TargetRegistrations.All )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeNativeTarget( );

            //if( registrations.HasFlag( TargetRegistration.TargetInfo ) )
            //    LLVMNative.InitializeNativeTargetInfo( );

            //if( registrations.HasFlag( TargetRegistration.TargetMachine ) )
            //    LLVMNative.InitializeNativeTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeNativeAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeNativeDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeNativeAsmParser( );
        }

        /// <summary>Registers components for ARM AArch64 target(s)</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterAArch64( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeAArch64Target( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeAArch64TargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeAArch64TargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeAArch64AsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeAArch64Disassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeAArch64AsmParser( );
        }

        /// <summary>Registers components for ARM 32bit and 16bit thumb targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterARM( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeARMTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeARMTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeARMTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeARMAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeARMDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeARMAsmParser( );
        }

        /// <summary>Registers components for the Hexagon CPU</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterHexagon( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeHexagonTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeHexagonTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeHexagonTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeHexagonAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeHexagonDisassembler( );

            //if( registrations.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeHexagonAsmParser( );
        }

        /// <summary>Registers components for MIPS targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterMips( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeMipsTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeMipsTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeMipsTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeMipsAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeMipsDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeMipsAsmParser( );
        }

        /// <summary>Registers components for MSP430 targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterMSP430( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeMSP430Target( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeMSP430TargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeMSP430TargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeMSP430AsmPrinter( );

            //if( registrations.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeMSP430Disassembler( );

            //if( registrations.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeMSP430AsmParser( );
        }

        /// <summary>Registers components for the NVPTX targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterNVPTX( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeNVPTXTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeNVPTXTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeNVPTXTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeNVPTXAsmPrinter( );

            //if( registrations.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeNVPTXDisassembler( );

            //if( registrations.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeNVPTXAsmParser( );
        }

        /// <summary>Registers components for the PowerPC targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterPowerPC( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializePowerPCTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializePowerPCTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializePowerPCTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializePowerPCAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializePowerPCDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializePowerPCAsmParser( );
        }

        /// <summary>Registers components for AMDGPU targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterAMDGPU( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeAMDGPUTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeAMDGPUTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeAMDGPUTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeAMDGPUAsmPrinter( );

            //if( registrations.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeAMDGPUDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeAMDGPUAsmParser( );
        }

        /// <summary>Registers components for SPARC targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterSparc( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeSparcTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeSparcTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeSparcTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeSparcAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeSparcDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeSparcAsmParser( );
        }

        /// <summary>Registers components for SystemZ targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterSystemZ( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeSystemZTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeSystemZTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeSystemZTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeSystemZAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeSystemZDisassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeSystemZAsmParser( );
        }

        /// <summary>Registers components for X86 targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterX86( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeX86Target( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeX86TargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeX86TargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeX86AsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeX86Disassembler( );

            if( registrations.HasFlag( TargetRegistrations.AsmParser ) )
                NativeMethods.InitializeX86AsmParser( );
        }

        /// <summary>Registers components for XCore targets</summary>
        /// <param name="registrations">Flags indicating which components to register/enable</param>
        public static void RegisterXCore( TargetRegistrations registrations = TargetRegistrations.All  )
        {
            if( registrations.HasFlag( TargetRegistrations.Target ) )
                NativeMethods.InitializeXCoreTarget( );

            if( registrations.HasFlag( TargetRegistrations.TargetInfo ) )
                NativeMethods.InitializeXCoreTargetInfo( );

            if( registrations.HasFlag( TargetRegistrations.TargetMachine ) )
                NativeMethods.InitializeXCoreTargetMC( );

            if( registrations.HasFlag( TargetRegistrations.AsmPrinter ) )
                NativeMethods.InitializeXCoreAsmPrinter( );

            if( registrations.HasFlag( TargetRegistrations.Disassembler ) )
                NativeMethods.InitializeXCoreDisassembler( );

            //if( registrations.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeXCoreAsmParser( );
        }
    }
}
