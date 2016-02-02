using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class BlockAddress : Constant
    {
        internal BlockAddress( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
