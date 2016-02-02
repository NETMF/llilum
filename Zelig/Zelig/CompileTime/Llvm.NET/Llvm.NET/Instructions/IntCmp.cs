using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class IntCmp
        : Cmp
    {
        internal IntCmp( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
