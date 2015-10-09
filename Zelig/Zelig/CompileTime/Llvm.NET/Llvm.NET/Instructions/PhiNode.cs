using System;
using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    public class PhiNode
        : Instruction
    {
        public void AddIncoming( Value value, BasicBlock srcBlock )
        {
            AddIncoming( Tuple.Create( value, srcBlock ) );
        }

        public void AddIncoming( Tuple<Value, BasicBlock> firstIncoming, params Tuple<Value, BasicBlock>[ ] additionalIncoming )
        {
            LLVMValueRef[] llvmValues= new LLVMValueRef[ additionalIncoming.Length + 1 ];
            llvmValues[ 0 ] = firstIncoming.Item1.ValueHandle;
            for( int i = 0; i < additionalIncoming.Length; ++i )
                llvmValues[ i + i ] = additionalIncoming[ i ].Item1.ValueHandle;

            LLVMBasicBlockRef[] llvmBlocks = new LLVMBasicBlockRef[ additionalIncoming.Length + 1 ];
            llvmBlocks[ 0 ] = firstIncoming.Item2.BlockHandle;
            for( int i = 0; i < additionalIncoming.Length; ++i )
                llvmBlocks[ i + i ] = additionalIncoming[ i ].Item2.BlockHandle;

            NativeMethods.AddIncoming( ValueHandle, out llvmValues[ 0 ], out llvmBlocks[ 0 ], ( uint )llvmValues.Length );
        }

        internal PhiNode( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal PhiNode( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAPHINode ) )
        {
        }
    }
}
