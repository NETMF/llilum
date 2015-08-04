using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    /// <summary>LLVM Structural type used for laying out types and computing offsets via the <see cref="Instructions.GetElementPtr"/> instruction</summary>
    public class StructType
        : TypeRef
    {
        /// <summary>Sets the body of the structure</summary>
        /// <param name="packed">Flag to indicate if the body elements are packed (e.g. no padding)</param>
        /// <param name="elements">Optional types of each element</param>
        /// <remarks>
        /// To set the body , at least one element type is required. If none are provided an exception is thrown.
        /// </remarks>
        public void SetBody( bool packed, params TypeRef[ ] elements )
        {
            if( elements.Length == 0 )
                return; //throw new ArgumentException( "There must be at least one field type specified to set the body of a structure" );

            LLVMTypeRef[ ] llvmArgs = elements.Select( e => e.TypeHandle ).ToArray( );

            LLVMNative.StructSetBody( TypeHandle, out llvmArgs[ 0 ], ( uint )llvmArgs.Length, packed );
        }

        /// <summary>Name of the structure</summary>
        public string Name
        {
            get
            {
                var ptr =  LLVMNative.GetStructName( TypeHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
        }

        /// <summary>Indicates if the structure is opaque (e.g. has no body defined yet)</summary>
        public bool IsOpaque => LLVMNative.IsOpaqueStruct( TypeHandle );

        /// <summary>Indicates if the structure is packed (e.g. no automatic alignment padding between elements)</summary>
        public bool IsPacked => LLVMNative.IsPackedStruct( TypeHandle );

        /// <summary>List of types for all member elements of the structure</summary>
        public IReadOnlyList<TypeRef> Members
        {
            get
            {
                var members = new List<TypeRef>( );
                if( Kind == TypeKind.Struct && !IsOpaque )
                {
                    LLVMTypeRef[] structElements = new LLVMTypeRef[ LLVMNative.CountStructElementTypes( TypeHandle ) ];
                    LLVMNative.GetStructElementTypes( TypeHandle, out structElements[ 0 ] );
                    members.AddRange( structElements.Select( TypeRef.FromHandle ) );
                }
                return members;
            }
        }

        internal StructType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static StructType FromHandle( LLVMTypeRef typeRef )
        {
            return ( StructType )Context.CurrentContext.GetTypeFor( typeRef, ( h ) => new StructType( h ) );
        }
    }
}
