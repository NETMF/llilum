using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;
using System;
using System.Diagnostics;
using Llvm.NET.Types;
using IR = Microsoft.Zelig.CodeGeneration.IR;
using TS = Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    /// <summary>Subset of AttributeKind values which apply specifically to functions.</summary>
    public enum FunctionAttribute : uint
    {
        AlwaysInline = AttributeKind.AlwaysInline,
        Builtin = AttributeKind.Builtin,
        Cold = AttributeKind.Cold,
        Convergent = AttributeKind.Convergent,
        InlineHint = AttributeKind.InlineHint,
        JumpTable = AttributeKind.JumpTable,
        MinSize = AttributeKind.MinSize,
        Naked = AttributeKind.Naked,
        NoBuiltin = AttributeKind.NoBuiltin,
        NoDuplicate = AttributeKind.NoDuplicate,
        NoImplicitFloat = AttributeKind.NoImplicitFloat,
        NoInline = AttributeKind.NoInline,
        NonLazyBind = AttributeKind.NonLazyBind,
        NoRedZone = AttributeKind.NoRedZone,
        NoReturn = AttributeKind.NoReturn,
        NoUnwind = AttributeKind.NoUnwind,
        OptimizeForSize = AttributeKind.OptimizeForSize,
        OptimizeNone = AttributeKind.OptimizeNone,
        ReadNone = AttributeKind.ReadNone,
        ReadOnly = AttributeKind.ReadOnly,
        ArgMemOnly = AttributeKind.ArgMemOnly,
        ReturnsTwice = AttributeKind.ReturnsTwice,
        StackAlignment = AttributeKind.StackAlignment,
        StackProtect = AttributeKind.StackProtect,
        StackProtectReq = AttributeKind.StackProtectReq,
        StackProtectStrong = AttributeKind.StackProtectStrong,
        SafeStack = AttributeKind.SafeStack,
        SanitizeAddress = AttributeKind.SanitizeAddress,
        SanitizeThread = AttributeKind.SanitizeThread,
        SanitizeMemory = AttributeKind.SanitizeMemory,
        UnwindTable = AttributeKind.UWTable,
    }

    // REVIEW:
    // Given that this has no data members beyond those from the base class
    // this class probably ought to be a static class with extensions methods 
    // on the Llvm.NET.Values.Function class. 
    public class _Function : _Value
    {
        internal _Function( _Module module, LLVMModuleManager manager, TS.MethodRepresentation method )
            : base( module
                  , manager.GetOrInsertType( method )
                  , CreateLLvmFunctionWithDebugInfo( module, manager, method )
                  )
        {
            var function = ( Function )LlvmValue;
            if( function.BasicBlocks.Count == 0 )
                function.Linkage( Linkage.ExternalWeak );
        }

        public Function LlvmFunction => ( Function )LlvmValue;

        public void AddAttribute( FunctionAttribute kind )
        {
            var func = ( Function )LlvmValue;
            func.AddAttributes( ( AttributeKind )kind );
        }

        public void AddAttribute( FunctionAttribute kind, ulong value )
        {
            var func = ( Function )LlvmValue;
            func.AddAttributes( new AttributeValue( (AttributeKind )kind, value ) );
        }

        public void RemoveAttribute( FunctionAttribute kind )
        {
            var func = ( Function )LlvmValue;
            func.RemoveAttribute( ( AttributeKind )kind );
        }

        public _BasicBlock GetOrInsertBasicBlock( string blockName )
        {
            var func = ( Function )LlvmValue;

            return new _BasicBlock( this, func.FindOrCreateNamedBlock( blockName ) );
        }

        public _Value GetLocalStackValue( TS.MethodRepresentation method
                                        , _BasicBlock block
                                        , IR.VariableExpression val
                                        , LLVMModuleManager manager )
        {
            if( block.CurDILocation == null )
            {
                block.SetDebugInfo( manager, method, null );
            }
            Debug.Assert( block.CurDILocation != null );
            Debug.Assert( block.CurDISubProgram != null );

            int index = 0;
            var tag = Tag.AutoVariable;
            var argExpression = val as IR.ArgumentVariableExpression;
            if( argExpression != null )
            {
                index = argExpression.Number;
                // adjust index since IR form keeps slot = 0 as the "this" param
                // for static methods it just sets that to null. LLVM doesn't
                // have any notion of that and only has a slot for an actual arg
                if( method is TS.StaticMethodRepresentation )
                    index--;

                tag = Tag.ArgVariable;
            }

            var retVal = GetLocalStackValue( val.ToString( ), manager.GetOrInsertType( val.Type ) );

            // If the local had a valid symbolic name in the source code, generate debug info for it.
            if( string.IsNullOrWhiteSpace( val.DebugName?.Name ) )
            {
                return retVal;
            }

            _Type valType = manager.GetOrInsertType( val.Type );
            DILocalVariable localSym;
            if( tag == Tag.ArgVariable )
            {
                localSym = Module.DIBuilder.CreateArgument( block.CurDISubProgram
                                                          , val.DebugName.Name
                                                          , block.CurDILocation?.Scope.File
                                                          , block.CurDILocation?.Line ?? 0U
                                                          , valType.DIType
                                                          , true
                                                          , 0
                                                          , ( uint )index
                                                          );
            }
            else
            {
                localSym = Module.DIBuilder.CreateLocalVariable( block.CurDISubProgram
                                                               , val.DebugName.Name
                                                               , block.CurDILocation?.Scope.File
                                                               , block.CurDILocation?.Line ?? 0U
                                                               , valType.DIType
                                                               , true
                                                               , 0
                                                               , ( uint )index
                                                               );
            }

            // For reference types passed as a pointer, tell debugger to deref the pointer.
            DIExpression expr = Module.DIBuilder.CreateExpression( );
            if( !retVal.Type.IsValueType )
            {
                expr = Module.DIBuilder.CreateExpression( ExpressionOp.deref );
            }

            Module.DIBuilder.InsertDeclare( retVal.LlvmValue, localSym, expr, block.CurDILocation, block.LlvmBasicBlock );
            return retVal;
        }

        public _Value GetLocalStackValue( string name, _Type type )
        {
            var fn = (Function)LlvmValue;

            if( fn.BasicBlocks.Count == 0 )
            {
                throw new Exception( "Trying to add local value to empty function." );
            }

            var bldr = new InstructionBuilder( type.DebugType.Context );
            bldr.PositionAtEnd( fn.EntryBlock );

            Value retVal = bldr.Alloca( type.DebugType )
                               .RegisterName( name );

            _Type pointerType = Module.GetOrInsertPointerType( type );
            return new _Value( Module, pointerType, retVal );
        }

        public void SetExternalLinkage( )
        {
            ( ( Function )LlvmValue ).Linkage = Linkage.External;
        }

        public void SetInternalLinkage( )
        {
            ( ( Function )LlvmValue ).Linkage = Linkage.Internal;
        }

        private static Function CreateLLvmFunctionWithDebugInfo( _Module module, LLVMModuleManager manager, TS.MethodRepresentation method )
        {
            string mangledName = LLVMModuleManager.GetFullMethodName( method );
            _Type functionType = manager.GetOrInsertType( method );
            Debugging.DebugInfo loc = manager.GetDebugInfoFor( method );
            Debug.Assert( loc != null );

            var containingType = module.GetOrInsertType( method.OwnerType );

            // Create the DISupprogram info
            var retVal = module.LlvmModule.CreateFunction( containingType?.DIType ?? ( DIScope )module.DICompileUnit
                                                         , method.Name
                                                         , mangledName
                                                         , module.GetOrCreateDIFile( loc.SrcFileName )
                                                         , ( uint )loc.BeginLineNumber
                                                         , (DebugFunctionType)functionType.DebugType
                                                         , true
                                                         , true
                                                         , ( uint )loc.EndLineNumber
                                                         , DebugInfoFlags.None // TODO: Map Zelig accesibility info etc... to flags
                                                         , false
                                                         );
            bool isStatic = method is TS.StaticMethodRepresentation;
            // "this" is always at index 0, for static functions the name for "this" is null
            int paramBase = isStatic ? 1 : 0;
            Debug.Assert( retVal != null && method.ArgumentNames.Length - paramBase == retVal.Parameters.Count );
            for( int i = paramBase; i < method.ArgumentNames.Length; ++i )
            {
                string name = method.ArgumentNames[ i ];
                if( string.IsNullOrWhiteSpace( name ) )
                    name = $"$ARG{i}";

                // adjust the index for the native type since there's not assumption that [0]=="this"
                retVal.Parameters[ i - paramBase ].Name = name;
            }
            return retVal;
        }
    }
}
