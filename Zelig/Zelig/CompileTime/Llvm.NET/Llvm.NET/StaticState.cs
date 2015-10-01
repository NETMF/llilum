using System;

namespace Llvm.NET
{
    [Flags]
    public enum TargetRegistration
    {
                 None = 0x00,
               Target = 0x01,
           TargetInfo = 0x02,
        TargetMachine = 0x04,
           AsmPrinter = 0x08,
         Disassembler = 0x10,
            AsmParser = 0x20,
              CodeGen = Target | TargetInfo | TargetMachine,
                  All = CodeGen | AsmPrinter | Disassembler | AsmParser
    }
    
    /// <summary>Provides support for various LLVM static state initialization and manipulation</summary>
    public static class StaticState
    {
        public static void ParseCommandLineOptions( string[] args, string overview )
        {
            LLVMNative.ParseCommandLineOptions( args.Length, args, overview );
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

        public static void RegisterAll( TargetRegistration regFlags = TargetRegistration.All )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeAllTargets( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeAllTargetInfos( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeAllTargetMCs( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeAllAsmPrinters( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeAllDisassemblers( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeAllAsmParsers( );
        }

        public static void RegisterNative( TargetRegistration regFlags = TargetRegistration.All )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeNativeTarget( );

            //if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
            //    LLVMNative.InitializeNativeTargetInfo( );

            //if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
            //    LLVMNative.InitializeNativeTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeNativeAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeNativeDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeNativeAsmParser( );
        }

        public static void RegisterAArch64( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeAArch64Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeAArch64TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeAArch64TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeAArch64AsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeAArch64Disassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeAArch64AsmParser( );
        }

        public static void RegisterARM( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeARMTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeARMTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeARMTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeARMAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeARMDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeARMAsmParser( );
        }

        public static void RegisterHexagon( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeHexagonTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeHexagonTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeHexagonTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeHexagonAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeHexagonDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeHexagonAsmParser( );
        }

        public static void RegisterMips( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeMipsTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeMipsTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeMipsTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeMipsAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeMipsDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeMipsAsmParser( );
        }

        public static void RegisterMSP430( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeMSP430Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeMSP430TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeMSP430TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeMSP430AsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeMSP430Disassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeMSP430AsmParser( );
        }

        public static void RegisterNVPTX( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeNVPTXTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeNVPTXTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeNVPTXTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeNVPTXAsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeNVPTXDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeNVPTXAsmParser( );
        }

        public static void RegisterPowerPC( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializePowerPCTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializePowerPCTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializePowerPCTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializePowerPCAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializePowerPCDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializePowerPCAsmParser( );
        }

        public static void RegisterAMDGPU( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeAMDGPUTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeAMDGPUTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeAMDGPUTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeAMDGPUAsmPrinter( );

            //if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
            //    LLVMNative.InitializeAMDGPUDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeAMDGPUAsmParser( );
        }

        public static void RegisterSparc( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeSparcTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeSparcTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeSparcTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeSparcAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeSparcDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeSparcAsmParser( );
        }

        public static void RegisterSystemZ( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeSystemZTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeSystemZTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeSystemZTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeSystemZAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeSystemZDisassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeSystemZAsmParser( );
        }

        public static void RegisterX86( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeX86Target( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeX86TargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeX86TargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeX86AsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeX86Disassembler( );

            if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
                LLVMNative.InitializeX86AsmParser( );
        }

        public static void RegisterXCore( TargetRegistration regFlags = TargetRegistration.All  )
        {
            if( regFlags.HasFlag( TargetRegistration.Target ) )
                LLVMNative.InitializeXCoreTarget( );

            if( regFlags.HasFlag( TargetRegistration.TargetInfo ) )
                LLVMNative.InitializeXCoreTargetInfo( );

            if( regFlags.HasFlag( TargetRegistration.TargetMachine ) )
                LLVMNative.InitializeXCoreTargetMC( );

            if( regFlags.HasFlag( TargetRegistration.AsmPrinter ) )
                LLVMNative.InitializeXCoreAsmPrinter( );

            if( regFlags.HasFlag( TargetRegistration.Disassembler ) )
                LLVMNative.InitializeXCoreDisassembler( );

            //if( regFlags.HasFlag( TargetRegistration.AsmParser ) )
            //    LLVMNative.InitializeXCoreAsmParser( );
        }
    }
}
