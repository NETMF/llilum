using System;

namespace Llvm.NET
{
    /// <summary>LLVM Sequence type</summary>
    /// <remarks>
    /// Sequence types represent a sequence of elements that are contiguous 
    /// in memory. These include Vectors, Arrays, and pointers
    /// </remarks>
    public class SequenceType
        : TypeRef
    {
        /// <summary>Type of element stored in the sequence</summary>
        public TypeRef ElementType
        {
            get
            {
                var type = LLVMNative.GetElementType( TypeHandle );
                if( type.Pointer == IntPtr.Zero )
                    return null;

                return TypeRef.FromHandle( type );
            }
        }

        internal SequenceType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
            if( !IsSequenceTypeRef( typeRef ) )
                throw new ArgumentException( "Expected a sequence type", nameof( typeRef ) );
        }

        internal static bool IsSequenceTypeRef( LLVMTypeRef typeRef )
        {
            var kind = ( TypeKind )LLVMNative.GetTypeKind( typeRef );
            return kind == TypeKind.Array
                || kind == TypeKind.Vector
                || kind == TypeKind.Pointer;
        }
    }
}
