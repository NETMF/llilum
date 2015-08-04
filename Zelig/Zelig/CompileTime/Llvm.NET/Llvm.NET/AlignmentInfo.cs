using System;
using System.Text;

namespace Llvm.NET
{
    /// <summary>Provides information about the ABI required and prefered alignment for a given value/type</summary>
    public class AlignmentInfo
    {
        /// <summary>Creates a new <see cref="AlignmentInfo"/> with the ABI required and preferred alignment are identical</summary>
        /// <param name="abiAlignment">Alignment in bits</param>
        public AlignmentInfo( int abiAlignment )
            : this( abiAlignment, abiAlignment )
        {
        }

        /// <summary>Creates a new <see cref="AlignmentInfo"/>with the ABI required and preferred alignment are distinct</summary>
        /// <param name="abiAlignment">ABI required alignment</param>
        /// <param name="preferredAlignment">Preferred alignment for the type/value</param>
        /// <remarks>
        /// The ABI alignment must be greater than or equal to the Preferred alignment or an exception is thrown.
        /// </remarks>
        public AlignmentInfo( int abiAlignement, int preferredAlignment )
        {
            if( abiAlignement < preferredAlignment )
                throw new ArgumentException( "ABI alignment < preferred Alignment" );

            AbiAlignment = abiAlignement;
            PreferredAlignment = preferredAlignment;
        }

        /// <summary>The preferred alignment for the type</summary>
        public int PreferredAlignment {get; }

        /// <summary>The ABI alignment for the type</summary>
        public int AbiAlignment { get; }

        /// <summary>Converts the alignmentinfo to a string</summary>
        /// <returns>string representing the AlignmentInfo</returns>
        /// <remarks>
        /// The formatted string is of the form "AbiAlignment[:PreferredAlignment]"
        /// the preferred alignment is only included if it is not equal to the
        /// ABI alignment. This is intended for direct use in forming the LLVM
        /// target layout string.
        /// </remarks>
        public override string ToString( )
        {
            var bldr = new StringBuilder( );
            bldr.Append( AbiAlignment );
            if( AbiAlignment != PreferredAlignment )
                bldr.AppendFormat( ":{0}", PreferredAlignment );

            return bldr.ToString( );
        }
    }
}
