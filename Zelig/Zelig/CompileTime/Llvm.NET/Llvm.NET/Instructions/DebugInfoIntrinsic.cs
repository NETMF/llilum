using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class DebugInfoIntrinsic
        : Intrinsic
    {
        internal DebugInfoIntrinsic( LLVMValueRef valueRef )
            : base( valueRef )
        { 
        }
    }
}
