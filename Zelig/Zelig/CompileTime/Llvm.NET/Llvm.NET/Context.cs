using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Llvm.NET.DebugInfo;
using Llvm.NET.Types;
using Llvm.NET.Values;

namespace Llvm.NET
{
    /// <summary>Encapsulates an LLVM context</summary>
    /// <remarks>
    /// <para>A context in LLVM is a container for interning (LLVM refers
    /// to this as "uniqueing") various types and values in the system. This
    /// allows running multiple LLVM tool transforms etc.. on different threads
    /// without causing them to collide namespaces and types even if they use
    /// the same name (e.g. module one may have a type Foo, and so does module
    /// two but they are completely distinct from each other)
    ///</para>
    /// <para>LLVM Debug information is ultimately all parented to a top level
    /// <see cref="DICompileUnit"/> as the scope, and a compilation
    /// unit is bound to a <see cref="NativeModule"/>, even though, technically
    /// the types are owned by a Context. Thus to keep things simpler and help
    /// make working with debug information easier. Lllvm.NET encapsulates the
    /// native type and the debug type in separate classes that are instances
    /// of the <see cref="IDebugType{NativeT, DebugT}"/> interface </para>
    /// <note type="note">It is important to be aware of the fact that a Context
    /// is not thread safe. The context itself and the object instances it owns
    /// are intended for use by a single thread only. Accessing and manipulating
    /// LLVM objects from multiple threads may lead to race conditions corrupted
    /// state and any number of other undefined issues.</note>
    /// </remarks>
    public sealed class Context : IDisposable
    {
        /// <summary>Creates a new context</summary>
        public Context( )
            : this( NativeMethods.ContextCreate( ) )
        {
        }

        /// <summary>Flag to indicate if this instance is still valid</summary>
        public bool IsDisposed => ContextHandle.Pointer == IntPtr.Zero;

        /// <summary>Get's the LLVM void type for this context</summary>
        public ITypeRef VoidType => TypeRef.FromHandle( NativeMethods.VoidTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM boolean type for this context</summary>
        public ITypeRef BoolType => TypeRef.FromHandle( NativeMethods.Int1TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 8 bit integer type for this context</summary>
        public ITypeRef Int8Type => TypeRef.FromHandle( NativeMethods.Int8TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 16 bit integer type for this context</summary>
        public ITypeRef Int16Type => TypeRef.FromHandle( NativeMethods.Int16TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 32 bit integer type for this context</summary>
        public ITypeRef Int32Type => TypeRef.FromHandle( NativeMethods.Int32TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 64 bit integer type for this context</summary>
        public ITypeRef Int64Type => TypeRef.FromHandle( NativeMethods.Int64TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM half precision floating point type for this context</summary>
        public ITypeRef HalfFloatType => TypeRef.FromHandle( NativeMethods.HalfTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM single precision floating point type for this context</summary>
        public ITypeRef FloatType => TypeRef.FromHandle( NativeMethods.FloatTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM double precision floating point type for this context</summary>
        public ITypeRef DoubleType => TypeRef.FromHandle( NativeMethods.DoubleTypeInContext( ContextHandle ) );

        /// <summary>Gets an enumerable collection of all the metadata created in this context</summary>
        public IEnumerable<LlvmMetadata> Metadata => MetadataCache.Values;

        /// <summary>Get a type that is a pointer to a value of a given type</summary>
        /// <param name="elementType">Type of value the pointer points to</param>
        /// <returns><see cref="IPointerType"/> for a pointer that references a value of type <paramref name="elementType"/></returns>
        public IPointerType GetPointerTypeFor( ITypeRef elementType )
        {
            if( elementType == null )
                throw new ArgumentNullException( nameof( elementType ) );

            if( elementType.Context != this )
                throw new ArgumentException( "Cannot mix types from different contexts", nameof( elementType ) );

            return TypeRef.FromHandle<IPointerType>( NativeMethods.PointerType( elementType.GetTypeRef( ), 0 ) );
        }

        /// <summary>Get's an LLVM integer type of arbitrary bit width</summary>
        /// <remarks>
        /// For standard integer bit widths (e.g. 1,8,16,32,64) this will return
        /// the same type as the corresponding specialized property.
        /// (e.g. GetIntType(1) is the same as <see cref="BoolType"/>,
        ///  GetIntType(16) is the same as <see cref="Int16Type"/>, etc... )
        /// </remarks>
        public ITypeRef GetIntType( uint bitWidth )
        {
            if( bitWidth == 0 )
                throw new ArgumentException( "integer bit width must be greater than 0" );

            switch( bitWidth )
            {
            case 1:
                return BoolType;
            case 8:
                return Int8Type;
            case 16:
                return Int16Type;
            case 32:
                return Int32Type;
            case 64:
                return Int64Type;
            default:
                return TypeRef.FromHandle( NativeMethods.IntTypeInContext( ContextHandle, bitWidth ) );
            }
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Optional set of function argument types</param>
        /// <returns>Signature type for the specified signature</returns>
        public IFunctionType GetFunctionType( ITypeRef returnType, params ITypeRef[ ] args )
        {
            return GetFunctionType( returnType, args, false );
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Potentially empty set of function argument types</param>
        /// <returns>Signature type for the specified signature</returns>
        public IFunctionType GetFunctionType( ITypeRef returnType, IEnumerable<ITypeRef> args )
        {
            return GetFunctionType( returnType, args, false );
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Potentially empty set of function argument types</param>
        /// <param name="isVarArgs">Flag to indicate if the method supports C/C++ style VarArgs</param>
        /// <returns>Signature type for the specified signature</returns>
        public IFunctionType GetFunctionType( ITypeRef returnType, IEnumerable<ITypeRef> args, bool isVarArgs )
        {
            if( returnType == null )
                throw new ArgumentNullException( nameof( returnType ) );

            if( ContextHandle.Pointer != returnType.Context.ContextHandle.Pointer )
                throw new ArgumentException( "Mismatched context", nameof( returnType ) );

            LLVMTypeRef[ ] llvmArgs = args.Select( a => a.GetTypeRef( ) ).ToArray( );
            var argCount = llvmArgs.Length;
            // have to pass a valid addressable object to native interop
            // so allocate space for a single value but tell LLVM the length is 0
            if( llvmArgs.Length == 0 )
                llvmArgs = new LLVMTypeRef[ 1 ];

            var signature = NativeMethods.FunctionType( returnType.GetTypeRef( ), out llvmArgs[ 0 ], ( uint )argCount, isVarArgs );
            return TypeRef.FromHandle<IFunctionType>( signature );
        }

        /// <summary>Creates a FunctionType with Debug information</summary>
        /// <param name="diBuilder"><see cref="DebugInfoBuilder"/>to use to create the debug information</param>
        /// <param name="retType">Return type of the function</param>
        /// <param name="argTypes">Argument types of the function</param>
        /// <returns>Function signature</returns>
        public DebugFunctionType CreateFunctionType( DebugInfoBuilder diBuilder
                                                   , IDebugType<ITypeRef, DIType> retType
                                                   , params IDebugType<ITypeRef, DIType>[ ] argTypes
                                                   )
        {
            return CreateFunctionType( diBuilder, false, retType, argTypes );
        }

        /// <summary>Creates a FunctionType with Debug information</summary>
        /// <param name="diBuilder"><see cref="DebugInfoBuilder"/>to use to create the debug information</param>
        /// <param name="retType">Return type of the function</param>
        /// <param name="argTypes">Argument types of the function</param>
        /// <returns>Function signature</returns>
        public DebugFunctionType CreateFunctionType( DebugInfoBuilder diBuilder
                                                   , IDebugType<ITypeRef, DIType> retType
                                                   , IEnumerable<IDebugType<ITypeRef, DIType>> argTypes
                                                   )
        {
            return CreateFunctionType( diBuilder, false, retType, argTypes );
        }

        /// <summary>Creates a FunctionType with Debug information</summary>
        /// <param name="diBuilder"><see cref="DebugInfoBuilder"/>to use to create the debug information</param>
        /// <param name="isVarArg">Flag to indicate if this function is variadic</param>
        /// <param name="retType">Return type of the function</param>
        /// <param name="argTypes">Argument types of the function</param>
        /// <returns>Function signature</returns>
        public DebugFunctionType CreateFunctionType( DebugInfoBuilder diBuilder
                                                   , bool isVarArg
                                                   , IDebugType<ITypeRef, DIType> retType
                                                   , params IDebugType<ITypeRef, DIType>[ ] argTypes
                                                   )
        {
            return CreateFunctionType( diBuilder, isVarArg, retType, ( IEnumerable<IDebugType<ITypeRef, DIType>> )argTypes );
        }

        /// <summary>Creates a FunctionType with Debug information</summary>
        /// <param name="diBuilder"><see cref="DebugInfoBuilder"/>to use to create the debug information</param>
        /// <param name="isVarArg">Flag to indicate if this function is variadic</param>
        /// <param name="retType">Return type of the function</param>
        /// <param name="argTypes">Argument types of the function</param>
        /// <returns>Function signature</returns>
        public DebugFunctionType CreateFunctionType( DebugInfoBuilder diBuilder
                                                   , bool isVarArg
                                                   , IDebugType<ITypeRef, DIType> retType
                                                   , IEnumerable<IDebugType<ITypeRef, DIType>> argTypes
                                                   )
        {
            if( diBuilder == null )
                throw new ArgumentNullException( nameof( diBuilder ) );

            if( !retType.HasDebugInfo( ) )
                throw new ArgumentNullException( nameof( retType ), "Return type does not have debug information" );

            var nativeArgTypes = new List<ITypeRef>( );
            var debugArgTypes = new List<DIType>( );
            var msg = new StringBuilder( "One or more parameter types do not include debug information:\n" );
            var missingDebugInfo = false;

            foreach( var indexedPair in argTypes.Select( ( t, i ) => new { Type = t, Index = i } ) )
            {
                nativeArgTypes.Add( indexedPair.Type.NativeType );
                debugArgTypes.Add( indexedPair.Type.DIType );
                if( indexedPair.Type.HasDebugInfo( ) )
                    continue;

                msg.AppendFormat( "\tArgument {0} does not contain debug type information", indexedPair.Index );
                missingDebugInfo = true;
            }

            // if any parameters don't have a valid DIType yet, then provide a hopefully helpful message indicating which one(s)
            if( missingDebugInfo )
                throw new ArgumentException( msg.ToString( ), nameof( argTypes ) );

            var llvmType = GetFunctionType( retType.NativeType, nativeArgTypes, isVarArg );

            var diType = diBuilder.CreateSubroutineType( 0, retType.DIType, debugArgTypes );
            Debug.Assert( diType != null && !diType.IsTemporary );

            return new DebugFunctionType( llvmType, diType );
        }

        /// <summary>Creates a constant structure from a set of values</summary>
        /// <param name="packed">Flag to indicate if the structure is packed and no alignment should be applied to the members</param>
        /// <param name="values">Set of values to use in forming the structure</param>
        /// <returns>Newly created <see cref="Constant"/></returns>
        /// <remarks>
        /// <note type="note">The actual concrete return type depends on the parameters provided and will be one of the following:
        /// <list type="table">
        /// <listheader>
        /// <term><see cref="Constant"/> derived type</term><description>Description</description>
        /// </listheader>
        /// <item><term>ConstantAggregateZero</term><description>If all the member values are zero constants</description></item>
        /// <item><term>UndefValue</term><description>If all the member values are UndefValue</description></item>
        /// <item><term>ConstantStruct</term><description>All other cases</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public Constant CreateConstantStruct( bool packed, params Constant[ ] values )
        {
            return CreateConstantStruct( packed, ( IEnumerable<Constant> )values );
        }

        /// <summary>Creates a constant structure from a set of values</summary>
        /// <param name="values">Set of values to use in forming the structure</param>
        /// <param name="packed">Flag to indicate if the structure is packed and no alignment should be applied to the members</param>
        /// <returns>Newly created <see cref="Constant"/></returns>
        /// <remarks>
        /// <note type="note">The actual concrete return type depends on the parameters provided and will be one of the following:
        /// <list type="table">
        /// <listheader>
        /// <term><see cref="Constant"/> derived type</term><description>Description</description>
        /// </listheader>
        /// <item><term>ConstantAggregateZero</term><description>If all the member values are zero constants</description></item>
        /// <item><term>UndefValue</term><description>If all the member values are UndefValue</description></item>
        /// <item><term>ConstantStruct</term><description>All other cases</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public Constant CreateConstantStruct( bool packed, IEnumerable<Constant> values )
        {
            var valueHandles = values.Select( v => v.ValueHandle ).ToArray( );
            if( valueHandles.Length == 0 )
                throw new ArgumentException( "structure must have at least one element", nameof( values ) );

            var handle = NativeMethods.ConstStructInContext( ContextHandle, out valueHandles[ 0 ], ( uint )valueHandles.Length, packed );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a constant instance of a specified structure type from a set of values</summary>
        /// <param name="type">Type of the structure to create</param>
        /// <param name="values">Set of values to use in forming the structure</param>
        /// <returns>Newly created <see cref="Constant"/></returns>
        /// <remarks>
        /// <note type="note">The actual concrete return type depends on the parameters provided and will be one of the following:
        /// <list type="table">
        /// <listheader>
        /// <term><see cref="Constant"/> derived type</term><description>Description</description>
        /// </listheader>
        /// <item><term>ConstantAggregateZero</term><description>If all the member values are zero constants</description></item>
        /// <item><term>UndefValue</term><description>If all the member values are UndefValue</description></item>
        /// <item><term>ConstantStruct</term><description>All other cases</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public Constant CreateNamedConstantStruct( IStructType type, params Constant[ ] values )
        {
            return CreateNamedConstantStruct( type, ( IEnumerable<Constant> )values );
        }

        /// <summary>Creates a constant instance of a specified structure type from a set of values</summary>
        /// <param name="type">Type of the structure to create</param>
        /// <param name="values">Set of values to use in forming the structure</param>
        /// <returns>Newly created <see cref="Constant"/></returns>
        /// <remarks>
        /// <note type="note">The actual concrete return type depends on the parameters provided and will be one of the following:
        /// <list type="table">
        /// <listheader>
        /// <term><see cref="Constant"/> derived type</term><description>Description</description>
        /// </listheader>
        /// <item><term>ConstantAggregateZero</term><description>If all the member values are zero constants</description></item>
        /// <item><term>UndefValue</term><description>If all the member values are UndefValue</description></item>
        /// <item><term>ConstantStruct</term><description>All other cases</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public Constant CreateNamedConstantStruct( IStructType type, IEnumerable<Constant> values )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            if( type.Context != this )
                throw new ArgumentException( "Cannot create named constant struct with type from another context", nameof( type ) );

            var valueList = values as IList<Constant> ?? values.ToList( );
            var valueHandles = valueList.Select( v => v.ValueHandle ).ToArray( );
            if( type.Members.Count != valueHandles.Length )
                throw new ArgumentException( "Number of values provided must match the number of elements in the specified type" );

            var mismatchedTypes = from indexedVal in valueList.Select( ( v, i ) => new { Value = v, Index = i } )
                                  where indexedVal.Value.NativeType != type.Members[ indexedVal.Index ]
                                  select indexedVal;

            if( mismatchedTypes.Any( ) )
            {
                var msg = new StringBuilder( "One or more values provided do not match the corresponding member type:" );
                msg.AppendLine( );
                foreach( var mismatch in mismatchedTypes )
                {
                    msg.AppendFormat( "\t[{0}]: member type={1}; value type={2}"
                                    , mismatch.Index
                                    , type.Members[ mismatch.Index ]
                                    , valueList[ mismatch.Index ].NativeType
                                    );
                }
                throw new ArgumentException( msg.ToString( ) );
            }

            // To interop correctly, we need to have an array of at least size one.
            uint valuesLength = (uint)valueHandles.Length;
            if( valuesLength == 0 )
            {
                valueHandles = new LLVMValueRef[1];
            }

            var handle = NativeMethods.ConstNamedStruct( type.GetTypeRef( ), out valueHandles[ 0 ], valuesLength );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Create an empty structure type</summary>
        /// <param name="name">Name of the type</param>
        /// <returns>New type</returns>
        public IStructType CreateStructType( string name )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentNullException( nameof( name ) );

            var handle = NativeMethods.StructCreateNamed( ContextHandle, name );
            return TypeRef.FromHandle<IStructType>( handle );
        }

        /// <summary>Create an anonymous structure type (e.g. Tuple)</summary>
        /// <param name="packed">Flag to indicate if the structure is "packed"</param>
        /// <param name="element0">Type of the first field of the structure</param>
        /// <param name="elements">Types of any additional fields of the structure</param>
        /// <returns>
        /// <see cref="IStructType"/> with the specified body defined.
        /// </returns>
        public IStructType CreateStructType( bool packed, ITypeRef element0, params ITypeRef[ ] elements )
        {
            if( elements == null )
                throw new ArgumentNullException( nameof( elements ) );

            LLVMTypeRef[ ] llvmArgs = new LLVMTypeRef[ elements.Length + 1 ];
            llvmArgs[ 0 ] = element0.GetTypeRef( );
            for( int i = 1; i < llvmArgs.Length; ++i )
                llvmArgs[ i ] = elements[ i - 1 ].GetTypeRef( );

            var handle = NativeMethods.StructTypeInContext( ContextHandle, out llvmArgs[ 0 ], ( uint )llvmArgs.Length, packed );
            return TypeRef.FromHandle<IStructType>( handle );
        }

        /// <summary>Creates a new structure type in this <see cref="Context"/></summary>
        /// <param name="name">Name of the structure</param>
        /// <param name="packed">Flag indicating if the structure is packed</param>
        /// <param name="elements">Types for the structures elements in layout order</param>
        /// <returns>
        /// <see cref="IStructType"/> with the specified body defined.
        /// </returns>
        /// <remarks>
        /// If the elements argument list is empty then an opaque type is created (e.g. a forward reference)
        /// The <see cref="IStructType.SetBody(bool, ITypeRef[])"/> method provides a means to add a body to
        /// an opaque type at a later time if the details of the body are required. (If only pointers to
        /// to the type are required the body isn't required) 
        /// </remarks>
        public IStructType CreateStructType( string name, bool packed, params ITypeRef[ ] elements )
        {
            if( elements == null )
                throw new ArgumentNullException( nameof( elements ) );

            var retVal = TypeRef.FromHandle<IStructType>( NativeMethods.StructCreateNamed( ContextHandle, name ) );
            if( elements.Length > 0 )
            {
                retVal.SetBody( packed, elements );
            }
            return retVal;
        }

        /// <summary>Creates a metadata string from the given string</summary>
        /// <param name="value">string to create as metadata</param>
        /// <returns>new metadata string</returns>
        public MDString CreateMetadataString( string value )
        {
            value = value ?? string.Empty;
            var handle = NativeMethods.MDString2( ContextHandle, value, ( uint )value.Length );
            return new MDString( handle );
        }

        /// <summary>Create a constant data string value</summary>
        /// <param name="value">string to convert into an LLVM constant value</param>
        /// <returns>new <see cref="ConstantDataArray"/></returns>
        /// <remarks>
        /// This converts th string to ANSI form and creates an LLVM constant array of i8 
        /// characters for the data without any terminating null character.
        /// </remarks>
        public ConstantDataArray CreateConstantString( string value )
        {
            if( value == null )
                throw new ArgumentNullException( nameof( value ) );

            var handle = NativeMethods.ConstStringInContext( ContextHandle, value, ( uint )value.Length, true );
            return Value.FromHandle<ConstantDataArray>( handle );
        }

        /// <summary>Create a constant data string value</summary>
        /// <param name="value">string to convert into an LLVM constant value</param>
        /// <param name="nullTerminate">flag to indicate if the string should include a null terminator</param>
        /// <returns>new <see cref="ConstantDataArray"/></returns>
        /// <remarks>
        /// This converts the string to ANSI form and creates an LLVM constant array of i8 
        /// characters for the data without any terminating null character.
        /// </remarks>
        public ConstantDataArray CreateConstantString( string value, bool nullTerminate )
        {
            if( value == null )
                throw new ArgumentNullException( nameof( value ) );

            var handle = NativeMethods.ConstStringInContext( ContextHandle, value, ( uint )value.Length, !nullTerminate );
            return Value.FromHandle<ConstantDataArray>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 1</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( bool constValue )
        {
            var handle = NativeMethods.ConstInt( BoolType.GetTypeRef( )
                                               , ( ulong )( constValue ? 1 : 0 )
                                               , new LLVMBool( 0 )
                                               );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 8</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( byte constValue )
        {
            var handle = NativeMethods.ConstInt( Int8Type.GetTypeRef( ), constValue, new LLVMBool( 0 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 8</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( sbyte constValue )
        {
            var handle = NativeMethods.ConstInt( Int8Type.GetTypeRef( ), ( ulong )constValue, new LLVMBool( 1 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 16</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( Int16 constValue )
        {
            var handle = NativeMethods.ConstInt( Int16Type.GetTypeRef( ), ( ulong )constValue, new LLVMBool( 1 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 16</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public Constant CreateConstant( UInt16 constValue )
        {
            var handle = NativeMethods.ConstInt( Int16Type.GetTypeRef( ), ( ulong )constValue, new LLVMBool( 0 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 32</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( Int32 constValue )
        {
            var handle = NativeMethods.ConstInt( Int32Type.GetTypeRef( ), ( ulong )constValue, new LLVMBool( 1 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 32</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( UInt32 constValue )
        {
            var handle = NativeMethods.ConstInt( Int32Type.GetTypeRef( ), constValue, new LLVMBool( 0 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( Int64 constValue )
        {
            var handle = NativeMethods.ConstInt( Int64Type.GetTypeRef( ), ( ulong )constValue, new LLVMBool( 1 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="constValue">Value for the constant</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public Constant CreateConstant( UInt64 constValue )
        {
            var handle = NativeMethods.ConstInt( Int64Type.GetTypeRef( ), constValue, new LLVMBool( 0 ) );
            return Value.FromHandle<Constant>( handle );
        }

        /// <summary>Creates a new <see cref="ConstantInt"/> with a bit length of 64</summary>
        /// <param name="bitWidth">Bit width of the integer</param>
        /// <param name="constValue">Value for the constant</param>
        /// <param name="signExtend">flag to indicate if the const value should be sign extended</param>
        /// <returns><see cref="ConstantInt"/> representing the value</returns>
        public Constant CreateConstant( uint bitWidth, UInt64 constValue, bool signExtend )
        {
            var intType = GetIntType( bitWidth );
            return CreateConstant( intType, constValue, signExtend );
        }

        /// <summary>Create a constant value of the specified integer type</summary>
        /// <param name="intType">Integer type</param>
        /// <param name="constValue">value</param>
        /// <param name="signExtend">flag to indicate if <paramref name="constValue"/> is sign extended</param>
        /// <returns>Constant for the specified value</returns>
        public Constant CreateConstant( ITypeRef intType, UInt64 constValue, bool signExtend )
        {
            if( intType == null )
                throw new ArgumentNullException( nameof( intType ) );

            if( intType.Context != this )
                throw new ArgumentException( "Cannot mix types from different contexts", nameof( intType ) );

            if( intType.Kind != TypeKind.Integer )
                throw new ArgumentException( "Integer type required", nameof( intType ) );

            return Value.FromHandle<Constant>( NativeMethods.ConstInt( intType.GetTypeRef( ), constValue, signExtend ) );
        }

        /// <summary>Creates a constant floating point value for a given value</summary>
        /// <param name="constValue">Value to make into a <see cref="ConstantFP"/></param>
        /// <returns>Constant value</returns>
        public ConstantFP CreateConstant( float constValue )
        {
            return Value.FromHandle<ConstantFP>( NativeMethods.ConstReal( FloatType.GetTypeRef( ), constValue ) );
        }

        /// <summary>Creates a constant floating point value for a given value</summary>
        /// <param name="constValue">Value to make into a <see cref="ConstantFP"/></param>
        /// <returns>Constant value</returns>
        public ConstantFP CreateConstant( double constValue )
        {
            return Value.FromHandle<ConstantFP>( NativeMethods.ConstReal( DoubleType.GetTypeRef( ), constValue ) );
        }

        /// <summary>Current context for the current thread</summary>
        /// <remarks>
        /// While most LLVM types contain the ability to retrieve their owning <see cref="Context"/>
        /// it is not always possible. For example, in LLVM, an empty <see cref="AttributeSet"/> doesn't 
        /// actually have a Context so adding attributes to an empty <see cref="AttributeSet"/> requires
        /// a <see cref="Context"/> but adding attributes to a non-empty set does not. Thus, without some
        /// other means of getting a default <see cref="Context"/> multiple overloads for the Add operation
        /// would need to exist. Furthermore, calling code would need to manage dealing with deciding which
        /// variation of the overloaded methods to call at runtime. All of that and other similar cases are
        /// eliminated by having a thread static property to access whenever a <see cref="Context"/> instance
        /// is required but otherwise not conveniently available. 
        /// </remarks>
        public static Context CurrentContext => CurrentThreadContext;

        // These methods provide unique mapping between the .NET wrappers and the underlying LLVM instances
        // The mapping ensures that any LibLLVM handle is always re-mappable to a exactly one wrapper instance.
        // This helps reduce the number of wrapper instances created and also allows reference equality to work
        // as expected for managed types.
        #region Interning Factories
        internal void AddModule( NativeModule module )
        {
            ModuleCache.Add( module.ModuleHandle.Pointer, module );
        }

        internal NativeModule GetModuleFor( LLVMModuleRef moduleRef )
        {
            if( moduleRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( moduleRef ) );

            var hModuleContext = NativeMethods.GetModuleContext( moduleRef );
            if( hModuleContext.Pointer != ContextHandle.Pointer )
                throw new ArgumentException( "Incorrect context for module" );

            NativeModule retVal;
            if( !ModuleCache.TryGetValue( moduleRef.Pointer, out retVal ) )
                retVal = new NativeModule( moduleRef );

            return retVal;
        }

        internal static Context GetContextFor( LLVMModuleRef moduleRef )
        {
            if( moduleRef.Pointer == IntPtr.Zero )
                return null;

            var hContext = NativeMethods.GetModuleContext( moduleRef );
            Debug.Assert( hContext.Pointer != IntPtr.Zero );
            return GetContextFor( hContext );
        }

        internal static Context GetContextFor( LLVMValueRef valueRef )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                return null;

            var hType = NativeMethods.TypeOf( valueRef );
            Debug.Assert( hType.Pointer != IntPtr.Zero );
            return GetContextFor( hType );
        }

        internal static Context GetContextFor( LLVMTypeRef typeRef )
        {
            if( typeRef.Pointer == IntPtr.Zero )
                return null;

            var hContext = NativeMethods.GetTypeContext( typeRef );
            Debug.Assert( hContext.Pointer != IntPtr.Zero );
            return GetContextFor( hContext );
        }

        internal static Context GetContextFor( LLVMContextRef contextRef )
        {
            if( contextRef.Pointer == IntPtr.Zero )
                return null;

            lock ( ContextCache )
            {
                Context retVal;
                if( ContextCache.TryGetValue( contextRef, out retVal ) )
                    return retVal;

                return new Context( contextRef );
            }
        }

        internal static Context GetContextFor( LLVMMetadataRef handle )
        {
            if( handle == LLVMMetadataRef.Zero )
                return null;

            var hContext = NativeMethods.GetNodeContext( handle );
            Debug.Assert( hContext.Pointer != IntPtr.Zero );
            return GetContextFor( hContext );
        }

        internal void RemoveDeletedNode( MDNode node )
        {
            MetadataCache.Remove( node.MetadataHandle );
        }

        internal LLVMContextRef ContextHandle { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conditional attribute makes this an empty method in release builds" )]
        [Conditional( "DEBUG" )]
        internal void AssertValueNotInterned( LLVMValueRef valueRef )
        {
            Debug.Assert( !ValueCache.ContainsKey( valueRef.Pointer ) );
        }

        internal Value GetValueFor( LLVMValueRef valueRef, Func<LLVMValueRef, Value> constructor )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( valueRef ) );

            Value retVal = null;
            if( ValueCache.TryGetValue( valueRef.Pointer, out retVal ) )
                return retVal;

            retVal = constructor( valueRef );
            ValueCache.Add( valueRef.Pointer, retVal );
            return retVal;
        }

        internal LlvmMetadata GetNodeFor( LLVMMetadataRef handle, Func<LLVMMetadataRef, LlvmMetadata> staticFactory )
        {
            if( handle == LLVMMetadataRef.Zero )
                throw new ArgumentNullException( nameof( handle ) );

            LlvmMetadata retVal = null;
            if( MetadataCache.TryGetValue( handle, out retVal ) )
                return retVal;

            retVal = staticFactory( handle );
            MetadataCache.Add( handle, retVal );
            return retVal;
        }

        internal MDOperand GetOperandFor( MDNode owningNode, LLVMMDOperandRef handle )
        {
            if( owningNode.Context != this )
                throw new ArgumentException( "Cannot get operand for a node from a different context", nameof( owningNode ) );

            if( handle.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( handle ) );

            MDOperand retVal = null;
            if( MDOperandCache.TryGetValue( handle, out retVal ) )
                return retVal;

            retVal = new MDOperand( owningNode, handle );
            MDOperandCache.Add( handle, retVal );
            return retVal;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conditional attribute makes this an empty method in release builds" )]
        [Conditional( "DEBUG" )]
        internal void AssertTypeNotInterned( LLVMTypeRef typeRef )
        {
            Debug.Assert( !TypeCache.ContainsKey( typeRef.Pointer ) );
        }

        internal ITypeRef GetTypeFor( LLVMTypeRef valueRef, Func<LLVMTypeRef, ITypeRef> constructor )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( valueRef ) );

            ITypeRef retVal;
            if( TypeCache.TryGetValue( valueRef.Pointer, out retVal ) )
                return retVal;

            retVal = constructor( valueRef );
            TypeCache.Add( valueRef.Pointer, retVal );
            return retVal;
        }
        #endregion

        #region IDisposable Support
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing" )]
        void Dispose( bool disposing )
        {
            if( ContextHandle.Pointer != IntPtr.Zero )
            {
                // allow creating another context after this one is disposed
                if( CurrentThreadContext == this )
                    CurrentThreadContext = null;

                lock ( ContextCache )
                {
                    ContextCache.Remove( ContextHandle );
                }
                NativeMethods.ContextDispose( ContextHandle );
                ContextHandle = default( LLVMContextRef );
            }
        }

        ~Context( )
        {
            Dispose( false );
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose( )
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
        #endregion

        private Context( LLVMContextRef contextRef )
        {
            if( CurrentThreadContext != null )
                throw new InvalidOperationException( "Context already exists for this thread" );

            ContextHandle = contextRef;
            lock ( ContextCache )
            {
                ContextCache.Add( contextRef, this );
            }
            NativeMethods.ContextSetDiagnosticHandler( ContextHandle, DiagnosticHandler, IntPtr.Zero );
            CurrentThreadContext = this;
        }

        [ThreadStatic]
        private static Context CurrentThreadContext;

        private void DiagnosticHandler( out LLVMOpaqueDiagnosticInfo param0, IntPtr param1 )
        {
            Debug.Assert( false );
        }

        private readonly Dictionary< IntPtr, Value > ValueCache = new Dictionary< IntPtr, Value >( );
        private readonly Dictionary< IntPtr, ITypeRef > TypeCache = new Dictionary< IntPtr, ITypeRef >( );
        private readonly Dictionary< IntPtr, NativeModule > ModuleCache = new Dictionary< IntPtr, NativeModule >( );
        private readonly Dictionary< LLVMMetadataRef, LlvmMetadata > MetadataCache = new Dictionary< LLVMMetadataRef, LlvmMetadata >( );
        private readonly Dictionary< LLVMMDOperandRef, MDOperand > MDOperandCache = new Dictionary< LLVMMDOperandRef, MDOperand >( );

        private static Dictionary<LLVMContextRef, Context> ContextCache = new Dictionary<LLVMContextRef, Context>( );

        // TODO: move fatal error handling to a static method of NativeMethods as it is not really tied to the context
        static void FatalErrorHandler( string Reason )
        {
            Trace.TraceError( Reason );
            throw new InternalCodeGeneratorException( Reason );
        }

        // lazy initialized singleton unmanaged delegate so it is never collected
        //private static Lazy<LLVMFatalErrorHandler> FatalErrorHandlerDelegate 
        //    = new Lazy<LLVMFatalErrorHandler>( ( ) => FatalErrorHandler, LazyThreadSafetyMode.PublicationOnly );
    }
}
