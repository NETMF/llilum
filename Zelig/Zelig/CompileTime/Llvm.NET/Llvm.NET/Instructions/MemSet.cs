using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class MemSet
        : MemIntrinsic
    {
        internal MemSet( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
