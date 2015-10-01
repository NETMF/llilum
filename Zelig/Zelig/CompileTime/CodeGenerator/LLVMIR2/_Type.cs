using System;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET;
using Llvm.NET.Types;
using System.Linq;
using Llvm.NET.DebugInfo;

namespace Microsoft.Zelig.LLVM
{
    public class _Type
    {
        internal _Type( _Module module, TypeImpl impl )
        {
            Impl = impl;
            Module = module;
        }

        public void AddField( uint offset, _Type type, bool forceInline, string name )
        {
            Impl.AddField( offset, type.Impl, forceInline, name );
        }

        public void Dump( )
        {
            Impl.Dump( );
        }

        public _Type GetBTUnderlyingType( )
        {
            return Module.GetType( $"LLVM.{Impl.GetName()}" );
        }

        public int GetSizeInBits( )
        {
            return (int)Impl.GetSizeInBits();
        }

        public int GetSizeInBitsForStorage( )
        {
            return (int)Impl.GetSizeInBitsForStorage();
        }

        public bool IsDouble( ) => Impl.GetLLVMObject().IsDouble();
        public bool IsFloat( ) => Impl.GetLLVMObject().IsFloat();
        public bool IsFloatingPoint( ) => Impl.GetLLVMObject().IsFloatingPoint();
        public bool IsInteger( ) => Impl.GetLLVMObject().IsInteger();
        public bool IsPointer( ) => Impl.GetLLVMObjectForStorage( ).IsPointer();
        public bool IsPointerPointer( )
        {
            return Impl.GetLLVMObject( ).IsPointer && ( ( PointerType )Impl.GetLLVMObject() ).ElementType.IsPointer;
        }
        public bool IsStruct( ) => Impl.GetLLVMObjectForStorage( ).IsStruct();

        public bool IsValueType( ) => Impl.IsValueType();

        public void SetupFields( )
        {
            Impl.SetupFields();
        }

        public void SetValueTypeFlag( bool val )
        {
            Impl.SetValueTypeFlag( val );
        }

        public bool __op_Equality( _Type b ) => ReferenceEquals( Impl, b.Impl );

        internal _Module Module { get; }
        internal TypeImpl Impl { get; }
    }

    internal class TypeImpl
    {
        private bool IsValueTypeFlag;
        private bool HasHeaderFlag;
        private bool IsBoxedFlag;
        private string Name;
        private uint SizeInBits;
        private readonly TypeRef LlvmType;
        private ModuleImpl Owner;

        internal DIType DIType { get; set; }

        internal List<TypeField> Fields { get; } = new List<TypeField>( );

        internal List<TypeImpl> FunctionArgs { get; } = new List<TypeImpl>();

        internal List<DIType> DiFields = new List<DIType>( );

        // REVIEW: Any reason these can't be consolidated into a single UnderlyingElementType?
        //         Not like you can have a boxed pointer...
        internal TypeImpl UnderlyingBoxedType;
        internal TypeImpl UnderlyingPointerType;

        internal TypeRef GetLLVMObject( ) => LlvmType;

        private void PrivateInit( ModuleImpl module, string name, uint sizeInBits)
        {
            Owner = module;
            SizeInBits = sizeInBits;
            Name = name;
            IsValueTypeFlag = true;
            HasHeaderFlag = false;
            IsBoxedFlag = false;
            UnderlyingBoxedType = null;
            UnderlyingPointerType = null;
        }

        internal TypeImpl( ModuleImpl module, string name, uint sizeInBits, TypeRef typeRef )
        {
            PrivateInit( module, name, sizeInBits );
            LlvmType = typeRef;
            TypeImplsReverseLookupForLlVMTypes[ LlvmType ] = this;
        }

        internal TypeImpl( ModuleImpl owner, string name, uint sizeInBits )
        {
            PrivateInit( owner, name, sizeInBits );
            Module module = owner.GetLLVMObject( );

            Debug.Assert( module.GetTypeByName( name ) == null, "Trying to override TypeImpl" );
            switch( name )
            {
            case "System.Void":
                DIType = owner.DiBuilder.CreateBasicType( name, 0, 0, 0 );
                LlvmType = module.Context.VoidType;
                break;

            case "LLVM.System.Char":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0, DiTypeKind.UTF );
                LlvmType = module.Context.GetIntType( ( uint )sizeInBits );
                break;

            case "LLVM.System.Boolean":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0, DiTypeKind.Boolean );
                LlvmType = module.Context.GetIntType( ( uint )sizeInBits );
                break;

            case "LLVM.System.SByte":
            case "LLVM.System.Int16":
            case "LLVM.System.Int32":
            case "LLVM.System.Int64":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0, DiTypeKind.Signed );
                LlvmType = module.Context.GetIntType( ( uint )sizeInBits );
                break;

            case "LLVM.System.Byte":
            case "LLVM.System.UInt16":
            case "LLVM.System.UInt32":
            case "LLVM.System.UInt64":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0,  DiTypeKind.Unsigned );
                LlvmType = module.Context.GetIntType( ( uint )sizeInBits );
                break;

            case "LLVM.System.Single":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0, 0 );
                LlvmType = module.Context.FloatType;
                break;

            case "LLVM.System.Double":
                DIType = owner.DiBuilder.CreateBasicType( name, sizeInBits, 0, 0 );
                LlvmType = module.Context.DoubleType;
                break;

            case "LLVM.System.IntPtr":
            case "LLVM.System.UIntPtr":
                DIType = owner.DiBuilder.CreatePointerType( owner.DiBuilder.CreateBasicType( "System.Void", 0, 0, 0 ), name, sizeInBits, 0 );
                LlvmType = module.Context.Int8Type.CreatePointerType( );
                break;

            default:
                // Creation of concrete DIType deferred until SetupFields when full field layout information is known
                DIType = owner.DiBuilder.CreateReplaceableCompositeType(Tag.StructureType, name, owner.DICompileUnit, null, 0 );
                LlvmType = module.Context.CreateStructType( name );
                break;
            }

            TypeImplsReverseLookupForLlVMTypes[ LlvmType ] = this;
        }

        internal static TypeImpl GetOrInsertTypeImpl( ModuleImpl owner, string name, uint sizeInBits, TypeRef ty )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new TypeImpl( owner, name, sizeInBits, ty );
            }

            return TypeImplMap[ name ];
        }

        internal static TypeImpl GetOrInsertTypeImpl( ModuleImpl owner, string name, uint sizeInBits )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new TypeImpl( owner, name, sizeInBits );
            }

            return TypeImplMap[ name ];
        }

        internal static TypeImpl GetTypeImpl( string name )
        {
            TypeImpl retVal;
            if( TypeImplMap.TryGetValue( name, out retVal ) )
                return retVal;

            return null;
        }

        internal static TypeImpl GetTypeImpl( TypeRef ty )
        {
            TypeImpl retVal;
            if( TypeImplsReverseLookupForLlVMTypes.TryGetValue( ty, out retVal ) )
                return retVal;

            var pointerType = ty as PointerType;
            if( pointerType != null && pointerType.ElementType.IsStruct( ) )
            {
                return TypeImplsReverseLookupForLlVMTypes[ pointerType.ElementType ];
            }

            Debug.Assert( false );
            return null;
        }

        internal uint GetSizeInBits( ) => SizeInBits;

        internal uint GetSizeInBitsForStorage( )
        {
            if( IsValueTypeFlag )
                return SizeInBits;

            return Owner.GetPointerSize( );
        }

        internal void FinalizeDebugInfo( )
        {
            if( !DIType.IsTemporary || DIType.IsResolved )
                return;

            if( UnderlyingPointerType != null )
            {
                var alignment = Owner.TargetMachine.TargetData.AbiAlignmentOf( LlvmType );
                var bitSize = Owner.TargetMachine.TargetData.AbiSizeOf( LlvmType );
                DIType = Owner.DiBuilder.CreatePointerType( UnderlyingPointerType.DIType
                                                          , $"{UnderlyingPointerType.Name}*"
                                                          , bitSize
                                                          , alignment
                                                          );
            }
            else
            {
                foreach( TypeField field in Fields )
                {
                    AddDiField( field );
                }
                var alignment = Fields.Count == 0 ? 0 : Owner.TargetMachine.TargetData.AbiAlignmentOf( LlvmType );
                var bitSize = Fields.Count == 0 ? 0 : Owner.TargetMachine.TargetData.AbiSizeOf( LlvmType );

                var concreteType = Owner.DiBuilder.CreateStructType( Owner.DICompileUnit, Name, null, 0, bitSize, alignment, 0, null, DiFields );
                DIType.ReplaceAllUsesWith( concreteType );
                DIType = concreteType;
            }
        }

        internal void AddField( uint offset, TypeImpl type, bool forceInline, string name )
        {
            var f = new TypeField( name, type, offset, forceInline );

            // Add the field in offset order, by moving all other pointer to fields
            // Not the most efficient approach for C# but mirrors the original C++ code
            Fields.Add( default(TypeField) );// increase count by one

            int i = 0;
            for( i = Fields.Count - 1; i > 0; --i )
            {
                if( Fields[ i - 1 ].Offset <= f.Offset )
                    break;

                Fields[ i ] = Fields[ i - 1 ];
            }
            Fields[ i ] = f;
        }

        private void AddTypeToStruct( TypeField field, IList<TypeRef> llvmFields, ref uint offset )
        {
            field.FinalIdx = ( uint )llvmFields.Count;
            if( field.ForceInline )
            {
                llvmFields.Add( field.MemberType.GetLLVMObject( ) );
                offset += field.MemberType.GetSizeInBits() / 8;
            }
            else
            {
                llvmFields.Add( field.MemberType.GetLLVMObjectForStorage( ) );
                offset += field.MemberType.GetSizeInBitsForStorage( ) / 8u;
            }
        }

        internal void SetupFields( )
        {
            if( !( GetLLVMObject( ) is StructType ) )
            {
                return;
            }

            List<TypeRef> llvmFields = new List<TypeRef>( );
            int idx = 0;
            uint offset = 0;
            while ( offset < SizeInBits / 8 )
            {
                if( ( idx < Fields.Count ) && ( Fields[ idx ].Offset == offset ) )
                {
                    AddTypeToStruct( Fields[ idx ], llvmFields, ref offset );
                    ++idx;
                }
                else
                {
                    // Add explicit padding if necessary.
                    // TODO: Clean this up with a single byte array [ n x i8 ]
                    llvmFields.Add( GetOrInsertTypeImpl( Owner, "System.Byte", 8 ).GetLLVMObject( ) );
                    ++offset;
                }
            }

            // Add in any remaining fields beyond the expected structure size. These should generally be zero-sized,
            // such as System.Object and the elements trailing strings/arrays.
            for ( ; idx < Fields.Count; ++idx )
            {
                AddTypeToStruct( Fields[ idx ], llvmFields, ref offset );
            }

            // BUGBUG: We don't yet handle explicit struct layout with overlapping fields.

            (( StructType )LlvmType ).SetBody( true, llvmFields.ToArray( ) );
        }

        private void AddDiField( TypeField field )
        {
            var structType = LlvmType as StructType;
            Debug.Assert( structType != null );
            Debug.Assert( field.MemberType.DIType != null );
            var memberDiType = field.MemberType.GetDiTypeForStack( );
            if( field.ForceInline )
                memberDiType = field.MemberType.DIType;

            var alignment = field.MemberType.LlvmType.IsSized ? Owner.TargetMachine.TargetData.AbiAlignmentOf( field.MemberType.LlvmType ) : 0;
            var bitSize = field.MemberType.LlvmType.IsSized ? Owner.TargetMachine.TargetData.AbiSizeOf( field.MemberType.LlvmType ) : 0;
            var offset = Owner.TargetMachine.TargetData.OffsetOfElement( structType, field.FinalIdx );
            var diField = Owner.DiBuilder.CreateMemberType( scope: Owner.DICompileUnit
                                                          , name: field.Name
                                                          , file: null
                                                          , line: 0
                                                          , bitSize: bitSize
                                                          , bitAlign: alignment
                                                          , bitOffset: offset
                                                          , flags: 0 // TODO: protected/public/private,...
                                                          , type: memberDiType
                                                          );
            DiFields.Add( diField );
        }

        internal void SetHasHeaderFlag( bool value ) => HasHeaderFlag = value;
        internal void SetValueTypeFlag( bool value ) => IsValueTypeFlag = value;
        internal void SetBoxedFlag( bool value ) => IsBoxedFlag = value;
        internal bool IsBoxed( ) => IsBoxedFlag;
        internal bool IsValueType( ) => IsValueTypeFlag;
        internal string GetName( ) => Name;

        internal TypeRef GetLLVMObjectForStorage( )
        {
            if( IsValueTypeFlag )
                return GetLLVMObject( );

            return GetLLVMObject( ).CreatePointerType( );
        }

        internal DIType GetDiTypeForStack( )
        {
            if( IsValueTypeFlag )
                return DIType;

            return Owner.DiBuilder.CreatePointerType( DIType, $"{Name}*", Owner.GetPointerSize( ), Owner.GetPointerSize( ) );
        }

        internal void Dump( )
        {
            Console.WriteLine( "Name:{0}\n", Name );
            Console.WriteLine( LlvmType.ToString() );
            Console.WriteLine( );
        }

        internal static void FinalizeAllDebugInfo( )
        {
            if( DebugInfoFinalized )
                return;

            foreach( TypeImpl typeImpl in TypeImplMap.Values )
            {
                typeImpl.FinalizeDebugInfo( );
            }
            DebugInfoFinalized = true;
        }

        static bool DebugInfoFinalized;
        static Dictionary<string, TypeImpl> TypeImplMap = new Dictionary<string, TypeImpl>( );
        static Dictionary<TypeRef, TypeImpl> TypeImplsReverseLookupForLlVMTypes = new Dictionary<TypeRef, TypeImpl>( );
    }
}
