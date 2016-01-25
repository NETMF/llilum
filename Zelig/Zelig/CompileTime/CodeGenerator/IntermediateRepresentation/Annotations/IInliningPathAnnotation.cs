using Microsoft.Zelig.Debugging;
using Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.CodeGeneration.IR
{
    /// <summary>This interface is used to provide inlined details information for a <see cref="VariableExpression"/></summary>
    /// <remarks>
    /// This is done as an interface to break circular dependency between the IntermediateRepresentation
    /// and CodeTransformation assemblies. It is expected, though technically not required, that this
    /// interface is implemented on Microsoft.Zelig.CodeGeneration.IR.InliningPathAnnotation to allow
    /// tracking where a variable/argument was inlined from and into.
    /// </remarks>
    public interface IInliningPathAnnotation
    {
        /// <summary>Applies transformations for the inlined path information</summary>
        /// <param name="context">Transformation context</param>
        void ApplyTransformation(TransformationContextForIR context);

        /// <summary>Method path for an inlined operator</summary>
        /// <remarks>
        /// The last entry in the list contains the original source method for the operator this annotation is attached to.
        /// <para>It is important to note that for compatability reasons this is an array, however callers should consider
        /// the contents as readonly and not modify the structure or undefined behavior will result</para>
        /// </remarks>
        MethodRepresentation[] Path { get; }

        /// <summary>Retrieves the source location information for the inlining chain</summary>
        /// <remarks>
        /// <para>It is possible for entries in this list to be null if there was no debug information
        /// for the call site the method is inlined into.</para>
        /// <para>It is worth noting that the debug info path does not "line up" with the <see cref="Path"/>
        /// array, it is in fact off by one index. This is due to the fact that the operator that this
        /// annotation applies to has its own DebugInfo indicating its source location. Thus, the last
        /// entry in DebugInfoPath contains the source location where the functions entry block was inlined *into*.</para>
        /// <para>It is important to note that for compatability reasons this is an array, however callers should consider
        /// the contents as readonly and not modify the structure or undefined behavior will result</para>
        /// </remarks>
        DebugInfo[] DebugInfoPath { get; }

        // temp: flag to indicate if the path has had elements removed due to reachability checks
        bool IsSquashed { get; }
    }
}
