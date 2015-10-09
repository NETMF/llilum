using System;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;

namespace Llvm.NET.Types
{
    /// <summary>LLVM Type</summary>
    internal class TypeRef 
        : ITypeRef
    {
        public IntPtr TypeHandle => TypeHandle_.Pointer;

        /// <summary>Flag to indicate if the type is sized</summary>
        public bool IsSized
        {
            get
            {
                if( Kind == TypeKind.Function )
                    return false;

                return NativeMethods.TypeIsSized( TypeHandle_ );
            }
        }

        /// <summary>LLVM Type kind for this type</summary>
        public TypeKind Kind => ( TypeKind )NativeMethods.GetTypeKind( TypeHandle_ );
        public bool IsInteger=> Kind == TypeKind.Integer;

        // Return true if value is 'float', a 32-bit IEEE fp type.
        public bool IsFloat => Kind == TypeKind.Float32;

        // Return true if this is 'double', a 64-bit IEEE fp type
        public bool IsDouble => Kind == TypeKind.Float64;

        public bool IsVoid => Kind == TypeKind.Void;

        public bool IsStruct => Kind == TypeKind.Struct;

        public bool IsPointer => Kind == TypeKind.Pointer;

        /// <summary>Flag to indicate if the type is a sequence type</summary>
        public bool IsSequence => Kind == TypeKind.Array || Kind == TypeKind.Vector || Kind == TypeKind.Pointer;

        public bool IsFloatingPoint
        {
            get
            {
                switch( Kind )
                {
                case TypeKind.Float16:
                case TypeKind.Float32:
                case TypeKind.Float64:
                case TypeKind.X86Float80:
                case TypeKind.Float128m112:
                case TypeKind.Float128:
                    return true;

                default:
                    return false;
                }
            }
        }

        public bool IsPointerPointer
        {
            get
            {
                var ptrType = this as IPointerType;
                return ptrType != null && ptrType.ElementType.Kind == TypeKind.Pointer;
            }
        }

        /// <summary>Context that owns this type</summary>
        public Context Context => Context.GetContextFor( TypeHandle_ );

        /// <summary>Integer bid width of this type or 0 for non integer types</summary>
        public uint IntegerBitWidth
        {
            get
            {
                if( Kind != TypeKind.Integer )
                    return 0;

                return NativeMethods.GetIntTypeWidth( TypeHandle_ );
            }
        }

        /// <summary>Gets a null value (e.g. all bits = 0 ) for the type</summary>
        /// <remarks>This is a getter function instead of a property as it can throw exceptions</remarks>
        public Constant GetNullValue() => Constant.NullValueFor( this );

        /// <summary>Retrieves an expression that results in the size of the type</summary>
        [Obsolete("Use TargetData layout information to compute size and create a constant from that")]
        public Constant GetSizeOfExpression( int pointerSize )
        {
            if( !IsSized
                || Kind == TypeKind.Void
                || Kind == TypeKind.Function
                || ( Kind == TypeKind.Struct && ( NativeMethods.IsOpaqueStruct( TypeHandle_ ) ) )
                )
            {
                return Context.CreateConstant( 0 );
            }

            var hSize = NativeMethods.SizeOf( TypeHandle_ );

            // LLVM uses an expression to construct Sizeof, however it is hard coded to
            // use an i64 as the type for the size, which isn't valid for 32 bit systems
            var sizeOfBitWidth = NativeMethods.GetIntTypeWidth( NativeMethods.TypeOf( hSize ) );
            var hIntPtr = new LLVMTypeRef( Context.GetIntType( ( uint )pointerSize ).TypeHandle );
            if( sizeOfBitWidth > pointerSize )
                hSize = NativeMethods.ConstTrunc( hSize, hIntPtr );
            else if( sizeOfBitWidth < pointerSize )
                hSize = NativeMethods.ConstZExt( hSize, hIntPtr );

            return Value.FromHandle<Constant>( hSize );
        }

        /// <summary>Array type factory for an array with elements of this type</summary>
        /// <param name="count">Number of elements in the array</param>
        /// <returns><see cref="IArrayType"/> for the array</returns>
        public IArrayType CreateArrayType( uint count ) => FromHandle<IArrayType>( NativeMethods.ArrayType( TypeHandle_, count ) );

        /// <summary>Get a <see cref="IPointerType"/> for a type that points to elements of this type in the default (0) address space</summary>
        /// <returns><see cref="IPointerType"/>corresponding to the type of a pointer that referns to elements of this type</returns>
        public IPointerType CreatePointerType( ) => CreatePointerType( 0 );

        /// <summary>Get a <see cref="IPointerType"/> for a type that points to elements of this type in the specified address space</summary>
        /// <param name="addressSpace">Address space for the pointer</param>
        /// <returns><see cref="IPointerType"/>corresponding to the type of a pointer that referns to elements of this type</returns>
        public IPointerType CreatePointerType( uint addressSpace ) => FromHandle<IPointerType>( NativeMethods.PointerType( TypeHandle_, addressSpace ) );

        public bool TryGetExtendedPropertyValue<T>( string id, out T value ) => ExtensibleProperties.TryGetExtendedPropertyValue<T>( id, out value );
        public void AddExtendedPropertyValue( string id, object value ) => ExtensibleProperties.AddExtendedPropertyValue( id, value );

        /// <summary>Builds a string repersentation for this type in LLVM assembly language form</summary>
        /// <returns>Formatted string for this type</returns>
        public override string ToString( )
        {
            var msgString = NativeMethods.PrintTypeToString( TypeHandle_ );
            return NativeMethods.MarshalMsg( msgString );
        }

        internal TypeRef( LLVMTypeRef typeRef )
        {
            TypeHandle_ = typeRef;
            if( typeRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( typeRef ) );

#if DEBUG
            var ctx = Llvm.NET.Context.GetContextFor( typeRef );
            ctx.AssertTypeNotInterned( typeRef );
#endif
        }

        internal static TypeRef FromHandle( LLVMTypeRef typeRef ) => FromHandle<TypeRef>( typeRef );

        internal static T FromHandle<T>( LLVMTypeRef typeRef )
            where T : class, ITypeRef

        {
            if( typeRef.Pointer == IntPtr.Zero )
                return null;

            var ctx = Context.GetContextFor( typeRef );
            return ( T )ctx.GetTypeFor( typeRef, StaticFactory );
        }

        private static ITypeRef StaticFactory( LLVMTypeRef typeRef )
        {
            var kind = (TypeKind)NativeMethods.GetTypeKind( typeRef );
            switch( kind )
            {
            case TypeKind.Struct:
                return new StructType( typeRef );

            case TypeKind.Array:
                return new ArrayType( typeRef );

            case TypeKind.Pointer:
                return new PointerType( typeRef );

            case TypeKind.Vector:
                return new VectorType( typeRef );

            case TypeKind.Function: // NOTE: This is a signature rather than a Function, which is a Value
                return new FunctionType( typeRef );

            // other types not yet supported in Object wrappers
            // but the pattern for doing so should be pretty obvious...
            case TypeKind.Void:
            case TypeKind.Float16:
            case TypeKind.Float32:
            case TypeKind.Float64:
            case TypeKind.X86Float80:
            case TypeKind.Float128m112:
            case TypeKind.Float128:
            case TypeKind.Label:
            case TypeKind.Integer:
            case TypeKind.Metadata:
            case TypeKind.X86MMX:
            default:
                return new TypeRef( typeRef );
            }
        }

        internal readonly LLVMTypeRef TypeHandle_;
        private readonly ExtensiblePropertyContainer ExtensibleProperties = new ExtensiblePropertyContainer( );
    }
}
