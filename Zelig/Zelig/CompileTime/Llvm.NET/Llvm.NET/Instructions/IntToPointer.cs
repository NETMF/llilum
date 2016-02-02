using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class IntToPointer
        : Cast
    {
        internal IntToPointer( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
