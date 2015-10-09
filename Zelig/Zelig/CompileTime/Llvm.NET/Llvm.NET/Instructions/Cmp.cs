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
                case Opcode.ICmp:
                    return ( Predicate )NativeMethods.GetICmpPredicate( ValueHandle );

                case Opcode.FCmp:
                    return ( Predicate )NativeMethods.GetFCmpPredicate( ValueHandle );

                default:
                    return Predicate.BadFcmpPredicate;
                }
            }
        }
    }
}
