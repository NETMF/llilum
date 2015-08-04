namespace Llvm.NET.Instructions
{
    public class Return
        : Terminator
    {
        internal Return( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Return( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAReturnInst ) )
        {
        }
    }
}
