using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Llvm.NET
{
    /// <summary>Encapsulates an LLVM context</summary>
    /// <remarks>
    /// <para>A context in LLVM is a container for uniqueing (e.g. interning)
    /// various types and values in the system. There are two kinds of
    /// contexts, the global context which is used as a default for single
    /// threaded applications. The second form is the per thread context,
    /// which provides uniqueness of items on a per thread basis. This allows
    /// running multiple LLVM tool transforms etc.. on different threads
    /// without causing them to colide namespaces and types even if 
    /// they use the same name (e.g. module one may have a type Foo, and
    /// so does module two but they are completely distinct from each other)
    /// The global context allows for sharing uniqued items that are common
    /// across multiple parallel processing of LLVM modules so they aren't
    /// re-created for each one. This is most often used for standard
    /// language and run-time support functions etc...</para>
    /// <para>Since a Context essentially owns the lifetime of various
    /// LLVM internal objects it must be disposed of to properly clean up
    /// and shutdown LLVM. While the Context class has a <see cref="CurrentContext"/>
    /// property to prevent needing to pass the context around to everything
    /// it is not automaticallly created on first use. To ensure that the 
    /// context is disposed in a controlled fashion the <see cref="CreateThreadContext"/>
    /// method is typically called in a using statement to automatically 
    /// dispose the context when work with LLVM is completed.
    /// </para>
    /// <para>
    /// It is important to note that a Context is not thread safe. The context 
    /// itself and the object instances it owns are intended for use by a single
    /// thread only. Accessing and manipulating LLVM objects from multiple threads
    /// may lead to race conditions corrupted state and any number of other issues.
    /// </para>
    /// </remarks>
    public class Context 
        : IDisposable
    {
        #region IDisposable Pattern
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( ContextHandle );
        }

        ~Context()
        {
            Dispose( false );
        }

        protected virtual void Dispose( bool disposing )
        {
            if( ContextHandle.Pointer != IntPtr.Zero )
            {
                if( disposing )
                {
                    lock(ContextCache)
                    {
                        ContextCache.Remove( ContextHandle );
                    }
                    foreach( var kvp in ModuleCache )
                        kvp.Value.Dispose( );

                    ModuleCache.Clear( );
                }

                LLVMNative.ContextDispose( ContextHandle );
                ContextHandle = new LLVMContextRef( );
                if( CurrentContext_ == this )
                    CurrentContext_ = null;
            }
        }
        #endregion

        /// <summary>Get's the LLVM void type for this context</summary>
        public TypeRef VoidType => TypeRef.FromHandle( LLVMNative.VoidTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM boolean type for this context</summary>
        public TypeRef BoolType => TypeRef.FromHandle( LLVMNative.Int1TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 8 bit integer type for this context</summary>
        public TypeRef Int8Type => TypeRef.FromHandle( LLVMNative.Int8TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 16 bit integer type for this context</summary>
        public TypeRef Int16Type => TypeRef.FromHandle( LLVMNative.Int16TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 32 bit integer type for this context</summary>
        public TypeRef Int32Type => TypeRef.FromHandle( LLVMNative.Int32TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM 64 bit integer type for this context</summary>
        public TypeRef Int64Type => TypeRef.FromHandle( LLVMNative.Int64TypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM half precision floating point type for this context</summary>
        public TypeRef HalfFloatType => TypeRef.FromHandle( LLVMNative.HalfTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM single precision floating point type for this context</summary>
        public TypeRef FloatType => TypeRef.FromHandle( LLVMNative.FloatTypeInContext( ContextHandle ) );

        /// <summary>Get's the LLVM double precision floating point type for this context</summary>
        public TypeRef DoubleType => TypeRef.FromHandle( LLVMNative.DoubleTypeInContext( ContextHandle ) );

        /// <summary>Get a type that is a pointer to a value of a given type</summary>
        /// <param name="elementType">Type of value the pointer points to</param>
        /// <returns><see cref="PointerType"/> for a pointer that references a value of type <paramref name="elementType"/></returns>
        public PointerType GetPointerTypeFor( TypeRef elementType ) => new PointerType( LLVMNative.PointerType( elementType.TypeHandle, 0 ) );

        /// <summary>Get's an LLVM integer type of arbitrary bit width</summary>
        /// <remarks>
        /// For standard integer bit widths (e.g. 1,8,16,32,64) this will return
        /// the same type as the corresponding specialized property.
        /// (e.g. GetIntType(1) is the same as <see cref="BoolType"/>,
        ///  GetIntType(16) is the same as <see cref="Int16Type"/>, etc... )
        /// </remarks>
        public TypeRef GetIntType( uint bitWidth )
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
                return TypeRef.FromHandle( LLVMNative.IntType( bitWidth ) );
            }
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Optional set of function argument types</param>
        /// <returns>Signature type for the specified signature</returns>
        public FunctionType GetFunctionType( TypeRef returnType, params TypeRef[ ] args )
        {
            return GetFunctionType( returnType, args, false );
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Potentially empty set of function argument types</param>
        /// <returns>Signature type for the specified signature</returns>
        public FunctionType GetFunctionType( TypeRef returnType, IEnumerable<TypeRef> args )
        {
            return GetFunctionType( returnType, args, false );
        }

        /// <summary>Get an LLVM Function type (e.g. signature)</summary>
        /// <param name="returnType">Return type of the function</param>
        /// <param name="args">Potentially empty set of function argument types</param>
        /// <param name="isVarArgs">Flag to indicate if the method supports C/C++ style VarArgs</param>
        /// <returns>Signature type for the specified signature</returns>
        public FunctionType GetFunctionType( TypeRef returnType, IEnumerable<TypeRef> args, bool isVarArgs )
        {
            if( ContextHandle.Pointer != returnType.Context.ContextHandle.Pointer ) 
                throw new ArgumentException( "Mismatched context", nameof( returnType ) );

            LLVMTypeRef[] llvmArgs = args.Select( a => a.TypeHandle).ToArray();
            var argCount = llvmArgs.Length;
            // have to pass a valid adrresable object to native interop
            // so allocate space for a single value but tell LLVM the length is 0
            if( llvmArgs.Length == 0 )
                llvmArgs = new LLVMTypeRef[ 1 ];

            var signature = LLVMNative.FunctionType( returnType.TypeHandle, out llvmArgs[ 0 ], (uint)argCount, isVarArgs );
            return FunctionType.FromHandle( signature );
        }

        ///<summary>Create a named structure (e.g. forward reference)</summary>
        public StructType CreateStructType( string name )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentNullException( nameof( name ) );

            var hType = LLVMNative.StructCreateNamed( ContextHandle, name );
            return StructType.FromHandle( hType );
        }

        /// <summary>Create a constant struct</summary>
        /// <param name="values">values for the members of the struct</param>
        /// <param name="packed">Flag to indicate if the members are packed (e.g. no automatic padding/alignment should be applied)</param>
        /// <returns>Constant value for the struct</returns>
        /// <remarks>
        /// Constant structs are often used as initializers for global data structures that are compile time constants such as
        /// initialized const data, vtables, etc...
        /// </remarks>
        public Constant CreateConstantStruct( IEnumerable<Constant> values, bool packed )
        {
            var valueHandles = values.Select( v => v.ValueHandle ).ToArray( );
            var handle = LLVMNative.ConstStructInContext( ContextHandle, out valueHandles[ 0 ], (uint)valueHandles.Length, packed );
            return Constant.FromHandle( handle );
        }

        public Constant CreateNamedConstantStruct( StructType type, IEnumerable<Constant> values )
        {
            if( type.Context != this )
                throw new ArgumentException( "Cannot create named constant struct with type from another context", nameof( type ) );

            var valueHandles = values.Select( v => v.ValueHandle ).ToArray( );
            var handle = LLVMNative.ConstNamedStruct(type.TypeHandle, out valueHandles[ 0 ], ( uint )valueHandles.Length );
            return Constant.FromHandle( handle );
        }

        /// <summary>Create an anonymous structure type (e.g. Tuple)</summary>
        /// <param name="packed">Flag to indicate if the structure is "packed"</param>
        /// <param name="element0">Type of the first field of the structure</param>
        /// <param name="elements">Types of any additional fields of the structure</param>
        /// <returns>
        /// <see cref="StructType"/> with the specified body defined.
        /// </returns>
        public StructType CreateStructType( bool packed, TypeRef element0, params TypeRef[] elements )
        {
            LLVMTypeRef[] llvmArgs = new LLVMTypeRef[ elements.Length + 1 ];
            llvmArgs[ 0 ] = element0.TypeHandle;
            for( int i = 1; i < llvmArgs.Length; ++i)
                llvmArgs[ i ] = elements[ i - 1 ].TypeHandle;

            var handle = LLVMNative.StructTypeInContext( ContextHandle, out llvmArgs[ 0 ], ( uint )llvmArgs.Length, packed );
            return StructType.FromHandle( handle );
        }

        /// <summary>Creates a new structure type in this <see cref="Context"/></summary>
        /// <param name="name">Name of the structure</param>
        /// <param name="packed">Flag indicating if the structure is packed</param>
        /// <param name="elements">Types for the structures elements in layout order</param>
        /// <returns>
        /// <see cref="StructType"/> with the specified body defined.
        /// </returns>
        /// <remarks>
        /// If the elements argument list is empty then an opaque type is created (e.g. a forward reference)
        /// The <see cref="StructType.SetBody(bool, TypeRef[])"/> method provides a means to add a body to
        /// an opaque type at a later time if the details of the body are required. (If only a pointers to
        /// to the type are required the body isn't required) 
        /// </remarks>
        public StructType CreateStructType( string name, bool packed, params TypeRef[] elements )
        {
            var retVal = StructType.FromHandle( LLVMNative.StructCreateNamed( ContextHandle, name ) );
            if( elements.Length > 0 )
            {
                retVal.SetBody( packed, elements );
            }
            return retVal;
        }

        /// <summary>Creates an LLVM bit code module for generating bit code into</summary>
        /// <param name="moduleId">Identifier for the module</param>
        /// <returns>LLVM <see cref="Module"/></returns>
        public Module CreateModule( string moduleId )
        {
            var retVal = LLVMNative.ModuleCreateWithNameInContext( moduleId, ContextHandle );
            if( retVal.Pointer == IntPtr.Zero )
                return null;

            return GetModuleFor( retVal );
        }

        /// <summary>Global context</summary>
        public static Context GlobalContext => LazyGlobalContext.Value;

        /// <summary>Current thread context</summary>
        /// <remarks>
        /// <para>Users of this context must never store or retain it outside of the immediate scope where this is accessed.
        /// The context is disposable and there can be only one owner with the resposibility to dispose the instance
        /// for proper clean up of LLVM native resources.</para>
        /// <para>If no owner has created a per thread context then this property will return null. The owner must call
        /// <see cref="CreateThreadContext"/>, typically in a using statement, to force the creatino of the context for
        /// the current thread.</para>
        /// </remarks>
        public static Context CurrentContext => CurrentContext_;

        /// <summary>Creates a per thread context for the current thread</summary>
        /// <returns>new <see cref="Context"/> for the current thread</returns>
        /// <remarks>
        /// The <see cref="CurrentContext"/> property is null until after this method is called. When this method is called
        /// a new context is created and set as the <see cref="CurrentContext"/>. When the returned <see cref="Context"/>
        /// is disposed <see cref="CurrentContext"/> is reset to null. Furthermore, if <see cref="CurrentContext"/> is not
        /// null when this is called an <see cref="InvalidOperationException"/> is thrown. This ensures that there is only
        /// one owner with the responsibility of disposing the context for a thread. 
        /// </remarks>
        public static Context CreateThreadContext()
        {
            if( CurrentContext_ != null )
                throw new InvalidOperationException( "Only one lifetime controlling point allowed for per thread context");

            CurrentContext_ = new Context();
            return CurrentContext_;
        }

        internal Context( )
            : this( LLVMNative.ContextCreate( ) )
        {
        }

        #region Interning Factories
        internal Module GetModuleFor( LLVMModuleRef moduleRef )
        {
            if( moduleRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( moduleRef ) );

            Module retVal;
            if( !ModuleCache.TryGetValue( moduleRef.Pointer, out retVal ) )
            { 
                retVal = new Module( moduleRef );
                ModuleCache.Add( moduleRef.Pointer, retVal );
            }
            return retVal;
        }

        internal static Context GetContextFor( LLVMContextRef contextRef )
        {
            lock( ContextCache )
            {
                Context retVal;
                if( ContextCache.TryGetValue( contextRef, out retVal ) )
                    return retVal;

                return new Context( contextRef );
            }
        }

        internal LLVMContextRef ContextHandle { get; private set; }

        [Conditional("DEBUG")]
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

            retVal = constructor(valueRef);
            ValueCache.Add( valueRef.Pointer, retVal );
            return retVal;
        }

        internal void InternValue( Value value )
        {
            if( ValueCache.ContainsKey( value.ValueHandle.Pointer ) )
                throw new ArgumentException( "Value already interned", nameof( value ) );

            ValueCache.Add( value.ValueHandle.Pointer, value );
        }

        internal TypeRef GetTypeFor( LLVMTypeRef valueRef, Func<LLVMTypeRef, TypeRef> constructor )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( valueRef ) );

            TypeRef retVal;
            if( TypeCache.TryGetValue( valueRef.Pointer, out retVal ) )
                return retVal;

            retVal = constructor( valueRef );
            TypeCache.Add( valueRef.Pointer, retVal );
            return retVal;
        }
        #endregion

        private Context( LLVMContextRef contextRef )
        {
            ContextHandle = contextRef;
            lock ( ContextCache )
            {
                ContextCache.Add( contextRef, this );
            }
            LLVMNative.ContextSetDiagnosticHandler( ContextHandle, DiagnosticHandler, IntPtr.Zero );
        }

        private void DiagnosticHandler(out LLVMOpaqueDiagnosticInfo param0, IntPtr param1)
        {
        }

        private Dictionary<IntPtr, Value> ValueCache = new Dictionary<IntPtr, Value>( );
        private Dictionary<IntPtr, TypeRef> TypeCache = new Dictionary<IntPtr, TypeRef>( );
        private Dictionary<IntPtr, Module> ModuleCache = new Dictionary<IntPtr, Module>( );

        static void FatalErrorHandler(string Reason)
        {
            Trace.TraceError( Reason );
            throw new InternalCodeGeneratorException( Reason );
        }

        private static Dictionary<LLVMContextRef, Context> ContextCache = new Dictionary<LLVMContextRef, Context>( );

        [ThreadStatic]
        private static Context CurrentContext_;
        private static Lazy<Context> LazyGlobalContext = new Lazy<Context>( ( ) => GetContextFor( LLVMNative.GetGlobalContext( ) )
                                                                          , LazyThreadSafetyMode.ExecutionAndPublication
                                                                          );

        // lazy init a singleton unmanaged delegate and hold on to it so it is never collected
        private static Lazy<LLVMFatalErrorHandler> FatalErrorHandlerDelegate 
            = new Lazy<LLVMFatalErrorHandler>( ( ) => FatalErrorHandler, LazyThreadSafetyMode.PublicationOnly );
    }
}
