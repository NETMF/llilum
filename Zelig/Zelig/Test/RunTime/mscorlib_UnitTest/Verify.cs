//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

namespace mscorlib_UnitTest
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Reflection;

    public class Debug
    {
        [Microsoft.Zelig.Runtime.NoInline]
        public static void Print( string txt )
        {
            //throw new NotImplementedException();
        }

        public static int AvailableMemory()
        {
            return 0;//throw new NotImplementedException();
        }
    }
}

namespace mscorlib_UnitTest.Verification
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Reflection;

    class Foo { }
    class Bar { }

    class TestReferences
    {
        void Method1( ref int a )
        {
            a = a + 1;
        }

        void Method2( ref object o )
        {
            o = o.GetType();
        }

        void Method3( ref Type t )
        {
            t = t.BaseType;
        }

        void Method4( ref DateTime d )
        {
            d = d.Add( new TimeSpan( 1, 2, 3, 4, 5 ) );
        }

        void Tester()
        {
            int      i   = 13;
            int[]    i_a = new int[10];
            object   o   = "Test";
            object[] o_a = new object[5];
            Type     t   = this.GetType();
            DateTime d   = DateTime.Now;

            Method1( ref i      );
            Method1( ref i_a[2] );

            Method2( ref o      );
            Method2( ref o_a[3] );

            Method3( ref t      );

            Method4( ref d      );
        }
    }

    public class TestReferences2
    {
        static void Test1(     int      v ) { Debug.Print( "Test1: " + v            + " " + v.GetType().FullName );                               }
        static void Test2( ref int      v ) { Debug.Print( "Test2: " + v            + " " + v.GetType().FullName ); v++;                          }
        static void Test3(     Enum     v ) { Debug.Print( "Test3: " + v            + " " + v.GetType().FullName );                               }
        static void Test4( ref Enum     v ) { Debug.Print( "Test4: " + v            + " " + v.GetType().FullName ); v = Resources.Tag.TIME;       }
        static void Test5(     TestEnum v ) { Debug.Print( "Test5: " + v            + " " + v.GetType().FullName );                               }
        static void Test6( ref TestEnum v ) { Debug.Print( "Test6: " + v            + " " + v.GetType().FullName ); v++;                          }
        static void Test7(     DateTime v ) { Debug.Print( "Test7: " + v.ToString() + " " + v.GetType().FullName );                               }
        static void Test8( ref DateTime v ) { Debug.Print( "Test8: " + v.ToString() + " " + v.GetType().FullName ); v += new TimeSpan( 1, 1, 1 ); }
        static void Test9(     object   v ) { Debug.Print( "Test9: " + v.ToString() + " " + v.GetType().FullName );                               }
        static void TestA( ref object   v ) { Debug.Print( "TestA: " + v.ToString() + " " + v.GetType().FullName ); v = 1;                        }

        static public void TestPlain()
        {
            int      i  = 100;
            TestEnum e  = TestEnum.P1;
            Enum     e2 = e;
            DateTime d  = DateTime.Now;

            Test2( ref i  );
            Test1(     i  );
            Test4( ref e2 );
            Test3(     e2 );
            Test6( ref e  );
            Test5(     e  );
            Test8( ref d  );
            Test7(     d  );
        }

        static public void TestArray()
        {
            int     [] i  = new int     [1]; i [0] = 100;
            TestEnum[] e  = new TestEnum[1]; e [0] = TestEnum.P1;
            Enum    [] e2 = new Enum    [1]; e2[0] = e[0];
            DateTime[] d  = new DateTime[1]; d [0] = DateTime.Now;
            object  [] o  = new object  [1]; o [0] = 3.0;

            Test2( ref i [0] );
            Test1(     i [0] );
            Test4( ref e2[0] );
            Test3(     e2[0] );
            Test6( ref e [0] );
            Test5(     e [0] );
            Test8( ref d [0] );
            Test7(     d [0] );
            TestA( ref o [0] );
            Test9(     o [0] );
        }
    }


    public class TestSpecialValueTypes
    {
        static int DateTimeByRef( ref DateTime dt )
        {
            int res = dt.Month;

            dt += new TimeSpan( 0, 1, 2 );

            Debug.Print( "DateTimeByRef " + dt.ToString() );

            return res;
        }

        static public void DateTimeUsage()
        {
            DateTime dt1;
            DateTime dt2 = DateTime.Now;

            int day = dt2.Day;

            dt1 = dt2;

            object o = dt1;

            Debug.Print( "Type " + o.GetType().FullName );

            DateTime[] da1 = new DateTime[2];

            DateTimeByRef( ref da1[0] );
            DateTimeByRef( ref dt1    );

            Debug.Print( "DateTimeUsage " + dt1   .ToString() );
            Debug.Print( "DateTimeUsage " + da1[0].ToString() );
            Debug.Print( "DateTimeUsage " + da1[1].ToString() );
        }
    }

//    [AttributeUsage(AttributeTargets.All,Inherited=true)]
//    public class Local2Attribute : Microsoft.SPOT.LocalAttribute
//    {
//        public Local2Attribute( string name ) : base(name)
//        {
//            this.length = 20;
//        }
//
//        public Local2Attribute( string name, int length ) : base(name)
//        {
//            this.length = length;
//        }
//
//        public string extra;
//    }
//
//    [AttributeUsage(AttributeTargets.All,Inherited=true)]
//    public class Local3Attribute : Attribute
//    {
//        public string info;
//    }
//
//    public enum EnumTester
//    {
//        [Microsoft.SPOT.LocalAttribute("name of Value1", active=true , length=32)] Value1 = 12,
//        [Microsoft.SPOT.LocalAttribute("name of Value2", active=false, length=64)] Value2 = 14,
//        [Local3                           (info="name of Value2"                    )] Value3 = 15,
//    }
//
//    [Microsoft.SPOT.Local("name of AttributeTester|name of AttributeTester|name of AttributeTester|name of AttributeTester|name of AttributeTester|name of AttributeTester", active=false)]
//    public class AttributeTester
//    {
//        [Local2("name of Field1", 66, active=true)]
//        public int Field1;
//
//        [Local2("name of Method1", length=100, target=typeof(Value), extra="we")]
//        public void Method1( int i )
//        {
//        }
//    }

    [StructLayout(LayoutKind.Sequential)]
    struct Value
    {
        public int    i1;
        public int    i2;
        public string s1;

        public Value( int i )
        {
            i1 = i;
            i2 = 0;
            s1 = null;
        }

        public int Result()
        {
            return i1 + i2;
        }
    }

    enum TestEnum :byte
    {
        P1 = 1,
        P2 = 2,
        P3 = 3,
    }

    enum TestEnum2 :short
    {
        S1 = 1,
        S2 = 2,
        S3 = 3,
    }

    public class Resources
    {
        public enum Tag
        {
            TIME,
        }
    }

    delegate int Compute( int a, int b, Value v );

    delegate void OnKey( int a );

    interface Interface
    {
        short Method1( int c );

        int Method2( int a, int b, Value v );
    }

    //--//

////public class InvokeTestBed
////{
////    public enum FooEnum
////    {
////        Blah,
////    }
////
////    public void MethodInt( int t )
////    {
////        Debug.Print( t.GetType().ToString() );
////    }
////
////    public void MethodEnum( FooEnum t )
////    {
////        Debug.Print( t.GetType().ToString() );
////    }
////}

    //--//

    public class FinalizeTest
    {
        protected int m_id;

        public FinalizeTest( int id )
        {
            m_id = id;
        }

        ~FinalizeTest()
        {
            Debug.Print( "~FinalizeTest " + m_id );
            Thread.Sleep( 100 );
        }
    }

    public class FinalizeTest2 : FinalizeTest
    {
        public FinalizeTest m_child;

        public FinalizeTest2( int id ) : base( id )
        {
            m_child = new FinalizeTest( id + 1 );
        }

        ~FinalizeTest2()
        {
            Debug.Print( "~FinalizeTest2 " + m_id );

            if(m_id == 4 && m_child != null)
            {
                System.GC.SuppressFinalize( m_child );
            }
        }
    }

    //--//

    class Verify : Interface
    {
        [Serializable]
        public struct DayState
        {
            //If the temperature is unknown, it is sent down as sbyte.MinValue (-128)

            public sbyte m_high;
            public sbyte m_low;

            public DayState( sbyte high, sbyte low )
            {
                m_high = high;
                m_low  = low;
            }
        }

        public class Sub1
        {
            public interface SubInterface
            {
                short Method1( int c );
            }

            public class Sub2 : SubInterface
            {
                short Verify.Sub1.SubInterface.Method1( int c ) { return 1; }
            }
        }

        //static int    s_i = 1;
        //static string s_s = "test static";
        static object s_o = new Object();

        OnKey m_onKey;

        public void Thread_Start1()
        {
            const int limit = 100000;

            for(int i=1000000; i<1000000+limit;i++)
            {
            }
        }

        public short Method1( int c )
        {
            return (short)(c * 10);
        }

        public int Method2( int a, int b, Value v )
        {
            Thread.Sleep( 50 );

            return a + b + v.i1;
        }

        public int Method2( int a, int b, Value v, DateTime dt )
        {
            Thread.Sleep( 50 );

            return a + b + v.i1;
        }

        static public int Method2_b( int a, int b, Value v )
        {
            return a + b * v.i1;
        }

        static public int Method3( int a, out int b, ref int c )
        {
            b = a + c;

            c = a * 2;

            return a + b + c;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public int Method4( params int[] a )
        {
            return a.Length;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static public int Method4s( params int[] a )
        {
            return a.Length;
        }

        public void Method5( TestEnum e )
        {
            switch(e)
            {
                case TestEnum.P1: Debug.Print( "P1" ); break;
                case TestEnum.P2: Debug.Print( "P2" ); break;
                case TestEnum.P3: Debug.Print( "P3" ); break;
            }
        }

        public void Method6( Enum e )
        {
            Debug.Print( "Enum: " + e.GetType().FullName );

            Method6( ref e );
        }

        public void Method6( object state )
        {
            Debug.Print( "Timer: " + DateTime.Now.ToString() + " " + state.GetType().FullName );
        }

        public void Method6( ref Enum e )
        {
            Debug.Print( "EnumByRef: " + e.GetType().FullName );
        }

        public void TestEvent1( int a )
        {
            Debug.Print( "TestEvent1: " + a );
        }

        static public void TestEvent2( int a )
        {
            Debug.Print( "TestEvent2: " + a );
        }

        static public void TestEvent3( int a )
        {
            Debug.Print( "TestEvent3: " + a );
        }

        //--//

        private void TestNumeric()
        {
            int   i;
            int   s1 =  1242323;

            long   l1 =  1298182;
            long   l2 = -1242323;
            long[] l3 = new long[5];

            ulong   ul1 = 1298182;
            ulong   ul2 = 1242323;
            ulong[] ul3 = new ulong[5];
            long[]  sl3 = new long[5];
            double[] fl3 = new double[4];

            ul3[0] = ul1 + ul2;
            ul3[1] = ul1 - ul2;
            ul3[2] = ul1 * ul2;
            ul3[3] = ul1 / ul2;
            ul3[4] = ul1 % ul2;
            for(i=0; i<ul3.Length; i++) Debug.Print( "UL " + ul3[i] );

            l3[0] = l1 + l2;
            l3[1] = l1 - l2;
            l3[2] = l1 * l2;
            l3[3] = l1 / l2;
            l3[4] = l1 % l2;
            for(i=0; i<l3.Length; i++) Debug.Print( "L " + l3[i] );

            sl3[0] = s1 + l2;
            sl3[1] = s1 - l2;
            sl3[2] = s1 * l2;
            sl3[3] = s1 / l2;
            sl3[4] = s1 % l2;
            for(i=0; i<sl3.Length; i++) Debug.Print( "SL " + sl3[i] );

            fl3[0] = (double)s1 + (double)l2;
            fl3[1] = (double)s1 - (double)l2;
            fl3[2] = (double)s1 * (double)l2;
            fl3[3] = (double)s1 / (double)l2;
            for(i=0; i<fl3.Length; i++) Debug.Print( "FL " + fl3[i] );

            float  fl1 = -2f ; Debug.Print( "FLT " + fl1 );
            double db1 = -0.5; Debug.Print( "DBL " + db1 );

            Debug.Print( "FLT MUL " + (fl1 * db1) );

            uint size1 = (uint)Marshal.SizeOf( typeof(Value) );
            uint size3 = sizeof(TestEnum);

            size1 += size3;
        }

        private void TestNumericConversion()
        {
            byte   u1a = byte.MaxValue;
            byte   u1b = byte.MinValue;

            sbyte  i1a = sbyte.MaxValue;
            sbyte  i1b = sbyte.MinValue;

            ushort u2a = ushort.MaxValue;
            ushort u2b = ushort.MinValue;

            short  i2a = short.MaxValue;
            short  i2b = short.MinValue;

            uint   u4a = uint.MaxValue;
            uint   u4b = uint.MinValue;

            int    i4a = int.MaxValue;
            int    i4b = int.MinValue;

            ulong  u8a = ulong.MaxValue;
            ulong  u8b = ulong.MinValue;

            long   i8a = long.MaxValue;
            long   i8b = long.MinValue;

            float  r4a =  2097151;
            float  r4b = -2097152;

            double r8a =  2097151;
            double r8b = -2097152;

            byte   u1a_c;
            byte   u1b_c;

            sbyte  i1a_c;
            sbyte  i1b_c;

            ushort u2a_c;
            ushort u2b_c;

            short  i2a_c;
            short  i2b_c;

            uint   u4a_c;
            uint   u4b_c;

            int    i4a_c;
            int    i4b_c;

            ulong  u8a_c;
            ulong  u8b_c;

            long   i8a_c;
            long   i8b_c;

            float  r4a_c;
            float  r4b_c;

            double r8a_c;
            double r8b_c;


            Debug.Print( "byte   u1a = " + u1a );
            Debug.Print( "byte   u1b = " + u1b );
            Debug.Print( "sbyte  i1a = " + i1a );
            Debug.Print( "sbyte  i1b = " + i1b );
            Debug.Print( "ushort u2a = " + u2a );
            Debug.Print( "ushort u2b = " + u2b );
            Debug.Print( "short  i2a = " + i2a );
            Debug.Print( "short  i2b = " + i2b );
            Debug.Print( "uint   u4a = " + u4a );
            Debug.Print( "uint   u4b = " + u4b );
            Debug.Print( "int    i4a = " + i4a );
            Debug.Print( "int    i4b = " + i4b );
            Debug.Print( "ulong  u8a = " + u8a );
            Debug.Print( "ulong  u8b = " + u8b );
            Debug.Print( "long   i8a = " + i8a );
            Debug.Print( "long   i8b = " + i8b );
            Debug.Print( "float  r4a = " + r4a );
            Debug.Print( "float  r4b = " + r4b );
            Debug.Print( "double r8a = " + r8a );
            Debug.Print( "double r8b = " + r8b );
            Debug.Print( ""                    );

            u1a_c = (byte  )u1a; Debug.Print( "(byte  )u1a = " + u1a_c );
            u1b_c = (byte  )u1b; Debug.Print( "(byte  )u1b = " + u1b_c );
            i1a_c = (sbyte )u1a; Debug.Print( "(sbyte )u1a = " + i1a_c );
            i1b_c = (sbyte )u1b; Debug.Print( "(sbyte )u1b = " + i1b_c );
            u2a_c = (ushort)u1a; Debug.Print( "(ushort)u1a = " + u2a_c );
            u2b_c = (ushort)u1b; Debug.Print( "(ushort)u1b = " + u2b_c );
            i2a_c = (short )u1a; Debug.Print( "(short )u1a = " + i2a_c );
            i2b_c = (short )u1b; Debug.Print( "(short )u1b = " + i2b_c );
            u4a_c = (uint  )u1a; Debug.Print( "(uint  )u1a = " + u4a_c );
            u4b_c = (uint  )u1b; Debug.Print( "(uint  )u1b = " + u4b_c );
            i4a_c = (int   )u1a; Debug.Print( "(int   )u1a = " + i4a_c );
            i4b_c = (int   )u1b; Debug.Print( "(int   )u1b = " + i4b_c );
            u8a_c = (ulong )u1a; Debug.Print( "(ulong )u1a = " + u8a_c );
            u8b_c = (ulong )u1b; Debug.Print( "(ulong )u1b = " + u8b_c );
            i8a_c = (long  )u1a; Debug.Print( "(long  )u1a = " + i8a_c );
            i8b_c = (long  )u1b; Debug.Print( "(long  )u1b = " + i8b_c );
            r4a_c = (float )u1a; Debug.Print( "(float )u1a = " + r4a_c );
            r4b_c = (float )u1b; Debug.Print( "(float )u1b = " + r4b_c );
            r8a_c = (double)u1a; Debug.Print( "(double)u1a = " + r8a_c );
            r8b_c = (double)u1b; Debug.Print( "(double)u1b = " + r8b_c );
            Debug.Print( "" );

            u1a_c =(byte  )i1a ; Debug.Print( "(byte  )i1a = " + u1a_c );
            u1b_c =(byte  )i1b ; Debug.Print( "(byte  )i1b = " + u1b_c );
            i1a_c =(sbyte )i1a ; Debug.Print( "(sbyte )i1a = " + i1a_c );
            i1b_c =(sbyte )i1b ; Debug.Print( "(sbyte )i1b = " + i1b_c );
            u2a_c =(ushort)i1a ; Debug.Print( "(ushort)i1a = " + u2a_c );
            u2b_c =(ushort)i1b ; Debug.Print( "(ushort)i1b = " + u2b_c );
            i2a_c =(short )i1a ; Debug.Print( "(short )i1a = " + i2a_c );
            i2b_c =(short )i1b ; Debug.Print( "(short )i1b = " + i2b_c );
            u4a_c =(uint  )i1a ; Debug.Print( "(uint  )i1a = " + u4a_c );
            u4b_c =(uint  )i1b ; Debug.Print( "(uint  )i1b = " + u4b_c );
            i4a_c =(int   )i1a ; Debug.Print( "(int   )i1a = " + i4a_c );
            i4b_c =(int   )i1b ; Debug.Print( "(int   )i1b = " + i4b_c );
            u8a_c =(ulong )i1a ; Debug.Print( "(ulong )i1a = " + u8a_c );
            u8b_c =(ulong )i1b ; Debug.Print( "(ulong )i1b = " + u8b_c );
            i8a_c =(long  )i1a ; Debug.Print( "(long  )i1a = " + i8a_c );
            i8b_c =(long  )i1b ; Debug.Print( "(long  )i1b = " + i8b_c );
            r4a_c =(float )i1a ; Debug.Print( "(float )i1a = " + r4a_c );
            r4b_c =(float )i1b ; Debug.Print( "(float )i1b = " + r4b_c );
            r8a_c =(double)i1a ; Debug.Print( "(double)i1a = " + r8a_c );
            r8b_c =(double)i1b ; Debug.Print( "(double)i1b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )u2a; Debug.Print( "(byte  )u2a = " + u1a_c );
            u1b_c = (byte  )u2b; Debug.Print( "(byte  )u2b = " + u1b_c );
            i1a_c = (sbyte )u2a; Debug.Print( "(sbyte )u2a = " + i1a_c );
            i1b_c = (sbyte )u2b; Debug.Print( "(sbyte )u2b = " + i1b_c );
            u2a_c = (ushort)u2a; Debug.Print( "(ushort)u2a = " + u2a_c );
            u2b_c = (ushort)u2b; Debug.Print( "(ushort)u2b = " + u2b_c );
            i2a_c = (short )u2a; Debug.Print( "(short )u2a = " + i2a_c );
            i2b_c = (short )u2b; Debug.Print( "(short )u2b = " + i2b_c );
            u4a_c = (uint  )u2a; Debug.Print( "(uint  )u2a = " + u4a_c );
            u4b_c = (uint  )u2b; Debug.Print( "(uint  )u2b = " + u4b_c );
            i4a_c = (int   )u2a; Debug.Print( "(int   )u2a = " + i4a_c );
            i4b_c = (int   )u2b; Debug.Print( "(int   )u2b = " + i4b_c );
            u8a_c = (ulong )u2a; Debug.Print( "(ulong )u2a = " + u8a_c );
            u8b_c = (ulong )u2b; Debug.Print( "(ulong )u2b = " + u8b_c );
            i8a_c = (long  )u2a; Debug.Print( "(long  )u2a = " + i8a_c );
            i8b_c = (long  )u2b; Debug.Print( "(long  )u2b = " + i8b_c );
            r4a_c = (float )u2a; Debug.Print( "(float )u2a = " + r4a_c );
            r4b_c = (float )u2b; Debug.Print( "(float )u2b = " + r4b_c );
            r8a_c = (double)u2a; Debug.Print( "(double)u2a = " + r8a_c );
            r8b_c = (double)u2b; Debug.Print( "(double)u2b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )i2a; Debug.Print( "(byte  )i2a = " + u1a_c );
            u1b_c = (byte  )i2b; Debug.Print( "(byte  )i2b = " + u1b_c );
            i1a_c = (sbyte )i2a; Debug.Print( "(sbyte )i2a = " + i1a_c );
            i1b_c = (sbyte )i2b; Debug.Print( "(sbyte )i2b = " + i1b_c );
            u2a_c = (ushort)i2a; Debug.Print( "(ushort)i2a = " + u2a_c );
            u2b_c = (ushort)i2b; Debug.Print( "(ushort)i2b = " + u2b_c );
            i2a_c = (short )i2a; Debug.Print( "(short )i2a = " + i2a_c );
            i2b_c = (short )i2b; Debug.Print( "(short )i2b = " + i2b_c );
            u4a_c = (uint  )i2a; Debug.Print( "(uint  )i2a = " + u4a_c );
            u4b_c = (uint  )i2b; Debug.Print( "(uint  )i2b = " + u4b_c );
            i4a_c = (int   )i2a; Debug.Print( "(int   )i2a = " + i4a_c );
            i4b_c = (int   )i2b; Debug.Print( "(int   )i2b = " + i4b_c );
            u8a_c = (ulong )i2a; Debug.Print( "(ulong )i2a = " + u8a_c );
            u8b_c = (ulong )i2b; Debug.Print( "(ulong )i2b = " + u8b_c );
            i8a_c = (long  )i2a; Debug.Print( "(long  )i2a = " + i8a_c );
            i8b_c = (long  )i2b; Debug.Print( "(long  )i2b = " + i8b_c );
            r4a_c = (float )i2a; Debug.Print( "(float )i2a = " + r4a_c );
            r4b_c = (float )i2b; Debug.Print( "(float )i2b = " + r4b_c );
            r8a_c = (double)i2a; Debug.Print( "(double)i2a = " + r8a_c );
            r8b_c = (double)i2b; Debug.Print( "(double)i2b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )u4a; Debug.Print( "(byte  )u4a = " + u1a_c );
            u1b_c = (byte  )u4b; Debug.Print( "(byte  )u4b = " + u1b_c );
            i1a_c = (sbyte )u4a; Debug.Print( "(sbyte )u4a = " + i1a_c );
            i1b_c = (sbyte )u4b; Debug.Print( "(sbyte )u4b = " + i1b_c );
            u2a_c = (ushort)u4a; Debug.Print( "(ushort)u4a = " + u2a_c );
            u2b_c = (ushort)u4b; Debug.Print( "(ushort)u4b = " + u2b_c );
            i2a_c = (short )u4a; Debug.Print( "(short )u4a = " + i2a_c );
            i2b_c = (short )u4b; Debug.Print( "(short )u4b = " + i2b_c );
            u4a_c = (uint  )u4a; Debug.Print( "(uint  )u4a = " + u4a_c );
            u4b_c = (uint  )u4b; Debug.Print( "(uint  )u4b = " + u4b_c );
            i4a_c = (int   )u4a; Debug.Print( "(int   )u4a = " + i4a_c );
            i4b_c = (int   )u4b; Debug.Print( "(int   )u4b = " + i4b_c );
            u8a_c = (ulong )u4a; Debug.Print( "(ulong )u4a = " + u8a_c );
            u8b_c = (ulong )u4b; Debug.Print( "(ulong )u4b = " + u8b_c );
            i8a_c = (long  )u4a; Debug.Print( "(long  )u4a = " + i8a_c );
            i8b_c = (long  )u4b; Debug.Print( "(long  )u4b = " + i8b_c );
            r4a_c = (float )u4a; Debug.Print( "(float )u4a = " + r4a_c );
            r4b_c = (float )u4b; Debug.Print( "(float )u4b = " + r4b_c );
            r8a_c = (double)u4a; Debug.Print( "(double)u4a = " + r8a_c );
            r8b_c = (double)u4b; Debug.Print( "(double)u4b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )i4a; Debug.Print( "(byte  )i4a = " + u1a_c );
            u1b_c = (byte  )i4b; Debug.Print( "(byte  )i4b = " + u1b_c );
            i1a_c = (sbyte )i4a; Debug.Print( "(sbyte )i4a = " + i1a_c );
            i1b_c = (sbyte )i4b; Debug.Print( "(sbyte )i4b = " + i1b_c );
            u2a_c = (ushort)i4a; Debug.Print( "(ushort)i4a = " + u2a_c );
            u2b_c = (ushort)i4b; Debug.Print( "(ushort)i4b = " + u2b_c );
            i2a_c = (short )i4a; Debug.Print( "(short )i4a = " + i2a_c );
            i2b_c = (short )i4b; Debug.Print( "(short )i4b = " + i2b_c );
            u4a_c = (uint  )i4a; Debug.Print( "(uint  )i4a = " + u4a_c );
            u4b_c = (uint  )i4b; Debug.Print( "(uint  )i4b = " + u4b_c );
            i4a_c = (int   )i4a; Debug.Print( "(int   )i4a = " + i4a_c );
            i4b_c = (int   )i4b; Debug.Print( "(int   )i4b = " + i4b_c );
            u8a_c = (ulong )i4a; Debug.Print( "(ulong )i4a = " + u8a_c );
            u8b_c = (ulong )i4b; Debug.Print( "(ulong )i4b = " + u8b_c );
            i8a_c = (long  )i4a; Debug.Print( "(long  )i4a = " + i8a_c );
            i8b_c = (long  )i4b; Debug.Print( "(long  )i4b = " + i8b_c );
            r4a_c = (float )i4a; Debug.Print( "(float )i4a = " + r4a_c );
            r4b_c = (float )i4b; Debug.Print( "(float )i4b = " + r4b_c );
            r8a_c = (double)i4a; Debug.Print( "(double)i4a = " + r8a_c );
            r8b_c = (double)i4b; Debug.Print( "(double)i4b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )u8a; Debug.Print( "(byte  )u8a = " + u1a_c );
            u1b_c = (byte  )u8b; Debug.Print( "(byte  )u8b = " + u1b_c );
            i1a_c = (sbyte )u8a; Debug.Print( "(sbyte )u8a = " + i1a_c );
            i1b_c = (sbyte )u8b; Debug.Print( "(sbyte )u8b = " + i1b_c );
            u2a_c = (ushort)u8a; Debug.Print( "(ushort)u8a = " + u2a_c );
            u2b_c = (ushort)u8b; Debug.Print( "(ushort)u8b = " + u2b_c );
            i2a_c = (short )u8a; Debug.Print( "(short )u8a = " + i2a_c );
            i2b_c = (short )u8b; Debug.Print( "(short )u8b = " + i2b_c );
            u4a_c = (uint  )u8a; Debug.Print( "(uint  )u8a = " + u4a_c );
            u4b_c = (uint  )u8b; Debug.Print( "(uint  )u8b = " + u4b_c );
            i4a_c = (int   )u8a; Debug.Print( "(int   )u8a = " + i4a_c );
            i4b_c = (int   )u8b; Debug.Print( "(int   )u8b = " + i4b_c );
            u8a_c = (ulong )u8a; Debug.Print( "(ulong )u8a = " + u8a_c );
            u8b_c = (ulong )u8b; Debug.Print( "(ulong )u8b = " + u8b_c );
            i8a_c = (long  )u8a; Debug.Print( "(long  )u8a = " + i8a_c );
            i8b_c = (long  )u8b; Debug.Print( "(long  )u8b = " + i8b_c );
            r4a_c = (float )u8a; Debug.Print( "(float )u8a = " + r4a_c );
            r4b_c = (float )u8b; Debug.Print( "(float )u8b = " + r4b_c );
            r8a_c = (double)u8a; Debug.Print( "(double)u8a = " + r8a_c );
            r8b_c = (double)u8b; Debug.Print( "(double)u8b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )i8a; Debug.Print( "(byte  )i8a = " + u1a_c );
            u1b_c = (byte  )i8b; Debug.Print( "(byte  )i8b = " + u1b_c );
            i1a_c = (sbyte )i8a; Debug.Print( "(sbyte )i8a = " + i1a_c );
            i1b_c = (sbyte )i8b; Debug.Print( "(sbyte )i8b = " + i1b_c );
            u2a_c = (ushort)i8a; Debug.Print( "(ushort)i8a = " + u2a_c );
            u2b_c = (ushort)i8b; Debug.Print( "(ushort)i8b = " + u2b_c );
            i2a_c = (short )i8a; Debug.Print( "(short )i8a = " + i2a_c );
            i2b_c = (short )i8b; Debug.Print( "(short )i8b = " + i2b_c );
            u4a_c = (uint  )i8a; Debug.Print( "(uint  )i8a = " + u4a_c );
            u4b_c = (uint  )i8b; Debug.Print( "(uint  )i8b = " + u4b_c );
            i4a_c = (int   )i8a; Debug.Print( "(int   )i8a = " + i4a_c );
            i4b_c = (int   )i8b; Debug.Print( "(int   )i8b = " + i4b_c );
            u8a_c = (ulong )i8a; Debug.Print( "(ulong )i8a = " + u8a_c );
            u8b_c = (ulong )i8b; Debug.Print( "(ulong )i8b = " + u8b_c );
            i8a_c = (long  )i8a; Debug.Print( "(long  )i8a = " + i8a_c );
            i8b_c = (long  )i8b; Debug.Print( "(long  )i8b = " + i8b_c );
            r4a_c = (float )i8a; Debug.Print( "(float )i8a = " + r4a_c );
            r4b_c = (float )i8b; Debug.Print( "(float )i8b = " + r4b_c );
            r8a_c = (double)i8a; Debug.Print( "(double)i8a = " + r8a_c );
            r8b_c = (double)i8b; Debug.Print( "(double)i8b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )r4a; Debug.Print( "(byte  )r4a = " + u1a_c );
            u1b_c = (byte  )r4b; Debug.Print( "(byte  )r4b = " + u1b_c );
            i1a_c = (sbyte )r4a; Debug.Print( "(sbyte )r4a = " + i1a_c );
            i1b_c = (sbyte )r4b; Debug.Print( "(sbyte )r4b = " + i1b_c );
            u2a_c = (ushort)r4a; Debug.Print( "(ushort)r4a = " + u2a_c );
            u2b_c = (ushort)r4b; Debug.Print( "(ushort)r4b = " + u2b_c );
            i2a_c = (short )r4a; Debug.Print( "(short )r4a = " + i2a_c );
            i2b_c = (short )r4b; Debug.Print( "(short )r4b = " + i2b_c );
            u4a_c = (uint  )r4a; Debug.Print( "(uint  )r4a = " + u4a_c );
            u4b_c = (uint  )r4b; Debug.Print( "(uint  )r4b = " + u4b_c );
            i4a_c = (int   )r4a; Debug.Print( "(int   )r4a = " + i4a_c );
            i4b_c = (int   )r4b; Debug.Print( "(int   )r4b = " + i4b_c );
            u8a_c = (ulong )r4a; Debug.Print( "(ulong )r4a = " + u8a_c );
            u8b_c = (ulong )r4b; Debug.Print( "(ulong )r4b = " + u8b_c );
            i8a_c = (long  )r4a; Debug.Print( "(long  )r4a = " + i8a_c );
            i8b_c = (long  )r4b; Debug.Print( "(long  )r4b = " + i8b_c );
            r4a_c = (float )r4a; Debug.Print( "(float )r4a = " + r4a_c );
            r4b_c = (float )r4b; Debug.Print( "(float )r4b = " + r4b_c );
            r8a_c = (double)r4a; Debug.Print( "(double)r4a = " + r8a_c );
            r8b_c = (double)r4b; Debug.Print( "(double)r4b = " + r8b_c );
            Debug.Print( "" );

            u1a_c = (byte  )r8a; Debug.Print( "(byte  )r8a = " + u1a_c );
            u1b_c = (byte  )r8b; Debug.Print( "(byte  )r8b = " + u1b_c );
            i1a_c = (sbyte )r8a; Debug.Print( "(sbyte )r8a = " + i1a_c );
            i1b_c = (sbyte )r8b; Debug.Print( "(sbyte )r8b = " + i1b_c );
            u2a_c = (ushort)r8a; Debug.Print( "(ushort)r8a = " + u2a_c );
            u2b_c = (ushort)r8b; Debug.Print( "(ushort)r8b = " + u2b_c );
            i2a_c = (short )r8a; Debug.Print( "(short )r8a = " + i2a_c );
            i2b_c = (short )r8b; Debug.Print( "(short )r8b = " + i2b_c );
            u4a_c = (uint  )r8a; Debug.Print( "(uint  )r8a = " + u4a_c );
            u4b_c = (uint  )r8b; Debug.Print( "(uint  )r8b = " + u4b_c );
            i4a_c = (int   )r8a; Debug.Print( "(int   )r8a = " + i4a_c );
            i4b_c = (int   )r8b; Debug.Print( "(int   )r8b = " + i4b_c );
            u8a_c = (ulong )r8a; Debug.Print( "(ulong )r8a = " + u8a_c );
            u8b_c = (ulong )r8b; Debug.Print( "(ulong )r8b = " + u8b_c );
            i8a_c = (long  )r8a; Debug.Print( "(long  )r8a = " + i8a_c );
            i8b_c = (long  )r8b; Debug.Print( "(long  )r8b = " + i8b_c );
            r4a_c = (float )r8a; Debug.Print( "(float )r8a = " + r4a_c );
            r4b_c = (float )r8b; Debug.Print( "(float )r8b = " + r4b_c );
            r8a_c = (double)r8a; Debug.Print( "(double)r8a = " + r8a_c );
            r8b_c = (double)r8b; Debug.Print( "(double)r8b = " + r8b_c );
            Debug.Print( "" );
        }

        private void TestImplicitPromotion_Print( short a )
        {
            Debug.Print( a.ToString() );
        }

        private void TestImplicitPromotion()
        {
            int x   = -(256*256 + 34);
            sbyte y = (sbyte)x;
            short z =        y;

            Debug.Print( "x = " + x );
            Debug.Print( "y = " + y );
            Debug.Print( "z = " + z );

            TestImplicitPromotion_Print( y );
        }

        private void TestByref()
        {
            int   a = 1;
            int   b = 2;
            int[] c = new int[2]; c[1] = 4;

            int d = Method3( a, out b, ref c[1] );

            int e = Method4 ( a, 4, 1 );
            int f = Method4s( a, 4, 1 );

            TestReferences2.TestPlain();
            TestReferences2.TestArray();
        }

        private void TestEnums()
        {
            TestEnum e1 = TestEnum.P1;

            this.Method5( e1           );
            this.Method6( TestEnum.P2  );
            this.Method6( TestEnum2.S3 );
        }

        private void Fail( int i )
        {
            Debug.Print( "Fail " + i );
        }

        private void Pass( int i )
        {
            Debug.Print( "Pass " + i );
        }

        private void TestCasts()
        {
            String[][]   s  = new String[1][];
            DateTime[]   d  = new DateTime[1];
            object[]     ao = new object[1];
            int[]        ai = new int   [1];
            System.Array ar = null;

            try { ar = (System.Array)(object)d; Pass( 1 ); } catch { Fail( 1 ); }
            try { ao = (object[])(object)d;     Fail( 2 ); } catch { Pass( 2 ); }
            try { s  = (string[][])ao;          Fail( 3 ); } catch { Pass( 3 ); }

            ar = ao;

            try { Array.Copy( ar, 0, ao, 0, 1 ); Pass( 4 ); } catch { Fail( 4 ); }
            try { Array.Copy( ao, 0, ai, 0, 1 ); Fail( 5 ); } catch { Pass( 5 ); }
            try { Array.Copy( ar, 0, ai, 0, 1 ); Fail( 6 ); } catch { Pass( 6 ); }

            try { ao = (object[])ar; Pass( 7 ); } catch { Fail( 7 ); }
            try { ai = (int   [])ar; Fail( 8 ); } catch { Pass( 8 ); }
        }

        private void TestLockAndSleep()
        {
            int a1 = 1;
            int b2 = 2;
            int c3 = this.Method1( b2 );

            lock(typeof(System.Int32))
            {
                int j = a1 * b2;
            }

            lock(s_o)
            {
                int   a  = 1;
                int   b  = 2;
                Value v1 = new Value( 1 );
                DateTime dt = new DateTime( 2002, 10, 1 );

                for(int i=0; i<10; i++)
                {
                    this.Method2( a, b, v1, dt );
                }
            }
        }

        private void TestStrings()
        {
            String s1 = "test";
            String s2 = "test2\u0234\u0740";
            int    i;

            if(s1 == s2)
            {
                i = 1;
            }

            String s3 = s1 + s2;

            Debug.Print( s3 );

            if(s3 == "testtest2")
            {
                i = 2;
            }

            switch(s1)
            {
                case "test":
                    Debug.Print( "switch : test" );
                    break;

                case "test2":
                    Debug.Print( "switch : test2" );
                    break;
            }

            //--//

            for(i=0; i<s2.Length; i++)
            {
                Debug.Print( "Char : " + i + " " + s2[i] );
            }

            //--//

            char[] ca = s2.ToCharArray( 2, 4 );
            foreach(char cac in ca)
            {
                Debug.Print( "ToCharArray : " + cac );
            }

            //--//

            s2 = "test1 test2\u0234\u0740 test3 test4\u0740";
            string[] s2a = s2.Split( '\u0740', ' ' );
            foreach(string s2b in s2a)
            {
                Debug.Print( "Split : " + s2b );
            }

            //--//

            Debug.Print( "Substring : " + s2.Substring( 2     ) );
            Debug.Print( "Substring : " + s2.Substring( 2, 10 ) );

            //--//

            s2 = "    white space   ";

            Debug.Print( "Trim : >" + s2.Trim     () + "<" );
            Debug.Print( "Trim : >" + s2.TrimEnd  () + "<" );
            Debug.Print( "Trim : >" + s2.TrimStart() + "<" );

            s2 = "!@#    white space   !@#";

            Debug.Print( "Trim : >" + s2.Trim     ( '!', '@', '#' ) + "<" );
            Debug.Print( "Trim : >" + s2.TrimEnd  ( '!', '@', '#' ) + "<" );
            Debug.Print( "Trim : >" + s2.TrimStart( '!', '@', '#' ) + "<" );

            Debug.Print( "IndexOf: " + s2.IndexOf( 'w'        ) );
            Debug.Print( "IndexOf: " + s2.IndexOf( 'w', 4     ) );
            Debug.Print( "IndexOf: " + s2.IndexOf( 'w', 10    ) );
            Debug.Print( "IndexOf: " + s2.IndexOf( 'w', 4, 7  ) );
            Debug.Print( "IndexOf: " + s2.IndexOf( 'w', 4, 3  ) );

            Debug.Print( "IndexOfAny : " + s2.IndexOfAny ( new char[] { 'a', 'd' } ) );
            Debug.Print( "IndexOf    : " + s2.IndexOf    ( "spa"                   ) );
            Debug.Print( "LastIndexOf: " + s2.LastIndexOf( ' '                     ) );

            Debug.Print( "IndexOf: " + "Multiple\nLines\nTest".IndexOf( '\n' ) );
            Debug.Print( "ToUpper: " + "Multiple Cases Test".ToUpper() );
            Debug.Print( "ToLower: " + "Multiple Cases Test".ToLower() );

            s2 = new string( '#', 80 );
            Debug.Print( ".ctor : >" + s2 + "<" );

            ca = new char[4];
            ca[0] = 'a';
            ca[1] = 'b';
            ca[2] = 'c';
            ca[3] = 'd';

            s2 = new string( ca );
            Debug.Print( ".ctor : >" + s2 + "<" );

            s2 = new string( ca, 1, 2 );
            Debug.Print( ".ctor : >" + s2 + "<" );
        }

        private void TestValueTypes()
        {
            Interface itf = this;
            Value     v1  = new Value( 1 );
            Value     v2;

            v1.i1 = 2;
            v2    = v1;

            v2.i1 = itf.Method1( 1        );
            v2.i2 = itf.Method2( 1, 2, v1 );
        }


        private int TestDayState( ref DayState a )
        {
            a.m_low *= 2;

            return a.m_high;
        }

        private void TestValueTypeArrayReferences()
        {
            DayState[] nat_cities = new DayState[2];
            int        i          = 1;

            nat_cities[i] = new DayState( 50, 37 );

            DayState d = nat_cities[i];

            Debug.Print( "High assign = " + d            .m_high );
            Debug.Print( "High direct = " + nat_cities[i].m_high );

            d.m_low = 10;
            TestDayState( ref d );
            Debug.Print( "Low assign = " + d.m_low );

            nat_cities[i].m_low = 15;
            TestDayState( ref nat_cities[i] );
            Debug.Print( "Low direct = " + nat_cities[i].m_low );
        }

        private int TestBoxing()
        {
            Value v1 = new Value( 1 );
            int   i;

            try
            {
                object o1 = v1;
                object o2 = 3;

                o1.Equals( new Value() );

                Value v3 = (Value)o1;
                int   i3 = (int)o2;

                Debug.Print( "Int: " + i3    );
                Debug.Print( "Val: " + v3.i1 );
            }
            catch(Exception e)
            {
                Debug.Print( "Exception: " + e );

                i = 1;
            }
            finally
            {
                i = 2;
            }

            return i;
        }


        private void TestExceptions()
        {
            try
            {
                try
                {
                    Debug.Print( "Exception: 1" );
                }
                finally
                {
                    Debug.Print( "Exception: 2" );

                    try
                    {
                        Exception e = new Exception( "test" );

                        throw e;
                    }
                    catch
                    {
                        Debug.Print( "Exception: 2b" );

                        throw;
                    }
                    finally
                    {
                        Debug.Print( "Exception: 3" );
                    }
                }
            }
            catch(Exception)
            {
                Debug.Print( "Exception: 4" );
            }
            finally
            {
                Debug.Print( "Exception: 5" );
            }


            //
            // Nesting test
            //
            try
            {
                try
                {
                    Debug.Print( "Exception: 1" );
                }
                catch(Exception)
                {
                    Debug.Print( "Exception: 3" );
                }
                finally
                {
                    Debug.Print( "Exception: 4" );
                }

                try
                {
                    Debug.Print( "Exception: 5" );
                    throw new Exception();
                }
                catch(Exception)
                {
                    Debug.Print( "Exception: 7" );
                }
                finally
                {
                    Debug.Print( "Exception: 8" );
                    throw new Exception();
                }
            }
            catch(Exception)
            {
                try
                {
                    Debug.Print( "Exception: 10" );
                }
                catch
                {
                    Debug.Print( "Exception: 11" );
                }
            }
            finally
            {
                Debug.Print( "Exception: 12" );
            }
        }

        private void TestArrays()
        {
            object[] ar = new Object[20];
            int      a  = 1;
            int      c  = 4;

            //////////////////////////////////////////////////////////////////////////////////////
            //
            // Array tests.
            //
            ar[0] = this;

            string[] ars = new string[10];

            ars[0] = "test";
            ars[2] = "test2";

            foreach(string s in ars)
            {
                int len = s != null ? s.Length : 0;
            }

            object[] ar2 = new object[5];

            System.Array.Copy( ar, 0, ar2, 2, 3 );
            Debug.Print( "System.Array.Copy: " + (ar2[2] != null) );

            Debug.Print( "System.Array.Clear: " + (ar[0] != null) );
            System.Array.Clear( ar, 0, ar.Length );
            Debug.Print( "System.Array.Clear: " + (ar[0] != null) );

            try
            {
                string[]   ar3 = new string[10];
                object[]   ar4 = ar3;
                DateTime[] ar5 = new DateTime[2];

                ar[0] = this;
                System.Array.Copy( ar, ar4, 10 );

                Debug.Print( "System.Array.Copy: Incompatible types: FAIL" );
            }
            catch
            {
                Debug.Print( "System.Array.Copy: Incompatible types: SUCCESS" );
            }

            //////////////////////////////////////////////////////////////////////////////////////
            //
            // More array tests and Exception tests.
            //
            string s1 = a.ToString();

            ar[1] = a;

            c = (int)ar[1];
        }

        private void TestArrayList()
        {
            ArrayList lst = new ArrayList();
            int       i;

            Debug.Print( "System.ArrayList.Add      : " + lst.Add( 1 )          );
            Debug.Print( "System.ArrayList.Add      : " + lst.Add( "test" )     );
            Debug.Print( "System.ArrayList.Contains : " + lst.Contains( 1 )     );
            lst.Insert( 1, "ll" );
            Debug.Print( "System.ArrayList.IndexOf  : " + lst.IndexOf( "test" ) );

            i = 0;
            foreach(object o in lst)
            {
                Debug.Print( "System.ArrayList.Enum : " + i++ + " " + o );
            }

            //--//

            lst = new ArrayList();

            lst.Add( "string" );
            lst.Add( "test"   );
            lst.Add( null     );

            string[] arS = new string[3];

            lst.CopyTo( arS );

            //--//

            lst = new ArrayList();

            lst.Add( new Foo() );
            lst.Add( null      );

            Foo[] arO = new Foo[3];

            lst.CopyTo( arO );

            //--//

            lst = new ArrayList();

            lst.Add( TestEnum.P2 );

            TestEnum[] arE = new TestEnum[3];

            lst.CopyTo( arE ); Debug.Print( "lst.CopyTo( arE ); " + arE[0].ToString() );

            //--//

            lst = new ArrayList();

            lst.Add( 12  );
            lst.Add( 532 );
            lst.Add( 12  );

            int[] arI = new int[3];

            lst.CopyTo( arI );

            ArrayList lst2 = (ArrayList)lst.Clone();
            Debug.Print( "System.ArrayList.Clone " + (lst2.Count == lst.Count) );
            lst2.RemoveAt( 0 );
            Debug.Print( "System.ArrayList.Clone " + (lst2[0] == lst[1]) );

            try
            {
                lst.CopyTo( arS );

                Debug.Print( "Invalid lst.CopyTo( arS ); FAILURE" );
            }
            catch
            {
                Debug.Print( "Invalid lst.CopyTo( arS ); SUCCESS" );
            }

            try
            {
                lst[1] = (byte)12;
                lst.CopyTo( arI );

                Debug.Print( "Invalid lst.CopyTo( arI ); FAILURE" );
            }
            catch
            {
                Debug.Print( "Invalid lst.CopyTo( arI ); SUCCESS" );
            }

            try
            {
                arI = new int[1];
                lst.CopyTo( arI );

                Debug.Print( "Invalid lst.CopyTo( arI ); FAILURE" );
            }
            catch
            {
                Debug.Print( "Invalid lst.CopyTo( arI ); SUCCESS" );
            }
        }

        private void TestDelegates()
        {
            Interface itf  = this;
            Value     v1   = new Value( 1 );
            Compute   cmp  = new Compute( itf.Method2      );
            Compute   cmp2 = new Compute( Verify.Method2_b );

            cmp ( 1, 2, v1 );
            cmp2( 1, 2, v1 );

            this.m_onKey += new OnKey( this  .TestEvent1 );
            this.m_onKey += new OnKey( Verify.TestEvent2 );
            this.m_onKey += new OnKey( Verify.TestEvent3 );
            this.m_onKey -= new OnKey( Verify.TestEvent2 );
            this.m_onKey -= new OnKey( Verify.TestEvent3 );
            this.m_onKey -= new OnKey( Verify.TestEvent3 ); // Removing entry not in the list. Should do nothing.
            this.m_onKey += new OnKey( Verify.TestEvent3 );
            this.m_onKey += new OnKey( Verify.TestEvent2 );
            this.m_onKey += new OnKey( Verify.TestEvent2 ); // Adding entry already in the list. Should do nothing.

            this.m_onKey( 12 );

////        Verify testWeak = new Verify( 1, 2 );
////
////        this.m_onKey = (OnKey)Microsoft.SPOT.WeakDelegate.Combine( this.m_onKey, new OnKey( testWeak.TestEvent1 ) );
////        this.m_onKey -= new OnKey( Verify.TestEvent3 );
////        this.m_onKey += new OnKey( Verify.TestEvent3 );
////        this.m_onKey( 12 );
////
////        testWeak = null;

            System.GC.Collect();
            this.m_onKey( 12 );

            this.m_onKey -= new OnKey( Verify.TestEvent3 );
        }

////    private void TestGetType( string typeName, bool fShouldExist )
////    {
////        Type   t      = Type.GetType( typeName );
////        bool   fExist = (t != null);
////        string res    = (fExist == fShouldExist) ? "OK" : "FAIL";
////
////        Debug.Print( typeName + " : " + res );
////    }
////
////    private void TestReflection()
////    {
////        Interface itf = this;
////        Compute   cmp = new Compute( itf.Method2 );
////
////        object cmp_m = cmp.Method;
////        object cmp_t = cmp.Target;
////        Type   t1;
////
////        t1 = typeof(string[]);
////        t1 = cmp.GetType();
////        t1 = this.GetType();
////
////        TestGetType( "System.Object"                                    , true  );
////        TestGetType( "Microsoft.SPOT.Verification.Verify+Sub1+Sub2"     , true  );
////        TestGetType( "Microsoft.SPOT.Verification.Verify+Sub3"          , false );
////        TestGetType( "Microsoft.SPOT.Verification.Verify+Sub1+Sub2+Sub3", false );
////
////        ConstructorInfo ci = t1.GetConstructor( new Type[] { typeof(int), typeof(int) } );
////        object resCI = ci.Invoke( new Object[] { 2, 12 } );
////
////        MethodInfo mi = t1.GetMethod( "Method1", new Type[] { typeof(int) } );
////
////        object res = mi.Invoke( this, new Object[] { 2 } );
////        if(res is short)
////        {
////            Debug.Print( "Invoke: " + (short)res );
////        }
////        else
////        {
////            Debug.Print( "Invoke: <null>" );
////        }
////
////        Type[] at = Microsoft.SPOT.Reflection.GetTypesImplementingInterface( typeof(Interface) );
////        if(at != null)
////        {
////            foreach(Type t2 in at)
////            {
////                ConstructorInfo ci2 = t2.GetConstructor( new Type[] { typeof(int), typeof(int) } );
////                object resCI2 = ci2.Invoke( new Object[] { 2, 12 } );
////
////                Interface itf2 = (Interface)resCI2;
////            }
////        }
////
////        Assembly[] aa = Microsoft.SPOT.Reflection.GetAssemblies();
////        foreach(Assembly a in aa)
////        {
////            Debug.Print( "name: " + a.FullName + " - " + Microsoft.SPOT.Reflection.GetAssemblyHash( a ) );
////        }
////
////        //            Type t3 = typeof(string[,]);
////
////        //            Value v2 = new Value( 3 );
////        //            int   i2 = v2.Result();
////
////        //            v.i1 = itf.Method1( 1       );
////        //            v.i2 = itf.Method2( 1, 2, v );
////
////        object oCls = this;
////        bool   ba   = oCls is Interface;
////        bool   bb   = oCls is Type;
////        bool   bc   = oCls is Verify;
////
////        FooBar.TestFOO();
////        FooBar.TestBAR();
////
////        typeof(InvokeTestBed).GetMethod( "MethodInt"  ).Invoke( new InvokeTestBed(), new object [] { 1                          } );
////        typeof(InvokeTestBed).GetMethod( "MethodEnum" ).Invoke( new InvokeTestBed(), new object [] { InvokeTestBed.FooEnum.Blah } );
////    }

        private void TestThread_AbortWorker()
        {
            try
            {
                while(true);
            }
            catch(ThreadAbortException e)
            {
                Debug.Print( "ThreadAbortException: " + e.ToString() );
            }
            catch( Exception e1 )
            {
                Debug.Print( "Generic Exception: " + e1.ToString() );
            }
            finally
            {
                Debug.Print( "Finally block" );
            }
        }

        public void TestThread_Abort()
        {
            Thread t = new Thread( new ThreadStart( TestThread_AbortWorker ) );
            t.Start();
            Thread.Sleep(1000);
            t.Abort();
            Thread.Sleep(1000);
            t.Join();
        }

        //--//

        private void TestDateTime()
        {
            DateTime dt1;
            DateTime dt1b;
            DateTime dt2;
            TimeSpan ts1;
            TimeSpan ts2 = new TimeSpan( 1, 0, 0 );
            TimeSpan ts3 = new TimeSpan( 1, 0, 0, 0 );

            dt1 = new DateTime( 2003, 1, 1 );
            Debug.Print( "Microsoft.SPOT.ExtendedTimeZone.c_TicksTo20030101 " + dt1.ToString() );

            dt1 = dt1.Add( ts2 )                  ; Debug.Print( "dt1 = dt1.Add( ts2 )                   " + dt1.ToString() );
            dt1 = dt1.AddTicks( 10 * 1000 * 1000 ); Debug.Print( "dt1 = dt1.AddTicks( 10 * 1000 * 1000 ) " + dt1.ToString() );

            Debug.Print( "dt1.Date      " + dt1.Date.ToString() );
            Debug.Print( "dt1.Ticks     " + dt1.Ticks.ToString() );
            Debug.Print( "dt1.TimeOfDay " + dt1.TimeOfDay.ToString() );

            ts1 = dt1.Subtract( new DateTime(  2003, 1, 1 ) ); Debug.Print( ts1.ToString() );

            dt1 = dt1.Subtract( ts3 ); Debug.Print( "dt1 = dt1.Subtract( ts3 ) " + dt1.ToString() );
            dt2 = dt1 + ts2          ; Debug.Print( "dt2 = dt1 + ts2           " + dt2.ToString() );
            dt2 = dt1 - ts2          ; Debug.Print( "dt2 = dt1 - ts2           " + dt2.ToString() );
            ts1 = dt1 - dt2          ; Debug.Print( "ts1 = dt1 - dt2           " + ts1.ToString() );

            dt1b = dt1;

            Debug.Print( "(dt1 <  dt2) " + (dt1 <  dt2 ) );
            Debug.Print( "(dt1 >  dt2) " + (dt1 >  dt2 ) );
            Debug.Print( "(dt1 >= dt2) " + (dt1 >= dt2 ) );
            Debug.Print( "(dt1 <= dt2) " + (dt1 <= dt2 ) );
            Debug.Print( "(dt1 >= dt1) " + (dt1 >= dt1b) );
            Debug.Print( "(dt1 <= dt1) " + (dt1 <= dt1b) );
            Debug.Print( "(dt1 == dt1) " + (dt1 == dt1b) );
            Debug.Print( "(dt1 != dt1) " + (dt1 != dt1b) );

            Debug.Print( "DateTime.Compare( dt1, dt2 ) " + DateTime.Compare( dt1, dt2 ) );
            Debug.Print( "DateTime.Compare( dt2, dt1 ) " + DateTime.Compare( dt2, dt1 ) );
            Debug.Print( "DateTime.Compare( dt1, dt1 ) " + DateTime.Compare( dt1, dt1 ) );
            Debug.Print( "DateTime.Equals ( dt1, dt1 ) " + DateTime.Equals ( dt1, dt1 ) );
            Debug.Print( "DateTime.Equals ( dt1, dt2 ) " + DateTime.Equals ( dt1, dt2 ) );

            Debug.Print( "dt1.AddMilliseconds( 1.0 ) " + dt1.AddMilliseconds( 1.0 ).ToString() );
            Debug.Print( "dt1.AddSeconds     ( 1.0 ) " + dt1.AddSeconds     ( 1.0 ).ToString() );
            Debug.Print( "dt1.AddMinutes     ( 1.0 ) " + dt1.AddMinutes     ( 1.0 ).ToString() );
            Debug.Print( "dt1.AddHours       ( 1.0 ) " + dt1.AddHours       ( 1.0 ).ToString() );
            Debug.Print( "dt1.AddDays        ( 1.9 ) " + dt1.AddDays        ( 1.9 ).ToString() );
        }

        private void TestTimeSpan()
        {
            TimeSpan ts1 = new TimeSpan( 1, 2, 3, 4, 567 );

            Debug.Print( "ts1.Ticks        " + ts1.Ticks       .ToString() );
            Debug.Print( "ts1.Days         " + ts1.Days        .ToString() );
            Debug.Print( "ts1.Hours        " + ts1.Hours       .ToString() );
            Debug.Print( "ts1.Minutes      " + ts1.Minutes     .ToString() );
            Debug.Print( "ts1.Seconds      " + ts1.Seconds     .ToString() );
            Debug.Print( "ts1.Milliseconds " + ts1.Milliseconds.ToString() );
            Debug.Print( "ts1.Add( ts1 )   " + ts1.Add( ts1 )  .ToString() );
            Debug.Print( "-ts1             " + (-ts1)          .ToString() );
            Debug.Print( "-ts1.Duration    " + (-ts1).Duration().ToString() );
        }

        private void TestTime()
        {
            DateTime dt = DateTime.Now;
            Debug.Print( dt.ToString() );

            TimeSpan ts = new TimeSpan( 1, 20, 12, 30, 100 );
            Debug.Print( ts.ToString() );

            ts = ts.Add( new TimeSpan( 0, 0, 10 ) );
            Debug.Print( ts.ToString() );

            DateTime dt2 = dt;

            dt = dt.Add( ts );
            Debug.Print( dt.ToString() );

            TimeSpan ts2 = dt.Subtract( dt2 );
            Debug.Print( ts2.ToString() );
            Debug.Print( ts2.Days.ToString() );
            Debug.Print( ts2.Hours.ToString() );
            Debug.Print( ts2.Minutes.ToString() );
            Debug.Print( ts2.Seconds.ToString() );

            dt = dt.Subtract( ts );
            Debug.Print( dt.ToString() );

            Debug.Print( TimeSpan.MaxValue.ToString() );
            Debug.Print( TimeSpan.Zero    .ToString() );
            Debug.Print( TimeSpan.MinValue.ToString() );

            Debug.Print( DateTime.MaxValue.ToString() );
            Debug.Print( DateTime.MinValue.ToString() );

            Debug.Print( "############################" );
        }

        private void WaitButton()
        {
            Thread.Sleep( 1000 );
        }

        private void TestTimers()
        {
            Timer timer = new Timer( new TimerCallback( this.Method6 ), 1, 1000, 2000 );

            Thread t = new Thread( new ThreadStart( this.Thread_Start1 ) );

            t.Start();

            for(int i=0; i<10; i++)
            {
            }

            WaitButton();

            //////////////////////////////////////////////////////////////////////////////////////
            //
            // More Timer and Threading tests.
            //
            timer.Change( 1000, 500 );

            t.Abort();

            t.Join();

            Debug.Print( "Timer wait" );


            //
            // Test timer object release
            //
            timer = new Timer( new TimerCallback( this.Method6 ), "test", new TimeSpan( 1000 * 10000 ), new TimeSpan( 1000 * 10000 ) );

            timer = null;

            Debug.Print( "Timer alive..." );
            Thread.Sleep( 3000 );
            System.GC.Collect();
            Debug.Print( "Timer should be dead. Press button to continue." ); WaitButton();


////        Microsoft.SPOT.ExtendedTimer timer2;
////
////        Debug.Print( "Extended Timer 1" );
////        timer2 = new Microsoft.SPOT.ExtendedTimer( new TimerCallback( this.Method6 ), "test", 1000, 1000 );
////        Thread.Sleep( 3000 );
////        timer2.Dispose();
////
////        Debug.Print( "Extended Timer 2" );
////        timer2 = new Microsoft.SPOT.ExtendedTimer( new TimerCallback( this.Method6 ), "test", new DateTime( 2012, 12, 1, 14, 0, 2 ), new TimeSpan( 1000 * 10000 ) );
////        Debug.Print( "Now: " + DateTime.Now.ToString() );
////        Thread.Sleep( 1000 );
////
////        Microsoft.SPOT.Hardware.Utility.SetLocalTime( new DateTime( 2012, 12, 1, 14, 0, 0 ) );
////        Debug.Print( "Now: " + DateTime.Now.ToString() );
////        Thread.Sleep( 4000 );
////        timer2.Dispose();
////
////        Debug.Print( "Extended Timer 3" );
////        Thread.Sleep( 500 );
////
////        timer2 = new Microsoft.SPOT.ExtendedTimer( new TimerCallback( this.Method6 ), "test", Microsoft.SPOT.ExtendedTimer.TimeEvents.Minute );
////        Microsoft.SPOT.Hardware.Utility.SetLocalTime( new DateTime( 2012, 12, 1, 14, 0, 58 ) );
////        Debug.Print( "Now: " + DateTime.Now.ToString() );
////
////        Debug.Print( "Waiting on TimeEvents.Minute. Press button to continue." ); WaitButton();
////
////        timer2.Dispose();
////
////        Debug.Print( "Extended Timer 4" );
////        timer2 = new Microsoft.SPOT.ExtendedTimer( new TimerCallback( this.Method6 ), "test", Microsoft.SPOT.ExtendedTimer.TimeEvents.Hour );
////        Microsoft.SPOT.Hardware.Utility.SetLocalTime( new DateTime( 2012, 12, 1, 14, 59, 58 ) );
////        Debug.Print( "Now: " + DateTime.Now.ToString() );
////
////        Debug.Print( "Waiting on TimeEvents.Hour. Press button to continue." ); WaitButton();
////
////        Debug.Print( "Last Expiration: " + timer2.LastExpiration.ToString() );
////        timer2.Dispose();
////
////        Debug.Print( "Extended Timer 5" );
////        timer2 = new Microsoft.SPOT.ExtendedTimer( new TimerCallback( this.Method6 ), "test", Microsoft.SPOT.ExtendedTimer.TimeEvents.SetTime );
////
////        Debug.Print( "Press button to change time." ); WaitButton();
////        Microsoft.SPOT.Hardware.Utility.SetLocalTime( new DateTime( 2012, 12, 2, 14, 59, 58 ) );
////        Debug.Print( "Now: " + DateTime.Now.ToString() );
////        Debug.Print( "Waiting on TimeEvents.SetTime. Press button to continue." ); WaitButton();
////
////        timer2.Dispose();
        }

////    private void TestRegionConfiguration()
////    {
////        DateTime dt_20030101 = new DateTime( 2003, 01, 01 );
////        DateTime dt_20030420 = new DateTime( 2003, 04, 20 );
////        DateTime dt_20030430 = new DateTime( 2003, 04, 30 ); // Wednesday
////        DateTime dt_20030501 = new DateTime( 2003, 05, 01 );
////        DateTime dt_20030502 = new DateTime( 2003, 05, 02 );
////
////        DateTime dt_Sunday    = dt_20030101.AddDays( DayOfWeek.Sunday - dt_20030101.DayOfWeek );
////        DateTime dt_Monday    = dt_Sunday.AddDays( 1 );
////        DateTime dt_Tuesday   = dt_Sunday.AddDays( 2 );
////        DateTime dt_Wednesday = dt_Sunday.AddDays( 3 );
////        DateTime dt_Thursday  = dt_Sunday.AddDays( 4 );
////        DateTime dt_Friday    = dt_Sunday.AddDays( 5 );
////        DateTime dt_Saturday  = dt_Sunday.AddDays( 6 );
////
////
////        TimeSpan ts_080000   = new TimeSpan(  8, 0, 0 );
////        TimeSpan ts_170000   = new TimeSpan( 17, 0, 0 );
////        TimeSpan ts_180000   = new TimeSpan( 18, 0, 0 );
////        TimeSpan ts_200000   = new TimeSpan( 20, 0, 0 );
////    }

        private void TestFinalizers()
        {
            FinalizeTest ft1 = new FinalizeTest ( 1 );
            FinalizeTest ft2 = new FinalizeTest2( 2 );
            FinalizeTest ft3 = new FinalizeTest2( 4 );

            System.GC.Collect();
            Debug.Print( "GC 0: " + Debug.AvailableMemory() );
            //Debug.DumpHeap();

            ft1 = null;
            ft2 = null;
            ft3 = null;

            for(int i=1; i<10; i++)
            {
                System.GC.Collect();
                Debug.Print( "GC " + i + ": " + Debug.AvailableMemory() );
                //Debug.DumpHeap();
                System.GC.WaitForPendingFinalizers();
                //Thread.Sleep( 70 );
            }
        }

////    private void TestWeakReferences()
////    {
////        RPC_test.RecordSub   r1   = new RPC_test.RecordSub();
////        System.WeakReference weak = new System.WeakReference( r1 );
////
////        System.GC.Collect();
////        Debug.Print( "Weak reference: " + weak.IsAlive );
////
////        r1 = null;
////
////        System.GC.Collect();
////        Debug.Print( "Weak reference: " + weak.IsAlive );
////
////        //--//
////
////        Microsoft.SPOT.ExtendedWeakReference restore;
////        Microsoft.SPOT.ExtendedWeakReference weak2;
////        ArrayList                            lst = new ArrayList();
////        int                                  i;
////
////        lst = new ArrayList();
////
////        while((restore = Microsoft.SPOT.ExtendedWeakReference.Recover( typeof(DateTime), 0 )) != null)
////        {
////            lst.Add( restore );
////        }
////
////        for(i=0; i<lst.Count; i++)
////        {
////            weak2 = (Microsoft.SPOT.ExtendedWeakReference)lst[i]; Debug.Print( "Restored Type: " + weak2.GetType().FullName );
////        }
////
////        //--//
////
////        lst = new ArrayList();
////
////        r1 = new RPC_test.RecordSub();
////
////        for(i=0; i<5; i++)
////        {
////            weak2 = new Microsoft.SPOT.ExtendedWeakReference( r1, typeof(DateTime), 0, Microsoft.SPOT.ExtendedWeakReference.c_SurvivePowerdown );
////            lst.Add( weak2 );
////        }
////
////        for(i=0; i<lst.Count; i++)
////        {
////            weak2 = (Microsoft.SPOT.ExtendedWeakReference)lst[i]; Debug.Print( "Type: " + weak2.GetType().FullName );
////        }
////    }

        //--//

        static void Banner( string text )
        {
            Debug.Print( "################################################################################" );
            Debug.Print( text );
            Debug.Print( new string( '#', text.Length ) );
        }

        private void MainTestLoop()
        {
            Banner( "TestNumeric"                         ); TestNumeric                        ();
////        Banner( "TestNumericConversion"               ); TestNumericConversion              ();
////        Banner( "TestImplicitPromotion"               ); TestImplicitPromotion              ();
////        Banner( "TestByref"                           ); TestByref                          ();
////        Banner( "TestEnums"                           ); TestEnums                          ();
////
////        Banner( "TestSpecialValueTypes.DateTimeUsage" ); TestSpecialValueTypes.DateTimeUsage();
////        Banner( "TestFinalizers"                      ); TestFinalizers                     ();
////
////        Banner( "TestCasts"                           ); TestCasts                          ();
            Banner( "TestExceptions"                      ); TestExceptions                     ();
////        Banner( "TestThread_Abort"                    ); TestThread_Abort                   ();
////        Banner( "TestLockAndSleep"                    ); TestLockAndSleep                   ();
////        Banner( "TestStrings"                         ); TestStrings                        ();
////        Banner( "TestValueTypes"                      ); TestValueTypes                     ();
////        Banner( "TestValueTypeArrayReferences"        ); TestValueTypeArrayReferences       ();
////        Banner( "TestBoxing"                          ); TestBoxing                         ();
////        Banner( "TestArrays"                          ); TestArrays                         ();
////        Banner( "TestArrayList"                       ); TestArrayList                      ();
////        Banner( "TestDelegates"                       ); TestDelegates                      ();
////        Banner( "TestTime"                            ); TestTime                           ();
////        Banner( "TestDateTime"                        ); TestDateTime                       ();
////        Banner( "TestTimeSpan"                        ); TestTimeSpan                       ();
////        Banner( "TestTimers"                          ); TestTimers                         ();

//// ---    Banner( "TestReflection"                      ); TestReflection                     ();
//// ---    Banner( "TestRegionConfiguration"             ); TestRegionConfiguration            ();
//// ---    Banner( "TestWeakReferences"                  ); TestWeakReferences                 ();
        }

        public static void RunVerification()
        {
            Verify cls = new Verify();

            cls.MainTestLoop();
        }
    }
}
