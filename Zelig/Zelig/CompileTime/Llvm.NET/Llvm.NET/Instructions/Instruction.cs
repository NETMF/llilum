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

        public OpCode Opcode => (OpCode)NativeMethods.GetInstructionOpcode( ValueHandle );
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
        /// The alignemnt is always 0 for instructions other than Alloca, Load, and Store
        /// that deal with memory accesses. Setting the alignment for other instructions
        /// results in an InvalidOperationException()
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "alloca" )]
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
    /// thus losing the orignal type and requiring a cast to get back to it.
    /// </remarks>
    public static class InstructionExtensions
    {
        public static T Alignment<T>( this T self, uint value )
            where T : Instruction
        {
            if( self.IsMemoryAccess )
                self.Alignment = value;

            return self;
        }

        public static T IsVolatile<T>( this T self, uint value )
            where T : Instruction
        {
            if( self.IsMemoryAccess )
                self.Alignment = value;

            return self;
        }
    }
}
