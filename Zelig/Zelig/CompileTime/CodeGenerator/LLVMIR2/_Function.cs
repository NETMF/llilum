using System;

namespace Microsoft.Zelig.LLVM
{
    public class _Function : _Value
    {
        public _Function( _Module module, string name, _Type funcType )
            : base( module )
        {
            Llvm.NET.Function val = module.Impl.GetLLVMObject().AddFunction( name, ( Llvm.NET.FunctionType )( funcType.Impl.GetLLVMObject( ) ) );
            if( val.BasicBlocks.Count == 0 )
                val.Linkage = Llvm.NET.Linkage.ExternalWeak;

            Impl = new ValueImpl( funcType.Impl, val, false );
        }

        public _BasicBlock GetOrInsertBasicBlock( string blockName )
        {
            var func = ( Llvm.NET.Function )( Impl.GetLLVMObject( ) );

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

        public _Value GetLocalStackValue( string name, _Type type )
        {
            var fn = (Llvm.NET.Function)( Impl.GetLLVMObject( ) );

            if( fn.BasicBlocks.Count == 0 )
            {
                throw new Exception( "Trying to add local value to empty function." );
            }

            Llvm.NET.BasicBlock bb = fn.EntryBlock;

            var bldr = new Llvm.NET.InstructionBuilder( type.Impl.GetLLVMObject().Context );
            bldr.PositionBefore( bb.FirstInstruction );

            Llvm.NET.Value retVal = bldr.Alloca( type.Impl.GetLLVMObjectForStorage( ), name );

            return new _Value( Owner, new ValueImpl( type.Impl, retVal, false ) );
        }

        //public void DeleteBody( )
        //{
        //    throw new NotImplementedException( );
        //}

        public void SetExternalLinkage( )
        {
            ( ( Llvm.NET.Function )Impl.GetLLVMObject( ) ).Linkage = Llvm.NET.Linkage.External;
        }

        public void SetInternalLinkage( )
        {
            ( ( Llvm.NET.Function )Impl.GetLLVMObject( ) ).Linkage = Llvm.NET.Linkage.Internal;
        }

        BasicBlockImpl EntryBlock;
    }
}
