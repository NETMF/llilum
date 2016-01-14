namespace Llvm.NET.Instructions
{
    public class FCmp
        : Cmp
    {
        internal FCmp( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAFCmpInst ) )
        {
        }
    }
}
