namespace Llvm.NET.Instructions
{
    public class IntToPointer
        : Cast
    {
        internal IntToPointer( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAIntToPtrInst ) )
        {
        }
    }
}
