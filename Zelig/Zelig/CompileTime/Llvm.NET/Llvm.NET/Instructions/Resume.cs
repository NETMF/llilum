using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class ResumeInstruction
        : Terminator
    {
        internal ResumeInstruction( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
