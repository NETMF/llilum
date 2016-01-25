using Llvm.NET.DebugInfo;
using Microsoft.Zelig.Debugging;
using Microsoft.Zelig.Runtime.TypeSystem;
using System.Diagnostics;

namespace Microsoft.Zelig.LLVM
{
    internal static class DebugInfoExtensions
    {
        /// <summary>Converts between Zelig IR and LLVM debug location information</summary>
        /// <param name="debugInfo">Zelig IR debug information may be null if no info is available</param>
        /// <param name="module">Module that owns the resulting location</param>
        /// <param name="inlinedAt">Source location this location is inlined into (may be null if not inlined)</param>
        /// <returns>New DILocation for the given IR location</returns>
        internal static DILocation AsDILocation( this DebugInfo debugInfo, _Module module, DILocation inlinedAt )
        {
            if (debugInfo == null)
                return null;

            var md = debugInfo.Scope as MethodRepresentation;
            Debug.Assert(md != null);

            var scope = module.Manager.GetScopeFor(md);

#if DEBUG
            string irSrcPath = System.IO.Path.GetFullPath(debugInfo.SrcFileName);
            string llscopePath = System.IO.Path.GetFullPath(scope.File.Path);
            Debug.Assert( irSrcPath == llscopePath);
#endif
            return new DILocation(scope.Context, (uint)debugInfo.BeginLineNumber, (uint)debugInfo.BeginColumn, scope, inlinedAt);
        }

        internal static DILocation AsDILocation(this DebugInfo debugInfo, _Module module)
        {
            return AsDILocation(debugInfo, module, null);
        }
    }
}
