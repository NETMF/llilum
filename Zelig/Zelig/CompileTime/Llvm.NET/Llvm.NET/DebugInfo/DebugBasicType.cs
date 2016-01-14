using System;
using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Debug information binding between an LLVM native <see cref="ITypeRef"/> and a <see cref="DIBasicType"/></summary>
    /// <remarks>
    /// This class provides a binding between an LLVM type and a corresponding <see cref="DIBasicType"/>.
    /// In LLVM all primitive types are unnamed and interned. That is, any use of an i8 is always the same
    /// type. However, at the source language level it is common to have named primitive types that map 
    /// to the same underlying LLVM. For example, in C and C++ char maps to i8 but so does unsigned char
    /// (LLVM integral types don't have signed vs unsigned). This class is designed to handle this sort
    /// of one to many mapping of the lower level LLVM types to source level debugging types. Each
    /// instance of this class represents a source level basic type and the corresponding representation
    /// for LLVM.
    /// </remarks>
    public class DebugBasicType 
        : DebugType<ITypeRef, DIBasicType>
    {
        /// <summary>Create a debug type for a basic type</summary>
        /// <param name="llvmType">Type to wrap debug information for</param>
        /// <param name="module">Module to use when constructing the debug information</param>
        /// <param name="name">Source language name of the type</param>
        /// <param name="encoding">Encoding for the type</param>
        public DebugBasicType( ITypeRef llvmType, NativeModule module, string name, DiTypeKind encoding )
            : base( ValidateType( llvmType )
                  , module.VerifyArgNotNull( nameof(module) )
                          .DIBuilder
                          .CreateBasicType( name
                                          , module.VerifyArgNotNull( nameof( module ) ).Layout.BitSizeOf( llvmType )
                                          , module.VerifyArgNotNull( nameof( module ) ).Layout.AbiBitAlignmentOf( llvmType )
                                          , encoding
                                          )
                  )
        {
        }

        // Fluent style argument validator to verify arguments before passing to base class.
        // Only primitive types are supported.
        private static ITypeRef ValidateType( ITypeRef typeRef )
        {
            if( typeRef == null )
                throw new ArgumentNullException( nameof( typeRef ) );

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
