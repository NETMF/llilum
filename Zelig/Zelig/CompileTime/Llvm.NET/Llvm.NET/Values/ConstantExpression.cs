using System;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>While techincaly a type in LLVM ConstantExpression is primarily a static factory for Constants</summary>
    public class ConstantExpression
        : Constant
    {
        public OpCode Opcode => (OpCode)NativeMethods.GetConstOpcode( ValueHandle );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public static Constant IntToPtrExpression( Constant value, ITypeRef type )
        {
            if( value.NativeType.Kind != TypeKind.Integer )
                throw new ArgumentException( "Integer Type expected", nameof( value ) );

            if( !( type is IPointerType ) )
                throw new ArgumentException( "pointer type expected", nameof( type ) );

            return FromHandle<Constant>( NativeMethods.ConstIntToPtr( value.ValueHandle, type.GetTypeRef() ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public static Constant BitCast( Constant value, ITypeRef toType )
        {
            var handle = NativeMethods.ConstBitCast( value.ValueHandle, toType.GetTypeRef( ) );
            return FromHandle<Constant>( handle );
        }

        internal ConstantExpression( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantExpression( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantExpr ) )
        {
        }
    }
}
