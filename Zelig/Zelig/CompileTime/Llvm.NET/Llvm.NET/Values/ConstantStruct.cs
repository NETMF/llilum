using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class ConstantStruct : Constant
    {
        internal ConstantStruct( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
