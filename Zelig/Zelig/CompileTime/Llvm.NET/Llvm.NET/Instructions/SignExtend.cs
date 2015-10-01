namespace Llvm.NET.Instructions
{
    public class SignExtend
        : Cast
    {
        internal SignExtend( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal SignExtend( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsASExtInst ) )
        {
        }
    }
}
