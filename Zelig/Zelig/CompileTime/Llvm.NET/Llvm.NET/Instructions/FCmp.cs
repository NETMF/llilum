using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class FCmp
        : Cmp
    {
        internal FCmp( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
