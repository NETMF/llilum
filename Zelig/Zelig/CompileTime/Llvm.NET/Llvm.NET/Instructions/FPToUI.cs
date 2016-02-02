using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class FPToUI : Cast
    {
        internal FPToUI( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
