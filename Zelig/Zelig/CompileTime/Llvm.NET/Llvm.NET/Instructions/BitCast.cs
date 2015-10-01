namespace Llvm.NET.Instructions
{
    public class BitCast
        : Cast
    {
        internal BitCast( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal BitCast( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABitCastInst ) )
        {
        }
    }
}
