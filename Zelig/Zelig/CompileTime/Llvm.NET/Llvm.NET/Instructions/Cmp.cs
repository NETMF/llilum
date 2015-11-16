namespace Llvm.NET.Instructions
{
    public class Cmp
        : Instruction
    {
        internal Cmp( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsACmpInst ) )
        {
        }

        public Predicate Predicate
        {
            get
            {
                switch( Opcode )
                {
                case OpCode.ICmp:
                    return ( Predicate )NativeMethods.GetICmpPredicate( ValueHandle );

                case OpCode.FCmp:
                    return ( Predicate )NativeMethods.GetFCmpPredicate( ValueHandle );

                default:
                    return Predicate.BadFcmpPredicate;
                }
            }
        }
    }
}
