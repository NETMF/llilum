using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Llvm.NET.DebugInfo
{
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

                return ( Tag )NativeMethods.DIDescriptorGetTag( MetadataHandle );
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

            return NativeMethods.MarshalMsg( NativeMethods.DIDescriptorAsString( MetadataHandle ) );
        }

        internal static DINode FromHandle( LLVMMetadataRef handle ) => FromHandle<DINode>( handle );

        internal static T FromHandle<T>( LLVMMetadataRef handle )
            where T : DINode
        {
            if( handle == LLVMMetadataRef.Zero )
                return null;

            DINode retVal;
            if( NodeCache.TryGetValue( handle, out retVal ) )
                return retVal as T;

            retVal = StaticFactory( handle );
            NodeCache.Add( handle, retVal );
            return retVal as T;
        }

        private static DINode StaticFactory( LLVMMetadataRef handle )
        {
            var kind = ( Tag )NativeMethods.DIDescriptorGetTag( handle );
            switch( kind )
            {
            //case Tag.EntryPoint:
            //case Tag.FormalParameter:
            //case Tag.ImportedDeclaration:
            //case Tag.Label:
            //case Tag.Variant:
            //case Tag.CommonBlock:
            //case Tag.CommonInclusion:
            //case Tag.Inheritance:
            //case Tag.InlinedSubroutine:
            //case Tag.Module:
            //case Tag.WithStatement:
            //case Tag.AccessDeclaration:
            //case Tag.CatchBlock:
            //case Tag.Constant:
            //case Tag.NameList:
            //case Tag.NameListItem:
            //case Tag.ThrownType:
            //case Tag.TryBlock:
            //case Tag.VariantPart:
            //case Tag.DwarfProcedure:
            //case Tag.ImportedModule: //DIImportedEntity
            //case Tag.PartialUnit:
            //case Tag.InportedUnit:
            //case Tag.Condition:
            //case Tag.TypeUnit:
            //case Tag.TemplateAlias:
            //case Tag.CoArrayType:
            //case Tag.GenericSubrange:
            //case Tag.DynamicType:
            //case Tag.Expression:
            //case Tag.UserBase:
            //case Tag.MipsLoop:
            //case Tag.FormatLabel:
            //case Tag.FunctionTemplate:
            //case Tag.ClassTemplate:
            //case Tag.GnuTemplateTemplateParam:
            //case Tag.GnuTemplateParameterPack:
            //case Tag.GnuFormalParameterPack:
            //case Tag.LoUser:
            //case Tag.HiUser:
            default:
                return new DINode( handle );

            case Tag.ArrayType:
            case Tag.ClassType:
            case Tag.EnumerationType:
            case Tag.StructureType:
            case Tag.UnionType:
                return new DICompositeType( handle );

            case Tag.ReferenceType:
            case Tag.PointerType:
            case Tag.TypeDef:
            case Tag.PtrToMemberType:
            case Tag.Friend:
            case Tag.VolatileType:
            case Tag.RestrictType:
            case Tag.ConstType:
            case Tag.Member:
                return new DIDerivedType( handle );

            case Tag.CompileUnit:
                return new DICompileUnit( handle );

            case Tag.SetType:
            case Tag.StringType:
            case Tag.InterfaceType:
            case Tag.PackedType:
            case Tag.UnspecifiedType:
            case Tag.SharedType:
            case Tag.RValueReferenceType:
                return new DIType( handle );

            case Tag.SubroutineType:
                return new DISubroutineType( handle );

            case Tag.BaseType:
            case Tag.UnspecifiedParameters:
                return new DIBasicType( handle );

            case Tag.LexicalBlock:
                return new DILexicalBlock( handle );

            case Tag.SubrangeType:
                return new DISubrange( handle );

            case Tag.Enumerator:
                return new DIEnumerator( handle );

            case Tag.FileType:
                return new DIFile( handle );

            case Tag.SubProgram:
                return new DISubProgram( handle );

            case Tag.TemplateTypeParameter:
                return new DITemplateTypeParameter( handle );

            case Tag.TemplateValueParameter:
                return new DITemplateValueParameter( handle );

            case Tag.Variable:
                return new DIGlobalVariable( handle );

            case Tag.Namespace:
                return new DINamespace( handle );

            case Tag.AutoVariable:
            case Tag.ArgVariable:
                return new DILocalVariable( handle );

            case Tag.AppleProperty:
                return new DIObjCProperty( handle );
            }
        }

        private static readonly Dictionary< LLVMMetadataRef, DINode > NodeCache = new Dictionary< LLVMMetadataRef, DINode >( );
    }
}
