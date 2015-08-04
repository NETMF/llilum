using System;

namespace Llvm.NET
{
    /// <summary>LLVM pointer type</summary>
    public class PointerType
        : SequenceType
    {
        /// <summary>Address space the pointer refers to</summary>
        public uint AddressSpace => LLVMNative.GetPointerAddressSpace( TypeHandle );

        internal PointerType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
            if( LLVMNative.GetTypeKind( typeRef ) != LLVMTypeKind.LLVMPointerTypeKind )
                throw new ArgumentException( "Pointer type reference expected", nameof( typeRef ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static PointerType FromHandle( LLVMTypeRef typeRef )
        {
            return ( PointerType )Context.CurrentContext.GetTypeFor( typeRef, ( h ) => new PointerType( h ) );
        }
    }
}
