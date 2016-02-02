using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class IndirectBranch
        : Terminator
    {
        internal IndirectBranch( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
