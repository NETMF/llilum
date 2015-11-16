namespace Llvm.NET.Instructions
{
    public class Intrinsic
        : CallInstruction
    {
        internal Intrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAIntrinsicInst ) )
        { 
        }

        internal const string DoNothingName = "llvm.donothing";
        internal const string DebugTrapName = "llvm.debugtrap";
        
        // TODO: move these out of here to follow pattern in MemCpy
        internal const string MemMoveName = "llvm.memmove.p0i8.p0i8.i32";
        internal const string MemSetName = "llvm.memset.p0i8.i32";
    };
}
