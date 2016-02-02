using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class ConstantTokenNone : Constant
    {
        internal ConstantTokenNone( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
