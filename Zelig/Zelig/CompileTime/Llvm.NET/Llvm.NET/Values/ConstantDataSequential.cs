using System;
using System.Runtime.InteropServices;

namespace Llvm.NET.Values
{
    /// <summary>
    /// A vector or array constant whose element type is a simple 1/2/4/8-byte integer
    /// or float/double, and whose elements are just  simple data values
    /// (i.e. ConstantInt/ConstantFP).
    /// </summary>
    /// <remarks>
    /// This Constant node has no operands because
    /// it stores all of the elements of the constant as densely packed data, instead
    /// of as <see cref="Value"/>s
    /// </remarks>
    public class ConstantDataSequential : Constant
    {
        public bool IsString => LLVMNative.IsConstantString( ValueHandle );

        public string GetAsString()
        {
            if( !IsString )
                throw new InvalidOperationException( "ConstantDataSequential is not a string" );

            int len;
            var strPtr = LLVMNative.GetAsString( ValueHandle, out len );
            return Marshal.PtrToStringAnsi( strPtr, len );
        }

        internal ConstantDataSequential( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantDataSequential( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantDataSequential ) )
        {
        }
    }
}
