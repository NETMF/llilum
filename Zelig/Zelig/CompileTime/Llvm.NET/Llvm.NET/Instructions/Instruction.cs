using System;
using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    /// <summary>Exposes an LLVM Instruction</summary>
    public class Instruction
        : User
    {
        /// <summary>Block that contains this instruction</summary>
        public BasicBlock ContainingBlock => BasicBlock.FromHandle( LLVMNative.GetInstructionParent( ValueHandle ) );

        public void SetDebugLocation( uint line, uint column, DebugInfo.DIScope scope )
        {
            LLVMNative.SetDebugLoc( ValueHandle, line, column, scope.MetadataHandle );
        }
        
        public Opcode Opcode => (Opcode)LLVMNative.GetInstructionOpcode( ValueHandle );
        public bool IsMemoryAccess
        {
            get
            {
                var opCode = Opcode;
                return opCode == Opcode.Alloca
                    || opCode == Opcode.Load
                    || opCode == Opcode.Store;
            }
        }

        /// <summary>Alignment for the instruction</summary>
        /// <remarks>
        /// The alignemnt is always 0 for instructions other than Alloca, Load, and Store
        /// that deal with memory accesses. Setting the alignment for other instructions
        /// results in an InvalidOperationException()
        /// </remarks>
        public uint Alignment
        {
            get
            {
                return IsMemoryAccess ? LLVMNative.GetAlignment( ValueHandle ) : 0;
            }

            set
            {
                if( !IsMemoryAccess )
                    throw new InvalidOperationException( "Alignment can only be set for instructions dealing with memory read/write (alloca, load, store)" );
                LLVMNative.SetAlignment( ValueHandle, value );
            }
        }

        internal Instruction( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAInstruction ) )
        { 
        }
    }
}
