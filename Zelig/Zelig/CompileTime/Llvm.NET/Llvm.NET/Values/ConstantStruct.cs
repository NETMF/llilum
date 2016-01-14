namespace Llvm.NET.Values
{
    public class ConstantStruct : Constant
    {
        internal ConstantStruct( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantStruct ) )
        {
        }
    }
}
