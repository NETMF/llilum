namespace Llvm.NET.Instructions
{
    class FPToUI : Cast
    {
        internal FPToUI( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal FPToUI( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABranchInst ) )
        {
        }
    }
}
