using System;

namespace Llvm.NET
{
    /// <summary>Array type definition</summary>
    /// <remarks>
    /// Array's in LLVM are fixed length sequences of elements
    /// </remarks>
    public class ArrayType
        : SequenceType
    {
        /// <summary>Length of the array</summary>
        public uint Length => LLVMNative.GetArrayLength( TypeHandle );

        internal ArrayType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
            if( LLVMNative.GetTypeKind( typeRef ) != LLVMTypeKind.LLVMArrayTypeKind )
                throw new ArgumentException( "Array type reference expected", nameof( typeRef ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static ArrayType FromHandle( LLVMTypeRef typeRef )
        {
            return ( ArrayType )Context.CurrentContext.GetTypeFor( typeRef, ( h ) => new ArrayType( h ) );
        }
    }
}
