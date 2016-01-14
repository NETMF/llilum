namespace Llvm.NET.Instructions
{
    public class ReturnInstruction
        : Terminator
    {
        internal ReturnInstruction( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAReturnInst ) )
        {
        }
    }
}
