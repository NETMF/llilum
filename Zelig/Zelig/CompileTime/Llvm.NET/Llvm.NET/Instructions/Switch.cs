using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    public class Switch
        : Terminator
    {
        public BasicBlock Default => BasicBlock.FromHandle( NativeMethods.GetSwitchDefaultDest( ValueHandle ) );
        public void AddCase( Value onVal, BasicBlock destination )
        {
            NativeMethods.AddCase( ValueHandle, onVal.ValueHandle, destination.BlockHandle );
        }

        internal Switch( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Switch( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsASwitchInst ) )
        {
        }

    }
}
