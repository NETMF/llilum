using System;
using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    public class DebugBasicType 
        : DebugType<ITypeRef, DIBasicType>
    {
        /// <summary>Create a debug type for a basic type</summary>
        /// <param name="llvmType">Type to wrap debug information for</param>
        /// <param name="module">Module to use when constructing the debug information</param>
        /// <param name="name">Source language name of the type</param>
        /// <param name="encoding">Encoding for the type</param>
        public DebugBasicType( ITypeRef llvmType, Module module, string name, DiTypeKind encoding )
            : base( ValidateType( llvmType )
                  , module.DIBuilder.CreateBasicType( name
                                                    , module.Layout.BitSizeOf( llvmType )
                                                    , module.Layout.AbiBitAlignmentOf( llvmType )
                                                    , encoding
                                                    )
                  )
        {
        }

        private static ITypeRef ValidateType( ITypeRef typeRef )
        {
            switch( typeRef.Kind )
            {
            case TypeKind.Label:
            case TypeKind.Function:
            case TypeKind.Struct:
            case TypeKind.Array:
            case TypeKind.Pointer:
            case TypeKind.Vector:
            case TypeKind.Metadata:
                throw new ArgumentException( "Expected a primitive type", nameof( typeRef ) );

            default:
                return typeRef;
            }
        }
    }
}
