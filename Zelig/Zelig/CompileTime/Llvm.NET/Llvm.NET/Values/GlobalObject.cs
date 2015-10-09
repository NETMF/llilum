using System.Runtime.InteropServices;

namespace Llvm.NET.Values
{
    public class GlobalObject 
        : GlobalValue
    {
        internal GlobalObject( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAGlobalObject ) )
        {
        }

        /// <summary>Alignment requirements for this object</summary>
        public uint Alignment
        {
            get
            {
                return NativeMethods.GetAlignment( ValueHandle );
            }
            set
            {
                NativeMethods.SetAlignment( ValueHandle, value );
            }
        }

        /// <summary>Linker section this object belongs to</summary>
        public string Section
        {
            get
            {
                var ptr = NativeMethods.GetSection( ValueHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set
            {
                NativeMethods.SetSection( ValueHandle, value );
            }
        }
    }
}
