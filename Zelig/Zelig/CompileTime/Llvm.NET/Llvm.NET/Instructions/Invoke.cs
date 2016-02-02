using Llvm.NET.Native;
using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    public class Invoke
        : Terminator
        , IAttributeSetContainer
    {
        public AttributeSet Attributes
        {
            get 
            {
                if( TargetFunction == null )
                    return new AttributeSet();

                return new AttributeSet( NativeMethods.GetCallSiteAttributeSet( ValueHandle ));
            }

            set
            {
                // TODO: Verify the attributeSet doesn't contain any parameter indices not supported by the TargetFunction
                NativeMethods.SetCallSiteAttributeSet( ValueHandle, value.NativeAttributeSet );
            }
        }

        public Function TargetFunction
        {
            get
            {
                if( Operands.Count < 1 )
                    return null;

                // last Operand of the instruction is the target function
                return Operands[ Operands.Count - 1 ] as Function;
            }
        }

        internal Invoke( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}
