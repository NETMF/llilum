using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET.Values;
using Llvm.NET;
using Llvm.NET.Types;
using Llvm.NET.DebugInfo;
using Microsoft.Zelig.Runtime.TypeSystem;

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
            SetCurrentDIFile( "out.bc" );
        }

        public void SetCurrentDIFile( string fn )
        {
            DIFile DIFile;
            if( !DiFiles.TryGetValue( fn, out DIFile ) )
            {
                DIFile = DIBuilder.CreateFile( fn );
                DiFiles.Add( fn, DIFile );
            }

            CurDiFile = DIFile;
        }

        public bool CheckTypeExistenceByName( string name ) => LlvmModule.GetTypeByName( name ) != null;

        public _Type GetOrInsertType( string name, int sizeInBits ) => _Type.GetOrInsertTypeImpl( this, name, sizeInBits );

        public _Type GetOrInsertFunctionType( string name, _Type returnType, List<_Type> argTypes )
        {
            var llvmArgs = argTypes.Select( t => t.GetLLVMObjectForStorage( ) );

            var funcType = LlvmModule.Context.CreateFunctionType( LlvmModule.DIBuilder
                                                                , CurDiFile
                                                                , returnType.GetLLVMObjectForStorage( )
                                                                , llvmArgs
                                                                );
            _Type timpl = _Type.GetOrInsertTypeImpl( this, name, 0, funcType );
            timpl.FunctionArgs.AddRange( argTypes );
            return timpl;
        }

        public _Type GetOrInsertPointerType( string name, _Type underlyingType )
        {
            Debug.Assert( !underlyingType.GetLLVMObjectForStorage( ).IsVoid( ) );

            var ptrType = new DebugPointerType( underlyingType.GetLLVMObjectForStorage( ),  LlvmModule );
            var sizeAndAlign = PointerSize;
            var retVal = _Type.GetOrInsertTypeImpl( this, name, (int)sizeAndAlign, ptrType );
            retVal.IsValueType = true;
            retVal.HasHeader =  false;
            retVal.IsBoxed = false;
            retVal.UnderlyingPointerType = underlyingType;
            return retVal;
        }

        public _Type GetOrInsertPointerType( _Type underlyingType )
        {
            return GetOrInsertPointerType( $"{underlyingType.Name}*", underlyingType );
        }

        public _Type GetOrInsertBoxedType( string name, _Type headerType, _Type underlyingType )
        {
            var structType = new DebugStructType( LlvmModule, name, LlvmModule.DICompileUnit, name );
            var debugFields = new[ ]
                              { new DebugMemberInfo { Type = headerType.DebugType, Index=0, Name="$header", Flags=DebugInfoFlags.Artificial }
                              , new DebugMemberInfo { Type = underlyingType.DebugType, Index=1, Name="m_value", Flags=DebugInfoFlags.Artificial }
                              };

            structType.SetBody( true
                              , LlvmModule
                              , LlvmModule.DICompileUnit
                              , null
                              , 0
                              , DebugInfoFlags.Artificial
                              , debugFields
                              );
            var retVal = _Type.GetOrInsertTypeImpl( this
                                                  , name
                                                  , headerType.SizeInBits + underlyingType.GetSizeInBitsForStorage( )
                                                  , structType
                                                  );
            retVal.IsValueType = false;
            retVal.HasHeader = true;
            retVal.IsBoxed = true;
            retVal.UnderlyingBoxedType = underlyingType;
            return retVal;
        }

        public _Type GetOrInsertZeroSizedArray( _Type type )
        {
            type.GetLLVMObjectForStorage( );
            var arrayType = new DebugArrayType( type.GetLLVMObjectForStorage( ), LlvmModule, 0 );
            var retVal = _Type.GetOrInsertTypeImpl( this, $"MemoryArray of {type.Name}", type.GetSizeInBitsForStorage( ), arrayType );
            retVal.IsValueType = true;
            retVal.HasHeader = false;
            retVal.IsBoxed = false;
            return retVal;
        }

        public _Type GetType( string name ) => _Type.GetTypeImpl( name );

        public _Type GetVoidType( ) => GetType( "System.Void" );

        public _Value GetIntConstant( _Type type, ulong v, bool isSigned )
        {
            Constant val = LlvmModule.Context.CreateConstant( (uint)type.SizeInBits, v, isSigned );
            string name = type.Name;
            if( name == "LLVM.System.IntPtr" || name == "LLVM.System.UIntPtr" )
            {
                val = ConstantExpression.IntToPtrExpression( val, type.DebugType );
            }

            return new _Value( this, type, val, true );
        }

        public _Value GetFloatConstant( float c )
        {
            _Type t = GetType( "LLVM.System.Single" );
            Value val = LlvmModule.Context.CreateConstant( c );
            return new _Value( this, t, val, true );
        }

        public _Value GetDoubleConstant( double c )
        {
            _Type t = GetType( "LLVM.System.Double" );
            Value val = LlvmModule.Context.CreateConstant( c );
            return new _Value( this, t, val, true );
        }

        public _Value GetNullPointer( _Type type )
        {
            var val = type.GetLLVMObjectForStorage( ).GetNullValue( );
            return new _Value( this, type, val, true );
        }

        // REVIEW: Why does code at this low a level or stage need to be concerned with 
        // type hierarchy details? Shouldn't any such issues already be covered in the
        // layers above this?
        public bool CheckIfAExtendsB( _Type a, _Type b )
        {
            ITypeRef tyA = a.DebugType;
            ITypeRef tyB = b.DebugType;

            // REVIEW:
            // While this is from the original C++ it seems a bit odd
            // given the name of this method...
            // A type is generally not considered an extension of itself
            // ( It is possible the original semantic intent of this
            // function is more along the lines of IsAssignmentCompatible(a,b) )
            if( tyA == tyB )
                return true;

            if( tyA.IsStruct() && tyB.IsStruct( ) )
            {
                var stA = ( IStructType )tyA;

                if( stA.IsOpaque )
                    return false;

                ITypeRef tSuper = stA.Members[ 0 ];
                while( tSuper.IsStruct() )
                {
                    if( tSuper == tyB )
                        return true;

                    var stSuper = ( IStructType )tSuper;
                    if( stSuper.IsOpaque )
                        return false;

                    tSuper = stSuper.Members[ 0 ];
                }
            }
            return false;
        }

        public bool CheckIfBothTypesPertainToTheSameHierarchy( _Type a, _Type b )
        {
            return CheckIfAExtendsB( a, b ) || CheckIfAExtendsB( b, a );
        }

        public _Function GetOrInsertFunction( string name, _Type funcType )
        {
            return new _Function( this, name, funcType );
        }

        public Constant GetUCVStruct( _Type structType, List<Constant> structMembers, bool anon )
        {
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
                    else
                    {
                        if( curType.IsPointer( ) && curVal.Type != curType )
                        {
                            curVal = ConstantExpression.BitCast( curVal, curType );
                        }
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
            ITypeRef curType = arrayMemberType.GetLLVMObjectForStorage( );

            if( arrayMembers != null )
            {
                foreach( Constant ucv in arrayMembers )
                {
                    Constant curVal = ucv;

                    if( curType.IsPointer( ) && curVal.Type != curType )
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

        //REVIEW: This smells fishy...
        // Both the NullPointer and ZeroInitialized variants are identical (even in the original C++)
        // This seems problematic as, at least in LLVM, there's a distinction between a NullValue
        // (e.g. all zero value of type T) and a NullPointer ( e.g. a value with Type T* and contents all zero)
        public Constant GetUCVNullPointer( _Type type ) => type.GetLLVMObjectForStorage( ).GetNullValue( );

        public Constant GetUCVZeroInitialized( _Type type ) => type.GetLLVMObjectForStorage( ).GetNullValue( );

        public Constant GetUCVConstantPointerFromValue( _Value val ) => ( Constant )val.LlvmValue;

        public _Value GetGlobalFromUCV( _Type type, Constant ucv, bool isConstant )
        {
            var name = $"{type.Name}_{GetMonotonicUniqueId( )}";
            var gv =LlvmModule.AddGlobal( ucv.Type, name );
            gv.IsConstant = isConstant;
            gv.Linkage = Linkage.Internal;
            gv.Initializer = ucv;
            return new _Value( this, type, gv, !type.IsValueType );
        }

        public _Value GetUninitializedGlobal( _Type type )
        {
            var name = $"{type.Name}_{GetMonotonicUniqueId( )}";
            var gv = LlvmModule.AddGlobal( type.DebugType, name );
            gv.IsConstant = false;
            gv.Linkage = Linkage.Internal;
            return new _Value( this, type, gv, !type.IsValueType );
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

        public bool Compile( )
        {
            DIBuilder.Finish( );
            // performing a verify pass here is just a waste of time...
            // Emiting the bit code file will perform a verify
            //string msg;
            //if( !Module.Verify( out msg ) )
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

        internal uint NativeIntSize => LlvmModule.Layout.IntPtrType( LlvmModule.Context ).IntegerBitWidth;

        internal uint PointerSize => LlvmModule.Layout.IntPtrType( LlvmModule.Context ).IntegerBitWidth;

        internal Context LlvmContext => LlvmModule.Context;

        internal DebugInfoBuilder DIBuilder => LlvmModule.DIBuilder;

        internal DICompileUnit DICompileUnit => LlvmModule.DICompileUnit;

        internal Module LlvmModule { get; }
        
        internal TargetMachine TargetMachine { get; }

        internal DISubProgram GetDISubprogram( string fn )
        {
            DISubProgram retVal;
            if( DiSubPrograms.TryGetValue( fn, out retVal ) )
                return retVal;

            return null;
        }

        internal void SetDISubprogram( string fn, DISubProgram disub )
        {
            if( !DiSubPrograms.ContainsKey( fn ) )
            {
                DiSubPrograms[ fn ] = disub;
            }
        }

        internal DIFile CurDiFile;
        readonly Dictionary<string, DIFile> DiFiles = new Dictionary<string, DIFile>( );
        readonly Dictionary<string, DISubProgram> DiSubPrograms = new Dictionary<string, DISubProgram>( );

        static int GetMonotonicUniqueId( )
        {
            return ( int )System.Threading.Interlocked.Increment( ref globalsCounter );
        }

        static Int64 globalsCounter;
    }
}
