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
#if CREATE_FUNCTION_DEBUGINFO
                  // Ideally it would be cleaner to create the debug information at the same time as the function itself
                  // (Llvm.NET Debug info was designed to support that). Unfortunately, given how the Zelig engine
                  // processes the transformations in phases and handlers,etc.. not enough of the required debug
                  // information is available at the time this is called to pull that off. [ In fact, If this code and
                  // the related method below are enabled, then LLVM will crash deep in the call to DiBuilder.Finalize() ]
                  // So, for now, the information is created on-the-fly in _BasicBlock. This mixes the roles and
                  // responsibilites a bit, but gets the job done until we can fully unwind the final code gen from the
                  // transform engine. 
                  , CreateLLvmFunctionWithDebugInfo( module, manager, method )
#else
                  , module.LlvmModule.AddFunction( LLVMModuleManager.GetFullMethodName( method )
                                                 , ( IFunctionType )manager.GetOrInsertType( method ).DebugType
                                                 )
#endif
                  , false
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
            func.Attributes.Remove( ( AttributeKind )kind );
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
                var fn = ( Function )LlvmValue;
                index = argExpression.Number;
                if( method is TS.StaticMethodRepresentation )
                    index--;

                fn.Parameters[ index ].Name = argExpression?.DebugName?.Name ?? $"$ARG{argExpression.Number}";
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

            BasicBlock bb = fn.EntryBlock;

            var bldr = new InstructionBuilder( type.DebugType.Context );
            if (bb.FirstInstruction == null)
            {
                // The entry block is empty, which can happen when we're inserting the first InitialValueOperator.
                bldr.PositionAtEnd(bb);
            }
            else
            {
                bldr.PositionBefore(bb.FirstInstruction);
            }

            Value retVal = bldr.Alloca( type.DebugType )
                               .RegisterName( name );

            _Type pointerType = Module.GetOrInsertPointerType( type );
            return new _Value( Module, pointerType, retVal, false );
        }

        public void SetExternalLinkage( )
        {
            ( ( Function )LlvmValue ).Linkage = Linkage.External;
        }

        public void SetInternalLinkage( )
        {
            ( ( Function )LlvmValue ).Linkage = Linkage.Internal;
        }

#if CREATE_FUNCTION_DEBUGINFO
        private static Function CreateLLvmFunctionWithDebugInfo( _Module module, LLVMModuleManager manager, TS.MethodRepresentation method )
        {
            string mangledName = manager.GetFullMethodName( method );
            _Type functionType = manager.GetOrInsertType( method );
            DebugInfo loc = manager.GetDebugInfoFor( method );
            Debug.Assert( loc != null );

            // Create the DISupprogram info
            var retVal = module.LlvmModule.CreateFunction( module.DICompileUnit
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
            return retVal;
        }
#endif
    }
}
