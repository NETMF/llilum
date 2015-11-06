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
            
            tests.Add( new ArraysSimpleTests() );
            tests.Add( new ArraysOtherTests() );
            tests.Add( new BasicConceptTests() );

            foreach(ITestInterface t in tests)
            {
                try
                {
                    if(t.Initialize() == InitializeResult.ReadyToGo)
                    {
                        ((TestBase)t).Run( null ); 
                    }
                }
                catch
                {
                    Log.Comment( "caught exception while running tests" ); 
                }
            }
        }

        public static void Main( string[] args)
        {
            new TestRunner( 0 ).Run( ); 
        }
    }
}
