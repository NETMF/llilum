using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class ConstantAggregateZero : Constant
    {
        internal ConstantAggregateZero( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
