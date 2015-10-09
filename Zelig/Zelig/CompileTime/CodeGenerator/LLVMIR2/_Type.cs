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

        // REVIEW: Any reason these can't be consolidated into a single UnderlyingElementType?
        //         Not like you can have a boxed pointer...
        internal _Type UnderlyingBoxedType;
        internal _Type UnderlyingPointerType;

        public _Type( _Module module, string name, int sizeInBits, IDebugType<ITypeRef,DIType> typeRef )
        {
            Module = module;
            SizeInBits = sizeInBits;
            IsValueType = true;
            Name = name;
            DebugType = typeRef;
            TypeImplsReverseLookupForLlvmTypes[ DebugType.TypeHandle ] = this;
        }

        public _Type( _Module owner, string name, int sizeInBits )
        {
            SizeInBits = sizeInBits;
            IsValueType = true;
            Name = name;
            Module = owner;

            Debug.Assert( LlvmModule.GetTypeByName( name ) == null, "Trying to override _Type" );

            // basic types need to use DebugBasicType since the LLVM primitive types are all uniqued
            // and some of them would end up overlapping (i.e. System.UInt16 and Char are both i16 to
            // LLVM but need distinct DIBasicType instances to describe them for the debugger )
            switch( name )
            {
            case "System.Void":
                // DIType == null means void
                DebugType = Llvm.NET.DebugInfo.DebugType.Create( LlvmModule.Context.VoidType, ( DIType )null );
                break;

            case "LLVM.System.Char":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.UTF );
                break;

            case "LLVM.System.Boolean":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Boolean );
                break;

            case "LLVM.System.SByte":
            case "LLVM.System.Int16":
            case "LLVM.System.Int32":
            case "LLVM.System.Int64":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Signed );
                break;

            case "LLVM.System.Byte":
            case "LLVM.System.UInt16":
            case "LLVM.System.UInt32":
            case "LLVM.System.UInt64":
                DebugType = new DebugBasicType( LlvmModule.Context.GetIntType( ( uint )sizeInBits ), LlvmModule, name, DiTypeKind.Unsigned );
                break;

            case "LLVM.System.Single":
                DebugType = new DebugBasicType( LlvmModule.Context.FloatType, LlvmModule, name, DiTypeKind.Float );
                break;

            case "LLVM.System.Double":
                DebugType = new DebugBasicType( LlvmModule.Context.DoubleType, LlvmModule, name, DiTypeKind.Float );
                break;

            case "LLVM.System.IntPtr":
            case "LLVM.System.UIntPtr":
                // REVIEW: Shouldn't this be an integer value and not a pointer? 
                //         Standard  semantics for IntPtr is that of an integer
                //         that is the same size as a native pointer.
                DebugType = new DebugPointerType( LlvmModule.Context.Int8Type, LlvmModule, null);
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

        public bool IsPointer => GetLLVMObjectForStorage( ).IsPointer;

        public bool IsPointerPointer => DebugType.IsPointerPointer;

        public bool IsStruct => GetLLVMObjectForStorage( ).IsStruct;

        public bool IsValueType { get; set; }

        public bool IsBoxed { get; set; }

        // REVIEW:
        //  This appears to be unused, it is set from the constructors
        //  but has no calls to the getter... 
        public bool HasHeader { get; set; }

        internal _Module Module { get; }

        internal DIType DIType => DebugType.DIType;

        internal List< TypeField > Fields { get; } = new List< TypeField >( );

        internal List< _Type > FunctionArgs { get; } = new List< _Type >( );

        public IDebugType<ITypeRef,DIType> DebugType { get; }

        public int SizeInBits { get; }

        public string Name { get; }

        internal static _Type GetOrInsertTypeImpl( _Module owner, string name, int sizeInBits, IDebugType<ITypeRef,DIType> ty )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new _Type( owner, name, sizeInBits, ty );
            }

            return TypeImplMap[ name ];
        }

        internal static _Type GetOrInsertTypeImpl( _Module owner, string name, int sizeInBits )
        {
            if( !TypeImplMap.ContainsKey( name ) )
            {
                TypeImplMap[ name ] = new _Type( owner, name, sizeInBits );
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
            if( pointerType != null && pointerType.ElementType.IsStruct( ) )
            {
                return TypeImplsReverseLookupForLlvmTypes[ pointerType.ElementType.TypeHandle ];
            }

            Debug.Assert( false );
            return null;
        }

        public int GetSizeInBitsForStorage( )
        {
            if( IsValueType )
                return SizeInBits;

            return ( int ) ( LlvmModule.Layout.PointerSize( ) )*8;
        }

        public void AddField( uint offset, _Type type, bool forceInline, string name )
        {
            var f = new TypeField( name, type, offset, forceInline );

            // Add the field in offset order, by moving all other pointer to fields
            // Not the most efficient approach for C# but mirrors the original C++ code
            Fields.Add( default(TypeField) ); // increase count by one

            var i = 0;
            for( i = Fields.Count - 1; i > 0; --i )
            {
                if( Fields[ i - 1 ].Offset <= f.Offset )
                    break;

                Fields[ i ] = Fields[ i - 1 ];
            }
            Fields[ i ] = f;
        }

        public _Type GetBTUnderlyingType( ) => Module.GetType( $"LLVM.{Name}" );

        private void AddTypeToStruct( TypeField field, ICollection< ITypeRef > llvmFields, ref uint offset )
        {
            field.FinalIdx = ( uint ) llvmFields.Count;
            if( field.ForceInline )
            {
                llvmFields.Add( field.MemberType.DebugType );
                offset += ( uint ) field.MemberType.SizeInBits/8;
            }
            else
            {
                llvmFields.Add( field.MemberType.GetLLVMObjectForStorage( ) );
                offset += ( uint ) field.MemberType.GetSizeInBitsForStorage( )/8u;
            }
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
                    AddDiField( Fields[ idx ], idx, offset );
                    ++idx;
                }
                else
                {
                    // Add explicit padding if necessary.
                    // TODO: Clean this up with a single byte array [ n x i8 ]
                    llvmFields.Add( GetOrInsertTypeImpl( Module, "System.Byte", 8 ).DebugType );
                    ++offset;
                }
            }

            // Add in any remaining fields beyond the expected structure size. These should generally be zero-sized,
            // such as System.Object and the elements trailing strings/arrays.
            for( ; idx < Fields.Count; ++idx )
            {
                // NOTE: order of calls matters here as offset is a ref param and updated
                //       in the call to AddTypeToStruct
                AddDiField( Fields[ idx ], idx, offset );
                AddTypeToStruct( Fields[ idx ], llvmFields, ref offset );
            }

            // BUGBUG: We don't yet handle explicit struct layout with overlapping fields.

            structType.SetBody( true, LlvmModule, LlvmModule.DICompileUnit, null, 0, 0, llvmFields, DiFields, (uint)SizeInBits, 0 );
        }

        private void AddDiField( TypeField field, int index, uint offset )
        {
            var structType = DebugType as IStructType;
            Debug.Assert( structType != null );
            Debug.Assert( field.MemberType.DIType != null );
            var memberDiType = field.MemberType.GetDiTypeForStack( );
            if( field.ForceInline )
                memberDiType = field.MemberType.DebugType;

            // TODO: since Zelig takes explicit control of layout the alignment needs to come from Zelig somewhere...
            var debugMemberInfo = new DebugMemberInfo { Name = field.Name
                                                      , Type = memberDiType
                                                      , Index = (uint)index
                                                      , BitSize = (uint)field.MemberType.SizeInBits
                                                      , BitOffset = offset * 8UL
                                                      , BitAlignment = 0  /* ???? */
                                                      /* todo: file, line...*/
                                                      };
            DiFields.Add( debugMemberInfo );
        }

        internal IDebugType<ITypeRef, DIType> GetLLVMObjectForStorage( )
        {
            if( IsValueType )
                return DebugType;

            return new DebugPointerType( DebugType, LlvmModule );
        }

        internal IDebugType<ITypeRef,DIType> GetDiTypeForStack( )
        {
            if( IsValueType )
                return DebugType;

            return new DebugPointerType( DebugType, LlvmModule );
        }

        internal void Dump( )
        {
            Console.WriteLine( "Name:{0}\n", Name );
            Console.WriteLine( DebugType.ToString( ) );
            Console.WriteLine( );
        }
    }
}
