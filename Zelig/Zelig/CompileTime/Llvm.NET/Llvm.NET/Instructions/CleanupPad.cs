using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class CleanupPad
        : FuncletPad
    {
        internal CleanupPad( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
