namespace Llvm.NET.Instructions
{
    public class Unreachable
        : Terminator
    {
        internal Unreachable( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
