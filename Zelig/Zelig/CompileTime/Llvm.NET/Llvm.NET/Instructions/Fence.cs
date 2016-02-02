using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Fence
        : Instruction
    {
        internal Fence( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
