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
            : this( llvmType
                  , module.DIBuilder.CreatePointerType( elementType
                                                      , name
                                                      , module.Layout.BitSizeOf( llvmType )
                                                      , module.Layout.AbiBitAlignmentOf( llvmType )
                                                      )
                  ) 
        {
        }

        /// <summary>Constructs a new <see cref="DebugPointerType"/></summary>
        /// <param name="llvmType">Natice type of the pointee</param>
        /// <param name="debugType">Debug type for the pointer</param>
        /// <remarks>
        /// This constructor is typically used whne building typedefs to a basic type
        /// to provide namespace scoping for the typedef for languages that support
        /// such a concept. This is needed because basic types don't have any namespace
        /// information in the LLVM Debug information (they are implicitly in the global
        /// namespace)
        /// </remarks>
        public DebugPointerType( IPointerType llvmType, DIDerivedType debugType )
            : base( llvmType, debugType )
        {
        }

        /// <inheritdoc/>
        public uint AddressSpace => NativeType.AddressSpace;

        /// <inheritdoc/>
        public ITypeRef ElementType => NativeType.ElementType;
    }
}
