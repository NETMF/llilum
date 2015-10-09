using System;
using System.Collections.Generic;
using Llvm.NET.Instructions;

namespace Llvm.NET.Values
{
    /// <summary>Provides aaccess to an LLVM Basic block</summary>
    /// <remarks>
    /// A basic block is a sequence of instructions with a single entry
    /// and a single exit. The exit point must be a <see cref="Terminator"/>
    /// instruction or the block is not (yet) well-formed.
    /// </remarks>
    public class BasicBlock 
        : Value
    {
        /// <summary>Function containing the block</summary>
        public Function ContainingFunction
        {
            get
            {
                var parent = NativeMethods.GetBasicBlockParent( BlockHandle );
                if( parent.Pointer == IntPtr.Zero )
                    return null;

                // cache functions and use lookups to ensure
                // identity/interning remains consistent with actual
                // LLVM model of interning
                return Value.FromHandle<Function>( parent );
            }
        }

        /// <summary>First instruction in the block</summary>
        public Instruction FirstInstruction
        {
            get
            {
                var firstInst = NativeMethods.GetFirstInstruction( BlockHandle );
                if( firstInst.Pointer == IntPtr.Zero )
                    return null;

                return Value.FromHandle<Instruction>( firstInst );
            }
        }

        /// <summary>Last instruction in the block</summary>
        public Instruction LastInstruction
        {
            get
            {
                var lastInst = NativeMethods.GetLastInstruction( BlockHandle );
                if( lastInst.Pointer == IntPtr.Zero)
                    return null;

                return Value.FromHandle<Instruction>( lastInst );
            }
        }

        /// <summary>Terminator instruction for the block</summary>
        /// <remarks>May be null if the block is not yet well-formed
        ///  as is commonly the case while generating code for a new block
        /// </remarks>
        public Instruction Terminator
        {
            get
            {
                var terminator = NativeMethods.GetBasicBlockTerminator( BlockHandle );
                if( terminator.Pointer == IntPtr.Zero)
                    return null;

                return Value.FromHandle<Instruction>( terminator );
            }
        }

        /// <summary>Enumerable collection of all instructions in the block</summary>
        public IEnumerable<Instruction> Instructions
        {
            get
            {
                var current = FirstInstruction;
                while( current != null )
                {
                    yield return current;
                    current = GetNextInstruction( current );
                }
            }
        }

        /// <summary>Gets the instruction that follows a given instruction in a block</summary>
        /// <param name="instruction">instruction in the block to get the next instruction from</param>
        /// <returns>Next instruction or null if none</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref cref="Instruction"/> is from a different block</exception>
        public Instruction GetNextInstruction( Instruction instruction )
        {
            if( instruction == null )
                throw new ArgumentNullException( nameof( instruction ) );

            if( instruction.ContainingBlock != this )
                throw new ArgumentException( "Instruction is from a different block", nameof( instruction ) );

            var hInst = NativeMethods.GetNextInstruction( instruction.ValueHandle );
            return hInst.Pointer == IntPtr.Zero ? null : Value.FromHandle<Instruction>( hInst );
        }

        private BasicBlock( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        private BasicBlock( LLVMBasicBlockRef basicBlockRef )
            : this( NativeMethods.BasicBlockAsValue( basicBlockRef ), false )
        {
        }

        internal BasicBlock( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABasicBlock ) )
        {
        }

        internal LLVMBasicBlockRef BlockHandle => NativeMethods.ValueAsBasicBlock( ValueHandle );

        internal static BasicBlock FromHandle( LLVMBasicBlockRef basicBlockRef )
        {
            return Value.FromHandle<BasicBlock>( NativeMethods.BasicBlockAsValue( basicBlockRef ) );
        }
    }
}
