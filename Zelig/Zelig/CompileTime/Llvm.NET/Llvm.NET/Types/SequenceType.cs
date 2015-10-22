using System;

namespace Llvm.NET.Types
{
    /// <summary>Interface for an LLVM sequence type</summary>
    /// <remarks>
    /// Sequence types represent a sequence of elements of the same type
    /// that are contiguous in memory. These include Vectors, Arrays, and
    /// pointers.
    /// </remarks>
    public interface ISequenceType
        : ITypeRef
    {
        /// <summary>Type of elements in the sequence</summary>
        ITypeRef ElementType { get; }
    }

    internal class SequenceType
        : TypeRef
        , ISequenceType
    {
        public ITypeRef ElementType
        {
            get
            {
                var typeRef = NativeMethods.GetElementType( this.GetTypeRef() );
                if( typeRef.Pointer == IntPtr.Zero )
                    return null;

                return FromHandle( typeRef );
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
            var kind = ( TypeKind )NativeMethods.GetTypeKind( typeRef );
            return kind == TypeKind.Array
                || kind == TypeKind.Vector
                || kind == TypeKind.Pointer;
        }
    }
}
