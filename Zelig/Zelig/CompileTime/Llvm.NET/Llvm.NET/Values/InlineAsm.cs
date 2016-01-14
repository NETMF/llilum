namespace Llvm.NET.Values
{
    public enum AsmDialect
    {
        ATT,
        Intel
    }

    public class InlineAsm : Value
    {
        internal InlineAsm( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAInlineAsm ) )
        {
        }

        //bool HasSideEffects => LLVMNative.HasSideEffects( ValueHandle );
        //bool IsAlignStack => LLVMNative.IsAlignStack( ValueHandle );
        //AsmDialect Dialect => LLVMNative.GetAsmDialect( ValueHandle );
    }
}
