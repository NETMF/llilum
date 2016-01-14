using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>DebugMemberLayout is used to define custom layout information for structure members</summary>
    /// <remarks>
    /// Ordinarily layout information is handle automatically in
    /// <see cref="DebugStructType.SetBody(bool, NativeModule, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
    /// however in cases where explicitly controlled (or "packed") layout is required, instances of DebugMemberLayout are
    /// used to provide the information necessary to generate a proper type and debug information.
    /// </remarks>
    public class DebugMemberLayout
    {
        /// <summary>Constructs a new <see cref="DebugMemberLayout"/></summary>
        /// <param name="bitSize">Size of the member in bits</param>
        /// <param name="bitAlignment">Alignment of the member in bits</param>
        /// <param name="bitOffset">Offset of the member in bits</param>
        public DebugMemberLayout( uint bitSize, uint bitAlignment, ulong bitOffset )
        {
            BitSize = bitSize;
            BitAlignment = bitAlignment;
            BitOffset = bitOffset;
        }

        /// <summary>Bit size for the field</summary>
        public uint BitSize { get; }

        /// <summary>Bit alignment for the field</summary>
        public uint BitAlignment { get; }

        /// <summary>Bit offset for the field in it's containing type</summary>
        public ulong BitOffset { get; }
    }

    /// <summary>Describes a member/field of a type for creating debug information</summary>
    /// <remarks>
    /// <para>This class is used with <see cref="DebugStructType.SetBody(bool, NativeModule, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
    /// to provide debug information for a type.</para>
    /// <para>In order to support explicit layout structures the members relating to layout are all <see cref="System.Nullable{T}"/>.
    /// When they are null then modules <see cref="NativeModule.Layout"/> target specific layout information is used to determine 
    /// layout details. Setting the layout members of this class to non-null will override that behavior to define explicit
    /// layout details.</para>
    /// </remarks>
    public class DebugMemberInfo
    {
        /// <summary>LLVM structure element index this descriptor describes</summary>
        public uint Index { get; set; }

        /// <summary>Name of the field</summary>
        public string Name { get; set; }

        /// <summary>File the field is declared in</summary>
        public DIFile File { get; set; }

        /// <summary>Line the field is declared on</summary>
        public uint Line { get; set; }

        /// <summary>flags for the field declaration</summary>
        public DebugInfoFlags DebugInfoFlags { get; set; }

        /// <summary>Debug type information for this field</summary>
        public IDebugType<ITypeRef, DIType> DebugType { get; set; }

        /// <summary>Provides explicit layout information for this member</summary>
        /// <remarks>If this is null then <see cref="DebugStructType.SetBody(bool, NativeModule, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
        /// will default to using <see cref="NativeModule.Layout"/> to determine the size using the module's target specific layout.
        /// <note type="note">
        /// If this property is provided (e.g. is not <see langword="null"/>) for any member of a type, then
        /// it must be set for all members. In other words explicit layout must be defined for all members
        /// or none. Furthermore, for types using explicit layout, the type containing this member must
        /// include the "packed" modifier. 
        /// </note>
        /// </remarks>
        public DebugMemberLayout ExplicitLayout { get; set; }
    }
}
