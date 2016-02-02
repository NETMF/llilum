using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Terminator
        : Instruction
    {
        internal Terminator( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
