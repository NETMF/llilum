using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class GetElementPtr
        : Instruction
    {
        internal GetElementPtr( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
