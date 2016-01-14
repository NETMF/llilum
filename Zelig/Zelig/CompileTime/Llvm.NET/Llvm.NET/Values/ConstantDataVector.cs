namespace Llvm.NET.Values
{
    public class ConstantDataVector : ConstantDataSequential
    {
        internal ConstantDataVector( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantDataVector ) )
        {
        }
    }
}
