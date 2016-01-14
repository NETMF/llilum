namespace Llvm.NET.Values
{
    public class ConstantDataArray : ConstantDataSequential
    {
        internal ConstantDataArray( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantDataArray ) )
        {
        }
    }
}
