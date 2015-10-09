using Llvm.NET;

namespace Microsoft.Zelig.LLVM
{
    internal class TypeField
    {
        internal TypeField( string name, _Type memberType, uint offset, bool forceInline )
        {
            Name = name;
            MemberType = memberType;
            Offset = offset;
            ForceInline = forceInline;
        }

        internal string Name { get; set; }
        internal _Type MemberType { get; set; }
        internal uint Offset { get; set; }
        internal bool ForceInline { get; set; }
        internal uint FinalIdx { get; set; }
    }
}
