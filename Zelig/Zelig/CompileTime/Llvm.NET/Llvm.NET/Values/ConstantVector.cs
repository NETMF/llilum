using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class ConstantVector : Constant
    {
        internal ConstantVector( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
