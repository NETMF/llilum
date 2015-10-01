namespace Llvm.NET.Instructions
{
    public class Store
        : Instruction
    {
        public bool IsVolatile
        {
            get { return LLVMNative.GetVolatile( ValueHandle ); }
            set { LLVMNative.SetVolatile( ValueHandle, value ); }
        }

        internal Store( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Store( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAStoreInst ) )
        {
        }
    }
}
