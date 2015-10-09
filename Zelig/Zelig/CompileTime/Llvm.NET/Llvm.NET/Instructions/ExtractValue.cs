namespace Llvm.NET.Instructions
{
    public class ExtractValue
        : UnaryInstruction
    {
        internal ExtractValue( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ExtractValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractValueInst ) )
        {
        }
    }
}
