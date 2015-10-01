//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

namespace QuickTest
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    class Program
    {
        static void ThreadFunc()
        {
            for(int i=0; i<3; i++)
            {
                Console.WriteLine( "Testing..." );
                Thread.Sleep( 2000 );
            }
        }

        static void SetAccess( FileAccess access )
        {
            if(access != s_access)
            {
                s_access = access;
            }
        }

        static void TestBools( out bool canRead, out bool canWrite )
        {
            if(s_access == FileAccess.Write)
            {
                canRead = true;
                canWrite = false;
            }
            else
            {
                canRead = false;
                canWrite = true;
            }
        }

        static FileAccess s_access = FileAccess.ReadWrite;

        [Serializable, Flags]
        public enum FileAccess
        {
            // Specifies read access to the file. Data can be read from the file and
            // the file pointer can be moved. Combine with WRITE for read-write access.
            Read = 1,

            // Specifies write access to the file. Data can be written to the file and
            // the file pointer can be moved. Combine with READ for read-write access.
            Write = 2,

            // Specifies read and write access to the file. Data can be written to the
            // file and the file pointer can be moved. Data can also be read from the
            // file.
            ReadWrite = 3,
        }

        static void test1()
        {
            bool _canRead, _canWrite;
            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            bool wantsRead = ( s_access & FileAccess.Read ) == FileAccess.Read;
            bool wantsWrite = ( s_access & FileAccess.Write ) == FileAccess.Write;

            TestBools( out _canRead, out _canWrite );

            // Make sure the requests (wantsRead / wantsWrite) matches the filesystem capabilities (canRead / canWrite)
            // ZeligBUG - the second check fails if wantsWrite = true and _canWrite = false
            if(( wantsRead && !_canRead ) || ( wantsWrite && !_canWrite ))
            {
                Console.WriteLine( "PASS" );
            }
            else
            {
                Console.WriteLine( "FAIL" );
            }
        }

        static void Main()
        {
            //Thread th = new Thread( new ThreadStart( ThreadFunc ) );

            //th.Start();

            //GC.Collect();

            //th.Join();

            SetAccess( FileAccess.Write );

            test1();


            Console.WriteLine( "Finished" );
        }
    }
}
