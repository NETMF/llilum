using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class ReturnInstruction
        : Terminator
    {
        internal ReturnInstruction( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
