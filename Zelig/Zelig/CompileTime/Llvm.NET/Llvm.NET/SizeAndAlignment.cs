namespace Llvm.NET
{
    /// <summary>Provides Target layout size and alignment information for a type</summary>
    public class SizeAndAlignment
    {
        /// <summary>Constructs a new <see cref="SizeAndAlignment"/> instance</summary>
        /// <param name="size">Sixe of the type</param>
        /// <param name="abiAlignment">ABI required alignment for the type</param>
        public SizeAndAlignment( int size, int abiAlignment )
            : this( size, abiAlignment, abiAlignment )
        {
        }

        /// <summary>Constructs a new <see cref="SizeAndAlignment"/> instance</summary>
        /// <param name="size">Sixe of the type</param>
        /// <param name="abiAlignment">ABI required alignment for the type</param>
        /// <param name="preferredAlignment">Preferred alignment for the type</param>
        public SizeAndAlignment( int size, int abiAlignement, int preferredAlignment )
        {
            Alignment = new AlignmentInfo( abiAlignement, preferredAlignment );
            Size = size;
        }

        /// <summary>Alignment info for a type</summary>
        public AlignmentInfo Alignment { get; }

        /// <summary>Size of a type</summary>
        public int Size { get; }

        /// <summary>Builds a string representing the size and alignment information</summary>
        /// <returns>String, suitible for use in LLVM type layout strings for this size and alignment combination</returns>
        public override string ToString( ) => string.Format( "{0}:{1}", Size, Alignment.ToString( ) );
    }
}
