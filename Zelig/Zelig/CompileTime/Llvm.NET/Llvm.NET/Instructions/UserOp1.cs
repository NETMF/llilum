namespace Llvm.NET.Instructions
{
    class UserOp1 : Instruction
    {
        internal UserOp1( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
