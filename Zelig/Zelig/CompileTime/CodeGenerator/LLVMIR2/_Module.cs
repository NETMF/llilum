using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET.Values;
using Llvm.NET;
using Llvm.NET.Types;
using Llvm.NET.DebugInfo;

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
            var llvmArgs = argTypes.Select( t => t.Impl.GetLLVMObjectForStorage( ) );
            FunctionType funcType = Impl.LlvmContext.GetFunctionType( returnType.Impl.GetLLVMObjectForStorage( ), llvmArgs, false );
            TypeImpl timpl = TypeImpl.GetOrInsertTypeImpl( Impl, name, 0, funcType );

            // Create Debug info version of the signature
            var diArgTypes = argTypes.Select( t => t.Impl.DIType );
            timpl.FunctionArgs.AddRange( argTypes.Select( t => t.Impl ) );
            timpl.DIType = DiBuilder.CreateSubroutineType( Impl.CurDiFile, 0, returnType.Impl.DIType, diArgTypes );
            return GetOrInsertType( timpl );
        }

        public _Type GetOrInsertPointerType( string name, _Type underlyingType )
        {
            Debug.Assert( !underlyingType.Impl.GetLLVMObjectForStorage( ).IsVoid( ) );

            PointerType ptrType = underlyingType.Impl.GetLLVMObjectForStorage( ).CreatePointerType( );
            var sizeAndAlign = Impl.GetPointerSize( );
            var typeImpl = TypeImpl.GetOrInsertTypeImpl( Impl, name, sizeAndAlign, ptrType );
            typeImpl.DIType = DiBuilder.CreatePointerType( underlyingType.Impl.DIType, name, sizeAndAlign, sizeAndAlign );
            _Type retVal = GetOrInsertType( typeImpl );
            retVal.Impl.SetValueTypeFlag( true );
            retVal.Impl.SetHasHeaderFlag( false );
            retVal.Impl.SetBoxedFlag( false );
            retVal.Impl.UnderlyingPointerType = underlyingType.Impl;
            return retVal;
        }

        public _Type GetOrInsertPointerType( _Type underlyingType )
        {
            return GetOrInsertPointerType( $"{underlyingType.Impl.GetName()}*", underlyingType );
        }

        public _Type GetOrInsertBoxedType( string name, _Type headerType, _Type underlyingType )
        {
            StructType structType = Impl.LlvmContext.CreateStructType( name
                                                                     , true
                                                                     , headerType.Impl.GetLLVMObject( )
                                                                     , underlyingType.Impl.GetLLVMObjectForStorage( )
                                                                     );
            var impl = TypeImpl.GetOrInsertTypeImpl( Impl
                                                   , name
                                                   , ( uint )( headerType.GetSizeInBits( ) + underlyingType.GetSizeInBitsForStorage( ) )
                                                   , structType
                                                   );
            impl.DIType = DiBuilder.CreateReplaceableCompositeType( Tag.StructureType, name, DiGlobalScope, null, 0 );
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
            impl.DIType = DiBuilder.CreateArrayType( 0, 0, type.Impl.DIType, DiBuilder.CreateSubrange( 0, 0 ) );
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
            Constant val = ConstantInt.From( Impl.LlvmContext, type.Impl.GetSizeInBits(), v, isSigned );
            string name = type.Impl.GetName( );
            if( name == "LLVM.System.IntPtr" || name == "LLVM.System.UIntPtr" )
            {
                val = ConstantExpression.IntToPtrExpression( val, type.Impl.GetLLVMObject( ) );
            }

            return new _Value( this, new ValueImpl( type.Impl, val, true ) );
        }

        public _Value GetFloatConstant( float c )
        {
            _Type t = GetType( "LLVM.System.Single" );
            Value val = ConstantFP.From( Impl.LlvmContext, c );
            return new _Value( this, new ValueImpl( t.Impl, val, true ) );
        }

        public _Value GetDoubleConstant( double c )
        {
            _Type t = GetType( "LLVM.System.Double" );
            Value val = ConstantFP.From( Impl.LlvmContext, c );
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
            TypeRef tyA = a.Impl.GetLLVMObject();
            TypeRef tyB = b.Impl.GetLLVMObject();

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
                var stA = ( StructType )tyA;

                if( stA.IsOpaque )
                    return false;

                TypeRef tSuper = stA.Members[ 0 ];
                while( tSuper.IsStruct() )
                {
                    if( tSuper == tyB )
                        return true;

                    var stSuper = ( StructType )tSuper;
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
            var llvmStructType = ( StructType )( structType.Impl.GetLLVMObject( ) );
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
                retVal = Impl.LlvmContext.CreateConstantStruct( true, fields );
            }
            else
            {
                retVal = Impl.LlvmContext.CreateNamedConstantStruct( llvmStructType, fields );
            }

            return retVal;
        }

        public Constant GetUCVArray( _Type arrayMemberType, List<Constant> arrayMembers )
        {
            var members = new List<Constant>( );
            TypeRef curType = arrayMemberType.Impl.GetLLVMObjectForStorage( );

            if( arrayMembers != null )
            {
                foreach( Constant ucv in arrayMembers )
                {
                    Constant curVal = ucv;

                    if( curType.IsPointer( ) && curVal.Type != curType )
                    {
                        //It seems that llvm is wrongly switching the bitcast to a GEP.
                        //Let's try to force it with a ptrtoint, inttotr
                        //curVal = ConstantExpr::getPtrToInt( curVal, llvm::IntegerType::get( _pimpl->GetLLVMObject( )->getContext( ), 32 ) );
                        //curVal = ConstantExpr::getIntToPtr( curVal, curType );
                        curVal = ConstantExpression.BitCast( curVal, curType );
                    }

                    members.Add( curVal );
                }
            }

            return ConstantArray.From( curType, members );
        }

        public Constant GetUCVInt( _Type type, ulong v, bool isSigned )
        {
            return ConstantInt.From( type.Impl.GetLLVMObject( ), v, isSigned );
        }

        //REVIEW: This smells fishy...
        // Both the NullPointer and ZeroInitialized variants are identical (even in the original C++)
        // This seems problematic as, at least in LLVM, there's a distinction between a NullValue
        // (e.g. all zero value of type T) and a NullPointer ( e.g. a value with Type T* and contents all zero)
        public Constant GetUCVNullPointer( _Type type ) => type.Impl.GetLLVMObjectForStorage( ).GetNullValue( );
        public Constant GetUCVZeroInitialized( _Type type ) => type.Impl.GetLLVMObjectForStorage( ).GetNullValue( );

        public Constant GetUCVConstantPointerFromValue( _Value val ) => ( Constant )val.Impl.GetLLVMObject( );

        public _Value GetGlobalFromUCV( _Type type, Constant ucv, bool isConstant )
        {
            var name = $"G{GetMonotonicUniqueId( )}";
            var gv = Impl.GetLLVMObject().AddGlobal( ucv.Type, name );
            gv.IsConstant = isConstant;
            gv.Linkage = Linkage.Internal;
            gv.Initializer = ucv;
            return new _Value( this, new ValueImpl( type.Impl, gv, !type.IsValueType( ) ) );
        }

        public _Value GetUninitializedGlobal( _Type type )
        {
            var name = $"G{GetMonotonicUniqueId( )}";
            var gv = Impl.GetLLVMObject().AddGlobal( type.Impl.GetLLVMObject( ), name );
            gv.IsConstant = false;
            gv.Linkage = Linkage.Internal;
            return new _Value( this, new ValueImpl( type.Impl, gv, !type.IsValueType( ) ) );
        }

        public void CreateAlias( _Value val, string name )
        {
            Value llvmVal = val.Impl.GetLLVMObject( );
            var gv = llvmVal as GlobalObject;
            if( gv != null )
            {
                var alias = Impl.GetLLVMObject().AddAlias( gv, name );
                alias.Linkage = Linkage.External;
            }
            else
            {
                Console.Error.WriteLine( "Warning: Ignoring alias \"{0}\" because aliasee is not global.", name );
            }
        }

        public bool Compile( )
        {
            FinalizeDebugInfo( );
            return Impl.Compile( );
        }

        public bool DumpToFile( string fileName, OutputFormat format )
        {
            FinalizeDebugInfo( );
            return Impl.DumpToFile( fileName, format );
        }

        public DebugInfoBuilder DiBuilder => Impl.DiBuilder;

        public DIScope DiGlobalScope => Impl.DICompileUnit;

        public void FinalizeDebugInfo( )
        {
            TypeImpl.FinalizeAllDebugInfo( );
        }

        internal ModuleImpl Impl { get; }

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
            StaticState.RegisterAll( );

            // REVIEW: Need to deal with how LLVM thinks about threads and context
            // and figure out how to map it to how Zelig system uses threads...
            // for now assuming the LLVM code gen is done on a single thread
            // and ignoring the issue of disposing the context.
            LlvmContext = Context.CreateThreadContext( );
            var target = Target.FromTriple( "thumbv7m-none-eabi" );
            TargetMachine = target.CreateTargetMachine( "thumbv7m-none-eabi"
                                                      , string.Empty   // CPU
                                                      , string.Empty   // features
                                                      , CodeGenOpt.None // hard code no optimizations for easier debugging for now...
                                                      , Reloc.Static
                                                      , CodeModel.Default
                                                      );

            Module = LlvmContext.CreateModule( assembly );
            Module.TargetTriple = TargetMachine.Triple;
            Module.DataLayout = TargetMachine.TargetData.ToString( );

            IrBuilder = new InstructionBuilder( );
            NativeIntSize = nativeIntSize;

            Module.AddModuleFlag( ModuleFlagBehavior.Override, Module.DebugVersionValue, Module.DebugMetadataVersion );
            DiBuilder = new DebugInfoBuilder( Module );
            SetCurrentDIFile( "out.bc" );
            DICompileUnit = DiBuilder.CreateCompileUnit( SourceLanguage.CSharp, "out.bc", "", "ZeligIR2LLVMIR", true, "", 0 );
        }

        internal BasicBlock InsertBlock( Function func, string name)
        {
            return func.AppendBasicBlock( name );
        }

        internal bool DumpToFile( string filename, OutputFormat format )
        {
            DiBuilder.Finish( );
            try
            {
                switch( format )
                {
                case OutputFormat.BitCodeBinary:
                    Module.WriteToFile( filename );
                    return true;

                case OutputFormat.BitCodeSource:
                    System.IO.File.WriteAllText( filename, Module.AsString( ) );
                    return true;

                case OutputFormat.TargetObjectFile:
                    TargetMachine.EmitToFile( Module, filename, CodeGenFileType.ObjectFile );
                    return true;

                case OutputFormat.TargetAsmSource:
                    TargetMachine.EmitToFile( Module, filename, CodeGenFileType.AssemblySource );
                    return true;

                default:
                    return false;
                }
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

        internal Module GetLLVMObject( ) => Module;

        internal void SetCurrentDIFile( string fn )
        {
            DIFile DIFile;
            if( !DiFiles.TryGetValue( fn, out DIFile ) )
            {
                DIFile = DiBuilder.CreateFile( fn );
                DiFiles.Add( fn, DIFile );
            }

            CurDiFile = DIFile;
        }

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

        internal Context LlvmContext { get; }
        Module Module;
        InstructionBuilder IrBuilder;
        uint NativeIntSize;

        internal DebugInfoBuilder DiBuilder { get; }

        internal DICompileUnit DICompileUnit { get; }

        internal TargetMachine TargetMachine { get; }

        internal DIFile CurDiFile;
        readonly Dictionary<string, DIFile> DiFiles = new Dictionary<string, DIFile>( );
        readonly Dictionary<string, DISubProgram> DiSubPrograms = new Dictionary<string, DISubProgram>( );
    }
}
