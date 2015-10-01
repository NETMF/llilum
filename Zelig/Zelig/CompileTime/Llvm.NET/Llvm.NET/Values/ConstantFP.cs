namespace Llvm.NET.Values
{
    /// <summary>Floating point constant value in LLVM</summary>
    public class ConstantFP : Constant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static ConstantFP From( float constValue )
        {
            return FromHandle( LLVMNative.ConstReal( Context.CurrentContext.FloatType.TypeHandle, constValue ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static ConstantFP From( double constValue )
        {
            return FromHandle( LLVMNative.ConstReal( Context.CurrentContext.DoubleType.TypeHandle, constValue ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static ConstantFP From( Context context, float constValue )
        {
            return FromHandle( LLVMNative.ConstReal( context.FloatType.TypeHandle, constValue ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        public static ConstantFP From( Context context, double constValue )
        {
            return FromHandle( LLVMNative.ConstReal( context.DoubleType.TypeHandle, constValue ) );
        }

        public double Value
        {
            get
            {
                bool loosesInfo;
                return GetValue( out loosesInfo );
            }
        }

        public double GetValue( out bool loosesInfo )
        {
            loosesInfo = false;
            LLVMBool nativeLoosesInfo;
            var retVal = LLVMNative.ConstRealGetDouble( ValueHandle, out nativeLoosesInfo );
            loosesInfo = nativeLoosesInfo;
            return retVal;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static ConstantFP FromHandle( LLVMValueRef valueRef )
        {
            return ( ConstantFP )Context.CurrentContext.GetValueFor( valueRef, ( h ) => new ConstantFP( h ) );
        }

        internal ConstantFP( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantFP( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantFP ) )
        {
        }
    }
}
