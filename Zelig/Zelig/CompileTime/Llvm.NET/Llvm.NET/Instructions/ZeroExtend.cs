
namespace Llvm.NET.Instructions
{
    public class ZeroExtend
        : Cast
    {
        internal ZeroExtend( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ZeroExtend( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAZExtInst ) )
        {
        }
    }
}
