using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class CatchPad
        : FuncletPad
    {
        internal CatchPad( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
