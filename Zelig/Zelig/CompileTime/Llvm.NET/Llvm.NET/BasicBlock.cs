using System;
using System.Collections.Generic;
using Llvm.NET.Instructions;

namespace Llvm.NET
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
                var parent = LLVMNative.GetBasicBlockParent( BlockHandle );
                if( parent.Pointer == IntPtr.Zero )
                    return null;

                // cache functions and use lookups to ensure
                // identity/interning remains consistent with actual
                // LLVM model of interning
                return (Function)Value.FromHandle( parent );
            }
        }

        /// <summary>First instruction in the block</summary>
        public Instruction FirstInstruction
        {
            get
            {
                var firstInst = LLVMNative.GetFirstInstruction( BlockHandle );
                if( firstInst.Pointer == IntPtr.Zero )
                    return null;

                return (Instruction)Value.FromHandle( firstInst );
            }
        }

        /// <summary>Last instruction in the block</summary>
        public Instruction LastInstruction
        {
            get
            {
                var lastInst = LLVMNative.GetLastInstruction( BlockHandle );
                if( lastInst.Pointer == IntPtr.Zero)
                    return null;

                return (Instruction)Value.FromHandle( lastInst );
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
                var terminator = LLVMNative.GetBasicBlockTerminator( BlockHandle );
                if( terminator.Pointer == IntPtr.Zero)
                    return null;

                return (Instruction)Value.FromHandle( terminator );
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
        /// <exception cref="ArgumentException">Thrown when <paramref cref="instruction"/> is from a different block</exception>
        public Instruction GetNextInstruction( Instruction instruction )
        {
            if( instruction == null )
                throw new ArgumentNullException( nameof( instruction ) );

            if( instruction.ContainingBlock != this )
                throw new ArgumentException( "Instruction is from a different block", nameof( instruction ) );

            var hInst = LLVMNative.GetNextInstruction( instruction.ValueHandle );
            return hInst.Pointer == IntPtr.Zero ? null : Instruction.FromHandle( hInst );
        }

        private BasicBlock( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        private BasicBlock( LLVMBasicBlockRef basicBlockRef )
            : this( LLVMNative.BasicBlockAsValue( basicBlockRef ), false )
        {
        }

        internal BasicBlock( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABasicBlock ) )
        {
        }

        internal LLVMBasicBlockRef BlockHandle => LLVMNative.ValueAsBasicBlock( ValueHandle );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static BasicBlock FromHandle( LLVMValueRef valueRef )
        {
            return (BasicBlock)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new BasicBlock( h ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal static BasicBlock FromHandle( LLVMBasicBlockRef basicBlockRef )
        {
            return (BasicBlock)Context.CurrentContext.GetValueFor( LLVMNative.BasicBlockAsValue( basicBlockRef ), ( h )=>new BasicBlock( h, true ) );
        }
    }
}
