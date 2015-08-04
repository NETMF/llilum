using System;
using System.Threading;

namespace Llvm.NET
{
    public static class GlobalPassRegistry
    {

        public static void InitializeAll()
        {
           InitializeCore( );
           InitializeTransformUtils( );
           InitializeScalarOpts( );
           InitializeObjCARCOpts( );
           InitializeVectorization( );
           InitializeInstCombine( );
           InitializeIPO( );
           InitializeInstrumentation( );
           InitializeAnalysis( );
           InitializeIPA( );
           InitializeCodeGen( );
           InitializeTarget( );
        }

        public static void InitializeCore()
        {
            LLVMNative.InitializeCore( PassRegistryHandle.Value );
        }

        public static void InitializeTransformUtils()
        {
            LLVMNative.InitializeTransformUtils( PassRegistryHandle.Value );
        }

        public static void InitializeScalarOpts()
        {
            LLVMNative.InitializeScalarOpts( PassRegistryHandle.Value );
        }

        public static void InitializeObjCARCOpts()
        {
            LLVMNative.InitializeObjCARCOpts( PassRegistryHandle.Value );
        }

        public static void InitializeVectorization()
        {
            LLVMNative.InitializeVectorization( PassRegistryHandle.Value );
        }

        public static void InitializeInstCombine( )
        {
            LLVMNative.InitializeInstCombine( PassRegistryHandle.Value );
        }

        public static void InitializeIPO( )
        {
            LLVMNative.InitializeIPO( PassRegistryHandle.Value );
        }

        public static void InitializeInstrumentation( )
        {
            LLVMNative.InitializeInstrumentation( PassRegistryHandle.Value );
        }

        public static void InitializeAnalysis( )
        {
            LLVMNative.InitializeAnalysis( PassRegistryHandle.Value );
        }

        public static void InitializeIPA( )
        {
            LLVMNative.InitializeIPA( PassRegistryHandle.Value );
        }

        public static void InitializeCodeGen( )
        {
            LLVMNative.InitializeCodeGen( PassRegistryHandle.Value );
        }

        public static void InitializeTarget( )
        {
            LLVMNative.InitializeTarget( PassRegistryHandle.Value );
        }

        private static Lazy<LLVMPassRegistryRef> PassRegistryHandle
            = new Lazy<LLVMPassRegistryRef>( ( ) => LLVMNative.GetGlobalPassRegistry( ), LazyThreadSafetyMode.ExecutionAndPublication );
    }
}
