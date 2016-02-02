using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class FPToSI : Cast
    {
        internal FPToSI( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
