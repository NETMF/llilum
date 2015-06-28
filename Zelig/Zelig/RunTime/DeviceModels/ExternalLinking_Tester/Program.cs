//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

#define TEST_EXTERNAL_CALL

namespace ExternalLinking_Tester
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    class Tester
    {
        public static int s_intVal;

        //static void ThreadFunc()
        //{
        //    for(int i=0; i<2; i++)
        //    {
        //        Console.WriteLine( "Testing..." );
        //        Thread.Sleep( 2000 );
        //    }
        //}

        static void Main()
        {
            //Thread th = new Thread( new ThreadStart( ThreadFunc ) );

            //th.Start();

            //GC.Collect();

            //th.Join();

            //Console.WriteLine( "Finished" );

            Console.WriteLine("Starting tests");

            s_intVal = 0;

            var obj = new TestObj();

            s_intVal = obj.Add2Return(1, 2);

            obj.Add2SetInstance(3, 4);

            obj.SetStaticOnTester();

            TestObj.TestImportedFunctionCall();

            Console.WriteLine("Tests finished");
        }

    }

    class TestObj
    {
        const string c_ImportLibrary1 = @"zelig_interop.obj";
        const string c_ImportLibrary2 = @"zelig_interop2.obj";

        private int m_intVal;

        [RT.NoInline]
        public TestObj()
        {
            m_intVal = 0;
        }

        [RT.NoInline]
        public int Add2Return(int a, int b)
        {
            int val;

            val = a + b;

            return val;
        }

        [RT.NoInline]
        public void Add2SetInstance(int a, int b)
        {
            int val;

            val = a + b;

            m_intVal = val;
        }

        [RT.NoInline]
        public void SetStaticOnTester()
        {
            Tester.s_intVal = m_intVal;
        }

        [RT.NoInline]
        public static void TestImportedFunctionCall()
        {
            DateTime start = DateTime.Now;

            Console.WriteLine( start.Second.ToString() );

#if TEST_EXTERNAL_CALL
            int i;

            i = ImportedExternalFunction( 3, 7 );

            Console.WriteLine( "Expected 47, Got: " + i.ToString() );

            i = ImportedExternalFunction2( i );

            Console.WriteLine( "Expecting 30, Got: " + i.ToString() );

            ExportStruct es = new ExportStruct();

            double d = DoubleMultiply( 2.354, 76.66 ); // 180.45764

            Console.WriteLine( "Expecting 180.45764, Got: " + d.ToString() );

            string str = "Hello World!";

            char[] strData = str.ToCharArray();

            PassStringArgument( strData, str.Length ); // 

            Console.WriteLine( "Expecting '!dlroW olleH', Got: " + new string(strData) );

            byte[] data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 };

            PassByteArrayArg( data, data.Length ); // 

            str = "";
            for(int j = 0; j < data.Length; j++)
            {
                str += data[j].ToString();
            }
            Console.WriteLine( "Expecting '87654321', Got: " + str );

            PassStructArg( ref es );

            Console.WriteLine( "es.a <123>= " + es.a + " es.b <3.4775>= " + es.b + " es.c <'Z'>= " + es.c );

            i = StringLength( str );

            Console.WriteLine( "String Len <8>: " + i.ToString() );
#endif
            return;
        }
        
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary1 )]
        public static extern int ImportedExternalFunction(int a, int b);

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2, "MyExternalFunction" )]
        public static extern int ImportedExternalFunction2( int a );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2, "Multiply" )]
        public static extern double DoubleMultiply( double a, double b );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2 )]
        public static extern int PassStringArgument( char[] str, int len );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2 )]
        public static extern void PassByteArrayArg( byte[] data, int data_len );
        
        [StructLayout( LayoutKind.Explicit )]
        internal struct ExportStruct
        {
            [FieldOffset( 0 )]
            public int a;
            [FieldOffset( 8 )]
            public double b;
            [FieldOffset( 16 )]
            public char c;
        }
        
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2 )]
        public static extern void PassStructArg( ref ExportStruct pStruct );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_ImportLibrary2 )]
        public static extern int StringLength( string str );
        
        [RT.ExportedMethod]
        public static Int64 ExportedInternalFunction(Int64 a)
        {
            Console.WriteLine("Exported method called");

            return a + a;
        }
        
        [StructLayout( LayoutKind.Explicit )]
        public struct SYSTEMTIME
        {
            [FieldOffset( 0 )]
            public ushort wYear;
            [FieldOffset( 2 )]
            public ushort wMonth;
            [FieldOffset( 4 )]
            public ushort wDayOfWeek;
            [FieldOffset( 6 )]
            public ushort wDay;
            [FieldOffset( 8 )]
            public ushort wHour;
            [FieldOffset( 10 )]
            public ushort wMinute;
            [FieldOffset( 12 )]
            public ushort wSecond;
            [FieldOffset( 14 )]
            public ushort wMilliseconds;
        };

        [RT.ExportedMethod]
        public static int ExportedStructInt64( Int64 time, ref SYSTEMTIME sysTime )
        {
            DateTime dt = DateTime.Now;

            dt = new DateTime( dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, (int)time );

            sysTime.wYear = (ushort)dt.Year;
            sysTime.wMonth = (ushort)dt.Month;
            sysTime.wDay = (ushort)dt.Day;
            sysTime.wDayOfWeek = (ushort)dt.DayOfWeek;
            sysTime.wHour = (ushort)dt.Hour;
            sysTime.wMinute = (ushort)dt.Minute;
            sysTime.wSecond = (ushort)dt.Second;
            sysTime.wMilliseconds = (ushort)dt.Millisecond;

            return 1;
        }
    }
}
