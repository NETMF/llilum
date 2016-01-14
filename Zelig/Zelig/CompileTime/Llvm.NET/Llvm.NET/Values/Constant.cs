using System;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>Contains an LLVM Constant value</summary>
    public class Constant
        : User
    {
        /// <summary>Indicates if the constant is a Zero value for the its type</summary>
        public bool IsZeroValue => NativeMethods.IsConstantZeroValue( ValueHandle );

        /// <summary>Create a NULL pointer for a given type</summary>
        /// <param name="typeRef">Type of pointer to create a null vale for</param>
        /// <returns>Constant NULL pointer of the specified type</returns>
        public static Constant NullValueFor( ITypeRef typeRef )
        {
            if( typeRef == null )
                throw new ArgumentNullException( nameof( typeRef ) );

            var kind = typeRef.Kind;
            var structType = typeRef as StructType;
            if( kind == TypeKind.Label || kind == TypeKind.Function || ( structType != null && structType.IsOpaque ) )
                throw new ArgumentException( "Cannot get a Null value for labels, functions and opaque types" );

            return FromHandle<Constant>( NativeMethods.ConstNull( typeRef.GetTypeRef( ) ) );
        }

        /// <summary>Creates a constant instance of <paramref name="typeRef"/> with all bits in the instance set to 1</summary>
        /// <param name="typeRef">Type of value to create</param>
        /// <returns>Constant for the type with all instance bits set to 1</returns>
        public static Constant AllOnesValueFor( ITypeRef typeRef ) => FromHandle<Constant>( NativeMethods.ConstAllOnes( typeRef.GetTypeRef( ) ) );

        /// <summary>Creates an <see cref="Constant"/> representing an undefined value for <paramref name="typeRef"/></summary>
        /// <param name="typeRef">Type to create the undefined value for</param>
        /// <returns>
        /// <see cref="Constant"/> representing an undefined value of <paramref name="typeRef"/>
        /// </returns>
        public static Constant UndefinedValueFor( ITypeRef typeRef ) => FromHandle<Constant>( NativeMethods.GetUndef( typeRef.GetTypeRef( ) ) );

        /// <summary>Create a constant NULL pointer for a given type</summary>
        /// <param name="typeRef">Type of pointer to create a null value for</param>
        /// <returns>Constnat NULL pointer of the specified type</returns>
        public static Constant ConstPointerToNullFor( ITypeRef typeRef ) => FromHandle<Constant>( NativeMethods.ConstPointerNull( typeRef.GetTypeRef( ) ) );

        internal Constant( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Constant( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstant ) )
        {
        }
    }
}
