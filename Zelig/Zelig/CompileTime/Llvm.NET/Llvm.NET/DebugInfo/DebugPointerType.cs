using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    public class DebugPointerType
        : DebugType<IPointerType, DIDerivedType>
        , IPointerType
    {

        public DebugPointerType( IDebugType<ITypeRef, DIType> debugType, Module module, uint addressSpace = 0, string name = null )
            : this( debugType.NativeType, module, debugType.DIType, addressSpace, name )
        {
        }

        public DebugPointerType( ITypeRef llvmType, Module module, DIType elementType, uint addressSpace = 0, string name = null )
            : this( llvmType.CreatePointerType( addressSpace ), module, elementType, name )
        {
        }

        public DebugPointerType( IPointerType llvmType, Module module, DIType elementType, string name )
            : base( llvmType
                  , module.DIBuilder.CreatePointerType( elementType
                                                      , name
                                                      , module.Layout.BitSizeOf( llvmType )
                                                      , module.Layout.AbiBitAlignmentOf( llvmType )
                                                      )
                  ) 
        {
        }

        public uint AddressSpace => ( ( IPointerType )NativeType ).AddressSpace;

        public ITypeRef ElementType => ( ( IPointerType )NativeType ).ElementType;
    }
}
