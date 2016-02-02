using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class InsertElement
        : Instruction
    {
        internal InsertElement( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
