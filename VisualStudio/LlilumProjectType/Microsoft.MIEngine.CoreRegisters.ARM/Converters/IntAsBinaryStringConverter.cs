using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    /// <summary>Value converter to convert an integer into a string of binary digits in groups of 4 digits separated by spaces</summary>
    public class IntAsBinaryStringConverter
        : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var bldr = new StringBuilder(32 + 7);// 32 digits plus 7 spaces
            foreach( var c in GetBitChars( ( uint )value ) )
                bldr.Append( c );

            return bldr.ToString( );
        }

        // xxxx_xxxx_xxxx_xxxx_xxxx_xxxx_xxxx_xxxx
        private IEnumerable<char> GetBitChars( uint value )
        {
            var binString = System.Convert.ToString( value, 2 );
            int bitIndex = 0;
            for( var index = binString.Length - 32; index < binString.Length; ++index, ++bitIndex )
            {
                if( InsertDelimiter( bitIndex ) )
                    yield return ' ';

                yield return index < 0 ? '0' : binString[ index ];
            }
        }

        private static bool InsertDelimiter( int bitIndex )
        {
            return bitIndex != 0 && ( bitIndex % 4 == 0 ) && bitIndex <= 32;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException( );
        }
    }
}
