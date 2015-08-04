namespace Llvm.NET.Instructions
{
    public class Switch
        : Terminator
    {
        public BasicBlock Default => BasicBlock.FromHandle( LLVMNative.GetSwitchDefaultDest( ValueHandle ) );
        public void AddCase( Value onVal, BasicBlock destination )
        {
            LLVMNative.AddCase( ValueHandle, onVal.ValueHandle, destination.BlockHandle );
        }

        internal Switch( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Switch( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsASwitchInst ) )
        {
        }

    }
}
