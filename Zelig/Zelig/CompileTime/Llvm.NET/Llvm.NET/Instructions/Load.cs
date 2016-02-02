using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Load
        : UnaryInstruction
    {
        public bool IsVolatile
        {
            get { return NativeMethods.GetVolatile( ValueHandle ); }
            set { NativeMethods.SetVolatile( ValueHandle, value ); }
        }

        internal Load( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
