using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class MemIntrinsic
        : Intrinsic
    {
        internal MemIntrinsic( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
