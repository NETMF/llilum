using System;
using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Provides debug information binding between an <see cref="IArrayType"/>and a <see cref="DICompositeType"/></summary>
    public class DebugArrayType
        : DebugType<IArrayType, DICompositeType>
        , IArrayType
    {
        public DebugArrayType( IArrayType llvmType, IDebugType<ITypeRef, DIType> elementType, Module module, uint count, uint lowerBound = 0 )
            : base( llvmType
                  , module.DIBuilder.CreateArrayType( module.Layout.BitSizeOf( llvmType )
                                                    , module.Layout.AbiBitAlignmentOf( llvmType )
                                                    , elementType.DIType
                                                    , module.DIBuilder.CreateSubrange( lowerBound, count )
                                                    )
                  )
        {
            if( llvmType.ElementType.TypeHandle != elementType.TypeHandle )
                throw new ArgumentException( "elementType doesn't match array element type" );

            DebugElementType = elementType;
        }

        /// <summary>Constructs a new <see cref="DebugArrayType"/></summary>
        /// <param name="elementType">Type of elements in the array</param>
        /// <param name="module"><see cref="Module"/> to use for the context of the debug information</param>
        /// <param name="count">Number of elements in the array</param>
        /// <param name="lowerBound">Lowerbound value for the array indeces [Default: 0]</param>
        public DebugArrayType( IDebugType<ITypeRef, DIType> elementType, Module module, uint count, uint lowerBound = 0 )
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
        /// <param name="module"><see cref="Module"/> to use for the context of the debug information</param>
        /// <param name="elementType">Debug type of the array elements</param>
        /// <param name="count">Number of elements in the array</param>
        /// <param name="lowerbound">Lowerbound value for the array indeces [Default: 0]</param>
        public DebugArrayType( IArrayType llvmType, Module module, DIType elementType, uint count, uint lowerbound = 0 )
            : this( DebugType.Create( llvmType.ElementType, elementType), module, count, lowerbound )
        {
        }

        /// <summary>Full <see cref="IDebugType{NativeT, DebugT}"/> type for the elements</summary>
        public IDebugType<ITypeRef, DIType> DebugElementType { get; }

        /// <inheritdoc/>
        public ITypeRef ElementType => DebugElementType;

        ///<inheritdoc/>
        public uint Length => ( ( IArrayType )NativeType ).Length;
    }
}
