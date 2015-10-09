namespace Llvm.NET.Instructions
{
    public class IntCmp
        : Cmp
    {
        internal IntCmp( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal IntCmp( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAICmpInst ) )
        {
        }
    }
}
