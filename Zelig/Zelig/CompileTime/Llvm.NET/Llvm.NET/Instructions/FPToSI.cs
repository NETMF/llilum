namespace Llvm.NET.Instructions
{
    class FPToSI : Cast
    {
        internal FPToSI( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal FPToSI( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
