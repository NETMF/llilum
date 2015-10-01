using System;
using Llvm.NET.Types;

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
        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 1</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( bool constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.BoolType.TypeHandle, ( ulong )( constValue ? 1 : 0 ), new LLVMBool( 0 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 8</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( byte constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int8Type.TypeHandle, ( ulong )constValue, new LLVMBool( 0 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 8</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( sbyte constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int8Type.TypeHandle, ( ulong )constValue, new LLVMBool( 1 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 16</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( Int16 constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int16Type.TypeHandle, ( ulong )constValue, new LLVMBool( 1 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 16</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( UInt16 constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int16Type.TypeHandle, ( ulong )constValue, new LLVMBool( 0 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 32</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( Int32 constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int32Type.TypeHandle, ( ulong )constValue, new LLVMBool( 1 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 32</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( UInt32 constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int32Type.TypeHandle, constValue, new LLVMBool( 0 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( Int64 constValue )
        {
            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int64Type.TypeHandle, ( ulong )constValue, new LLVMBool( 1 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static Constant From( UInt64 constValue )
        {
            return (Constant)Value.FromHandle( LLVMNative.ConstInt( Context.CurrentContext.Int64Type.TypeHandle, constValue, new LLVMBool( 0 ) ) );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="bitWidth">Bit width of the integer</param>
        /// <param name="constValue">Value for the constant</param>
        /// <param name="signExtend">flag to indicate if the const value should be sign extended</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public static Constant From( uint bitWidth, UInt64 constValue, bool signExtend )
        {
            return From( Context.CurrentContext, bitWidth, constValue, signExtend );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="context">Context to create the constant in</param>
        /// <param name="bitWidth">Bit width of the integer</param>
        /// <param name="constValue">Value for the constant</param>
        /// <param name="signExtend">flag to indicate if the const value should be sign extended</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public static Constant From( Context context, uint bitWidth, UInt64 constValue, bool signExtend )
        {
            var intType = context.GetIntType( bitWidth );
            return From( intType, constValue, signExtend );
        }

        public static Constant From( TypeRef intType, UInt64 constValue, bool signExtend )
        {
            if( intType.Kind != TypeKind.Integer )
                throw new ArgumentException( "Integer type required", nameof( intType ) );

            return ( Constant )Value.FromHandle( LLVMNative.ConstInt( intType.TypeHandle, constValue, signExtend ) );
        }

        /// <summary>Retrieves the value of the constant zero extended to 64 bits</summary>
        public UInt64 ZeroExtendedValue => LLVMNative.ConstIntGetZExtValue( ValueHandle );

        /// <summary>Sign extends the value to a 64 bit value</summary>
        public Int64 SignExtendedValue => LLVMNative.ConstIntGetSExtValue( ValueHandle );

        internal ConstantInt( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantInt( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantInt ) )
        {
        }
    }
}
