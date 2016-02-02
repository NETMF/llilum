using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class FuncletPad
        : Instruction
    {
        internal FuncletPad( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
