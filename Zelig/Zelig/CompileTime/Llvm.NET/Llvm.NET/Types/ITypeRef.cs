using Llvm.NET.Values;
using System;
using Llvm.NET.Native;

namespace Llvm.NET.Types
{
    /// <summary>Interface for a Type in LLVM</summary>
    public interface ITypeRef 
        : IExtensiblePropertyContainer
    {
        /// <summary>LibLLVM handle for the type</summary>
        IntPtr TypeHandle { get; }

        /// <summary>Flag to indicate if the type is sized</summary>
        bool IsSized { get; }
        
        /// <summary>LLVM Type kind for this type</summary>
        TypeKind Kind { get; }

        /// <summary>Flag to indicate if this type is an integer</summary>
        bool IsInteger { get; }

        // Return true if value is 'float', a 32-bit IEEE floating point type.
        bool IsFloat { get; }

        // Return true if this is 'double', a 64-bit IEEE floating point type
        bool IsDouble { get; }

        /// <summary>Flag to indicate if this type represents the void type</summary>
        bool IsVoid { get; }

        /// <summary>Flag to indicate if this type is a structure type</summary>
        bool IsStruct { get; }

        /// <summary>Flag to indicate if this type is a pointer</summary>
        bool IsPointer { get; }

        /// <summary>Flag to indicate if this type is a sequence type</summary>
        bool IsSequence { get; }

        /// <summary>Flag to indicate if this type is a floating point type</summary>
        bool IsFloatingPoint { get; }

        /// <summary>FLag to indicate if this type is a pointer to a pointer</summary>
        bool IsPointerPointer { get; }

        /// <summary>Context that owns this type</summary>
        Context Context { get; }

        /// <summary>Integer bit width of this type or 0 for non integer types</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer" )]
        uint IntegerBitWidth { get; }

        /// <summary>Gets a null value (e.g. all bits == 0 ) for the type</summary>
        /// <remarks>
        /// This is a getter function instead of a property as it can throw exceptions
        /// for types that don't support such a thing (i.e. void )
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        Constant GetNullValue( );

        /// <summary>Array type factory for an array with elements of this type</summary>
        /// <param name="count">Number of elements in the array</param>
        /// <returns><see cref="IArrayType"/> for the array</returns>
        IArrayType CreateArrayType( uint count );

        /// <summary>Get a <see cref="IPointerType"/> for a type that points to elements of this type in the default (0) address space</summary>
        /// <returns><see cref="IPointerType"/>corresponding to the type of a pointer that refers to elements of this type</returns>
        IPointerType CreatePointerType( );

        /// <summary>Get a <see cref="IPointerType"/> for a type that points to elements of this type in the specified address space</summary>
        /// <param name="addressSpace">Address space for the pointer</param>
        /// <returns><see cref="IPointerType"/>corresponding to the type of a pointer that refers to elements of this type</returns>
        IPointerType CreatePointerType( uint addressSpace );
    }

    internal static class TypeRefExtensions
    {
        internal static LLVMTypeRef GetTypeRef( this ITypeRef self ) => new LLVMTypeRef( self.TypeHandle );
    }
}
