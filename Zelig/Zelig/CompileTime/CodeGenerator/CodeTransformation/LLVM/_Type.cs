using System;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Types;

namespace Microsoft.Zelig.LLVM
{
    public class _Type
    {
        private static readonly Dictionary< string, _Type > TypeImplMap = new Dictionary< string, _Type >( );
        private static readonly Dictionary< IntPtr, _Type > TypeImplsReverseLookupForLlvmTypes =
            new Dictionary< IntPtr, _Type >( );

        private Module LlvmModule => Module.LlvmModule;

        internal List< DebugMemberInfo > DiFields = new List< DebugMemberInfo >( );

        public _Type( _Module module, string name, int sizeInBits, bool isValueType, IDebugType<ITypeRef,DIType> typeRef )
        {
            Module = module;
            SizeInBits = sizeInBits;
            IsValueType = isValueType;
            Name = name;
            DebugType = typeRef;
            TypeImplsReverseLookupForLlvmTypes[ DebugType.TypeHandle ] = this;
        }

        public _Type( _Module owner, string name, int sizeInBits, bool isValueType )
        {
            SizeInBits = sizeInBits;
            IsValueType = isValueType;
            Name = name;
            Module = owner;

            Debug.Assert( LlvmModule.GetTypeByName( name ) == null, "Trying to override _Type" );
            var systemNamespace = LlvmModule.DIBuilder.CreateNamespace( LlvmModule.DICompileUnit, "System", null, 0 );

            // basic types need to use DebugBasicType since the LLVM primitive types are all uniqued
            // and some of them would end up overlapping (i.e. System.UInt16 and Char are both i16 to
            // LLVM but need distinct DIBasicType instances to describe them for the debugger)
            switch( name )
            {
            case "System.Void":
                // DIType == null means void
                DebugType = Llvm.NET.DebugInfo.DebugType.Create( LlvmModule.Context.VoidType, ( DIType )null );
                break;

            case "System.Char":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.UTF );
                IsPrimitiveType = true;
                break;

            case "System.Boolean":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Boolean );
                IsPrimitiveType = true;
                break;

            case "System.SByte":
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Signed );
                IsPrimitiveType = true;
                IsSigned = true;
                break;

            case "System.Byte":
            case "System.UInt16":
            case "System.UInt32":
            case "System.UInt64":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Unsigned );
                IsPrimitiveType = true;
                break;

            case "System.Single":
                DebugType = new DebugBasicType( LlvmModule.Context.FloatType, LlvmModule, name, DiTypeKind.Float );
                IsPrimitiveType = true;
                break;

            case "System.Double":
                DebugType = new DebugBasicType( LlvmModule.Context.DoubleType, LlvmModule, name, DiTypeKind.Float );
                IsPrimitiveType = true;
                break;

            case "System.IntPtr":
            case "System.UIntPtr":
                // While this is generally void* in C#, LLVM doesn't have a void pointer type.
                // To help keep the debug views more readable, this creates a typedef for the
                // pointer, since pointer types don't have names
                var baseType = new DebugPointerType( LlvmModule.Context.Int8Type, LlvmModule, null );
                var typeDef = LlvmModule.DIBuilder.CreateTypedef( baseType, name.Substring( 7 ), null, 0, systemNamespace );
                DebugType = new DebugPointerType( baseType.NativeType, typeDef );
                IsPrimitiveType = true;
                break;

            default:
                // All other types are created as structures the Debug type is a temporary.
                // Creation of concrete DIType deferred until SetupFields when full field
                // layout information is known.
                DebugType = new DebugStructType( LlvmModule, name, LlvmModule.DICompileUnit, name );
                break;
            }

            TypeImplsReverseLookupForLlvmTypes[ DebugType.TypeHandle ] = this;
        }

        public bool IsDouble => DebugType.IsDouble;

        public bool IsFloat => DebugType.IsFloat;

        public bool IsFloatingPoint => DebugType.IsFloatingPoint;

        public bool IsInteger => DebugType.IsInteger;

        public bool IsPointer => DebugType.IsPointer;

        public bool IsStruct => DebugType.IsStruct;

        public bool IsValueType { get; }

        public bool IsBoxed { get; set; }

        public bool IsPrimitiveType { get; }

        public bool IsSigned { get; }

        internal _Module Module { get; }

        internal DIType DIType => DebugType.DIType;

        internal List< TypeField > Fields { get; } = new List< TypeField >( );

        public IDebugType<ITypeRef,DIType> DebugType { get; }

        public int SizeInBits { get; }

        public string Name { get; }

        public _Type UnderlyingType { get; internal set; }

        internal static _Type GetOrInsertTypeImpl(
            _Module owner,
            string name,
            int sizeInBits,
            bool isValueType,
            IDebugType<ITypeRef,DIType> ty )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new _Type( owner, name, sizeInBits, isValueType, ty );
            }

            return TypeImplMap[ name ];
        }

        internal static _Type GetOrInsertTypeImpl( _Module owner, string name, int sizeInBits, bool isValueType )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new _Type( owner, name, sizeInBits, isValueType );
            }

            return TypeImplMap[ name ];
        }

        internal static _Type GetTypeImpl( string name )
        {
            _Type retVal;
            if( TypeImplMap.TryGetValue( name, out retVal ) )
                return retVal;

            return null;
        }

        internal static _Type GetTypeImpl( ITypeRef ty )
        {
            _Type retVal;
            if( TypeImplsReverseLookupForLlvmTypes.TryGetValue( ty.TypeHandle, out retVal ) )
                return retVal;

            var pointerType = ty as IPointerType;
            if( pointerType != null && pointerType.ElementType.IsStruct )
            {
                return TypeImplsReverseLookupForLlvmTypes[ pointerType.ElementType.TypeHandle ];
            }

            Debug.Assert( false );
            return null;
        }

        public void AddField( uint offset, _Type type, string name )
        {
            TypeField field = new TypeField( name, type, offset );

            // Add the field in offset order, by moving all other pointer to fields
            // Not the most efficient approach for C# but mirrors the original C++ code
            Fields.Add( default(TypeField) ); // increase count by one

            var i = 0;
            for( i = Fields.Count - 1; i > 0; --i )
            {
                if( Fields[ i - 1 ].Offset <= field.Offset )
                    break;

                Fields[ i ] = Fields[ i - 1 ];
            }

            Fields[ i ] = field;
        }

        private void AddTypeToStruct( TypeField field, ICollection< ITypeRef > llvmFields, ref uint offset )
        {
            field.FinalIdx = ( uint ) llvmFields.Count;
            llvmFields.Add( field.MemberType.DebugType );
            offset += ( uint ) field.MemberType.SizeInBits / 8;
        }

        public void SetupFields( )
        {
            var structType = DebugType as DebugStructType;
            if( structType == null )
                return;

            var llvmFields = new List< ITypeRef >( );
            var idx = 0;
            uint offset = 0;
            while( offset < SizeInBits/8 )
            {
                if( ( idx < Fields.Count ) && ( Fields[ idx ].Offset == offset ) )
                {
                    AddTypeToStruct( Fields[ idx ], llvmFields, ref offset );
                    AddDiField( Fields[ idx ], idx, offset, flags: 0 );
                    ++idx;
                }
                else
                {
                    // Add explicit padding if necessary.
                    // TODO: Clean this up with a single byte array [ n x i8 ]
                    llvmFields.Add( GetOrInsertTypeImpl( Module, "System.Byte", 8, true ).DebugType );
                    ++offset;
                }
            }

            // Add in any remaining fields beyond the expected structure size. These should generally be zero-sized,
            // such as System.Object and the elements trailing strings/arrays.
            for( ; idx < Fields.Count; ++idx )
            {
                // NOTE: order of calls matters here as offset is a ref param and updated in the call to AddTypeToStruct.
                AddDiField( Fields[ idx ], idx, offset, flags: 0 );
                AddTypeToStruct( Fields[ idx ], llvmFields, ref offset );
            }

            // BUGBUG: We don't yet handle explicit struct layout with overlapping fields.

            structType.SetBody( true, LlvmModule, LlvmModule.DICompileUnit, null, 0, 0, llvmFields, DiFields, (uint)SizeInBits, 0 );
        }

        private void AddDiField( TypeField field, int index, uint offset, DebugInfoFlags flags )
        {
            var structType = DebugType as IStructType;
            Debug.Assert( structType != null );
            Debug.Assert( field.MemberType.DIType != null );
            var memberDiType = field.MemberType.DebugType;

            // TODO: since Zelig takes explicit control of layout the alignment needs to come from Zelig somewhere...
            var debugMemberInfo = new DebugMemberInfo { Name = field.Name
                                                      , Type = memberDiType
                                                      , Index = (uint)index
                                                      , Flags = flags
                                                      , ExplicitLayout = new DebugMemberLayout( ( uint )field.MemberType.SizeInBits, 0  /* ???? */, offset * 8UL)
                                                      /* todo: file, line... (Zelig IL parsing doesn't seem to capture source locations for fields)*/
                                                      };
            DiFields.Add( debugMemberInfo );
        }
    }
}
