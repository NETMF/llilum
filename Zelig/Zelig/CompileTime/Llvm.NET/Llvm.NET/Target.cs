using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    /// <summary>LLVM Target Instruction Set Architecture</summary>
    public class Target
    {
        /// <summary>Name of this target</summary>
        public string Name => Marshal.PtrToStringAnsi( NativeMethods.GetTargetName( TargetHandle ) );

        /// <summary>Description of this target</summary>
        public string Description => NativeMethods.NormalizeLineEndings( NativeMethods.GetTargetDescription( TargetHandle ) );

        /// <summary>Flag indicating if this target has JIT support</summary>
        public bool HasJIT => 0 != NativeMethods.TargetHasJIT( TargetHandle ).Value;

        /// <summary>Flag indicating if this target has a TargetMachine initialized</summary>
        public bool HasTargetMachine => 0 != NativeMethods.TargetHasTargetMachine( TargetHandle ).Value;

        /// <summary>Flag indicating if this target has an Assembly code generating back end initialized</summary>
        public bool HasAsmBackEnd => 0 != NativeMethods.TargetHasAsmBackend( TargetHandle ).Value;

        /// <summary>Creates a <see cref="TargetMachine"/> for the target and specified parameters</summary>
        /// <param name="context">Context to use for LLVM objects created by this machine</param>
        /// <param name="triple">Target triple for this machine (e.g. -mtriple)</param>
        /// <param name="cpu">CPU for this machine (e.g. -mcpu)</param>
        /// <param name="features">Features for this machine (e.g. -mattr...)</param>
        /// <param name="optLevel">Optimization level</param>
        /// <param name="relocationMode">Relocation mode for generated code</param>
        /// <param name="codeModel"><see cref="CodeModel"/> to use for generated code</param>
        /// <returns><see cref="TargetMachine"/> based on the specified parameters</returns>
        public TargetMachine CreateTargetMachine( Context context
                                                , string triple
                                                , string cpu
                                                , string features
                                                , CodeGenOpt optLevel
                                                , Reloc relocationMode
                                                , CodeModel codeModel
                                                )
        {
            var targetMachineHandle = NativeMethods.CreateTargetMachine( TargetHandle
                                                                       , triple
                                                                       , cpu
                                                                       , features
                                                                       , ( LLVMCodeGenOptLevel )optLevel
                                                                       , ( LLVMRelocMode )relocationMode
                                                                       , ( LLVMCodeModel )codeModel
                                                                       );
            return new TargetMachine( context, targetMachineHandle );
        }

        internal Target( LLVMTargetRef targetHandle )
        {
            TargetHandle = targetHandle;
        }

        internal LLVMTargetRef TargetHandle { get; }

        /// <summary>Retrives an enumerable collection of the available targets built into this library</summary>
        public static IEnumerable<Target> AvailableTargets
        {
            get
            {
                var current = NativeMethods.GetFirstTarget( );
                while( current.Pointer != IntPtr.Zero )
                {
                    yield return FromHandle( current );
                    current = NativeMethods.GetNextTarget( current );
                }
            }
        }

        /// <summary>Gets the target for a given target "triple" value</summary>
        /// <param name="targetTriple">Target triple string describing the target</param>
        /// <returns>Target for the given triple</returns>
        public static Target FromTriple( string targetTriple )
        {
            LLVMTargetRef targetHandle;
            IntPtr msgPtr;
            if( 0 != NativeMethods.GetTargetFromTriple( targetTriple, out targetHandle, out msgPtr ).Value )
            {
                var msg = NativeMethods.MarshalMsg( msgPtr );
                throw new InternalCodeGeneratorException( msg );
            }

            return FromHandle( targetHandle );
        }

        internal static Target FromHandle( LLVMTargetRef targetHandle )
        {
            lock( TargetMap )
            {
                Target retVal;
                if( TargetMap.TryGetValue( targetHandle.Pointer, out retVal ) )
                    return retVal;

                retVal = new Target( targetHandle );
                TargetMap.Add( targetHandle.Pointer, retVal );
                return retVal;
            }
        }

        static Dictionary<IntPtr, Target> TargetMap = new Dictionary<IntPtr, Target>( );
    }
}
