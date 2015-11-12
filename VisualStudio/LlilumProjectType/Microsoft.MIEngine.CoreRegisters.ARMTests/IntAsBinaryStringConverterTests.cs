using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace Microsoft.MIEngine.CoreRegisters.ARM.Tests
{
    [TestClass( )]
    public class IntAsBinaryStringConverterTests
    {
        [TestMethod( )]
        public void ConvertTest( )
        {
            var converter = new IntAsBinaryStringConverter( );
            var result = converter.Convert( 0x12345678U, typeof( string ), null, CultureInfo.InvariantCulture );
            Assert.IsNotNull( result );
            var resultString = result as string;
            Assert.IsNotNull( resultString );
            Assert.AreEqual( "0001 0010 0011 0100 0101 0110 0111 1000", resultString );
        }

        [TestMethod]
        [ExpectedException( typeof( NotSupportedException ) )]
        public void ConvertBackTest( )
        {
            var converter = new IntAsBinaryStringConverter( );
            converter.ConvertBack( "1111 0000 1111 0000 1111 0000 1111 0000", typeof( uint ), null, CultureInfo.InvariantCulture );
        }
    }
}