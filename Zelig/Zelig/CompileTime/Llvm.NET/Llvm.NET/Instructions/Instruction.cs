using System;
using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    /// <summary>Exposes an LLVM Instruction</summary>
    public class Instruction
        : User
    {
        /// <summary>Block that contains this instruction</summary>
        public BasicBlock ContainingBlock => BasicBlock.FromHandle( NativeMethods.GetInstructionParent( ValueHandle ) );

        /// <summary>Gets the LLVM opcode for the instruction</summary>
        public OpCode Opcode => (OpCode)NativeMethods.GetInstructionOpcode( ValueHandle );

        /// <summary>FLag to indicate if the opcode is for a memory access <see cref="Alloca"/>, <see cref="Load"/>, <see cref="Store"/></summary>
        public bool IsMemoryAccess
        {
            get
            {
                var opCode = Opcode;
                return opCode == OpCode.Alloca
                    || opCode == OpCode.Load
                    || opCode == OpCode.Store;
            }
        }

        /// <summary>Alignment for the instruction</summary>
        /// <remarks>
        /// The alignment is always 0 for instructions other than Alloca, Load, and Store
        /// that deal with memory accesses. Setting the alignment for other instructions
        /// results in an InvalidOperationException()
        /// </remarks>
        public uint Alignment
        {
            get
            {
                return IsMemoryAccess ? NativeMethods.GetAlignment( ValueHandle ) : 0;
            }

            set
            {
                if( !IsMemoryAccess )
                    throw new InvalidOperationException( "Alignment can only be set for instructions dealing with memory read/write (alloca, load, store)" );
                NativeMethods.SetAlignment( ValueHandle, value );
            }
        }

        internal Instruction( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAInstruction ) )
        { 
        }
    }

    /// <summary>Provides extension methods to <see cref="Instruction"/> that cannot be achieved as members of the class</summary>
    /// <remarks>
    /// Using generic static extension methods allows for fluent coding while retaining the type of the "this" parameter.
    /// If these were members of the <see cref="Instruction"/> class then the only return type could be <see cref="Instruction"/>
    /// thus losing the original type and requiring a cast to get back to it, thereby defeating the purpose of the fluent style.
    /// </remarks>
    public static class InstructionExtensions
    {
        /// <summary>Fluent style extension method to set the <see cref="Instruction.Alignment"/> for an instruction</summary>
        /// <typeparam name="T">Type of the instruction (usually implicitly inferred from usage)</typeparam>
        /// <param name="self">Instruction to set the <see cref="Instruction.Alignment"/> for</param>
        /// <param name="value">New alignment for the instruction</param>
        /// <returns>To allow fluent style coding this returns the <paramref name="self"/> parameter</returns>
        public static T Alignment<T>( this T self, uint value )
            where T : Instruction
        {
            if( self.IsMemoryAccess )
                self.Alignment = value;

            return self;
        }

        /// <summary>Fluent style extension method to set the Volatile property of a <see cref="Load"/> or <see cref="Store"/> instruction</summary>
        /// <typeparam name="T">Type of the instruction (usually implicitly inferred from usage)</typeparam>
        /// <param name="self">Instruction to set the Volatile property for</param>
        /// <param name="value">Flag to indicate if the instruction's operation is volatile</param>
        /// <returns>To allow fluent style coding this returns the <paramref name="self"/> parameter</returns>
        public static T IsVolatile<T>( this T self, bool value )
            where T : Instruction
        {
            if( self.IsMemoryAccess )
            {
                // only load and store instructions have the volatile property
                var loadInst = self as Load;
                if( loadInst != null )
                    loadInst.IsVolatile = value;
                else
                {
                    var storeinst = self as Store;
                    if( storeinst != null )
                        storeinst.IsVolatile = value;
                }
            }

            return self;
        }
    }
}
