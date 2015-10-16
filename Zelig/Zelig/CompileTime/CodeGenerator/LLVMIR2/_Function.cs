using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;
using System;
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

    public class _Function : _Value
    {
        public _Function( _Module module, string name, _Type funcType )
            : base( module
                  , funcType
                  , module.LlvmModule.AddFunction( name, ( DebugFunctionType )funcType.DebugType )
                  , false
                  )
        {
            var function = ( Function )LlvmValue;
            if( function.BasicBlocks.Count == 0 )
                function.Linkage( Linkage.ExternalWeak );
        }

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

        public _Value GetLocalStackValue( TS.MethodRepresentation method, _BasicBlock block, IR.VariableExpression val, IModuleManager manager )
        {
            if( block.CurDiSubProgram == null )
            {
                block.SetDebugInfo( 0, 0, null, manager, method );
            }

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

            _Type valType = manager.LookupNativeTypeFor( val.Type );
            _Value retVal = GetLocalStackValue( val.ToString( ), valType );

            // If the local had a valid symbolic name in the source code, generate debug info for it.
            if( string.IsNullOrWhiteSpace( val.DebugName?.Name ) )
            {
                return retVal;
            }

            var module = Module;
            DILocalVariable localSym;
            if( tag == Tag.ArgVariable )
            {
                localSym = module.DIBuilder.CreateArgument( block.CurDiSubProgram
                                                          , val.DebugName.Name
                                                          , module.CurDiFile
                                                          , ( uint )block.DebugCurLine
                                                          , valType.DIType
                                                          , true
                                                          , 0
                                                          , ( uint )index
                                                          );
            }
            else
            {
                localSym = module.DIBuilder.CreateLocalVariable( block.CurDiSubProgram
                                                               , val.DebugName.Name
                                                               , module.CurDiFile
                                                               , ( uint )block.DebugCurLine
                                                               , valType.DIType
                                                               , true
                                                               , 0
                                                               , ( uint )index
                                                               );
            }

            // For reference types passed as a pointer, tell debugger to deref the pointer.
            DIExpression expr = module.DIBuilder.CreateExpression( );
            if( !retVal.Type.IsValueType )
            {
                expr = module.DIBuilder.CreateExpression( ExpressionOp.deref );
            }

            var loc = new DILocation( module.LlvmContext, ( uint )block.DebugCurLine, ( uint )block.DebugCurLine, block.CurDiSubProgram );
            module.DIBuilder.InsertDeclare( retVal.LlvmValue, localSym, expr, loc, block.LlvmBasicBlock );
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

            Value retVal = bldr.Alloca( type.DebugType ).RegisterName( name );

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
    }
}
