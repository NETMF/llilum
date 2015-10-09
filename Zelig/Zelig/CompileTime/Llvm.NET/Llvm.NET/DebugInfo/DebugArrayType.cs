using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    public class DebugArrayType
        : DebugType<IArrayType, DICompositeType>
        , IArrayType
    {
        public DebugArrayType( IDebugType<ITypeRef, DIType> elementType, Module module, uint count, uint lowerBound = 0 )
            : this( elementType.CreateArrayType( count ), module, elementType.DIType, count, lowerBound )
        {
        }

        public DebugArrayType( IArrayType llvmType, Module module, DIType elementType, uint count, uint lowerbound = 0 )
            : base( llvmType
                  , module.DIBuilder.CreateArrayType( module.Layout.BitSizeOf( llvmType )
                                                    , module.Layout.AbiBitAlignmentOf( llvmType )
                                                    , elementType
                                                    , module.DIBuilder.CreateSubrange( lowerbound, count )
                                                    )
                  )
        {
        }

        public ITypeRef ElementType => ( ( IArrayType )NativeType ).ElementType;
        public uint Length => ( ( IArrayType )NativeType ).Length;
    }
}
