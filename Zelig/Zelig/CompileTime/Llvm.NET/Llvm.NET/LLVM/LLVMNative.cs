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

    internal partial struct LLVMMetadataRef
    {
        internal static LLVMMetadataRef Zero = new LLVMMetadataRef( IntPtr.Zero );
    }

    internal static partial class LLVMNative
    {
        internal static ValueKind GetValueKind( LLVMValueRef valueRef ) => ( ValueKind )GetValueID( valueRef );

        internal static string MarshalMsg( IntPtr msg )
        {
            var retVal = string.Empty;
            if( msg != IntPtr.Zero )
            {
                try
                {
                    retVal = Marshal.PtrToStringAnsi( msg );
                }
                finally
                {
                    DisposeMessage( msg );
                }
            }
            return retVal;
        }

#if DYNAMICALLY_LOAD_LIBLLVM
        // force loading the appropriate architecture specific 
        // DLL before any use of the wrapped inter-op APIs to 
        // allow building this library as ANYCPU
        static LLVMNative()
        {
            var handle = NativeMethods.LoadWin32Library( "LibLLVM", libraryPath );
            if( handle == IntPtr.Zero )
                throw new InvalidOperationException( "Verification of DLL Load Failed!" );
        }
#endif
    }
}
