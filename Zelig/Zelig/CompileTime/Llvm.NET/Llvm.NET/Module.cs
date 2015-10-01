using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Llvm.NET.Types;
using Llvm.NET.Values;

namespace Llvm.NET
{
    /// <summary>LLVM Bit code module</summary>
    public class Module 
        : IDisposable
    {
        #region IDisposable Pattern
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        ~Module()
        {
            Dispose( false );
        }

        protected virtual void Dispose( bool disposing )
        {
            // if not already disposed, dispose the module
            // NOTE:
            // The test logic here is backwards from what you might think
            // Ordinarily unmanged resource are always released independent
            // of the value of the disposing param. However, since the Context
            // ultimately owns the module it will destroy it when it is finalized
            // furthermore. Finalization isn't deterministic in that the Context
            // can be finalized BEFORE the module is. But since LLVM will destroy 
            // the underlying module when the context is destroyed that ends up
            // with a dangling pointer/handle in the module. WHen the finalizer 
            // eventually gets to finalize the module the handle is invalid and
            // casues an exception in the native code of LLVM. Thus disposing the
            // module will release the resources for a module early so it doesn't 
            // leak if the same context is used to generate multiple modules. 
            if( disposing )
            {
                LLVMNative.DisposeModule( ModuleHandle );
                ModuleHandle = new LLVMModuleRef( );
            }
        }
        #endregion

        /// <summary><see cref="Context"/> this module belongs to</summary>
        public Context Context { get; }

        /// <summary>Data layout string</summary>
        /// <remarks>
        /// Note the data layout string doesn't do what seems obvious.
        /// That is, it doesn't force the target back-end to generate code
        /// or types with a particular layout. Rather the layout string has
        /// to match the implicit layout of the target. Instead the layout
        /// string provides hints to the optimization passes about the target
        /// at the expense of making the bit code and front-end a bit target
        /// dependent.
        /// </remarks>
        public string DataLayout
        {
            get
            {
                var ptr = LLVMNative.GetDataLayout( ModuleHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set
            {
                LLVMNative.SetDataLayout( ModuleHandle, value );
            }
        }

        /// <summary>Target Triple describing the target, ABI and OS</summary>
        public string TargetTriple
        {
            get
            {
                var ptr = LLVMNative.GetTarget( ModuleHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set 
            {
                LLVMNative.SetTarget( ModuleHandle, value );
            }
        }

        /// <summary>Globals contained by this module</summary>
        public IEnumerable<Value> Globals
        {
            get
            {
                var current = LLVMNative.GetFirstGlobal( ModuleHandle );
                while( current.Pointer != IntPtr.Zero )
                {
                    yield return Value.FromHandle( current );
                    current = LLVMNative.GetNextGlobal( current );
                }
            }
        }

        /// <summary>Enumerable collection of functions contained in this module</summary>
        public IEnumerable<Function> Functions
        {
            get
            {
                var current = LLVMNative.GetFirstFunction( ModuleHandle );
                while( current.Pointer != IntPtr.Zero )
                {
                    yield return (Function)Value.FromHandle( current );
                    current = LLVMNative.GetNextFunction( current );
                }
            }
        }

        public string Name
        {
            get
            {
                var ptr = LLVMNative.GetModuleName( ModuleHandle );
                if( ptr == IntPtr.Zero )
                    return string.Empty;

                return Marshal.PtrToStringAnsi( ptr );
            }
        }

        /// <summary>Link another module into the current module</summary>
        /// <param name="otherModule">Module to merge into this one</param>
        /// <param name="linkMode">Linker mode to use when merging</param>
        public void Link( Module otherModule, LinkerMode linkMode )
        {
            IntPtr errMsgPtr;
            if( 0 != LLVMNative.LinkModules( ModuleHandle, otherModule.ModuleHandle, (LLVMLinkerMode)linkMode, out errMsgPtr ).Value )
            {
                var errMsg = LLVMNative.MarshalMsg( errMsgPtr );
                throw new InternalCodeGeneratorException( errMsg );
            }
        }

        /// <summary>Verifies a bit-code module</summary>
        /// <param name="errmsg">Error messages describing any issues found in the bit-code</param>
        /// <returns>true if the verification succeeded and false if not.</returns>
        public bool Verify( out string errmsg )
        {
            errmsg = null;
            IntPtr msgPtr;
            LLVMBool result = LLVMNative.VerifyModule( ModuleHandle, LLVMVerifierFailureAction.LLVMReturnStatusAction, out msgPtr );
            try
            {
                if( result.Succeeded )
                    return true;

                errmsg = Marshal.PtrToStringAnsi( msgPtr );
                return false;
            }
            finally
            {
                if( msgPtr != IntPtr.Zero )
                    LLVMNative.DisposeMessage( msgPtr );
            }
        }

        /// <summary>Gets a function by name from this module</summary>
        /// <param name="name">Name of the function to get</param>
        /// <returns>The function or null if not found</returns>
        public Function GetFunction( string name )
        {
            var funcRef = LLVMNative.GetNamedFunction( ModuleHandle, name );
            if( funcRef.Pointer == IntPtr.Zero )
                return null;

            return (Function)Value.FromHandle( funcRef );
        }

        /// <summary>Add a function with the specified signature to the module</summary>
        /// <param name="name">Name of the function to add</param>
        /// <param name="signature">Signature of the function</param>
        /// <returns><see cref="Function"/>matching the specified signature and name</returns>
        /// <remarks>
        /// If a matching function already exists it is returned, and therefore the returned
        /// <see cref="Function"/> may have a body and additional attributes. If a function of
        /// the same name exists with a different signature an exception is thrown as LLVM does
        /// not perform any function overloading.
        /// </remarks>
        public Function AddFunction( string name, FunctionType signature )
        {
            return (Function)Value.FromHandle( LLVMNative.GetOrInsertFunction( ModuleHandle, name, signature.TypeHandle ) );
        }

        /// <summary>Writes a bit-code module to a file</summary>
        /// <param name="path">Path to write the bit-code into</param>
        /// <remarks>
        /// This is a blind write. (e.g. no verification is performed)
        /// So if an invalid module is saved it might not work with any
        /// later stage processing tools.
        /// </remarks>
        public void WriteToFile( string path )
        {
            var err = LLVMNative.WriteBitcodeToFile( ModuleHandle, path );
            if( err < 0 )
                throw new System.IO.IOException( );
        }

        /// <summary>Creates a string representation of the module</summary>
        /// <returns>LLVM textual representation of the module</returns>
        /// <remarks>
        /// This is intentionally NOT an override of ToString() as that is
        /// used by debuggers to show the value of a type and this can take
        /// an extremely long time (up to many seconds depending on complexity
        /// of the module) which is bad for the debugger.
        /// </remarks>
        public string AsString( )
        {
            string errMsg;
            if( !Verify( out errMsg ) )
                return $"Invalid Module: {errMsg}";

            return LLVMNative.MarshalMsg( LLVMNative.PrintModuleToString( ModuleHandle ) );
        }

        /// <summary>Add an alias to the module</summary>
        /// <param name="aliasee">Value being aliased</param>
        /// <param name="aliasName">Name of the alias</param>
        /// <returns><see cref="GlobalAlias"/> for the alias</returns>
        public GlobalAlias AddAlias( Value aliasee, string aliasName ) => AddAlias( aliasee.Type, aliasee, aliasName );

        /// <summary>Add an alias to the module</summary>
        /// <param name="typeRef">Type of the alias</param>
        /// <param name="aliasee">Value being aliased</param>
        /// <param name="aliasName">Name of the alias</param>
        /// <returns><see cref="GlobalAlias"/> for the alias</returns>
        /// <openissues>
        /// - What does LLVM do if creating a second alias with the same name (return null, throw, crash??,...)
        /// </openissues>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public GlobalAlias AddAlias( TypeRef typeRef, Value aliasee, string aliasName )
        {
            return (GlobalAlias)Value.FromHandle( LLVMNative.AddAlias( ModuleHandle, typeRef.TypeHandle, aliasee.ValueHandle, aliasName ) );
        }

        /// <summary>Adds a global to this module</summary>
        /// <param name="typeRef">Type of the global's value</param>
        /// <param name="name">Name of the global</param>
        /// <returns>The new <see cref="GlobalVariable"/></returns>
        /// <openissues>
        /// - What does LLVM do if creating a second Global with the same name (return null, throw, crash??,...)
        /// </openissues>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public GlobalVariable AddGlobal( TypeRef typeRef, string name )
        {
            var handle = LLVMNative.AddGlobal( ModuleHandle, typeRef.TypeHandle, name );
            return (GlobalVariable)Value.FromHandle( handle );
        }

        /// <summary>Adds a global to this module</summary>
        /// <param name="typeRef">Type of the global's value</param>
        /// <param name="isConst">Flag to indicate if this global is a constant</param>
        /// <param name="linkage">Linkage type for this global</param>
        /// <param name="constVal">Initial value for the global</param>
        /// <returns>New global variable</returns>
        public GlobalVariable AddGlobal( TypeRef typeRef, bool isConst, Linkage linkage, Constant constVal )
        {
            return AddGlobal( typeRef, isConst, linkage, constVal, string.Empty );
        }

        public GlobalVariable AddGlobal( TypeRef typeRef, bool isConst, Linkage linkage, Constant constVal, string name )
        {
            var retVal = AddGlobal( typeRef, name );
            retVal.IsConstant = isConst;
            retVal.Linkage = linkage;
            retVal.Initializer = constVal;
            return retVal;
        }

        /// <summary>Retrieves a <see cref="TypeRef"/> by name from the module</summary>
        /// <param name="name">Name of the type</param>
        /// <returns>The type or null if no type with the specified name exists in the module</returns>
        public TypeRef GetTypeByName( string name )
        {
            var hType = LLVMNative.GetTypeByName( ModuleHandle, name );
            return hType.Pointer == IntPtr.Zero ? null : TypeRef.FromHandle( hType );
        }

        /// <summary>Retrieves a named global from the module</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GlobalVariable GetNamedGlobal( string name )
        {
            var hGlobal = LLVMNative.GetNamedGlobal( ModuleHandle, name );
            if( hGlobal.Pointer == IntPtr.Zero )
                return null;

            return (GlobalVariable)Value.FromHandle( hGlobal );
        }

        /// <summary>Adds a module flag to the module</summary>
        /// <param name="behavior">Module flag behavior for this flag</param>
        /// <param name="name">Name of the flag</param>
        /// <param name="value">Value of the flag</param>
        public void AddModuleFlag( ModuleFlagBehavior behavior, string name, UInt32 value )
        {
            // AddModuleFlag comes from custom LLVMDebug-C api
            LLVMNative.AddModuleFlag( ModuleHandle, ( LLVMModFlagBehavior )behavior, name, value );
        }

        public void AddNamedMetadataOperand( string name, Metadata value )
        {
            LLVMNative.AddNamedMetadataOperand2( ModuleHandle, name, value.MetadataHandle );
        }

        public void AddVersionIdentMetadata( string version )
        {
            var elements = new LLVMMetadataRef[ ] { LLVMNative.MDString2( Context.ContextHandle, version, (uint)version.Length ) };
            var hNode = LLVMNative.MDNode2( Context.ContextHandle, out elements[ 0 ], 1 );
            LLVMNative.AddNamedMetadataOperand2( ModuleHandle, "llvm.ident", hNode );
        }

        /// <summary>Name of the Debug Version information module flag</summary>
        public const string DebugVersionValue = "Debug Info Version";
        public const string DwarfVersionValue = "Dwarf Version";

        /// <summary>Version of the Debug information Metadata</summary>
        public const UInt32 DebugMetadataVersion = 3; /* DEBUG_METADATA_VERSION (for LLVM v3.7.0) */

        /// <summary>Load a bit-code module from a given file</summary>
        /// <param name="path">path of the file to load</param>
        /// <returns>Loaded <see cref="Module"/></returns>
        public static Module LoadFrom( string path )
        {
            if( string.IsNullOrWhiteSpace( path ) )
                throw new ArgumentException( "path cannot be null or an empty string", nameof( path ) );

            if( !File.Exists( path ) )
                throw new FileNotFoundException( "Specified bit-code file does not exist", path );

            var ctx = Context.CurrentContext;
            using( var buffer = new MemoryBuffer( path ) )
            {
                LLVMModuleRef modRef;
                IntPtr errMsgPtr;
                if( LLVMNative.ParseBitcodeInContext( ctx.ContextHandle, buffer.BufferHandle, out modRef, out errMsgPtr ).Failed )
                {
                    var errMsg = Marshal.PtrToStringAnsi( errMsgPtr );
                    LLVMNative.DisposeMessage( errMsgPtr );
                    throw new InternalCodeGeneratorException( errMsg );
                }
                return ctx.GetModuleFor( modRef );
            }
        }

        internal Module( LLVMModuleRef moduleRef )
        {
            if( moduleRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( moduleRef ) );

            ModuleHandle = moduleRef;
            var hContext = LLVMNative.GetModuleContext( ModuleHandle );
            Context = Llvm.NET.Context.GetContextFor( hContext );
        }

        internal LLVMModuleRef ModuleHandle { get; private set; }
    }
}
