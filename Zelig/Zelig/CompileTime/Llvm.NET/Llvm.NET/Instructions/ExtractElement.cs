using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class ExtractElement
        : Instruction
    {
        internal ExtractElement( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
