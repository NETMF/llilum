using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Binding between a <see cref="DIDerivedType"/> and an <see cref="IPointerType"/></summary>
    public class DebugPointerType
        : DebugType<IPointerType, DIDerivedType>
        , IPointerType
    {

        /// <summary>Constructs a new <see cref="DebugPointerType"/></summary>
        /// <param name="debugType">Debug type of the pointee</param>
        /// <param name="module"><see cref="Module"/> used for creating the pointer type and debug information</param>
        /// <param name="addressSpace">Target address space for the pointer [Default: 0]</param>
        /// <param name="name">Name of the type [Default: null]</param>
        public DebugPointerType( IDebugType<ITypeRef, DIType> debugType, Module module, uint addressSpace = 0, string name = null )
            : this( debugType.NativeType, module, debugType.DIType, addressSpace, name )
        {
        }

        /// <summary>Constructs a new <see cref="DebugPointerType"/></summary>
        /// <param name="llvmType">Native type of the pointee</param>
        /// <param name="module"><see cref="Module"/> used for creating the pointer type and debug information</param>
        /// <param name="elementType">Debug type of the pointee</param>
        /// <param name="addressSpace">Target address space for the pointer [Default: 0]</param>
        /// <param name="name">Name of the type [Default: null]</param>
        public DebugPointerType( ITypeRef llvmType, Module module, DIType elementType, uint addressSpace = 0, string name = null )
            : this( llvmType.CreatePointerType( addressSpace ), module, elementType, name )
        {
        }

        /// <summary>Constructs a new <see cref="DebugPointerType"/></summary>
        /// <param name="llvmType">Native type of the pointee</param>
        /// <param name="module"><see cref="Module"/> used for creating the pointer type and debug information</param>
        /// <param name="elementType">Debug type of the pointee</param>
        /// <param name="name">Name of the type [Default: null]</param>
        public DebugPointerType( IPointerType llvmType, Module module, DIType elementType, string name = null)
            : base( llvmType
                  , module.DIBuilder.CreatePointerType( elementType
                                                      , name
                                                      , module.Layout.BitSizeOf( llvmType )
                                                      , module.Layout.AbiBitAlignmentOf( llvmType )
                                                      )
                  ) 
        {
        }

        /// <inheritdoc/>
        public uint AddressSpace => NativeType.AddressSpace;

        /// <inheritdoc/>
        public ITypeRef ElementType => NativeType.ElementType;
    }
}
