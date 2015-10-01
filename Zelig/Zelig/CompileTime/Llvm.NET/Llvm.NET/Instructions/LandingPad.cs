namespace Llvm.NET.Instructions
{
    public class LandingPad
        : Instruction
    {
        internal LandingPad( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal LandingPad( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAExtractElementInst ) )
        {
        }
    }
}
