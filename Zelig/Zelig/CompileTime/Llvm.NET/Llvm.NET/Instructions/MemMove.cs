using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class MemMove
        : MemIntrinsic
    {
        internal MemMove( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
