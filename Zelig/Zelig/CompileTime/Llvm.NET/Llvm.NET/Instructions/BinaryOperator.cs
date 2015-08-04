namespace Llvm.NET.Instructions
{
    public class BinaryOperator
        : Instruction
    {
        internal BinaryOperator( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal BinaryOperator( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABinaryOperator ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal static BinaryOperator FromHandle( LLVMValueRef valueRef, bool preValidated )
        {
            return (BinaryOperator)Context.CurrentContext.GetValueFor( valueRef, ( h ) => new BinaryOperator( h, preValidated ) );
        }
    }
}
