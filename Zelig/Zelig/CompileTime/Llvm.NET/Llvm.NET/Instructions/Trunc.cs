namespace Llvm.NET.Instructions
{
    public class Trunc
        : Cast
    {
        internal Trunc( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsATruncInst ) )
        {
        }
    }
}
