using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET.Values;
using Llvm.NET;
using Llvm.NET.Types;
using Llvm.NET.DebugInfo;
using Microsoft.Zelig.Runtime.TypeSystem;
using Microsoft.Zelig.Debugging;

namespace Microsoft.Zelig.LLVM
{
    public enum OutputFormat
    {
        BitCodeBinary,
        BitCodeSource,
        TargetObjectFile,
        TargetAsmSource
    }

    public class _Module
    {
        public _Module( string assemblyName )
        {
            StaticState.RegisterAll( );

            // REVIEW: Need to deal with how LLVM thinks about threads and context
            // and figure out how to map it to how Zelig system uses threads...
            // for now assuming the LLVM code gen is done on a single thread
            // and ignoring the issue of disposing the Module.
            // TODO: Get target triple from command line instead of hard coding it here.
            var target = Target.FromTriple( "thumbv7m-none-eabi" );
            TargetMachine = target.CreateTargetMachine( "thumbv7m-none-eabi"
                                                      , string.Empty   // CPU
                                                      , string.Empty   // features
                                                      , CodeGenOpt.None // hard code no optimizations for easier debugging for now...
                                                      , Reloc.Static
                                                      , CodeModel.Default
                                                      );

            // REVIEW: What happens when we start interacting with assemblies written in other languages?
            //         At the moment this is setting the entire module to CSharp.
            LlvmModule = new Module( assemblyName, SourceLanguage.CSharp, "out.bc", "ZeligIR2LLVMIR", false, "", 0 )
                        {
                            TargetTriple = TargetMachine.Triple,
                            Layout = TargetMachine.TargetData
                        };

            LlvmModule.AddModuleFlag( ModuleFlagBehavior.Override, Module.DebugVersionValue, Module.DebugMetadataVersion );
        }

        public _Type GetOrInsertType( string name, int sizeInBits, bool isValueType )
        {
            return _Type.GetOrInsertTypeImpl( this, name, sizeInBits, isValueType );
        }

        public _Type GetOrInsertFunctionType( string name, _Type returnType, List<_Type> argTypes )
        {
            var llvmArgs = argTypes.Select( t => t.DebugType );

            var funcType = LlvmModule.Context.CreateFunctionType( LlvmModule.DIBuilder
                                                                , null
                                                                , returnType.DebugType
                                                                , llvmArgs
                                                                );
            _Type timpl = _Type.GetOrInsertTypeImpl( this, name, 0, true, funcType );
            return timpl;
        }

        public _Type GetOrInsertPointerType( string name, _Type underlyingType )
        {
            Debug.Assert( !underlyingType.DebugType.IsVoid );

            var ptrType = new DebugPointerType( underlyingType.DebugType, LlvmModule );
            var sizeAndAlign = PointerSize;
            var retVal = _Type.GetOrInsertTypeImpl( this, name, ( int )sizeAndAlign, true, ptrType );
            retVal.IsBoxed = false;
            retVal.UnderlyingType = underlyingType;
            return retVal;
        }

        public _Type GetOrInsertPointerType( _Type underlyingType )
        {
            return GetOrInsertPointerType( underlyingType.Name + "*", underlyingType );
        }

        public _Type GetOrInsertBoxedType( string name, _Type headerType, _Type underlyingType )
        {
            int sizeInBits = headerType.SizeInBits + underlyingType.SizeInBits;
            _Type retVal = _Type.GetOrInsertTypeImpl( this, name, sizeInBits, isValueType: false );
            retVal.IsBoxed = true;
            retVal.UnderlyingType = underlyingType;

            retVal.AddField( 0, headerType, "$header" );
            retVal.AddField( 0, underlyingType, "m_value" );
            retVal.SetupFields( );

            return retVal;
        }

        public _Type GetOrInsertZeroSizedArray( _Type type )
        {
            var arrayType = new DebugArrayType( type.DebugType, LlvmModule, 0 );
            var retVal = _Type.GetOrInsertTypeImpl( this, $"MemoryArray of {type.Name}", type.SizeInBits, true, arrayType );
            retVal.IsBoxed = false;
            return retVal;
        }

        public _Type GetType( string name ) => _Type.GetTypeImpl( name );

        public _Type GetVoidType( ) => GetType( "System.Void" );

        public _Value GetIntConstant( _Type type, ulong v, bool isSigned )
        {
            Constant val = LlvmModule.Context.CreateConstant( (uint)type.SizeInBits, v, isSigned );
            string name = type.Name;
            if( name == "System.IntPtr" || name == "System.UIntPtr" )
            {
                val = ConstantExpression.IntToPtrExpression( val, type.DebugType );
            }

            return new _Value( this, type, val );
        }

        public _Value GetFloatConstant( float c )
        {
            _Type t = GetType( "System.Single" );
            Value val = LlvmModule.Context.CreateConstant( c );
            return new _Value( this, t, val );
        }

        public _Value GetDoubleConstant( double c )
        {
            _Type t = GetType( "System.Double" );
            Value val = LlvmModule.Context.CreateConstant( c );
            return new _Value( this, t, val );
        }

        public _Value GetNullPointer( _Type type )
        {
            // In addition to the usual pointer types (Object*, IntPtr, byte*, etc.), the runtime also treats some
            // structs as pointers, such as RuntimeTypeHandle and CodePointer. In those cases, we'll return a struct.
            return new _Value( this, type, type.DebugType.GetNullValue( ) );
        }

        public _Function GetOrInsertFunction( LLVMModuleManager manager, MethodRepresentation method )
        {
            _Function retVal;
            if( m_FunctionMap.TryGetValue( method.m_identity, out retVal ) )
                return retVal;

            retVal = new _Function( this, manager, method );
            m_FunctionMap.Add( method.m_identity, retVal );
            return retVal;
        }

        public Constant GetUCVStruct( _Type structType, List<Constant> structMembers, bool anon )
        {
            // Special case: Primitive types aren't wrapped in structs, so return the wrapped value directly.
            if ( structType.IsPrimitiveType )
            {
                return structMembers[0];
            }

            List<Constant> fields = new List<Constant>( );
            var llvmStructType = ( IStructType )( structType.DebugType );
            if( structMembers != null )
            {
                foreach( Constant ucv in structMembers )
                {
                    var curVal = ucv;
                    var curType = llvmStructType.Members[ fields.Count ];

                    //Zero initializer coercion shortcut:
                    if( curVal.IsZeroValue )
                    {
                        curVal = curType.GetNullValue( );
                    }
                    else if( curType.IsPointer && curVal.Type != curType )
                    {
                        curVal = ConstantExpression.BitCast( curVal, curType );
                    }

                    fields.Add( curVal );
                }
            }

            Constant retVal;
            if( anon )
            {
                retVal = LlvmModule.Context.CreateConstantStruct( true, fields );
            }
            else
            {
                retVal = LlvmModule.Context.CreateNamedConstantStruct( llvmStructType, fields );
            }

            return retVal;
        }

        public Constant GetUCVArray( _Type arrayMemberType, List<Constant> arrayMembers )
        {
            var members = new List<Constant>( );
            ITypeRef curType = arrayMemberType.DebugType;

            if( arrayMembers != null )
            {
                foreach( Constant ucv in arrayMembers )
                {
                    Constant curVal = ucv;

                    if( curType.IsPointer && curVal.Type != curType )
                    {
                        curVal = ConstantExpression.BitCast( curVal, curType );
                    }

                    members.Add( curVal );
                }
            }

            return ConstantArray.From( curType, members );
        }

        public Constant GetUCVInt( _Type type, ulong v, bool isSigned )
        {
            return LlvmModule.Context.CreateConstant( type.DebugType, v, isSigned );
        }

        public Constant GetUCVZeroInitialized( _Type type )
        {
            return type.DebugType.GetNullValue( );
        }

        public Constant GetUCVConstantPointerFromValue( _Value val ) => ( Constant )val.LlvmValue;

        public _Value GetGlobalFromUCV( _Type type, Constant ucv, bool isConstant )
        {
            string name = $"{type.Name}_{GetMonotonicUniqueId( )}";
            var gv = LlvmModule.AddGlobal( ucv.Type, name );
            gv.IsConstant = isConstant;
            gv.Linkage = Linkage.Internal;
            gv.Initializer = ucv;

            // This returns the equivalent of a "this" pointer for the object.
            _Type pointerType = GetOrInsertPointerType( type );
            return new _Value( this, pointerType, gv );
        }

        public _Value GetUninitializedGlobal( _Type type )
        {
            string name = $"{type.Name}_{GetMonotonicUniqueId( )}";
            var gv = LlvmModule.AddGlobal( type.DebugType, name );
            gv.IsConstant = false;
            gv.Linkage = Linkage.Internal;

            // This returns the equivalent of a "this" pointer for the object.
            _Type pointerType = GetOrInsertPointerType( type );
            return new _Value( this, pointerType, gv );
        }

        public void CreateAlias( _Value val, string name )
        {
            Value llvmVal = val.LlvmValue;
            var gv = llvmVal as GlobalObject;
            if( gv != null )
            {
                var alias =LlvmModule.AddAlias( gv, name );
                alias.Linkage = Linkage.External;
            }
            else
            {
                Console.Error.WriteLine( "Warning: Ignoring alias \"{0}\" because aliasee is not global.", name );
            }
        }

        public _Function GetFunctionWithDebugInfoFor( LLVMModuleManager manager, MethodRepresentation method )
        {
            // get the function for this method, if it doesn't have debug info yet - generate it. 
            _Function func = GetOrInsertFunction( manager, method );
            if( func.LlvmFunction.DISubProgram == null )
            {
                var func2 = CreateLLvmFunctionWithDebugInfo( manager, method );
                Debug.Assert( func2 == func.LlvmFunction );
                Debug.Assert( func.LlvmFunction.DISubProgram != null );
            }
            return func;
        }

        public bool Compile( )
        {
            DIBuilder.Finish( );
            // performing a verify pass here is just a waste of time...
            // Emiting the bit code file will perform a verify
            //string msg;
            //if( !LlvmModule.Verify( out msg ) )
            //{
            //    // TODO: we need a better error handling strategy than this!
            //    Console.Error.WriteLine( msg );
            //    throw new ApplicationException( $"LLVM module verification failed:\n{msg}" );
            //}

            // TODO: Generate object file here...
            // REVIEW: What file name should it use for the output?
            return true;
        }

        public bool DumpToFile( string fileName, OutputFormat format )
        {
            DIBuilder.Finish( );
            try
            {
                switch( format )
                {
                case OutputFormat.BitCodeBinary:
                    LlvmModule.WriteToFile( fileName );
                    return true;

                case OutputFormat.BitCodeSource:
                    System.IO.File.WriteAllText( fileName, LlvmModule.AsString( ) );
                    return true;

                case OutputFormat.TargetObjectFile:
                    TargetMachine.EmitToFile( LlvmModule, fileName, CodeGenFileType.ObjectFile );
                    return true;

                case OutputFormat.TargetAsmSource:
                    TargetMachine.EmitToFile( LlvmModule, fileName, CodeGenFileType.AssemblySource );
                    return true;

                default:
                    return false;
                }
            }
            catch( System.IO.IOException ex )
            {
                Console.Error.WriteLine( ex.ToString( ) );
                return false;
            }
        }

        internal uint NativeIntSize => LlvmModule.Layout.IntPtrType( ).IntegerBitWidth;

        internal uint PointerSize => LlvmModule.Layout.IntPtrType( ).IntegerBitWidth;

        internal Context LlvmContext => LlvmModule.Context;

        internal DebugInfoBuilder DIBuilder => LlvmModule.DIBuilder;

        internal DICompileUnit DICompileUnit => LlvmModule.DICompileUnit;

        internal Module LlvmModule { get; }
        
        internal TargetMachine TargetMachine { get; }

        internal DIFile GetOrCreateDIFile( string fn )
        {
            if( string.IsNullOrWhiteSpace( fn ) )
                return null;

            DIFile retVal;
            if( m_DiFiles.TryGetValue( fn, out retVal ) )
                return retVal;

            retVal = DIBuilder.CreateFile( fn );
            m_DiFiles.Add( fn, retVal );

            return retVal;
        }


        private Function CreateLLvmFunctionWithDebugInfo( LLVMModuleManager manager, MethodRepresentation method )
        {
            string mangledName = LLVMModuleManager.GetFullMethodName( method );
            _Type functionType = manager.GetOrInsertType( method );
            DebugInfo loc = method.DebugInfo ?? manager.GetDebugInfoFor( method );
            Debug.Assert( loc != null );

            // Create the DISupprogram info
            var retVal = LlvmModule.CreateFunction( DICompileUnit
                                                  , method.Name
                                                  , mangledName
                                                  , GetOrCreateDIFile( loc.SrcFileName )
                                                  , ( uint )loc.BeginLineNumber
                                                  , ( DebugFunctionType )functionType.DebugType
                                                  , true
                                                  , true
                                                  , ( uint )loc.EndLineNumber
                                                  , DebugInfoFlags.None // TODO: Map Zelig accesibility info etc... to flags
                                                  , false
                                                  );
            return retVal;
        }

        private readonly Dictionary<string, DIFile> m_DiFiles = new Dictionary<string, DIFile>( );
        private readonly Dictionary<int, _Function> m_FunctionMap = new Dictionary<int, _Function>( );

        static int GetMonotonicUniqueId( )
        {
            return ( int )System.Threading.Interlocked.Increment( ref globalsCounter );
        }

        static Int64 globalsCounter;
    }
}
