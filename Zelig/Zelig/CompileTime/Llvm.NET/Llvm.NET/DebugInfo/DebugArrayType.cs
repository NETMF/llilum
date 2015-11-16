using System;
using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Provides debug information binding between an <see cref="IArrayType"/>and a <see cref="DICompositeType"/></summary>
    public class DebugArrayType
        : DebugType<IArrayType, DICompositeType>
        , IArrayType
    {
        /// <summary>Creates a new <see cref="DebugArrayType"/></summary>
        /// <param name="llvmType">Underlying LLVM array type to bind debug info to</param>
        /// <param name="elementType">Array element type with debug information</param>
        /// <param name="module">module to use for creating debug information</param>
        /// <param name="count">Number of elements in the array</param>
        /// <param name="lowerBound">Lower bound of the array [default = 0]</param>
        public DebugArrayType( IArrayType llvmType
                             , IDebugType<ITypeRef, DIType> elementType
                             , NativeModule module
                             , uint count
                             , uint lowerBound = 0
                             )
            : base( llvmType
                  , CreateDebugInfoForArray( llvmType, elementType, module, count, lowerBound )
                  )
        {
            if( llvmType.ElementType.TypeHandle != elementType.TypeHandle )
                throw new ArgumentException( "elementType doesn't match array element type" );

            DebugElementType = elementType;
        }

        /// <summary>Constructs a new <see cref="DebugArrayType"/></summary>
        /// <param name="elementType">Type of elements in the array</param>
        /// <param name="module"><see cref="NativeModule"/> to use for the context of the debug information</param>
        /// <param name="count">Number of elements in the array</param>
        /// <param name="lowerBound">Lowerbound value for the array indeces [Default: 0]</param>
        public DebugArrayType( IDebugType<ITypeRef, DIType> elementType, NativeModule module, uint count, uint lowerBound = 0 )
            : this( elementType.CreateArrayType( count )
                  , elementType
                  , module
                  , count
                  , lowerBound
                  )
        {
        }

        /// <summary>Constructs a new <see cref="DebugArrayType"/></summary>
        /// <param name="llvmType">Native LLVM type for the elements</param>
        /// <param name="module"><see cref="NativeModule"/> to use for the context of the debug information</param>
        /// <param name="elementType">Debug type of the array elements</param>
        /// <param name="count">Number of elements in the array</param>
        /// <param name="lowerbound">Lowerbound value for the array indeces [Default: 0]</param>
        public DebugArrayType( IArrayType llvmType, NativeModule module, DIType elementType, uint count, uint lowerbound = 0 )
            : this( DebugType.Create( llvmType.ElementType, elementType), module, count, lowerbound )
        {
        }

        /// <summary>Full <see cref="IDebugType{NativeT, DebugT}"/> type for the elements</summary>
        public IDebugType<ITypeRef, DIType> DebugElementType { get; }

        /// <inheritdoc/>
        public ITypeRef ElementType => DebugElementType;

        ///<inheritdoc/>
        public uint Length => NativeType.Length;

        /// <summary>Lower bound of the array, usually but not always zero</summary>
        public uint LowerBound { get; }

        /// <summary>Resolves a temporary metadata node for the array if full size information wasn't available at creation time</summary>
        /// <param name="layout">Type layout information</param>
        /// <param name="diBuilder">Debug information builder for creating the new debug information</param>
        public void ResolveTemporary( TargetData layout, DebugInfoBuilder diBuilder )
        {
            if( DIType.IsTemporary && !DIType.IsResolved )
            {
                DIType = diBuilder.CreateArrayType( layout.BitSizeOf( NativeType )
                                                  , layout.AbiBitAlignmentOf( NativeType )
                                                  , DebugElementType.DIType
                                                  , diBuilder.CreateSubrange( LowerBound, NativeType.Length )
                                                  );
            }
        }

        private static DICompositeType CreateDebugInfoForArray( IArrayType llvmType
                                                              , IDebugType<ITypeRef, DIType> elementType
                                                              , NativeModule module
                                                              , uint count
                                                              , uint lowerBound
                                                              )
        {
            if( llvmType.IsSized )
            {
                return module.DIBuilder.CreateArrayType( module.Layout.BitSizeOf( llvmType )
                                                       , module.Layout.AbiBitAlignmentOf( llvmType )
                                                       , elementType.DIType
                                                       , module.DIBuilder.CreateSubrange( lowerBound, count )
                                                       );
            }
            else
            {
                return module.DIBuilder.CreateReplaceableCompositeType( Tag.ArrayType, string.Empty, module.DICompileUnit, null, 0 );
            }
        }
    }
}
