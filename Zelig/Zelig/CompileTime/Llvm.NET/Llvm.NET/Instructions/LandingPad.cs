using Llvm.NET.Values;
using System;

namespace Llvm.NET.Instructions
{
    public class LandingPad
        : Instruction
    {
        internal LandingPad( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }

        public void AddClause(Value clause)
        {
            if( clause == null )
                throw new ArgumentNullException( nameof( clause ) );

            NativeMethods.AddClause(ValueHandle, clause.ValueHandle);
        }

        public void SetCleanup( bool value ) => NativeMethods.SetCleanup( ValueHandle, value );
    }
}
