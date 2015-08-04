using System;

namespace Llvm.NET
{
    public class VectorType : SequenceType
    {
        public uint Size => LLVMNative.GetVectorSize( TypeHandle );

        internal VectorType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
            if( LLVMNative.GetTypeKind( typeRef ) != LLVMTypeKind.LLVMVectorTypeKind )
                throw new ArgumentException( "Vector type reference expected", nameof( typeRef ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static VectorType FromHandle( LLVMTypeRef typeRef )
        {
            return ( VectorType )Context.CurrentContext.GetTypeFor( typeRef, ( h ) => new VectorType( h ) );
        }
    }
}
