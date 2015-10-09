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
            NativeMethods.InitializeCore( PassRegistryHandle.Value );
        }

        public static void InitializeTransformUtils()
        {
            NativeMethods.InitializeTransformUtils( PassRegistryHandle.Value );
        }

        public static void InitializeScalarOpts()
        {
            NativeMethods.InitializeScalarOpts( PassRegistryHandle.Value );
        }

        public static void InitializeObjCARCOpts()
        {
            NativeMethods.InitializeObjCARCOpts( PassRegistryHandle.Value );
        }

        public static void InitializeVectorization()
        {
            NativeMethods.InitializeVectorization( PassRegistryHandle.Value );
        }

        public static void InitializeInstCombine( )
        {
            NativeMethods.InitializeInstCombine( PassRegistryHandle.Value );
        }

        public static void InitializeIPO( )
        {
            NativeMethods.InitializeIPO( PassRegistryHandle.Value );
        }

        public static void InitializeInstrumentation( )
        {
            NativeMethods.InitializeInstrumentation( PassRegistryHandle.Value );
        }

        public static void InitializeAnalysis( )
        {
            NativeMethods.InitializeAnalysis( PassRegistryHandle.Value );
        }

        public static void InitializeIPA( )
        {
            NativeMethods.InitializeIPA( PassRegistryHandle.Value );
        }

        public static void InitializeCodeGen( )
        {
            NativeMethods.InitializeCodeGen( PassRegistryHandle.Value );
        }

        public static void InitializeTarget( )
        {
            NativeMethods.InitializeTarget( PassRegistryHandle.Value );
        }

        private static Lazy<LLVMPassRegistryRef> PassRegistryHandle
            = new Lazy<LLVMPassRegistryRef>( ( ) => NativeMethods.GetGlobalPassRegistry( ), LazyThreadSafetyMode.ExecutionAndPublication );
    }
}
