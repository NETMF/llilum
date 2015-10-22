using System;
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

    /// <summary>Root of the LLVM Metadata hierarchy</summary>
    public class Metadata
    {
        //public uint Id => LLVMNative.GetMetadataId( );
        internal Metadata( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        /// <summary>Replace all uses of this descriptor with another</summary>
        /// <param name="other">New descriptor to replace this one with</param>
        public virtual void ReplaceAllUsesWith( Metadata other )
        {
            if( MetadataHandle.Pointer == IntPtr.Zero )
                throw new InvalidOperationException( "Cannot Replace all uses of a null descriptor" );

            NativeMethods.MetadataReplaceAllUsesWith( MetadataHandle, other.MetadataHandle );
            MetadataHandle = LLVMMetadataRef.Zero;
        }

        internal LLVMMetadataRef MetadataHandle { get; set; }
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

        public static implicit operator Metadata( MetadataAsValue self )
        {
            // TODO: Add support to get the metadata ref from the value...
            // e.g. call C++ MetadataAsValue.getMetadata()
            return new Metadata( LLVMMetadataRef.Zero );
        }
    }

    public class MDNode : Metadata
    {
        internal MDNode( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public bool IsTemporary => NativeMethods.IsTemporary( MetadataHandle );
        public bool IsResolved => NativeMethods.IsResolved( MetadataHandle );
        public void ResolveCycles( ) => NativeMethods.MDNodeResolveCycles( MetadataHandle );

        public override void ReplaceAllUsesWith( Metadata other )
        {
            if( !IsTemporary || IsResolved )
                throw new InvalidOperationException( "Cannot replace non temporary or resolved  MDNode" );

            if( MetadataHandle.Pointer == IntPtr.Zero )
                throw new InvalidOperationException( "Cannot Replace all uses of a null descriptor" );

            NativeMethods.MDNodeReplaceAllUsesWith( MetadataHandle, other.MetadataHandle );
            MetadataHandle = LLVMMetadataRef.Zero;
        }
    }

    public class MDString : Metadata
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
