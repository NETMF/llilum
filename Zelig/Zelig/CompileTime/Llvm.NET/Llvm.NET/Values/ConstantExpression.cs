using System;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>WHile techincaly a type in LLVM ConstantExpression is primarily a static factory for Constants</summary>
    public class ConstantExpression
        : Constant
    {
        public Opcode Opcode => (Opcode)LLVMNative.GetConstOpcode( ValueHandle );

        public static Constant IntToPtrExpression( Constant value, TypeRef type )
        {
            if( value.Type.Kind != TypeKind.Integer )
                throw new ArgumentException( "Integer Type expected", nameof( value ) );

            if( !( type is PointerType ) )
                throw new ArgumentException( "pointer type expected", nameof( type ) );

            return Constant.FromHandle( LLVMNative.ConstIntToPtr( value.ValueHandle, type.TypeHandle ) );
        }

        public static Constant BitCast( Constant value, TypeRef toType )
        {
            return Constant.FromHandle( LLVMNative.ConstBitCast( value.ValueHandle, toType.TypeHandle ) );
        }

        internal ConstantExpression( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantExpression( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantExpr ) )
        {
        }

    }
}
