namespace Llvm.NET.Instructions
{
    class FPExt : Cast
    {
        internal FPExt( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal FPExt( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
