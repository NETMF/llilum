using System.Runtime.InteropServices;
using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class GlobalObject 
        : GlobalValue
    {
        internal GlobalObject( LLVMValueRef valueRef )
            : base( valueRef )
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
