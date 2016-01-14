namespace Llvm.NET.Instructions
{
    public class PointerToInt
        : Cast
    {
        internal PointerToInt( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAPtrToIntInst ) )
        {
        }
    }
}
