using Llvm.NET;

namespace Microsoft.Zelig.LLVM
{
    internal class TypeField
    {
        internal TypeField( string name, _Type memberType, uint offset )
        {
            Name = name;
            MemberType = memberType;
            Offset = offset;
        }

        internal string Name { get; set; }
        internal _Type MemberType { get; set; }
        internal uint Offset { get; set; }
        internal uint FinalIdx { get; set; }
    }
}
