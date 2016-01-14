namespace Llvm.NET.Values
{
    public class UndefValue : Constant
    {
        internal UndefValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAUndefValue ) )
        {
        }
    }
}
