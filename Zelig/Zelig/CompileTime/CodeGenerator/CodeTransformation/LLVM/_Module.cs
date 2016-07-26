using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Llvm.NET.Values;
using Llvm.NET;
using Llvm.NET.Types;
using Llvm.NET.DebugInfo;
using Microsoft.Zelig.CodeGeneration.IR;
using Microsoft.Zelig.Runtime.TypeSystem;
using Microsoft.Zelig.Debugging;
using ConstantExpression = Llvm.NET.Values.ConstantExpression;

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
        public _Module( string assemblyName, TypeSystemForCodeTransformation typeSystem )
        {
            StaticState.RegisterAll( );

            // REVIEW: Need to deal with how LLVM thinks about threads and context
            // and figure out how to map it to how Zelig system uses threads...
            // for now assuming the LLVM code gen is done on a single thread
            // and ignoring the issue of disposing the Context.
            var context = new Context( );

            // TODO: Get target triple from command line instead of hard coding it here.
            var target = Target.FromTriple( Target.ThumbV7mEabiTriple );
            TargetMachine = target.CreateTargetMachine( context
                                                      , Target.ThumbV7mEabiTriple
                                                      , string.Empty   // CPU
                                                      , string.Empty   // features
                                                      , CodeGenOpt.None // hard code no optimizations for easier debugging for now...
                                                      , Reloc.Static
                                                      , CodeModel.Default
                                                      );

            // REVIEW: What happens when we start interacting with assemblies written in other languages?
            //         At the moment this is setting the entire module to CSharp.
            LlvmModule = new NativeModule( assemblyName, context, SourceLanguage.CSharp, "out.bc", "ZeligIR2LLVMIR", false, "", 0 )
                        {
                            TargetTriple = TargetMachine.Triple,
                            Layout = TargetMachine.TargetData
                        };

            LlvmModule.AddModuleFlag( ModuleFlagBehavior.Override, NativeModule.DebugVersionValue, NativeModule.DebugMetadataVersion );
            TypeSystem = typeSystem;
        }

        internal LLVMModuleManager Manager => TypeSystem.Module;

        internal TypeSystemForCodeTransformation TypeSystem { get; }

        public _Type GetOrInsertType( TypeRepresentation tr )
        {
            return _Type.GetOrInsertTypeImpl( this, tr );
        }

        public _Type GetOrInsertFunctionType( string name, _Type returnType, List<_Type> argTypes )
        {
            var llvmArgs = argTypes.Select( t => t.DebugType );

            var funcType = LlvmModule.Context.CreateFunctionType( LlvmModule.DIBuilder
                                                                , returnType.DebugType
                                                                , llvmArgs
                                                                );
            _Type timpl = _Type.GetOrInsertTypeImpl( this, name, 0, true, funcType );
            return timpl;
        }

        public _Type GetOrInsertPointerType( PointerTypeRepresentation tr, _Type underlyingType )
        {
            Debug.Assert( !underlyingType.DebugType.IsVoid );

            var ptrType = new DebugPointerType( underlyingType.DebugType, LlvmModule );
            var retVal = _Type.GetOrInsertTypeImpl( this, tr, ptrType );
            retVal.UnderlyingType = underlyingType;
            return retVal;
        }

        // REVIEW: Figure out if we can remove this, it's one of the reasons we have to keep using strings as the key for cacheing
        public _Type GetOrInsertPointerType( _Type underlyingType )
        {
            Debug.Assert( !underlyingType.DebugType.IsVoid );

            var ptrType = new DebugPointerType( underlyingType.DebugType, LlvmModule );
            var sizeAndAlign = PointerSize;
            var retVal = _Type.GetOrInsertTypeImpl( this, underlyingType.Name + "*", ( int )sizeAndAlign, true, ptrType );
            retVal.UnderlyingType = underlyingType;
            return retVal;
        }

        public _Type GetOrInsertZeroSizedArray( _Type type )
        {
            var arrayType = new DebugArrayType( type.DebugType, LlvmModule, 0 );
            var retVal = _Type.GetOrInsertTypeImpl( this, $"MemoryArray of {type.Name}", type.SizeInBits, true, arrayType );
            return retVal;
        }

        public _Type GetType( TypeRepresentation tr ) => _Type.GetOrInsertTypeImpl( this, tr );

        public _Type GetVoidType( ) => GetType( TypeSystem.WellKnownTypes.System_Void );

        public _Type GetNativeBoolType()
        {
            var boolType = new DebugBasicType(LlvmModule.Context.GetIntType(1), LlvmModule, "bool", DiTypeKind.Boolean);
            return _Type.GetOrInsertTypeImpl(this, "bool", 1, true, boolType);
        }

        internal DINamespace GetOrCreateDINamespace( TypeRepresentation tr )
        {
            return GetOrCreateDINamespace( tr.Namespace );
        }

        // Get a debug namespace descriptor from a Fully Qualified Name (FQN) for a namespace
        // the FQNS name uses the standard .NET dotted form (parent.nested1.nested2[...])
        private DINamespace GetOrCreateDINamespace( string fullName )
        {
            // "global" namespace is null
            if( string.IsNullOrWhiteSpace( fullName ) )
                return null;

            // try the cached mapping first
            DINamespace retVal;
            if( m_DiNamespaces.TryGetValue( fullName, out retVal ) )
                return retVal;

            // find last "." in the name to get the most nested name
            var parentLen = fullName.LastIndexOf( '.' );
            if( parentLen < 0 )
                parentLen = fullName.Length;

            var parentNamespaceName = parentLen == fullName.Length ? null : fullName.Substring( 0, parentLen ); // take everything up to, but not including the last '.'
            var namespaceName = parentNamespaceName == null ? fullName : fullName.Substring( parentLen + 1 ); // take everything after the last '.' in the name

            var parent = GetOrCreateDINamespace( parentNamespaceName );
            retVal = DIBuilder.CreateNamespace( parent, namespaceName, null, 0 );
            m_DiNamespaces.Add( fullName, retVal );
            return retVal;
        }

        // produces an iterator of all the Fully Qualified Names (FQN) for a given namespace starting with the root name and ending with the given FQN itself
        IEnumerable<string> GetQualifiedNamespaceNames( string fullName )
        {
            if( string.IsNullOrWhiteSpace( fullName ) )
            {
                yield return null;
                yield break;
            }

            for( int pos = fullName.IndexOf( '.' ); pos > 0 && pos < fullName.Length - 1; pos = fullName.IndexOf( '.', pos + 1 ) )
            {
                // produce name up to but not including the next '.' delimiter
                yield return fullName.Substring( 0, pos );
            }
            yield return fullName;
        }

        public Constant GetScalarConstant(_Type type, object value)
        {
            Constant ucv = GetUCVScalar(type, value);
            ucv.SetDebugType(type);
            return ucv;
        }

        public Constant GetNullValue(_Type type)
        {
            Constant ucv = type.DebugType.GetNullValue();
            ucv.SetDebugType(type);
            return ucv;
        }

        public _Function GetOrInsertFunction( MethodRepresentation method )
        {
            _Function retVal;
            if( m_FunctionMap.TryGetValue( method.Identity, out retVal ) )
                return retVal;

            retVal = new _Function( this, method );
            m_FunctionMap.Add( method.Identity, retVal );
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
            var llvmStructType = ( IStructType )structType.DebugType;
            if( structMembers != null )
            {
                foreach( Constant ucv in structMembers )
                {
                    var curVal = ucv;
                    var curType = llvmStructType.Members[ fields.Count ];

                    // Special case: Constant objects containing arrays may not strictly match the target pointer type,
                    // as the variable type always has zero elements. In these cases, we need to bitcast the pointer.
                    if( curType.IsPointer && ( curVal.NativeType != curType ) )
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

                    if( curType.IsPointer && curVal.NativeType != curType )
                    {
                        curVal = ConstantExpression.BitCast( curVal, curType );
                    }

                    members.Add( curVal );
                }
            }

            return ConstantArray.From( curType, members );
        }

        public Constant GetUCVScalar( _Type type, object value )
        {
            if( type.IsInteger )
            {
                // Note: This does not cover enums.
                ulong intValue = ( value is ulong ) ? (ulong)value : (ulong)Convert.ToInt64( value );
                return LlvmModule.Context.CreateConstant( type.DebugType, intValue, type.IsSigned );
            }
            else if( type.IsFloat )
            {
                return LlvmModule.Context.CreateConstant( (float)value );
            }
            else if( type.IsDouble )
            {
                return LlvmModule.Context.CreateConstant( (double)value );
            }
            else if( type.TypeRepresentation == TypeSystem.WellKnownTypes.System_IntPtr )
            {
                Constant ucv = LlvmModule.Context.CreateConstant( (uint)type.SizeInBits, (ulong)value, true );
                return ConstantExpression.IntToPtrExpression( ucv, type.DebugType );
            }
            else if( type.TypeRepresentation == TypeSystem.WellKnownTypes.System_UIntPtr )
            {
                Constant ucv = LlvmModule.Context.CreateConstant( (uint)type.SizeInBits, value is ulong ?  (ulong)value : ((UIntPtr)value).ToUInt64(), false );
                return ConstantExpression.IntToPtrExpression( ucv, type.DebugType );
            }

            throw new InvalidCastException( "Cannot create a scalar constant of non-scalar type." );
        }

        public Constant GetUCVZeroInitialized( _Type type )
        {
            return type.DebugType.GetNullValue( );
        }

        public Constant GetUninitializedGlobal(_Type type)
        {
            var gv = LlvmModule.AddGlobal(type.DebugType, string.Empty);
            gv.IsConstant = false;
            gv.Linkage = Linkage.Internal;

            // This returns the equivalent of a "this" pointer for the object.
            gv.SetDebugType(GetOrInsertPointerType(type));
            return gv;
        }

        public Constant GetGlobalFromUCV(
            _Type type,
            Constant header,
            Constant ucv,
            bool isConstant,
            string name,
            string sectionName)
        {
            string uniqueName = $"{name}_{GetMonotonicUniqueId()}";

            // Wrap the value with a header if one is provided.
            if (header != null)
            {
                ucv = LlvmModule.Context.CreateConstantStruct(true, header, ucv);
            }

            GlobalVariable global = LlvmModule.AddGlobal(ucv.NativeType, uniqueName);
            global.IsConstant = isConstant;
            global.Linkage = Linkage.Internal;
            global.Initializer = ucv;
            global.Section = sectionName;

            // For real constant data mark it as unnammed_addr to allow the backend to coalesce identical content.
            // (e.g. if three globals are declared of the same type with the same constant data initializer, then
            // only one copy is really required in the final image generated by the linker.
            global.UnnamedAddress = isConstant;

            // Return a pointer to the beginning of the unwrapped payload.
            Constant result = global;
            if (header != null)
            {
                // Ensure the header portion of the global is marked as used.
                m_usedGlobals.Add(global);

                Constant[] indices = {
                    LlvmModule.Context.CreateConstant(0),
                    LlvmModule.Context.CreateConstant(1), };
                result = ConstantExpression.GetElementPtr(result, indices);
            }

            // This returns the equivalent of a "this" pointer for the object.
            result.SetDebugType(GetOrInsertPointerType(type));
            return result;
        }

        public void FinalizeGlobals()
        {
            // Add all marked globals to a keep-alive data structure that LLVM recognizes.
            if (m_usedGlobals.Count != 0)
            {
                _Type pointerType = GetOrInsertType(TypeSystem.WellKnownTypes.System_IntPtr);

                var globalPointers = new List<Constant>(m_usedGlobals.Count);
                foreach (var global in m_usedGlobals)
                {
                    Constant pointer = ConstantExpression.BitCast(global, pointerType.DebugType);
                    globalPointers.Add(pointer);
                }

                // Note: The following global and section names are prescribed by LLVM and must not change.
                Constant ucv = GetUCVArray(pointerType, globalPointers);
                var llvmUsed = LlvmModule.AddGlobal(ucv.NativeType, false, Linkage.Append, ucv, "llvm.used");
                llvmUsed.Section = "llvm.metadata";
            }
        }

        public void CreateAlias(Value value, string name)
        {
            var gv = value as GlobalObject;
            if (gv != null)
            {
                var alias = LlvmModule.AddAlias(gv, name);
                alias.Linkage = Linkage.External;
            }
            else
            {
                Console.Error.WriteLine("Warning: Ignoring alias \"{0}\" because aliasee is not global.", name);
            }
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
            //    throw new NotSupportedException( $"LLVM module verification failed:\n{msg}" );
            //}
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

        // REVIEW: This can be generalized to creating any function by prototype.
        public Function GetPersonalityFunction(string functionName)
        {
            Function function = LlvmModule.GetFunction(functionName);
            if (function == null)
            {
                _Type intType = _Type.GetOrInsertTypeImpl(this, TypeSystem.WellKnownTypes.System_Int32);
                function = LlvmModule.CreateFunction(functionName, true, intType.DebugType);
            }

            return function;
        }

        internal uint NativeIntSize => LlvmModule.Layout.IntPtrType( ).IntegerBitWidth;

        internal uint PointerSize => LlvmModule.Layout.IntPtrType( ).IntegerBitWidth;

        internal Context LlvmContext => LlvmModule.Context;

        internal DebugInfoBuilder DIBuilder => LlvmModule.DIBuilder;

        internal DICompileUnit DICompileUnit => LlvmModule.DICompileUnit;

        internal NativeModule LlvmModule { get; }
        
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

        private Function CreateLLvmFunctionWithDebugInfo( MethodRepresentation method )
        {
            string mangledName = LLVMModuleManager.GetFullMethodName( method );
            _Type functionType = Manager.GetOrInsertType( method );
            DebugInfo loc = method.DebugInfo ?? Manager.GetDebugInfoFor( method );
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
        private readonly Dictionary<string, DINamespace> m_DiNamespaces = new Dictionary<string, DINamespace>( );
        private readonly List<GlobalValue> m_usedGlobals = new List<GlobalValue>();

        static int GetMonotonicUniqueId( )
        {
            return ( int )System.Threading.Interlocked.Increment( ref globalsCounter );
        }

        static Int64 globalsCounter;
    }
}
