using Llvm.NET;
using Microsoft.Zelig.CodeGeneration.IR;
using Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    /// <summary>Interface for an object file section name provider</summary>
    /// <remarks>
    /// This interface is used to allow toolchain and ABI specific implementations of section names
    /// whithout causing a lot hard coding or conditional coding to handle variations. Once we get
    /// to where Llilum supports multiple target platforms/ABIs there can be multiple implementations
    /// of this interface keyed by the LLVM Target "triple" for the target.
    /// </remarks>
    public interface ISectionNameProvider
    {
        /// <summary>LLVM target triple this provider applies to</summary>
        string TargetTriple { get; }

        /// <summary>Gets the section name for a given <see cref="DataManager.DataDescriptor"/></summary>
        /// <param name="dd">Descriptor for the global data to get a section name for</param>
        /// <returns>Section name the data should go into</returns>
        string GetSectionNameFor( DataManager.DataDescriptor dd );

        /// <summary>Gets the section name for a given <see cref="MethodImplRepresentation"/></summary>
        /// <param name="md">Method to get the section name for</param>
        /// <returns>Name for the section the method should go into</returns>
        string GetSectionNameFor( MethodRepresentation md );
    }

    /// <summary>Section name provider for unknown or underspecified target</summary>
    public class EmptySectionNameProvider
        : ISectionNameProvider
    {
        /// <summary>Constructs a new provider</summary>
        /// <param name="typeSystem">Type system information to use when looking up well known types</param>
        /// <remarks>
        /// The <paramref name="typeSystem"/> parameter is used to construct a section name suffix for
        /// some special types (i.e. VTable, String ).
        /// </remarks>
        public EmptySectionNameProvider(TypeSystem typeSystem)
        {
        }

        /// <inheritdoc/>
        public string TargetTriple => Target.UnknownEabiTriple;

        /// <inheritdoc/>
        public string GetSectionNameFor(MethodRepresentation md)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public string GetSectionNameFor(DataManager.DataDescriptor dd)
        {
            return string.Empty;
        }
    }
    
    /// <summary>Section name provider for unknown or underspecified target</summary>
    public class Win32SectionNameProvider
        : ISectionNameProvider
    {
        /// <summary>Constructs a new provider</summary>
        /// <param name="typeSystem">Type system information to use when looking up well known types</param>
        /// <remarks>
        /// The <paramref name="typeSystem"/> parameter is used to construct a section name suffix for
        /// some special types (i.e. VTable, String ).
        /// </remarks>
        public Win32SectionNameProvider(TypeSystem typeSystem)
        {
        }

        /// <inheritdoc/>
        public string TargetTriple => Target.Win32EabiTriple;

        /// <inheritdoc/>
        public string GetSectionNameFor(MethodRepresentation md)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public string GetSectionNameFor(DataManager.DataDescriptor dd)
        {
            return string.Empty;
        }
    }

    /// <summary>Section name provider for the ARMv7m Thumb2 EABI target</summary>
    public class Thumb2EabiSectionNameProvider
        : ISectionNameProvider
    {
        /// <summary>Constructs a new provider</summary>
        /// <param name="typeSystem">Type system information to use when looking up well known types</param>
        /// <remarks>
        /// The <paramref name="typeSystem"/> parameter is used to construct a section name suffix for
        /// some special types (i.e. VTable, String ).
        /// </remarks>
        public Thumb2EabiSectionNameProvider( TypeSystem typeSystem )
        {
            m_TypeSystem = typeSystem;
        }

        /// <inheritdoc/>
        public string TargetTriple => Target.ThumbV7mEabiTriple;

        /// <inheritdoc/>
        public string GetSectionNameFor( MethodRepresentation md )
        {
            // TODO: Check method representation for custom attributes etc...
            // to get any special section names for this method. For now,
            // hardcode all to a custom text section as Llilum doesn't currently
            // have an attribute dedicated to specifying a section.
            return LlilumTextSectionName;
        }

        /// <inheritdoc/>
        public string GetSectionNameFor( DataManager.DataDescriptor dd )
        {
            var section = dd.IsMutable ? LlilumRWDataSectionName : LlilumRODataSectionName;
            return $"{section}{GetSectionSuffixForType( dd.Context )}";
        }

        /// <summary>Computes a section name suffix for</summary>
        /// <param name="type">type to get the suffix for</param>
        /// <returns>suffix for the type or <see cref="string.Empty"/> if no special suffix required</returns>
        private string GetSectionSuffixForType( TypeRepresentation type )
        {
            if( type == m_TypeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_VTable )
            {
                return LlilumVTableSectionNameSuffix;
            }
            else if( type == m_TypeSystem.WellKnownTypes.System_String )
            {
                return LlilumStringSectionNameSuffix;
            }
            // [Lather, rinse, repeat for any additional special type section naming...]
            else
            {
                var options = m_TypeSystem.GetEnvironmentService<ITargetSectionOptions>( );
                if( options != null && options.GenerateDataSectionPerType )
                {
                    // use full type name for all other types of data
                    // Replace any [] in the name with _SZARRAY to prevent issues with invalid section names
                    return "." + type.FullName.Replace( "[]", "_SZARRAY" );
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private readonly TypeSystem m_TypeSystem;

        private const string LlilumTextSectionName          = ".text.llilum";
        private const string LlilumRWDataSectionName        = ".data.llilum";
        private const string LlilumRODataSectionName        = ".rodata.llilum";
        private const string LlilumVTableSectionNameSuffix  = ".VTable";
        private const string LlilumStringSectionNameSuffix  = ".String";
    }
}
