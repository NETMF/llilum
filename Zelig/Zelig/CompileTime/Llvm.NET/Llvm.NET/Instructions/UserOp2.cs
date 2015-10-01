namespace Llvm.NET.Instructions
{
    class UserOp2 : Instruction
    {
        internal UserOp2( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal UserOp2( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABranchInst ) )
        {
        }
    }
}
