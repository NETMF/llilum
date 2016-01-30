using System;
using System.Collections.Generic;

namespace Microsoft.Zelig.Test
{
    public class TestRunner
    {
        private int m_timeOut = 900000; // 15 minutes default time out.
        
        /// <summary>
        /// An overloaded constructor that takes the test objects in its arguments.
        /// </summary>
        /// <param name="args">A list of test objects.</param>
        public TestRunner(int timeout) 
        {
            m_timeOut = timeout; 
        }

        public void Run()
        {
            var tests = new List<TestBase>( );
            bool testResults = true;

            //
            // System.Exception
            //
            tests.Add( new ExceptionTests( ) ); 

            //
            // System.Text.StringBuilder
            //
            tests.Add( new StringBuilderTests( ) ); // https://github.com/NETMF/llilum/issues/109, https://github.com/NETMF/llilum/issues/108

            ////////
            //////// System.IO.MemoryStream
            ////////
            tests.Add(new CanRead());
            tests.Add(new CanSeek());
            tests.Add(new CanWrite());
            tests.Add(new Close());
            tests.Add(new Flush());
            tests.Add(new Length());
            tests.Add(new MemoryStream_Ctor());
            tests.Add(new Position());
            tests.Add(new Read());
            tests.Add(new ReadByte());
            tests.Add(new Seek());
            tests.Add(new SetLength());
            tests.Add(new ToArray());
            tests.Add(new Write());
            tests.Add(new WriteByte());
            tests.Add(new WriteTo());

            ////////
            //////// mscorlib
            ////////
            tests.Add(new ArraysSimpleTests());
            tests.Add(new ArraysOtherTests());
            tests.Add(new BasicConceptTests());
            tests.Add(new ConstructorsTests());
            tests.Add(new ConstTests());
            tests.Add(new DeclarationsTests());
            tests.Add(new DestructorsTests());
            tests.Add(new EnumTests());
            tests.Add(new EventsTests());
            tests.Add(new FieldsTests());
            tests.Add(new IndexersTests());
            tests.Add(new InterfaceTests());
            tests.Add(new OperatorsTests());
            tests.Add(new PropertiesTests());
            tests.Add(new Static_InstTests());
            tests.Add(new MembersTests());
            tests.Add(new MethodsTests());
            tests.Add(new StructsTests());

            foreach(ITestInterface t in tests)
            {
                try
                {
                    if(t.Initialize() == InitializeResult.ReadyToGo)
                    {
                        var test = (TestBase)t;
                        TestConsole.WriteLine($"Test '{test.Name}' running...");
                        TestResult result = test.Run(null);

                        string resultString = "Passed";
                        if(( result & TestResult.Fail ) != 0)
                        {
                            resultString = "Failed";
                            testResults = false;
                        }

                        TestConsole.WriteLine("Result: " + resultString);
                    }
                }
                catch
                {
                    Log.Comment("caught exception while running tests");
                }
            }

            TestConsole.WriteLine("All tests complete.");
            TestConsole.WriteLine("");
            TestConsole.WriteLine("*************************");
            TestConsole.WriteLine("* Test Run Result: " + ( testResults ? "PASS" : "FAIL *" ));
            TestConsole.WriteLine("*************************");
        }

        public static void Main( string[] args)
        {
            new TestRunner( 0 ).Run( ); 
        }
    }
}
