#define DUMP_INLINING_PATH_DETAILS
using Llvm.NET;
using Llvm.NET.DebugInfo;
using Microsoft.Zelig.CodeGeneration.IR;
using Microsoft.Zelig.Debugging;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Zelig.LLVM
{
    internal static class InlinedPathDetailsExtensions
    {

#if DUMP_INLINING_PATH_DETAILS
        static Lazy<IndentedTextWriter> InliningLog = new Lazy<IndentedTextWriter>( ()=> new IndentedTextWriter(new StreamWriter("InlinePath.log"))
                                                                                  , false
                                                                                  );
#endif

        [Conditional("DUMP_INLINING_PATH_DETAILS")]
        private static void Log( DILocation location )
        {
            if (location == null)
                return;

            InliningLog.Value.WriteLine("Location: {0}", location );
            InliningLog.Value.Write("Scope: {0}", location.Scope);
            if( location.InlinedAt != null )
            {
                ++InliningLog.Value.Indent;
                InliningLog.Value.WriteLine();
                Log(location.InlinedAt);
                --InliningLog.Value.Indent;
            }
            InliningLog.Value.WriteLine();
            InliningLog.Value.Flush();
        }

        /// <summary>Generates a DILocation with a full scope chain for inlined locations from an InliningPathAnnotation</summary>
        /// <param name="pathDetails">Annotation to get the full path information from</param>
        /// <param name="module">Module to use for resolving Zelig IR class instanes to their corresponding LLVM instances</param>
        /// <param name="outerScope">LLVM function for the outermost scope</param>
        /// <param name="innermostDebugInfo">Zelig IR Debug info for the innermost source location</param>
        /// <returns><see cref="DebugInfo"/> with full chained scoping (e.g. InlinedAt is set for full scope chain) for inlined functions</returns>
        /// <remarks>
        /// LLVM Locations require full tracking from the innermost location to the outer most, however the Zelig IR
        /// <see cref="IInlinedPathDetails"/> doesn't store the innermost source location, nor the outermost function
        /// scope. Thus an InliningPathAnnotation on its own is insufficient to construct the LLVM debug information.
        /// This method takes care of that by using the additional parameters to complete the information.
        /// </remarks>
        internal static DILocation GetDebugLocationFor( this IInliningPathAnnotation pathDetails, _Module module, _Function outerScope, DebugInfo innermostDebugInfo )
        {
            if (pathDetails == null)
            {
                throw new ArgumentNullException(nameof(pathDetails));
            }

            // if elements of the inline path are removed (e.g. the path is squashed)
            // then scopes may be off and can't be handle by this code yet. (This can
            // occur for extension methods that were inlined into something) Conceptually
            // extension methods are more like a pre-processor macro insertion than
            // inlining but this code doesn't have enough information to make a decision
            // on that here.So, for the moment, this just treats it as not inlined. 
            if ( pathDetails.IsSquashed )
            {
                return module.Manager.GetDebugInfoFor(outerScope.Method)?.AsDILocation( module );
            }

            //construct inlining scope and info array with full path
            //starting from outermost scope and moving inward. This
            //completes the full chain and "re-aligns" the scope and
            //location arrays to have a 1:1 relationship
            var inlineLocations = ScalarEnumerable.Combine( pathDetails.DebugInfoPath, innermostDebugInfo);

            // walk the scope and location collection from outermost to innermost to
            // construct proper LLVM chain. The resulting location is for the innermost
            // location with the "InlinedAt" property referring to the next outer
            // scope, and so on all the way up to the final outer most scope
            DILocation location = null;
            foreach(var debugInfo in inlineLocations )
            {
                location = debugInfo.AsDILocation(module, location);
            }

            Log(location);
            return location;
        }
    }
}
