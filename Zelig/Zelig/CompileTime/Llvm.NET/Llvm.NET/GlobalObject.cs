using System.Runtime.InteropServices;

namespace Llvm.NET
{
    public class GlobalObject 
        : GlobalValue
    {
        internal GlobalObject( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAGlobalObject ) )
        {
        }

        /// <summary>Alignment requirements for this object</summary>
        public uint Alignment
        {
            get
            {
                return LLVMNative.GetAlignment( ValueHandle );
            }
            set
            {
                LLVMNative.SetAlignment( ValueHandle, value );
            }
        }

        /// <summary>Linker section this object belongs to</summary>
        public string Section
        {
            get
            {
                var ptr = LLVMNative.GetSection( ValueHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set
            {
                LLVMNative.SetSection( ValueHandle, value );
            }
        }
    }
}
