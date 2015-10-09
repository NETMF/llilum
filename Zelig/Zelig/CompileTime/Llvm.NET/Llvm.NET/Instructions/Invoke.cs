namespace Llvm.NET.Instructions
{
    public class Invoke
        : Terminator
    {
        internal Invoke( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Invoke( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}
