namespace Llvm.NET.Instructions
{
    public class MemSet
        : MemIntrinsic
    {
        internal MemSet( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAMemSetInst ) )
        {
        }
    }
}
