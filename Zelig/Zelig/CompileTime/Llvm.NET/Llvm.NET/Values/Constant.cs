using System;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>Contains an LLVM Constant value</summary>
    public class Constant 
        : User
    {
        /// <summary>Indicates if the constant is a Zero value for the its type</summary>
        public bool IsZeroValue => LLVMNative.IsConstantZeroValue( ValueHandle );
        
        /// <summary>Create a NULL pointer for a given type</summary>
        /// <param name="typeRef">Type of pointer to create a null vale for</param>
        /// <returns>Constnat NULL pointer of the specified type</returns>
        public static Constant NullValueFor( TypeRef typeRef )
        {
            var kind = typeRef.Kind;
            var structType = typeRef as StructType;
            if( kind == TypeKind.Label || kind == TypeKind.Function || ( structType != null && structType.IsOpaque ) )
                throw new ArgumentException( "Cannot get a Null value for labels, functions and opaque types" );

            return FromHandle( LLVMNative.ConstNull( typeRef.TypeHandle ) );
        }

        /// <summary>Creates a constant instance of <paramref name="typeRef"/> with all bits in the instance set to 1</summary>
        /// <param name="typeRef">Type of value to create</param>
        /// <returns>Constant for the type with all instance bits set to 1</returns>
        public static Constant AllOnesValueFor( TypeRef typeRef ) => FromHandle( LLVMNative.ConstAllOnes( typeRef.TypeHandle ) );

        /// <summary>Creates an <see cref="Constant"/> representing an undefined value for <paramref name="typeRef"/></summary>
        /// <param name="typeRef">Type to create the undefined value for</param>
        /// <returns>
        /// <see cref="Constant"/> representing an undefined value of <paramref name="typeRef"/>
        /// </returns>
        public static Constant UndefinedValueFor( TypeRef typeRef ) => FromHandle( LLVMNative.GetUndef( typeRef.TypeHandle ) );
        
        /// <summary>Create a constant NULL pointer for a given type</summary>
        /// <param name="typeRef">Type of pointer to create a null vale for</param>
        /// <returns>Constnat NULL pointer of the specified type</returns>
        public static Constant ConstPointerToNullFor( TypeRef typeRef ) => FromHandle( LLVMNative.ConstPointerNull( typeRef.TypeHandle ) );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static Constant FromHandle( LLVMValueRef valueRef )
        {
            return ( Constant )Context.CurrentContext.GetValueFor( valueRef, ( h ) => new Constant( h ) );
        }

        internal Constant( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Constant( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstant ) )
        {
        }
    }
}
