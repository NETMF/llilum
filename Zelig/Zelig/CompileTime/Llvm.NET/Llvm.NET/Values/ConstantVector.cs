namespace Llvm.NET.Values
{
    public class ConstantVector : Constant
    {
        internal ConstantVector( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantVector( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantVector ) )
        {
        }
    }
}
