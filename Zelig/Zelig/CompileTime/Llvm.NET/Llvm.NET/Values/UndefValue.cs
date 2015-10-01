namespace Llvm.NET.Values
{
    public class UndefValue : Constant
    {
        internal UndefValue( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal UndefValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAUndefValue ) )
        {
        }
    }
}
