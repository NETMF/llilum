using System;

namespace Llvm.NET.Types
{
    public interface ISequenceType
        : ITypeRef
    {
        ITypeRef ElementType { get; }
    }

    /// <summary>LLVM Sequence type</summary>
    /// <remarks>
    /// Sequence types represent a sequence of elements that are contiguous 
    /// in memory. These include Vectors, Arrays, and pointers
    /// </remarks>
    internal class SequenceType
        : TypeRef
        , ISequenceType
    {
        /// <summary>Type of element stored in the sequence</summary>
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
