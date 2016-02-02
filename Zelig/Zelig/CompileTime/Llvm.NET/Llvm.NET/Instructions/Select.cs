using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Select : Instruction
    {
        internal Select( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
