using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class FPTrunc : Cast
    {
        internal FPTrunc( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
