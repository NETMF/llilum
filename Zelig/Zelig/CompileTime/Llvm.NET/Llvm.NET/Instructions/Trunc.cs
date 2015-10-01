namespace Llvm.NET.Instructions
{
    public class Trunc
        : Cast
    {
        internal Trunc( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Trunc( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsATruncInst ) )
        {
        }
    }
}
