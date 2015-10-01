using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;
using System;
using IR = Microsoft.Zelig.CodeGeneration.IR;
using TS = Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    /// <summary>
    /// Subset of AttributeKind values which apply specifically to functions.
    /// </summary>
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
        UnwindTable = AttributeKind.UnwindTable,
    }

    public class _Function : _Value
    {
        public _Function( _Module module, string name, _Type funcType )
            : base( module )
        {
            Function val = module.Impl.GetLLVMObject().AddFunction( name, ( Llvm.NET.Types.FunctionType )( funcType.Impl.GetLLVMObject( ) ) );
            if( val.BasicBlocks.Count == 0 )
                val.Linkage = Llvm.NET.Linkage.ExternalWeak;

            Impl = new ValueImpl( funcType.Impl, val, false );
        }

        public void AddAttribute( FunctionAttribute kind, ulong value = 0 )
        {
            var func = ( Function )( Impl.GetLLVMObject( ) );
            func.AddAttribute( ( AttributeKind )kind, value );
        }

        public void RemoveAttribute( FunctionAttribute kind )
        {
            var func = ( Function )( Impl.GetLLVMObject( ) );
            func.RemoveAttribute( ( AttributeKind )kind );
        }

        public _BasicBlock GetOrInsertBasicBlock( string blockName )
        {
            var func = ( Function )( Impl.GetLLVMObject( ) );

            foreach( var block in func.BasicBlocks )
            {
                if( block.Name == blockName )
                {
                    return new _BasicBlock( this, new BasicBlockImpl( block ) );
                }
            }

            BasicBlockImpl bbi = new BasicBlockImpl( func.AppendBasicBlock( blockName ) );

            if( EntryBlock == null )
            {
                EntryBlock = bbi;
            }

            return new _BasicBlock( this, bbi );
        }

        public _Value GetLocalStackValue( TS.MethodRepresentation method, _BasicBlock block, IR.VariableExpression val, IModuleManager manager )
        {
            if( block.CurDiSubProgram == null )
            {
                block.SetDebugInfo( 0, 0, null, manager, method );
            }

            var valType = manager.LookupNativeTypeFor( val.Type );
            int index = 0;
            var tag = Tag.AutoVariable;
            var argExpression = val as IR.ArgumentVariableExpression;
            if( argExpression != null )
            {
                var fn = ( Function )( Impl.GetLLVMObject( ) );
                index = argExpression.Number;
                if( method is TS.StaticMethodRepresentation )
                    index--;

                fn.Parameters[ index ].Name = argExpression?.DebugName?.Name;
                tag = Tag.ArgVariable;
            }

            var retVal = GetLocalStackValue( val.ToString( ), manager.LookupNativeTypeFor( val.Type ) );
            // if the local had a valid symbolic name in the source code, generate debug info for it
            if( val.DebugName != null && !string.IsNullOrWhiteSpace( val.DebugName.Name ) )
            {
                var module = Module( );
                var localSym = module.DiBuilder.CreateLocalVariable( ( uint )tag
                                                                   , block.CurDiSubProgram
                                                                   , val.DebugName.Name
                                                                   , module.Impl.CurDiFile
                                                                   , ( uint )block.DebugCurLine
                                                                   , valType.Impl.GetDiTypeForStack( )
                                                                   , true
                                                                   , 0
                                                                   , ( uint )index
                                                                   );
                var loc = new DILocation( ( uint )block.DebugCurLine, ( uint )block.DebugCurLine, block.CurDiSubProgram );
                // for reference types passed as a pointer, tell debugger to deref the pointer
                var expr = retVal.Type( ).Impl.IsValueType( ) ? module.DiBuilder.CreateExpression() : module.DiBuilder.CreateExpression( ExpressionOp.deref );
                module.DiBuilder.InsertDeclare( retVal.Impl.GetLLVMObject( ), localSym, loc, block.Impl.GetLLVMObject( ) );
            }
            return retVal;
        }

        public _Value GetLocalStackValue( string name, _Type type )
        {
            var fn = (Function)( Impl.GetLLVMObject( ) );

            if( fn.BasicBlocks.Count == 0 )
            {
                throw new Exception( "Trying to add local value to empty function." );
            }

            BasicBlock bb = fn.EntryBlock;

            var bldr = new Llvm.NET.InstructionBuilder(type.Impl.GetLLVMObject().Context);
            if (bb.FirstInstruction == null)
            {
                // The entry block is empty, which can happen when we're inserting the first InitialValueOperator.
                bldr.PositionAtEnd(bb);
            }
            else
            {
                bldr.PositionBefore(bb.FirstInstruction);
            }

            Value retVal = bldr.Alloca( type.Impl.GetLLVMObjectForStorage( ), name );

            return new _Value( Owner, new ValueImpl( type.Impl, retVal, false ) );
        }

        //public void DeleteBody( )
        //{
        //    throw new NotImplementedException( );
        //}

        public void SetExternalLinkage( )
        {
            ( ( Function )Impl.GetLLVMObject( ) ).Linkage = Llvm.NET.Linkage.External;
        }

        public void SetInternalLinkage( )
        {
            ( ( Function )Impl.GetLLVMObject( ) ).Linkage = Llvm.NET.Linkage.Internal;
        }

        BasicBlockImpl EntryBlock;
    }
}
