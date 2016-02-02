using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class SIToFP : Cast
    {
        internal SIToFP( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
