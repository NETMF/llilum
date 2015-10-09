using Llvm.NET.Types;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Describes a member/field of a type for creating debug information</summary>
    /// <remarks>
    /// <para>This class is used with <see cref="DebugStructType.SetBody(bool, Module, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
    /// to provide debug information for a type.</para>
    /// <para>In order to support explicit layout structures the members relating to layout are all <see cref="System.Nullable{T}"/>.
    /// When they are null then modules <see cref="Module.Layout"/> target specific layout information is used to determine 
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
        public DebugInfoFlags Flags { get; set; }

        /// <summary>Debug type information for this field</summary>
        public IDebugType<ITypeRef, DIType> Type { get; set; }

        // TODO: Pull these out to a new MemberLayout class type and add a factory extension method for TargetData class to create it. 
        //       This can help enforce the all or nothing rule checks in SetBody(). (e.g. ALL three must be set or none of them. Fuithermore,
        //       if one field has them set all fields must have them set and the native type must be marked as "packed"

        /// <summary>Bit size for the field</summary>
        /// <remarks>If this is null then <see cref="DebugStructType.SetBody(bool, Module, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
        /// will default to using <see cref="Module.Layout"/> to determine the size using the module's target specific layout.
        /// </remarks>
        public uint? BitSize { get; set; }

        /// <summary>Bit alignment for the field</summary>
        /// <remarks>If this is null then <see cref="DebugStructType.SetBody(bool, Module, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
        /// will default to using <see cref="Module.Layout"/> to determine the alignment using the module's target specific layout.
        /// </remarks>
        public uint? BitAlignment { get; set; }

        /// <summary>Bit offset for the field in it's containing type</summary>
        /// <remarks>If this is null then <see cref="DebugStructType.SetBody(bool, Module, DIScope, DIFile, uint, DebugInfoFlags, System.Collections.Generic.IEnumerable{DebugMemberInfo})"/>
        /// will default to using <see cref="Module.Layout"/> to determine the offset using the module's target specific layout.
        /// </remarks>
        public ulong? BitOffset { get; set; }
    }
}
