namespace Llvm.NET.Values
{
    public class BlockAddress : Constant
    {
        internal BlockAddress( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal BlockAddress( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABlockAddress ) )
        {
        }
    }
}
