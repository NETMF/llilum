using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            return Impl.GetLLVMObject( ).IsPointer && ( ( Llvm.NET.PointerType )Impl.GetLLVMObject() ).ElementType.IsPointer;
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
        private readonly Llvm.NET.TypeRef LlvmType;
        private ModuleImpl Owner;

        internal List<TypeField> Fields { get; } = new List<TypeField>();
        internal List<TypeImpl> FunctionArgs { get; } = new List<TypeImpl>();

        // REVIEW: Any reason these can't be consolidated into a single UnderlyingElementType?
        //         Not like you can have a boxed pointer...
        internal TypeImpl UnderlyingBoxedType;
        internal TypeImpl UnderlyingPointerType;

        internal Llvm.NET.TypeRef GetLLVMObject( ) => LlvmType;

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

        internal TypeImpl( ModuleImpl module, string name, uint sizeInBits, Llvm.NET.TypeRef typeRef )
        {
            PrivateInit( module, name, sizeInBits );
            LlvmType = typeRef;
            TypeImplsReverseLookupForLlVMTypes[ LlvmType ] = this;
        }

        internal TypeImpl( ModuleImpl owner, string name, uint sizeInBits )
        {
            PrivateInit( owner, name, sizeInBits );
            Llvm.NET.Module module = owner.GetLLVMObject( );

            Debug.Assert( module.GetTypeByName( name ) == null, "Trying to override TypeImpl" );
            switch( name )
            {
            case "System.Void":
                LlvmType = module.Context.VoidType;
                break;

            case "LLVM.System.Boolean":
            case "LLVM.System.Char":
            case "LLVM.System.SByte":
            case "LLVM.System.Byte":
            case "LLVM.System.Int16":
            case "LLVM.System.UInt16":
            case "LLVM.System.Int32":
            case "LLVM.System.UInt32":
            case "LLVM.System.Int64":
            case "LLVM.System.UInt64":
                LlvmType = module.Context.GetIntType( ( uint )sizeInBits );
                break;

            case "LLVM.System.Single":
                LlvmType = module.Context.FloatType;
                break;

            case "LLVM.System.Double":
                LlvmType = module.Context.DoubleType;
                break;

            case "LLVM.System.IntPtr":
            case "LLVM.System.UIntPtr":
                LlvmType = module.Context.Int8Type.CreatePointerType( );
                break;

            default:
                LlvmType = module.Context.CreateStructType( name );
                break;
            }

            TypeImplsReverseLookupForLlVMTypes[ LlvmType ] = this;
        }

        internal static TypeImpl GetOrInsertTypeImpl( ModuleImpl owner, string name, uint sizeInBits, Llvm.NET.TypeRef ty )
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

        internal static TypeImpl GetTypeImpl( Llvm.NET.TypeRef ty )
        {
            TypeImpl retVal;
            if( TypeImplsReverseLookupForLlVMTypes.TryGetValue( ty, out retVal ) )
                return retVal;

            var pointerType = ty as Llvm.NET.PointerType;
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

        private void AddTypeToStruct( ref int idx, IList<Llvm.NET.TypeRef> llvmFields, ref uint i )
        {
            Fields[ idx ].FinalIdx = ( uint )llvmFields.Count;
            if( Fields[ idx ].ForceInline )
            {
                llvmFields.Add( Fields[ idx ].MemberType.GetLLVMObject( ) );
                i = ( i - 1 ) + ( uint )Fields[ idx ].MemberType.GetSizeInBits() / 8;
            }
            else
            {
                llvmFields.Add( Fields[ idx ].MemberType.GetLLVMObjectForStorage( ) );
                i = ( i - 1 ) + ( uint )Fields[ idx ].MemberType.GetSizeInBitsForStorage( ) / 8u;
            }
            idx++;
        }

        internal void SetupFields( )
        {
            if( !( GetLLVMObject( ) is Llvm.NET.StructType ) || Fields.Count == 0 )
                return;

            List<Llvm.NET.TypeRef> llvmFields = new List<Llvm.NET.TypeRef>( );
            int idx = 0;

            for( uint i = 0; i < SizeInBits / 8; ++i )
            {
                if( idx < Fields.Count && Fields[ idx ].Offset == i )
                {
                    AddTypeToStruct( ref idx, llvmFields, ref i );
                }
                else
                {
                    // TODO: Clean this up with a single byte array [ n x i8 ]
                    // add padding with a byte member
                    llvmFields.Add( GetOrInsertTypeImpl( Owner, "System.Byte", 8 ).GetLLVMObject( ) );
                }
            }

            for( ; idx < Fields.Count; ++idx )
            {
                uint i = 0;
                AddTypeToStruct( ref idx, llvmFields, ref i );
            }

            ( ( Llvm.NET.StructType )LlvmType ).SetBody( true, llvmFields.ToArray( ) );
        }

        internal void SetHasHeaderFlag( bool value ) => HasHeaderFlag = value;
        internal void SetValueTypeFlag( bool value ) => IsValueTypeFlag = value;
        internal void SetBoxedFlag( bool value ) => IsBoxedFlag = value;
        internal bool IsBoxed( ) => IsBoxedFlag;
        internal bool IsValueType( ) => IsValueTypeFlag;
        internal string GetName( ) => Name;

        internal Llvm.NET.TypeRef GetLLVMObjectForStorage( )
        {
            if( IsValueTypeFlag )
                return GetLLVMObject( );

            return GetLLVMObject( ).CreatePointerType( );
        }

        internal void Dump( )
        {
            Console.WriteLine( "Name:{0}\n", Name );
            Console.WriteLine( LlvmType.ToString() );
            Console.WriteLine( );
        }

        static Dictionary<string, TypeImpl> TypeImplMap = new Dictionary<string, TypeImpl>( );
        static Dictionary<Llvm.NET.TypeRef, TypeImpl> TypeImplsReverseLookupForLlVMTypes = new Dictionary<Llvm.NET.TypeRef, TypeImpl>( );
    }
}
