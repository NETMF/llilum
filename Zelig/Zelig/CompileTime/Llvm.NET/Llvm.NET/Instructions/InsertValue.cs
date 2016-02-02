using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class InsertValue
        : Instruction
    {
        internal InsertValue( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
