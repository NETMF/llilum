namespace Llvm.NET.Values
{
    /// <summary>Floating point constant value in LLVM</summary>
    public class ConstantFP : Constant
    {
        public double Value
        {
            get
            {
                bool loosesInfo;
                return GetValueWithLoss( out loosesInfo );
            }
        }

        public double GetValueWithLoss( out bool loosesInfo )
        {
            loosesInfo = false;
            LLVMBool nativeLoosesInfo;
            var retVal = NativeMethods.ConstRealGetDouble( ValueHandle, out nativeLoosesInfo );
            loosesInfo = nativeLoosesInfo;
            return retVal;
        }

        internal ConstantFP( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantFP ) )
        {
        }
    }
}
