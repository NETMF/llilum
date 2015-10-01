namespace Llvm.NET.Values
{
    /// <summary>LLVM Global Alias for a function or global value</summary>
    public class GlobalAlias
        : GlobalValue
    {
        internal GlobalAlias( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal GlobalAlias( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAGlobalAlias ) )
        {
        }
    }
}
