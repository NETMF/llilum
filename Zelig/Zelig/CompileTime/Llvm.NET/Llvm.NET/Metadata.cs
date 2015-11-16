using System;
using System.Collections.Generic;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;

namespace Llvm.NET
{
    /// <summary>Enumeration to define debug information metadata nodes</summary>
    public enum MetadataKind : uint
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

    public enum MetadataFormat
    {
        Default = 0,
        AsOperand = 1
    }

    /// <summary>Root of the LLVM Metadata hierarchy</summary>
    /// <remarks>In LLVM this is just "Metadata" however tha name has the potential
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

            return (T)context.GetNodeFor( handle, StaticFactory );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity" )]
        private static LlvmMetadata StaticFactory( LLVMMetadataRef handle )
        {
            var kind = (MetadataKind)NativeMethods.GetMetadataID( handle );
            switch( kind )
            {
            case MetadataKind.MDTuple:
                return new MDTuple( handle );

            case MetadataKind.DILocation:
                return new DILocation( handle );

            case MetadataKind.GenericDINode:
                return new GenericDINode( handle );

            case MetadataKind.DISubrange:
                return new DISubrange( handle );

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

    public class ValueAsMetadata 
        : LlvmMetadata
    {
        internal ValueAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class ConstantAsMetadata
        : ValueAsMetadata
    {
        internal ConstantAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class LocalAsMetadata
        : ValueAsMetadata
    {
        internal LocalAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class MetadataAsValue : Value
    {
        internal MetadataAsValue( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal MetadataAsValue( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, IsAMetadataAsValue ) )
        {
        }

        internal static LLVMValueRef IsAMetadataAsValue( LLVMValueRef value )
        {
            if( value.Pointer == IntPtr.Zero )
                return value;

            return NativeMethods.GetValueKind( value ) == ValueKind.MetadataAsValue ? value : default(LLVMValueRef);
        }

        //public static implicit operator Metadata( MetadataAsValue self )
        //{
        //    // TODO: Add support to get the metadata ref from the value...
        //    // e.g. call C++ MetadataAsValue.getMetadata()
        //    throw new NotImplementedException();
        //}
    }

    public class MDOperand
    {
        public MDNode OwningNode { get; }
        public LlvmMetadata Metadata => LlvmMetadata.FromHandle<LlvmMetadata>( OwningNode.Context, NativeMethods.GetOperandNode( OperandHandle ) );

        internal MDOperand( MDNode owningNode, LLVMMDOperandRef handle )
        {
            if( handle.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( handle ) );

            OperandHandle = handle;
            OwningNode = owningNode;
        }

        internal static MDOperand FromHandle( MDNode owningNode, LLVMMDOperandRef handle )
        {
            return owningNode.Context.GetOperandFor( owningNode, handle );
        }

        private readonly LLVMMDOperandRef OperandHandle;
    }

    public class MDNode : LlvmMetadata
    {
        internal MDNode( LLVMMetadataRef handle )
            : base( handle )
        {
            Operands = new MDNodeOperandList( this );
        }

        public Context Context => Context.GetContextFor( MetadataHandle );
        public bool IsDeleted => MetadataHandle == LLVMMetadataRef.Zero;
        public bool IsTemporary => NativeMethods.IsTemporary( MetadataHandle );
        public bool IsResolved => NativeMethods.IsResolved( MetadataHandle );
        public bool IsUniqued => NativeMethods.IsUniqued( MetadataHandle );
        public bool IsDistinct => NativeMethods.IsDistinct( MetadataHandle );
        public IReadOnlyList<MDOperand> Operands { get; }

        public void ResolveCycles( ) => NativeMethods.MDNodeResolveCycles( MetadataHandle );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MDNode" )]
        public override void ReplaceAllUsesWith( LlvmMetadata other )
        {
            if( !IsTemporary || IsResolved )
                throw new InvalidOperationException( "Cannot replace non temporary or resolved  MDNode" );

            if( MetadataHandle.Pointer == IntPtr.Zero )
                throw new InvalidOperationException( "Cannot Replace all uses of a null descriptor" );

            Context.RemoveDeletedNode( this );
            NativeMethods.MDNodeReplaceAllUsesWith( MetadataHandle, other.MetadataHandle );
            MetadataHandle = LLVMMetadataRef.Zero;
        }
    }

    public class MDTuple : MDNode
    {
        internal MDTuple( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class MDString : LlvmMetadata
    {
        internal MDString( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public override string ToString( )
        {
            uint len;
            var ptr = NativeMethods.GetMDStringText( MetadataHandle, out len );
            return NativeMethods.NormalizeLineEndings( ptr, (int)len );
        }
    } 
}
