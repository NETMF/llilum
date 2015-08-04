using System;

namespace Llvm.NET.Instructions
{
    /// <summary>Exposes an LLVM Instruction</summary>
    public class Instruction
        : User
    {
        /// <summary>Block that contains this instruction</summary>
        public BasicBlock ContainingBlock => BasicBlock.FromHandle( LLVMNative.GetInstructionParent( ValueHandle ) );

        public void SetDebugLocation( uint line, uint column, DebugInfo.Scope scope )
        {
            LLVMNative.SetDebugLoc( ValueHandle, line, column, scope.MetadataHandle );
        }
        
        public Opcode Opcode => (Opcode)LLVMNative.GetInstructionOpcode( ValueHandle );

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
                var opCode = (Opcode)LLVMNative.GetInstructionOpcode( ValueHandle );
                switch( opCode )
                {
                case Opcode.Alloca:
                case Opcode.Load:
                case Opcode.Store:
                    return LLVMNative.GetAlignment( ValueHandle );
                default:
                    return 0;
                }
            }

            set
            {
                var opCode = (Opcode)LLVMNative.GetInstructionOpcode( ValueHandle );
                switch( opCode )
                {
                case Opcode.Alloca:
                case Opcode.Load:
                case Opcode.Store:
                    LLVMNative.SetAlignment( ValueHandle, value );
                    break;

                default:
                    throw new InvalidOperationException( "Alignment can only be set for instructions dealing with memory read/write (alloca, load, store)" );
                }
            }
        }

        internal Instruction( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAInstruction ) )
        { 
        }

        internal new static Instruction FromHandle( LLVMValueRef valueRef )
        {
            var opcode = (Opcode)LLVMNative.GetInstructionOpcode( valueRef );
            switch( opcode )
            {
            case Opcode.Return:
                return Return.FromHandle( valueRef );

            case Opcode.Branch:
                return Branch.FromHandle( valueRef );

            case Opcode.Add:
            case Opcode.FAdd:
            case Opcode.Sub:
            case Opcode.FSub:
            case Opcode.Mul:
            case Opcode.FMul:
            case Opcode.UDiv:
            case Opcode.SDiv:
            case Opcode.FDiv:
            case Opcode.URem:
            case Opcode.SRem:
            case Opcode.FRem:
            case Opcode.Shl:
            case Opcode.LShr:
            case Opcode.AShr:
            case Opcode.And:
            case Opcode.Or:
            case Opcode.Xor:
                return BinaryOperator.FromHandle( valueRef, true );

            case Opcode.Alloca:
                return Alloca.FromHandle( valueRef );

            case Opcode.Load:
                return Load.FromHandle( valueRef );

            case Opcode.Store:
                return Store.FromHandle( valueRef );

            case Opcode.GetElementPtr:
                return GetElementPtr.FromHandle( valueRef );

            case Opcode.ICmp:
                return IntCmp.FromHandle( valueRef );

            case Opcode.FCmp:
                return FCmp.FromHandle( valueRef );

            case Opcode.Call:
                return Call.FromHandle( valueRef );

            case Opcode.ExtractElement:
                return ExtractElement.FromHandle( valueRef );

            case Opcode.Phi:
                return PhiNode.FromHandle( valueRef );

            case Opcode.Trunc:
                return Trunc.FromHandle( valueRef );

            case Opcode.ZeroExtend:
                return ZeroExtend.FromHandle( valueRef );

            case Opcode.PtrToInt:
                return PointerToInt.FromHandle( valueRef );

            case Opcode.IntToPtr:
                return IntToPointer.FromHandle( valueRef );

            case Opcode.BitCast:
                return BitCast.FromHandle( valueRef );

            case Opcode.SignExtend:
                return SignExtend.FromHandle( valueRef );

            case Opcode.Unreachable:
            case Opcode.IndirectBranch:
            case Opcode.Invoke:
            case Opcode.Switch:
            case Opcode.FPToUI:
            case Opcode.FPToSI:
            case Opcode.UIToFP:
            case Opcode.SIToFP:
            case Opcode.FPTrunc:
            case Opcode.FPExt:
            case Opcode.AddrSpaceCast:
            case Opcode.Select:
            case Opcode.UserOp1:
            case Opcode.UserOp2:
            case Opcode.VaArg:
            case Opcode.InsertElement:
            case Opcode.ShuffleVector:
            case Opcode.ExtractValue:
            case Opcode.InsertValue:
            case Opcode.Fence:
            case Opcode.AtomicCmpXchg:
            case Opcode.AtomicRMW:
            case Opcode.Resume:
            case Opcode.LandingPad:
            default:
                throw new NotImplementedException( );
            }
        }
    }
}
