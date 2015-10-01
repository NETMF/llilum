using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llvm.NET
{
    /// <summary>Provides a wrapper around an LLVM Pass Manager</summary>
    /// <remarks>This class is still in the experimental stage as there is a lack of full support from the C API</remarks>
    public sealed class PassManagerBuilder
        : IDisposable
    {
        public PassManagerBuilder( )
        {
            PassManagerBuilderHandle = LLVMNative.PassManagerBuilderCreate( );
        }

        public void SetOptLevel( uint optLevel )
        {
            LLVMNative.PassManagerBuilderSetOptLevel( PassManagerBuilderHandle, optLevel );
        }

        public void SetSizeLevel( uint sizeLevel )
        {
            LLVMNative.PassManagerBuilderSetSizeLevel( PassManagerBuilderHandle, sizeLevel );
        }

        public void SetDisableUnitAtATime( bool value )
        {
            LLVMNative.PassManagerBuilderSetDisableUnitAtATime( PassManagerBuilderHandle, value );
        }

        public void SetDisableUnrollLoops( bool value )
        {
            LLVMNative.PassManagerBuilderSetDisableUnrollLoops( PassManagerBuilderHandle, value );
        }

        public void SetDisableSimplifyLibCalls( bool value )
        {
            LLVMNative.PassManagerBuilderSetDisableSimplifyLibCalls( PassManagerBuilderHandle, value );
        }

        //public void PopulateFunctionPassManager( PassManager passManager )
        //{
        //    LLVMNative.PassManagerBuilderPopulateFunctionPassManager( PassManagerBuilderHandle, passManager.PassManagerHandle );
        //}

        //public void PopulateModulePassManager( PassManager passManager )
        //{
        //    LLVMNative.PassManagerBuilderPopulateModulePassManager( PassManagerBuilderHandle, passManager.PassManagerHandle );
        //}

        //public void PopulateLTOPassManager( PassManager passManager, bool internalize, bool runInliner )
        //{
        //    LLVMNative.PassManagerBuilderPopulateLTOPassManager( PassManagerBuilderHandle
        //                                                       , passManager.PassManagerHandle
        //                                                       , internalize
        //                                                       , runInliner
        //                                                       );
        //}

        public void Dispose( )
        {
            if( PassManagerBuilderHandle.Pointer != IntPtr.Zero )
            {
                LLVMNative.PassManagerBuilderDispose( PassManagerBuilderHandle );
                PassManagerBuilderHandle = default( LLVMPassManagerBuilderRef );
            }
        }

        LLVMPassManagerBuilderRef PassManagerBuilderHandle;
    }
}
