using System;

namespace Llvm.NET.Values
{
    /// <summary>Represents an arbitrary bit width integer constant in LLVM</summary>
    /// <remarks>
    /// Note - for integers, in LLVM, signed or unsigned is not part of the type of
    /// the integer. The distinction between them is determined entirely by the
    /// instructions used on the integer values.
    /// </remarks>
    public class ConstantInt
        : Constant
    {
        /// <summary>Retrieves the value of the constant zero extended to 64 bits</summary>
        public UInt64 ZeroExtendedValue => NativeMethods.ConstIntGetZExtValue( ValueHandle );

        /// <summary>Sign extends the value to a 64 bit value</summary>
        public Int64 SignExtendedValue => NativeMethods.ConstIntGetSExtValue( ValueHandle );

        internal ConstantInt( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantInt( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantInt ) )
        {
        }
    }
}
