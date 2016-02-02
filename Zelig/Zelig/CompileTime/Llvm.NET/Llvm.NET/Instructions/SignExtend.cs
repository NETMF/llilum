using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class SignExtend
        : Cast
    {
        internal SignExtend( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
