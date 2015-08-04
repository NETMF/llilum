namespace Llvm.NET.Instructions
{
    public class Load
        : UnaryInstruction
    {
        public bool IsVolatile
        {
            get { return LLVMNative.GetVolatile( ValueHandle ); }
            set { LLVMNative.SetVolatile( ValueHandle, value ); }
        }

        internal Load( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Load( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsALoadInst ) )
        {
        }
    }
}
