namespace Llvm.NET.Instructions
{
    class SIToFP : Cast
    {
        internal SIToFP( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
