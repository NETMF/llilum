using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.MIEngine.Extensions.Tests
{
    [TestClass]
    public class MIEngineResultParsersTests
    {
        const string ValidRegValueTestInput = "result-class: done\r\n"
                                            + "register-values: [{number=0, value=0x10006210},{number=1,value=0x0},{number=2,value=0x40000},{number=3,value=0xf014},{number=4,value=0x3f00f707},{number=5,value=0x1000335c},{number=6,value=0x0},{number=7,value=0x3fe00000},{number=8,value=0x10003330},{number=9,value=0x1},{number=10,value=0x0},{number=11,value=0x10001b9c},{number=12,value=0xf000},{number=13,value=0x10006180},{number=14,value=0x9357},{number=15,value=0x385c},{number=25,value=0x1000000},{number=91,value=0x10007fb0},{number=92,value=0x10006180},{number=93,value=0x0},{number=94,value=0x2},{number=95,value=0xf8},{number=96,value=0x0}]";

        const string ValidRegisterNamesTestInput = "result-class: done\r\n"
                                                 + "register-names: [r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,sp,lr,pc,,,,,,,,,,xpsr,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,msp,psp,primask,control,basepri,faultmask]";

        const string ErrorMsgResultTestInput = "result-class: error\r\n"
                                             + "msg: -data-list-register-values: Usage: -data-list-register-values [--skip-unavailable] <format> [<regnum1>...<regnumN>]";

        const string IncompleteResult = "result-class: done\r\n";

        static readonly RegisterIdNamePair[] Registers = {
            new RegisterIdNamePair( 0 , "r0" ),
            new RegisterIdNamePair( 1 ,  "r1"),
            new RegisterIdNamePair( 2 ,  "r2"),
            new RegisterIdNamePair( 3 ,  "r3"),
            new RegisterIdNamePair( 4 ,  "r4"),
            new RegisterIdNamePair( 5 ,  "r5"),
            new RegisterIdNamePair( 6 ,  "r6"),
            new RegisterIdNamePair( 7 ,  "r7"),
            new RegisterIdNamePair( 8 ,  "r8"),
            new RegisterIdNamePair( 9 ,  "r9"),
            new RegisterIdNamePair( 10,  "r10"),
            new RegisterIdNamePair( 11,  "r11"),
            new RegisterIdNamePair( 12,  "r12"),
            new RegisterIdNamePair( 13,  "sp"),
            new RegisterIdNamePair( 14,  "lr"),
            new RegisterIdNamePair( 15,  "pc"),
            new RegisterIdNamePair( 25,  "xpsr"),
            new RegisterIdNamePair( 91,  "msp"),
            new RegisterIdNamePair( 92,  "psp"),
            new RegisterIdNamePair( 93,  "primask"),
            new RegisterIdNamePair( 94,  "control"),
            new RegisterIdNamePair( 95,  "basepri"),
            new RegisterIdNamePair( 96,  "faultmask")
        };

        [ TestMethod ]
        public void ParseRegisterValuesResultAsyncTest( )
        {
            var parsedResult = MIEngineResultParsers.ParseRegisterValuesResultAsync( ValidRegValueTestInput ).Result;
            Assert.IsNotNull( parsedResult );
            Assert.AreEqual( ResultStatus.Done, parsedResult.Status );
            Assert.AreEqual( 23, parsedResult.Registers.Count );
            CheckRegisterValue( parsedResult.Registers[ 0 ], 0,  0x10006210);
            CheckRegisterValue( parsedResult.Registers[ 1 ], 1,  0x0);
            CheckRegisterValue( parsedResult.Registers[ 2 ], 2,  0x40000);
            CheckRegisterValue( parsedResult.Registers[ 3 ], 3,  0xf014);
            CheckRegisterValue( parsedResult.Registers[ 4 ], 4,  0x3f00f707);
            CheckRegisterValue( parsedResult.Registers[ 5 ], 5,  0x1000335c);
            CheckRegisterValue( parsedResult.Registers[ 6 ], 6,  0x0);
            CheckRegisterValue( parsedResult.Registers[ 7 ], 7,  0x3fe00000);
            CheckRegisterValue( parsedResult.Registers[ 8 ], 8,  0x10003330);
            CheckRegisterValue( parsedResult.Registers[ 9 ], 9,  0x1);
            CheckRegisterValue( parsedResult.Registers[ 10 ], 10, 0x0);
            CheckRegisterValue( parsedResult.Registers[ 11 ], 11, 0x10001b9c);
            CheckRegisterValue( parsedResult.Registers[ 12 ], 12, 0xf000);
            CheckRegisterValue( parsedResult.Registers[ 13 ], 13, 0x10006180);
            CheckRegisterValue( parsedResult.Registers[ 14 ], 14, 0x9357);
            CheckRegisterValue( parsedResult.Registers[ 15 ], 15, 0x385c);
            CheckRegisterValue( parsedResult.Registers[ 16 ], 25, 0x1000000);
            CheckRegisterValue( parsedResult.Registers[ 17 ], 91, 0x10007fb0);
            CheckRegisterValue( parsedResult.Registers[ 18 ], 92, 0x10006180);
            CheckRegisterValue( parsedResult.Registers[ 19 ], 93, 0x0);
            CheckRegisterValue( parsedResult.Registers[ 20 ], 94, 0x2);
            CheckRegisterValue( parsedResult.Registers[ 21 ], 95, 0xf8);
            CheckRegisterValue( parsedResult.Registers[ 22 ], 96, 0x0);
        }

        [TestMethod]
        public void ParseRegisterValuePartialInputTest( )
        {
            bool exceptionOccurred = false;
            try
            {
                var parsedResult = MIEngineResultParsers.ParseRegisterValuesResultAsync( IncompleteResult ).Result;
            }
            catch(AggregateException ex)
            {
                Assert.IsInstanceOfType( ex.InnerException, typeof( Sprache.ParseException ) );
                exceptionOccurred = true;
            }
            Assert.IsTrue( exceptionOccurred );
        }


        private static void CheckRegisterValue( RegisterIdValuePair x, int expectedIndex, uint expectedValue )
        {
            Assert.AreEqual( expectedIndex, x.Id );
            Assert.AreEqual( expectedValue, x.Value );
        }

        [TestMethod( )]
        public void ParseChangedRegistersResultAsyncTest( )
        {
            Assert.Inconclusive( );
        }

        [TestMethod( )]
        public void ParseRegisterNamesResultAsyncTest( )
        {
            var parsedResult = MIEngineResultParsers.ParseRegisterNamesResultAsync( ValidRegisterNamesTestInput ).Result;
            Assert.IsNotNull( parsedResult );
            Assert.AreEqual( ResultStatus.Done, parsedResult.Status );
            Assert.AreEqual( Registers.Length, parsedResult.Names.Count );
            for( int i = 0; i < Registers.Length; ++i )
            {
                Assert.AreEqual( Registers[ i ].Id, parsedResult.Names[ i ].Id );
                Assert.AreEqual( Registers[ i ].Name, parsedResult.Names[ i ].Name );
            }
        }
    }
}