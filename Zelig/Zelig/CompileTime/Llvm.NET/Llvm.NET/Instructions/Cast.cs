namespace Llvm.NET.Instructions
{
    public class Cast
        : UnaryInstruction
    {
        internal Cast( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsACastInst ) )
        {
        }
    }
}
