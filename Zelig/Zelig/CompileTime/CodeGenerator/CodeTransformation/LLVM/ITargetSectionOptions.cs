namespace Microsoft.Zelig.LLVM
{
    /// <summary>Provides options for generating target data sections</summary>
    /// <remarks>
    /// This interface is implemented on a component provided by the application hosting
    /// the Llilum compilation engine. Typically the host's command line options object
    /// would implement this interface to provide settings parse from the command line. 
    /// </remarks>
    public interface ITargetSectionOptions
    {
        /// <summary>Gets a flag to indicate if section names for blobal data should include the type name</summary>
        /// <remarks><para>If this flag is <see langword="true"/> then the LLVM code generation phase will output
        /// all global data into sections with a name that includes the full type of the data. This is useful
        /// for diagnostics and understanding memory usage. Though it can have an adverse impact on the overall
        /// data size. Since sections have a platform, target and toolchain minimum alignment if there are a
        /// large number of types (and therefore sections) it is possible the alignment requirements for them
        /// could actually increase the size of the image. Thus this is considered a diagnostic option rather 
        /// than a default setting.</para>
        /// <para>If this is <see langword="false"/> then the global data is placed into sections that include
        /// the Llilum name to help identify Llilum specific data. (i.e. for ARM Cortex-M processors readonly 
        /// data would go to .rodata.llilum and writeable data to .data.llilum)</para>
        /// </remarks>
        bool GenerateDataSectionPerType { get; }
    }
}
