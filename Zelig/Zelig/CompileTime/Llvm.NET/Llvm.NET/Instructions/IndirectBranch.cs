namespace Llvm.NET.Instructions
{
    public class IndirectBranch
        : Terminator
    {
        internal IndirectBranch( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal IndirectBranch( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
