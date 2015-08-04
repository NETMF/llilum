namespace Llvm.NET.Instructions
{
    public class Intrinsic
        : Call
    {
        internal Intrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAIntrinsicInst ) )
        { 
        }

        internal const string DoNothingName = "llvm.donothing";
        internal const string DebugTrapName = "llvm.debugtrap";
        internal const string MemCpyName = "llvm.memcpy.p0i8.p0i8.i32";
        internal const string MemMoveName = "llvm.memmove.p0i8.p0i8.i32";
        internal const string MemSetName = "llvm.memset.p0i8.i32";
    };
}
