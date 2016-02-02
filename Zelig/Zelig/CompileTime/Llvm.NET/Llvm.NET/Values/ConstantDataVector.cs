using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class ConstantDataVector : ConstantDataSequential
    {
        internal ConstantDataVector( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
