using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Llvm.NET.Types
{
    /// <summary>Interface for an LLVM structure type</summary>
    public interface IStructType
        : ITypeRef
    {
        /// <summary>Name of the structure</summary>
        string Name { get; }

        /// <summary>Indicates if the structure is opaque (e.g. has no body defined yet)</summary>
        bool IsOpaque { get; }

        /// <summary>Indicates if the structure is packed (e.g. no automatic alignment padding between elements)</summary>
        bool IsPacked { get; }

        /// <summary>List of types for all member elements of the structure</summary>
        IReadOnlyList<ITypeRef> Members { get; }

        /// <summary>Sets the body of the structure</summary>
        /// <param name="packed">Flag to indicate if the body elements are packed (e.g. no padding)</param>
        /// <param name="elements">Optional types of each element</param>
        /// <remarks>
        /// To set the body , at least one element type is required. If none are provided this is a NOP.
        /// </remarks>
        void SetBody( bool packed, params ITypeRef[ ] elements );
    }

    internal class StructType
        : TypeRef
        , IStructType
    {
        public void SetBody( bool packed, params ITypeRef[ ] elements )
        {
            LLVMTypeRef[ ] llvmArgs = elements.Select( e => e.GetTypeRef() ).ToArray( );
            uint argsLength = (uint)llvmArgs.Length;

            // To interop correctly, we need to have an array of at least size one.
            if ( argsLength == 0 )
                llvmArgs = new LLVMTypeRef[ 1 ];

            NativeMethods.StructSetBody( TypeHandle_, out llvmArgs[ 0 ], argsLength, packed );
        }

        public string Name
        {
            get
            {
                var ptr =  NativeMethods.GetStructName( TypeHandle_ );
                return Marshal.PtrToStringAnsi( ptr );
            }
        }

        public bool IsOpaque => NativeMethods.IsOpaqueStruct( TypeHandle_ );
        public bool IsPacked => NativeMethods.IsPackedStruct( TypeHandle_ );
        public IReadOnlyList<ITypeRef> Members
        {
            get
            {
                var members = new List<ITypeRef>( );
                if( Kind == TypeKind.Struct && !IsOpaque )
                {
                    uint count = NativeMethods.CountStructElementTypes( TypeHandle_ );
                    if (count > 0)
                    {
                        LLVMTypeRef[] structElements = new LLVMTypeRef[ count ];
                        NativeMethods.GetStructElementTypes( TypeHandle_, out structElements[ 0 ] );
                        members.AddRange( structElements.Select( h => FromHandle<ITypeRef>( h ) ) );
                    }
                }

                return members;
            }
        }

        internal StructType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
        }
    }
}
