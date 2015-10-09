namespace Llvm.NET.Instructions
{
    public class MemMove
        : MemIntrinsic
    {
        internal MemMove( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAMemMoveInst ) )
        {
        }
    }
}
