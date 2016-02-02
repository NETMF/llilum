using Llvm.NET.Native;
using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    public class Switch
        : Terminator
    {
        /// <summary>Default <see cref="BasicBlock"/>for the switch</summary>
        public BasicBlock Default => BasicBlock.FromHandle( NativeMethods.GetSwitchDefaultDest( ValueHandle ) );

        /// <summary>Adds a new case to the <see cref="Switch"/> instruction</summary>
        /// <param name="onVal">Value for the case to match</param>
        /// <param name="destination">Destination <see cref="BasicBlock"/> if the case matches</param>
        public void AddCase( Value onVal, BasicBlock destination )
        {
            if( onVal == null )
                throw new System.ArgumentNullException( nameof( onVal ) );

            if( destination == null )
                throw new System.ArgumentNullException( nameof( destination ) );

            NativeMethods.AddCase( ValueHandle, onVal.ValueHandle, destination.BlockHandle );
        }

        internal Switch( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }

    }
}
