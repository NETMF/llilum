using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class FPExt : Cast
    {
        internal FPExt( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
