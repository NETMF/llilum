namespace Llvm.NET.Instructions
{
    public class Store
        : Instruction
    {
        public bool IsVolatile
        {
            get { return NativeMethods.GetVolatile( ValueHandle ); }
            set { NativeMethods.SetVolatile( ValueHandle, value ); }
        }

        internal Store( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Store( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAStoreInst ) )
        {
        }
    }
}
