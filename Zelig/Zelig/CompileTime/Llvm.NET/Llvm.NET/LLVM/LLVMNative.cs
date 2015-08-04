using System;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    // add implicit conversions to/from C# bool for convenience
    internal partial struct LLVMBool
    {
        // sometimes LLVMBool values are actually success/failure codes
        // and thus 0 actually means success and not "false".
        public bool Succeeded => Value == 0;
        public bool Failed => !Succeeded;

        public static implicit operator LLVMBool( bool value ) => new LLVMBool( value ? 1 : 0 );
        public static implicit operator bool( LLVMBool value ) => value.Value != 0;
    }

    internal static partial class LLVMNative
    {
        internal static ValueKind GetValueKind( LLVMValueRef valueRef ) => ( ValueKind )GetValueID( valueRef );

        internal static string MarshalMsg( IntPtr msg )
        {
            var retVal = Marshal.PtrToStringAnsi( msg );
            DisposeMessage( msg );
            return retVal;
        }

#if DYNAMICALLY_LOAD_LIBLLVM
        // force loading the appropriate architecture specific 
        // DLL before any use of the wrapped interop APIs to 
        // allow building this library as ANYCPU
        static LLVMNative()
        {
            var handle = LLvmDllHandle.Value;
            if( handle == IntPtr.Zero )
                throw new InvalidOperationException( "Verification of DLL Load Failed!" );
        }

        private static Lazy<IntPtr> LLvmDllHandle 
            = new Lazy<IntPtr>( ( ) => NativeMethods.LoadWin32Library( "LLVM", libraryPath ), LazyThreadSafetyMode.ExecutionAndPublication );
#endif
    }
}
