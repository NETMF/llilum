using System;

namespace Llvm.NET.DebugInfo
{
    // for now the DebugInfo hierarchy is mostly empty
    // classes. This is due to the "in transition" state
    // of the underlying LLVM C++ model. All of these
    // are just a wrapper around a Metadata* allocated
    // in the LLVM native libraries. The only properties
    // or methods exposed are those required by current
    // projects. This keeps the code churn to move into
    // 3.7 minimal while allowing us to achieve progress
    // on current projects.

    /// <summary>Array of see <a href="Type"/> nodes for use with see <a href="DebugInfoBuilder"/> methods</summary>
    public class DITypeArray
    {
        internal DITypeArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }

    /// <summary>Array of see <a href="Descriptor"/> nodes for use with see <a href="DebugInfoBuilder"/> methods</summary>
    public class DIArray
    {
        internal DIArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }

    /// <summary>Root of the object hierarchy for Debug information metadata nodes</summary>
    public class DINode : MDNode
    {
        /// <summary>Dwarf tag for the descriptor</summary>
        public Tag Tag
        {
            get
            {
                if( MetadataHandle.Pointer == IntPtr.Zero )
                    return (Tag)(ushort.MaxValue);

                return ( Tag )LLVMNative.DIDescriptorGetTag( MetadataHandle );
            }
        }

        internal DINode( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        /// <inheritdoc/>
        public override string ToString( )
        {
            if( MetadataHandle.Pointer == IntPtr.Zero )
                return string.Empty;

            return LLVMNative.MarshalMsg( LLVMNative.DIDescriptorAsString( MetadataHandle ) );
        }
    }

    public class DILocation : MDNode
    {
        public DILocation( uint line, uint column, DILocalScope scope )
            : this( line, column, scope, null )
        {
        }

        public DILocation( uint line, uint column, DILocalScope scope, DILocation inlinedAt )
            : this( Context.CurrentContext, line, column, scope, inlinedAt )
        {
        }

        public DILocation( Context context, uint line, uint column, DILocalScope scope, DILocation inlinedAt )
            : base( LLVMNative.DILocation( context.ContextHandle, line, column, scope.MetadataHandle, inlinedAt?.MetadataHandle ?? LLVMMetadataRef.Zero ) )
        {
        }

        internal DILocation( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class GenericDINode : DINode
    {
        internal GenericDINode( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diexpression"/></summary>
    public class DIExpression : MDNode
    {
        internal DIExpression( LLVMMetadataRef handle )
            : base( handle )
        {
        }

    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diglobalvariable"/></summary>
    public class DIGlobalVariable : DIVariable
    {
        internal DIGlobalVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DIVariable : DINode
    {
        internal DIVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilocalvariable"/></summary>
    public class DILocalVariable : DIVariable
    {
        internal DILocalVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dienumerator"/></summary>
    public class DIEnumerator : DINode
    {
        internal DIEnumerator( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubrange"/></summary>
    public class DISubrange : DINode
    {
        internal DISubrange( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>Base class for all Debug info scopes</summary>
    public class DIScope : DINode
    {
        internal DIScope( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>Legal scope for lexical blocks, local variables, and debug info locations</summary>
    public class DILocalScope : DIScope
    {
        internal DILocalScope( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        // returns "this" if the scope is a subprogram, otherwise walks up the scopes to find 
        // the containing subprogram.
        //public DISubProgram Subprogram { get; }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dicompileunit"/></summary>
    public class DICompileUnit : DIScope
    {
        internal DICompileUnit( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DIModule : DIScope
    {
        internal DIModule( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DITemplateParameter : DINode
    {
        internal DITemplateParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DITemplateTypeParameter : DITemplateParameter
    {
        internal DITemplateTypeParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DITemplateValueParameter : DITemplateParameter
    {
        internal DITemplateValueParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#difile"/></summary>
    public class DIFile : DIScope
    {
        internal DIFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DILexicalBlockBase : DILocalScope
    {
        internal DILexicalBlockBase( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilexicalblock"/></summary>
    public class DILexicalBlock : DILexicalBlockBase
    {
        internal DILexicalBlock( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilexicalblockfile"/></summary>
    public class DILexicalBlockFile : DILexicalBlockBase
    {
        internal DILexicalBlockFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dinamespace"/></summary>
    public class DINamespace : DIScope
    {
        internal DINamespace( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubprogram"/></summary>
    public class DISubProgram : DILocalScope
    {
        internal DISubProgram( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>Base class for Debug info types</summary>
    public class DIType : DIScope
    {
        internal DIType( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public DebugInfoFlags Flags
        {
            get
            {
                if( MetadataHandle.Pointer == IntPtr.Zero )
                    return 0;

                return ( DebugInfoFlags )LLVMNative.DITypeGetFlags( MetadataHandle );
            }
        }

        public string Name => LLVMNative.MarshalMsg( LLVMNative.GetDITypeName( MetadataHandle ) );
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dibasictype"/></summary>
    public class DIBasicType : DIType
    {
        internal DIBasicType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diderivedtype"/></summary>
    public class DIDerivedType : DIType
    {
        internal DIDerivedType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dicompositetype"/></summary>
    public class DICompositeType : DIType
    {
        internal DICompositeType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubroutinetype"/></summary>
    public class DISubroutineType : DICompositeType
    {
        internal DISubroutineType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DIObjCProperty : DINode
    {
        internal DIObjCProperty( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
