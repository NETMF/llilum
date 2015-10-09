namespace Llvm.NET.Values
{
    public class ConstantAggregateZero : Constant
    {
        internal ConstantAggregateZero( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantAggregateZero( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantAggregateZero ) )
        {
        }
    }
}
