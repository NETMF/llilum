using System;
using Llvm.NET.DebugInfo;
using Llvm.NET.Native;

namespace Llvm.NET
{
    /// <summary>Root of the LLVM Metadata hierarchy</summary>
    /// <remarks>In LLVM this is just "Metadata" however that name has the potential
    /// to conflict with the .NET runtime namespace of the same name, so the name
    /// is changed in the .NET bindings to avoid the conflict.</remarks>
    public abstract class LlvmMetadata
    {
        // ideally this would be protected + internal but C#
        // doesn't have any syntax to allow such a thing so it
        // is internal and internal code should ensure it is 
        // only ever used by derived type constructors
        internal /*protected*/ LlvmMetadata( LLVMMetadataRef handle )
        {
            if( handle == LLVMMetadataRef.Zero )
                throw new ArgumentNullException( nameof( handle ) );

            MetadataHandle = handle;
        }

        /// <summary>Replace all uses of this descriptor with another</summary>
        /// <param name="other">New descriptor to replace this one with</param>
        public virtual void ReplaceAllUsesWith( LlvmMetadata other )
        {
            if( other == null )
                throw new ArgumentNullException( nameof( other ) );

            if( MetadataHandle.Pointer == IntPtr.Zero )
                throw new InvalidOperationException( "Cannot Replace all uses of a null descriptor" );

            NativeMethods.MetadataReplaceAllUsesWith( MetadataHandle, other.MetadataHandle );
            MetadataHandle = LLVMMetadataRef.Zero;
        }

        /// <inheritdoc/>
        public override string ToString( )
        {
            if( MetadataHandle.Pointer == IntPtr.Zero )
                return string.Empty;

            return NativeMethods.MarshalMsg( NativeMethods.MetadataAsString( MetadataHandle ) );
        }

        internal LLVMMetadataRef MetadataHandle { get; /*protected*/ set; }

        internal static T FromHandle<T>( Context context, LLVMMetadataRef handle )
            where T : LlvmMetadata
        {
            if( handle == LLVMMetadataRef.Zero )
                return null;

            return ( T )context.GetNodeFor( handle, StaticFactory );
        }
        /// <summary>Enumeration to define debug information metadata nodes</summary>
        private enum MetadataKind : uint
        {
            MDTuple,
            DILocation,
            GenericDINode,
            DISubrange,
            DIEnumerator,
            DIBasicType,
            DIDerivedType,
            DICompositeType,
            DISubroutineType,
            DIFile,
            DICompileUnit,
            DISubprogram,
            DILexicalBlock,
            DILexicalBlockFile,
            DINamespace,
            DIModule,
            DITemplateTypeParameter,
            DITemplateValueParameter,
            DIGlobalVariable,
            DILocalVariable,
            DIExpression,
            DIObjCProperty,
            DIImportedEntity,
            ConstantAsMetadata,
            LocalAsMetadata,
            MDString
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Static factory method" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Static factory method" )]
        private static LlvmMetadata StaticFactory( LLVMMetadataRef handle )
        {   // use the native kind value to determine the managed type
            // that should wrap this particular handle
            var kind = ( MetadataKind )NativeMethods.GetMetadataID( handle );
            switch( kind )
            {
            case MetadataKind.MDTuple:
                return new MDTuple( handle );

            case MetadataKind.DILocation:
                return new DILocation( handle );

            case MetadataKind.GenericDINode:
                return new GenericDINode( handle );

            case MetadataKind.DISubrange:
                return new DISubRange( handle );

            case MetadataKind.DIEnumerator:
                return new DIEnumerator( handle );

            case MetadataKind.DIBasicType:
                return new DIBasicType( handle );

            case MetadataKind.DIDerivedType:
                return new DIDerivedType( handle );

            case MetadataKind.DICompositeType:
                return new DICompositeType( handle );

            case MetadataKind.DISubroutineType:
                return new DISubroutineType( handle );

            case MetadataKind.DIFile:
                return new DIFile( handle );

            case MetadataKind.DICompileUnit:
                return new DICompileUnit( handle );

            case MetadataKind.DISubprogram:
                return new DISubProgram( handle );

            case MetadataKind.DILexicalBlock:
                return new DILexicalBlock( handle );

            case MetadataKind.DILexicalBlockFile:
                return new DILexicalBlockFile( handle );

            case MetadataKind.DINamespace:
                return new DINamespace( handle );

            case MetadataKind.DIModule:
                return new DIModule( handle );

            case MetadataKind.DITemplateTypeParameter:
                return new DITemplateTypeParameter( handle );

            case MetadataKind.DITemplateValueParameter:
                return new DITemplateValueParameter( handle );

            case MetadataKind.DIGlobalVariable:
                return new DIGlobalVariable( handle );

            case MetadataKind.DILocalVariable:
                return new DILocalVariable( handle );

            case MetadataKind.DIExpression:
                return new DIExpression( handle );

            case MetadataKind.DIObjCProperty:
                return new DIObjCProperty( handle );

            case MetadataKind.DIImportedEntity:
                return new DIImportedEntity( handle );

            case MetadataKind.ConstantAsMetadata:
                return new ConstantAsMetadata( handle );

            case MetadataKind.LocalAsMetadata:
                return new LocalAsMetadata( handle );

            case MetadataKind.MDString:
                return new MDString( handle );

            default:
                throw new NotImplementedException( );
            }
        }
    }

}
