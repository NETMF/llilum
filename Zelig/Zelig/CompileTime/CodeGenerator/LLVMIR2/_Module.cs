using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Zelig.LLVM
{
    public class _Module
    {
        public _Module( string assemblyName, uint nativeIntSize )
        {
            Impl = new ModuleImpl( assemblyName, nativeIntSize );
        }

        public void SetCurrentDIFile( string fn ) => Impl.SetCurrentDIFile( fn );

        public bool CheckTypeExistenceByName( string name ) => Impl.GetLLVMObject( ).GetTypeByName( name ) != null;

        internal _Type GetOrInsertType( TypeImpl timpl )
        {
            return new _Type( this, timpl );
        }

        public _Type GetOrInsertType( string name, int sizeInBits )
        {
            return GetOrInsertType( TypeImpl.GetOrInsertTypeImpl( Impl, name, ( uint )sizeInBits ) );
        }

        public _Type GetOrInsertFunctionType( string name, _Type returnType, List<_Type> argTypes )
        {
            var llvmArgs = from argType in argTypes
                           select argType.Impl.GetLLVMObjectForStorage( );

            Llvm.NET.FunctionType funcType = Impl.LlvmContext.GetFunctionType( returnType.Impl.GetLLVMObjectForStorage( ), llvmArgs, false );
            TypeImpl timpl = TypeImpl.GetOrInsertTypeImpl( Impl, name, 0, funcType );

            timpl.FunctionArgs.AddRange( argTypes.Select( t => t.Impl ) );
            return GetOrInsertType( timpl );
        }

        public _Type GetOrInsertPointerType( string name, _Type underlyingType )
        {
            Debug.Assert( !underlyingType.Impl.GetLLVMObjectForStorage( ).IsVoid( ) );

            Llvm.NET.PointerType ptrType = underlyingType.Impl.GetLLVMObjectForStorage( ).CreatePointerType( );
            _Type retVal = GetOrInsertType( TypeImpl.GetOrInsertTypeImpl( Impl, name, Impl.GetPointerSize( ), ptrType ) );
            retVal.Impl.SetValueTypeFlag( true );
            retVal.Impl.SetHasHeaderFlag( false );
            retVal.Impl.SetBoxedFlag( false );
            retVal.Impl.UnderlyingPointerType = underlyingType.Impl;
            return retVal;
        }

        public _Type GetOrInsertPointerType( _Type underlyingType )
        {
            return GetOrInsertPointerType( $"{underlyingType.Impl.GetName()} *", underlyingType );
        }

        public _Type GetOrInsertBoxedType( string name, _Type headerType, _Type underlyingType )
        {
            Llvm.NET.StructType structType = Impl.LlvmContext.CreateStructType( name
                                                                              , true
                                                                              , headerType.Impl.GetLLVMObject( )
                                                                              , underlyingType.Impl.GetLLVMObjectForStorage( )
                                                                              );
            var impl = TypeImpl.GetOrInsertTypeImpl( Impl
                                                   , name
                                                   , ( uint )( headerType.GetSizeInBits( ) + underlyingType.GetSizeInBitsForStorage( ) )
                                                   , structType
                                                   );
            _Type retVal = GetOrInsertType( impl );
            retVal.Impl.SetValueTypeFlag( false );
            retVal.Impl.SetHasHeaderFlag( true );
            retVal.Impl.SetBoxedFlag( true );
            retVal.Impl.UnderlyingBoxedType = underlyingType.Impl;
            return retVal;
        }

        public _Type GetOrInsertZeroSizedArray( _Type type )
        {
            var arrayType = type.Impl.GetLLVMObjectForStorage( ).CreateArrayType( 0 );
            var impl = TypeImpl.GetOrInsertTypeImpl( Impl, $"MemoryArray of {type.Impl.GetName()}", ( uint )type.GetSizeInBitsForStorage( ), arrayType );
            _Type retVal = GetOrInsertType( impl );
            retVal.Impl.SetValueTypeFlag( true );
            retVal.Impl.SetHasHeaderFlag( false );
            retVal.Impl.SetBoxedFlag( false );
            return retVal;
        }

        public _Type GetType( string name )
        {
            var impl = TypeImpl.GetTypeImpl( name );
            if( impl == null )
                return null;

            return GetOrInsertType( impl );
        }

        public _Type GetVoidType( )
        {
            return GetType( "System.Void" );
        }

        public _Value GetIntConstant( _Type type, ulong v, bool isSigned )
        {
            Llvm.NET.Constant val = Llvm.NET.ConstantInt.From( Impl.LlvmContext, type.Impl.GetSizeInBits(), v, isSigned );
            string name = type.Impl.GetName( );
            if( name == "LLVM.System.IntPtr" || name == "LLVM.System.UIntPtr" )
            {
                val = Llvm.NET.ConstantExpression.IntToPtrExpression( val, type.Impl.GetLLVMObject( ) );
            }

            return new _Value( this, new ValueImpl( type.Impl, val, true ) );
        }

        public _Value GetFloatConstant( float c )
        {
            _Type t = GetType( "LLVM.System.Single" );
            Llvm.NET.Value val = Llvm.NET.ConstantFP.From( Impl.LlvmContext, c );
            return new _Value( this, new ValueImpl( t.Impl, val, true ) );
        }

        public _Value GetDoubleConstant( double c )
        {
            _Type t = GetType( "LLVM.System.Double" );
            Llvm.NET.Value val = Llvm.NET.ConstantFP.From( Impl.LlvmContext, c );
            return new _Value( this, new ValueImpl( t.Impl, val, true ) );
        }

        public _Value GetNullPointer( _Type type )
        {
            var val = type.Impl.GetLLVMObjectForStorage( ).GetNullValue( );
            return new _Value( this, new ValueImpl( type.Impl, val, true ) );
        }

        // REVIEW: Why does code at this low a level or stage need to be concerned with 
        // type hierarchy details? Shouldn't any such issues already be covered in the
        // layers above this?
        public bool CheckIfAExtendsB( _Type a, _Type b )
        {
            Llvm.NET.TypeRef tyA = a.Impl.GetLLVMObject();
            Llvm.NET.TypeRef tyB = b.Impl.GetLLVMObject();

            // REVIEW:
            // While this is from the original C++ it seems a bit odd
            // given the name of this method...
            // A type is generally not considered an extension of itself
            // ( It is possible the orignal semantic intent of this
            // function is more along the lines of IsAssignmentCompatible(a,b) )
            if( tyA == tyB )
                return true;

            if( tyA.IsStruct() && tyB.IsStruct( ) )
            {
                var stA = ( Llvm.NET.StructType )tyA;

                if( stA.IsOpaque )
                    return false;

                Llvm.NET.TypeRef tSuper = stA.Members[ 0 ];
                while( tSuper.IsStruct() )
                {
                    if( tSuper == tyB )
                        return true;

                    var stSuper = ( Llvm.NET.StructType )tSuper;
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

        public Llvm.NET.Constant GetUCVStruct( _Type structType, List<Llvm.NET.Constant> structMembers, bool anon )
        {
            List<Llvm.NET.Constant> fields = new List<Llvm.NET.Constant>( );
            var llvmStructType = ( Llvm.NET.StructType )( structType.Impl.GetLLVMObject( ) );
            if( structMembers != null )
            {
                foreach( Llvm.NET.Constant ucv in structMembers )
                {
                    var curVal = ucv;
                    var curType = llvmStructType.Members[ fields.Count ];

                    //Zero initializer coersion shortcut:

                    if( curVal.IsZeroValue )
                    {
                        curVal = curType.GetNullValue( );
                    }
                    else
                    {
                        if( curType.IsPointer( ) && curVal.Type != curType )
                        {
                            curVal = Llvm.NET.ConstantExpression.BitCast( curVal, curType );
                        }
                    }

                    fields.Add( curVal );
                }
            }

            Llvm.NET.Constant retVal;
            if( anon )
            {
                retVal = Impl.LlvmContext.CreateConstantStruct( fields, true );
            }
            else
            {
                retVal = Impl.LlvmContext.CreateNamedConstantStruct( llvmStructType, fields );
            }

            return retVal;
        }

        public Llvm.NET.Constant GetUCVArray( _Type arrayMemberType, List<Llvm.NET.Constant> arrayMembers )
        {
            var members = new List<Llvm.NET.Constant>( );
            Llvm.NET.TypeRef curType = arrayMemberType.Impl.GetLLVMObjectForStorage( );

            if( arrayMembers != null )
            {
                foreach( Llvm.NET.Constant ucv in arrayMembers )
                {
                    Llvm.NET.Constant curVal = ucv;

                    if( curType.IsPointer( ) && curVal.Type != curType )
                    {
                        //It seems that llvm is wrongly switching the bitcast to a GEP.
                        //Let's try to force it with a ptrtoint, inttotr
                        //curVal = ConstantExpr::getPtrToInt( curVal, llvm::IntegerType::get( _pimpl->GetLLVMObject( )->getContext( ), 32 ) );
                        //curVal = ConstantExpr::getIntToPtr( curVal, curType );
                        curVal = Llvm.NET.ConstantExpression.BitCast( curVal, curType );
                    }

                    members.Add( curVal );
                }
            }

            return Llvm.NET.ConstantArray.From( curType, members );
        }

        public Llvm.NET.Constant GetUCVInt( _Type type, ulong v, bool isSigned )
        {
            return Llvm.NET.ConstantInt.From( type.Impl.GetLLVMObject( ), v, isSigned );
        }

        //REVIEW: This smells fishy...
        // Both the NullPointer and ZeroInitialized variants are identical (even in the original C++)
        // This seems problematic as, at least in LLVM, there's a distinction between a NullValue
        // (e.g. all zero value of type T) and a NullPointer ( e.g. a value with Type T* and contents all zero)
        public Llvm.NET.Constant GetUCVNullPointer( _Type type ) => type.Impl.GetLLVMObjectForStorage( ).GetNullValue( );
        public Llvm.NET.Constant GetUCVZeroInitialized( _Type type ) => type.Impl.GetLLVMObjectForStorage( ).GetNullValue( );

        public Llvm.NET.Constant GetUCVConstantPointerFromValue( _Value val ) => ( Llvm.NET.Constant )val.Impl.GetLLVMObject( );

        public _Value GetGlobalFromUCV( _Type type, Llvm.NET.Constant ucv )
        {
            var name = $"G{GetMonotonicUniqueId( )}";
            var gv = Impl.GetLLVMObject().AddGlobal( ucv.Type, name );
            gv.IsConstant = false;
            gv.Linkage = Llvm.NET.Linkage.Internal;
            gv.Initializer = ucv;
            return new _Value( this, new ValueImpl( type.Impl, gv, !type.IsValueType( ) ) );
        }

        public _Value GetUninitializedGlobal( _Type type )
        {
            var name = $"G{GetMonotonicUniqueId( )}";
            var gv = Impl.GetLLVMObject().AddGlobal( type.Impl.GetLLVMObject( ), name );
            gv.IsConstant = false;
            gv.Linkage = Llvm.NET.Linkage.Internal;
            return new _Value( this, new ValueImpl( type.Impl, gv, !type.IsValueType( ) ) );
        }

        public void CreateAlias( _Value val, string name )
        {
            Llvm.NET.Value llvmVal = val.Impl.GetLLVMObject( );
            var gv = llvmVal as Llvm.NET.GlobalObject;
            if( gv != null )
            {
                var alias = Impl.GetLLVMObject().AddAlias( gv, name );
                alias.Linkage = Llvm.NET.Linkage.External;
            }
            else
            {
                Console.Error.WriteLine( "Warning: Ignoring alias \"{0}\" because aliasee is not global.", name );
            }
        }

        public bool Compile( ) => Impl.Compile( );

        public bool DumpToFile( string fileName, bool text ) => Impl.DumpToFile( fileName, text );

        internal ModuleImpl Impl { get; }
        readonly Dictionary<string, _Type> TypeNameMap = new Dictionary<string, _Type>( );

        static int GetMonotonicUniqueId( )
        {
            return ( int )System.Threading.Interlocked.Increment( ref globalsCounter );
        }

        static Int64 globalsCounter;
    }

    internal class ModuleImpl
    {
        internal ModuleImpl( string assembly, uint nativeIntSize )
        {
            // REVIEW: This is suspect...
            // It initializes the LLVM system for the native
            // target, which is the target of the host system
            // this application is running on. For Zelig that
            // is pretty much never what we want.... (This is
            // what the original C++ code did so currently
            // following the pattern during conversion)
            Llvm.NET.StaticState.RegisterNative( );

            // REVIEW: Need to deal with how LLVM thinks about threads and context
            // and figure out how to map it to how Zelig system uses threads...
            // for now assuming the LLVM code gen is done on a single thread
            // and ignoring the issue of disposing the context.
            LlvmContext = Llvm.NET.Context.CreateThreadContext( );
            Module = LlvmContext.CreateModule( assembly );
            IrBuilder = new Llvm.NET.InstructionBuilder( );
            NativeIntSize = nativeIntSize;

            Module.AddModuleFlag( Llvm.NET.ModuleFlagBehavior.Override, Llvm.NET.Module.DebugVersionValue, Llvm.NET.Module.DebugMetadataVersion );
            DiBuilder = new Llvm.NET.DebugInfo.DebugInfoBuilder( Module );
            SetCurrentDIFile( "out.bc" );
            DiCompileUnit = DiBuilder.CreateCompileUnit( Llvm.NET.Dwarf.SourceLanguage.UserMin + 0xabc, "out.bc", "", "ZeligIR2LLVMIR", true, "", 0 );
        }

        internal Llvm.NET.BasicBlock InsertBlock( Llvm.NET.Function func, string name)
        {
            return func.AppendBasicBlock( name );
        }

        internal bool DumpToFile( string filename, bool text )
        {
            DiBuilder.Finish( );
            try
            {
                if( text )
                {
                    System.IO.File.WriteAllText( filename, Module.AsString( ) );
                }
                else
                {
                    Module.WriteToFile( filename );
                }
                return true;
            }
            catch( System.IO.IOException ex)
            {
                Console.Error.WriteLine( ex.ToString( ) );
                return false;
            }
        }

        internal bool Compile()
        {
            string msg;
            DiBuilder.Finish( );
            if( !Module.Verify( out msg ) )
            {
                // TODO: we need a better error handling strategy than this!
                Console.Error.WriteLine( msg );
                throw new ApplicationException( $"LLVM module verification failed:\n{msg}" );
            }

            // TODO: Generate object file here...
            // REVIEW: What file name should it use for the output?
            return true;
        }

        internal uint GetNativeIntSize()
        {
            return NativeIntSize;
        }

        internal uint GetPointerSize()
        {
            return GetNativeIntSize( );
        }

        internal Llvm.NET.Module GetLLVMObject( ) => Module;

        internal void SetCurrentDIFile( string fn )
        {
            Llvm.NET.DebugInfo.File diFile;
            if( !DiFiles.TryGetValue( fn, out diFile ) )
            {
                diFile = DiBuilder.CreateFile( fn );
                DiFiles.Add( fn, diFile );
            }

            CurDiFile = diFile;
        }

        internal Llvm.NET.DebugInfo.SubProgram GetDISubprogram( string fn )
        {
            Llvm.NET.DebugInfo.SubProgram retVal;
            if( DiSubPrograms.TryGetValue( fn, out retVal ) )
                return retVal;

            return null;
        }

        internal void SetDISubprogram( string fn, Llvm.NET.DebugInfo.SubProgram disub )
        {
            if( !DiSubPrograms.ContainsKey( fn ) )
            {
                DiSubPrograms[ fn ] = disub;
            }
        }

        internal Llvm.NET.Context LlvmContext { get; }
        Llvm.NET.Module Module;
        Llvm.NET.InstructionBuilder IrBuilder;
        uint NativeIntSize;

        internal Llvm.NET.DebugInfo.DebugInfoBuilder DiBuilder { get; }

        Llvm.NET.DebugInfo.CompileUnit DiCompileUnit;
        internal Llvm.NET.DebugInfo.File CurDiFile;
        Dictionary<string, Llvm.NET.DebugInfo.File> DiFiles = new Dictionary<string, Llvm.NET.DebugInfo.File>( );
        Dictionary<string, Llvm.NET.DebugInfo.SubProgram> DiSubPrograms = new Dictionary<string, Llvm.NET.DebugInfo.SubProgram>( );
    }
}
