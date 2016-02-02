using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class UIToFP : Cast
    {
        internal UIToFP( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
