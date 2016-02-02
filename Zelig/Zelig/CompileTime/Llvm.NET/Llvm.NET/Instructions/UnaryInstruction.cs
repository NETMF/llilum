using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class UnaryInstruction
        : Instruction
    {
        internal UnaryInstruction( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
