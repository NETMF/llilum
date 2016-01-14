namespace Llvm.NET.Instructions
{
    public class SignExtend
        : Cast
    {
        internal SignExtend( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsASExtInst ) )
        {
        }
    }
}
