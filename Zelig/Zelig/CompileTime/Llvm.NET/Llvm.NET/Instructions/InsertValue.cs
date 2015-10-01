namespace Llvm.NET.Instructions
{
    public class InsertValue
        : Instruction
    {
        internal InsertValue( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal InsertValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAInsertValueInst ) )
        {
        }
    }
}
