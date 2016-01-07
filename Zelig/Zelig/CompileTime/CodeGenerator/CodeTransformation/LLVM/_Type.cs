using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Types;
using Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    public class _Type
    {
        private static readonly Dictionary< string, _Type > TypeImplMap = new Dictionary< string, _Type >( );

        private NativeModule LlvmModule => Module.LlvmModule;

        internal List< DebugMemberInfo > DiFields = new List< DebugMemberInfo >( );

        internal _Type( _Module owner, string name, int sizeInBits, bool isValueType, IDebugType<ITypeRef, DIType> typeRef )
        {
            Debug.Assert( typeRef != null );
            Module = owner;
            SizeInBits = sizeInBits;
            IsValueType = isValueType;
            DebugType = typeRef;
            Name = name;
        }

        public _Type( _Module owner, TypeRepresentation tr, IDebugType<ITypeRef,DIType> typeRef )
        {
            TypeRepresentation = tr;
            Module = owner;
            SizeInBits = (int)tr.Size * 8;
            IsValueType = tr is ValueTypeRepresentation;
            Name = tr.FullName;
            DebugType = typeRef;
        }

        public _Type( _Module owner, TypeRepresentation tr )
        {
            TypeRepresentation = tr;
            Module = owner;
            SizeInBits = ( int )tr.Size * 8;
            IsValueType = tr is ValueTypeRepresentation;
            Name = tr.FullName;

            Debug.Assert( LlvmModule.GetTypeByName( Name ) == null, "Trying to override _Type" );
            DINamespace diNamespace = Module.GetOrCreateDINamespace( tr );
            IDebugType<ITypeRef, DIType> baseType;

            // Basic types need to use DebugBasicType since the LLVM primitive types are all uniqued
            // and some of them would end up overlapping (i.e. System.UInt16 and Char are both i16 to
            // LLVM but need distinct DIBasicType instances to describe them for the debugger)
            switch( Name )
            {
            case "System.Void":
                // DIType == null means void
                baseType = Llvm.NET.DebugInfo.DebugType.Create( LlvmModule.Context.VoidType, ( DIType )null );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                break;

            case "System.Char":
                baseType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )SizeInBits ), LlvmModule, tr.FullName, DiTypeKind.UTF );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                break;

            case "System.Boolean":
                baseType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )SizeInBits ), LlvmModule, tr.FullName, DiTypeKind.Boolean );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                break;

            case "System.SByte":
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
                baseType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )SizeInBits ), LlvmModule, tr.FullName, DiTypeKind.Signed );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                IsSigned = true;
                break;

            case "System.Byte":
            case "System.UInt16":
            case "System.UInt32":
            case "System.UInt64":
                baseType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )SizeInBits ), LlvmModule, tr.FullName, DiTypeKind.Unsigned );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                break;

            case "System.Single":
                baseType = new DebugBasicType( LlvmModule.Context.FloatType, LlvmModule, tr.FullName, DiTypeKind.Float );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                break;

            case "System.Double":
                baseType = new DebugBasicType( LlvmModule.Context.DoubleType, LlvmModule, tr.FullName, DiTypeKind.Float );
                DebugType = CreateBasicTypeDef( baseType, tr.Name, diNamespace );
                IsPrimitiveType = true;
                break;

            case "System.IntPtr":
            case "System.UIntPtr":
                // While this is generally void* in C#, LLVM doesn't have a void pointer type.
                // To help keep the debug views more readable, this creates a typedef for the
                // pointer, since pointer types don't have names
                var basePtrType = new DebugPointerType( LlvmModule.Context.Int8Type, LlvmModule, null );
                var typeDef = LlvmModule.DIBuilder.CreateTypedef( basePtrType, tr.Name, null, 0, diNamespace );
                DebugType = new DebugPointerType( basePtrType.NativeType, typeDef );
                IsPrimitiveType = true;
                break;

            default:
                // All other types are created as structures the Debug type is a temporary.
                // Creation of concrete DIType deferred until SetupFields when full field
                // layout information is known.
                DebugType = new DebugStructType( LlvmModule, tr.FullName, (DIScope)diNamespace ?? LlvmModule.DICompileUnit, tr.FullName );
                break;
            }
        }

        internal TypeRepresentation TypeRepresentation { get; }
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

        // This overload is used for synthetic types such as function signatures, memory arrays, pointers and boxed value types
        // that don't have a corresponding TypeRepresentation
        internal static _Type GetOrInsertTypeImpl( _Module owner, string name, int sizeInBits, bool isValueType, IDebugType<ITypeRef, DIType> ty )
        {
            _Type retVal;
            if( TypeImplMap.TryGetValue( name, out retVal ) )
                return retVal;

            retVal = new _Type( owner, name, sizeInBits, isValueType, ty );
            TypeImplMap.Add( name, retVal );
            return retVal;
        }

        internal static _Type GetOrInsertTypeImpl( _Module owner, TypeRepresentation tr, IDebugType<ITypeRef,DIType> ty )
        {
            _Type retVal;
            if( TypeImplMap.TryGetValue( tr.FullName, out retVal ) )
                return retVal;

            retVal = new _Type( owner, tr, ty );
            TypeImplMap.Add( tr.FullName, retVal );
            return retVal;
        }

        internal static _Type GetOrInsertTypeImpl( _Module owner, TypeRepresentation tr )
        {
            _Type retVal;
            if( TypeImplMap.TryGetValue( tr.FullName, out retVal ) )
                return retVal;

            retVal = new _Type( owner, tr );
            TypeImplMap.Add( tr.FullName, retVal );
            return retVal;
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
                    llvmFields.Add( GetOrInsertTypeImpl( Module, Module.TypeSystem.WellKnownTypes.System_Byte ).DebugType );
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
                                                      , DebugType = memberDiType
                                                      , Index = (uint)index
                                                      , DebugInfoFlags = flags
                                                      , ExplicitLayout = new DebugMemberLayout( ( uint )field.MemberType.SizeInBits, 0  /* ???? */, offset * 8UL)
                                                      /* todo: file, line... (Zelig IL parsing doesn't seem to capture source locations for fields)*/
                                                      };
            DiFields.Add( debugMemberInfo );
        }

        private IDebugType<ITypeRef, DIType> CreateBasicTypeDef( IDebugType<ITypeRef, DIType> baseType, string name, DINamespace ns )
        {
            //baseType = Llvm.NET.DebugInfo.DebugType.Create( LlvmModule.Context.VoidType, ( DIType )null );
            var typeDef = LlvmModule.DIBuilder.CreateTypedef( baseType.DIType, name, null, 0, ns );
            return Llvm.NET.DebugInfo.DebugType.Create( baseType.NativeType, typeDef );
        }
    }
}
