namespace Llvm.NET.Instructions
{
    public class InsertValue
        : Instruction
    {
        internal InsertValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAInsertValueInst ) )
        {
        }
    }
}
