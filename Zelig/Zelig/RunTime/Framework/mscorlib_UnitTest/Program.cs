//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

//#define INCLUDE_BASIC_TESTS
//#define INCLUDE_VERIFICATION
//#define GC_TEST_MULTIPOINTER_STRUCTS
//#define GC_STRESS
//#define BITFIELD_TEST

namespace mscorlib_UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;

    public class Program
    {
#if BITFIELD_TEST
        public enum EnumBitField
        {
            Test1,
            Test2,
            Test3,
        }

        [RT.MemoryMappedPeripheral(Base=0x08000000U,Length=0x00000020U)]
        public class BitFieldTest
        {
            [RT.BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct BitFieldReg
            {
                [RT.BitFieldRegister(Position=0,Size= 3)] public uint         Field1;
                [RT.BitFieldRegister(Position=3,Size= 2)] public uint         Field2;
                [RT.BitFieldRegister(Position=5,Size=10)] public EnumBitField Field3;
                [RT.BitFieldRegister(Position=31,Size=1)] public bool         Field4;

                [RT.BitFieldSplitRegister(Position=16,Size=4,Offset=4)]
                [RT.BitFieldSplitRegister(Position=20,Size=4,Offset=0)] public int Field5;
 
            }

            [RT.Register(Offset=0)] public BitFieldReg reg1;

            //
            // Access Methods
            //

            public static extern BitFieldTest Instance
            {
                [RT.SingletonFactory()]
                [MethodImpl( MethodImplOptions.InternalCall )]
                get;
            }
        }

        static void TestBitField()
        {
            BitFieldTest bft = BitFieldTest.Instance;

            bft.reg1.Field1 = 2;
            bft.reg1.Field2 += 2;

            bft.reg1.Field5++;

            var reg1 = bft.reg1;
    
            if(reg1.Field4)
            {
                reg1.Field3 = EnumBitField.Test3;
            }
    
            bft.reg1 = reg1;
    
            BitFieldTest.BitFieldReg reg2 = new BitFieldTest.BitFieldReg();
    
            reg2.Field1 = 12;
            reg2.Field2 = 2;
            reg2.Field5 = -2;
    
            bft.reg1 = reg2;
        }
#endif

        class TestFinalizer
        {
            int m_a;

            public TestFinalizer( int a )
            {
                m_a = a;
            }

            ~TestFinalizer()
            {
                Console.WriteLine( "{0}", m_a );
            }
        }

        static void TestFinalizers()
        {
            for(int i = 0; i < 10; i++)
            {
                var a = new TestFinalizer( i + 100 );
                var b = new TestFinalizer( i + 200 );
    
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        static void TestSyncBlocks()
        {
            for(int i = 0; i < 10; i++)
            {
                object obj = new object();
                lock(obj)
                {
                    object obj2 = new object();
                    lock(obj2)
                    {
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }


        static string TestArrayInitializers()
        {
            try
            {
                int[]  a = new int[] { 1, 2, 3, 4, 5 };
                char[] b = new char[] { 'a', 'b', 'c' };
                string[] c = new string[] { "a", "b", "c" };
                byte[] payload = Files.ZeligRefreshBinaryDrop;

                object     obj = c;
                ICloneable cld = (ICloneable)obj; object obj2 = cld;//.Clone();
                string[]   c2  = (string[])obj;

                Buffer.BlockCopy( a, 0 * sizeof(int), a, 3 * sizeof(int), 2 * sizeof(int) );
                Buffer.BlockCopy( a, 1 * sizeof(int), a, 0 * sizeof(int), 2 * sizeof(int) );

                ICollection< string > icoll = c;

                foreach(var s in icoll)
                {
                    Console.WriteLine( s );
                }

                return c2[0];
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        static int[] TestArrayBoundChecks( int[] a  ,
                                           int   c1 ,
                                           int   c2 )
        {
            for(int i = 0; i < a.Length; i++)
            {
                if(i < c1 && (i < c2 || i > c2))
                {
                    a[i] = 3;
                }

                if(i < c1 || (i < c2 && i >= 2))
                {
                    a[i] = 4;
                }

                a[i] *= 2;
            }

            return a;
        }

        static void TestArrayBoundChecks()
        {
            int[] a = new int[] { 1, 2, 3, 4, 5 };

            TestArrayBoundChecks( a, 3, 5 );
        }

        static int TestCheckedOperations( int a ,
                                          int b ,
                                          int c )
        {
            byte aB = (byte)a;
            byte bB = (byte)b;
            byte cB = (byte)c;
            
            byte rB = checked( (byte)(cB + bB + cB) );

            return rB;
        }

        public abstract class Sub1
        {
            public abstract int Code();
        }

        public class Sub2 : Sub1
        {
            public override int Code()
            {
                return 2;
            }
        }

        public class Sub3 : Sub2
        {
            public override int Code()
            {
                return 3;
            }
        }

        public class Sub4 : Sub2
        {
            public override int Code()
            {
                return 4;
            }
        }

        class DelegateTester
        {
            delegate int DelegateTest( int a );


            static int StaticDelegateTarget( int a )
            {
                return a * a;
            }

            int InstanceDelegateTarget( int a )
            {
                return a * a;
            }

            internal virtual int VirtualDelegateTarget( int a )
            {
                return a * a;
            }

            internal static void Run()
            {
                DelegateTest   tst;
                DelegateTester pThis = new SubDelegateTester();
                
                tst =       StaticDelegateTarget  ; tst( 2 );
                tst = pThis.InstanceDelegateTarget; tst( 2 );
                tst = pThis.VirtualDelegateTarget ; tst( 2 );
            }
        }

        class SubDelegateTester : DelegateTester
        {
            internal override int VirtualDelegateTarget( int a )
            {
                return a * a + 5;
            }
        }

        [RT.NoInline]
        private static float[] TestNumeric( float a ,
                                            float b ,
                                            float c )
        {
            float[] res = new float[32];
            int     i   = 0;

            res[i++] =   -  b;
            res[i++] = a +  b;
            res[i++] = a -  b;
            res[i++] = b -  a;
            res[i++] = a +  c;
            res[i++] = a -  c;
            res[i++] = c -  a;
            res[i++] = a *  b;
            res[i++] = a /  b;
            res[i++] = a <  b ? 1 : 0;
            res[i++] = a == b ? 1 : 0;
            res[i++] = a >  b ? 1 : 0;

            return res;
        }

        [RT.NoInline]
        private static double[] TestNumeric( double a ,
                                             double b ,
                                             double c )
        {
            double[] res = new double[32];
            int      i   = 0;

            res[i++] =   -  b;
            res[i++] = a +  b;
            res[i++] = a -  b;
            res[i++] = b -  a;
            res[i++] = a +  c;
            res[i++] = a -  c;
            res[i++] = c -  a;
            res[i++] = a *  b;
            res[i++] = a /  b;
            res[i++] = a <  b ? 1 : 0;
            res[i++] = a == b ? 1 : 0;
            res[i++] = a >  b ? 1 : 0;

            return res;
        }

        //--//

        private static void TestFloatingPoint()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            for(int loop = 0; loop < 32; loop++)
            {
                stopwatch.Start();
                float[] res1 = TestNumeric( 1.3f,  3.5f, 0f );
                float[] res2 = TestNumeric( 1.3f, -3.5f, 0f );
                stopwatch.Stop();
                long val = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }

            for(int loop = 0; loop < 32; loop++)
            {
                double[] res1 = TestNumeric( 1.3d,  3.5d, 0d );
                double[] res2 = TestNumeric( 1.3d, -3.5d, 0d );
            }
        }

        private static void TestList()
        {
            var lst = new List< int >();

            lst.Add( 1 );
            lst.Add( 2 );
            lst.Add( 3 );
            lst.Add( 4 );

            lst.Contains( 3 );
            lst.Contains( 5 );

            var lst2 = new List< string >();

            lst2.Add( "test1" );
            lst2.Add( "test2" );
            lst2.Add( "test3" );
            lst2.Add( "test4" );

            lst2.Contains( "test3" );
            lst2.Contains( "test5" );
        }

        private static int TestDictionary()
        {
            var dictInt = new Dictionary< int, int >();

            dictInt[1] = 10;
            dictInt[2] = 20;
            dictInt[3] = 30;
            dictInt[4] = 40;

            int val = dictInt[3];

            var dict = new Dictionary< string, int >( StringComparer.Ordinal );

            dict["test" ] = 1;
            dict["test2"] = 2;
            dict["TEST" ] = 3;
            dict["TEST2"] = 4;

            int res = dict["TEST"];

            return res;
        }

        private static void TestExceptions()
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
        }

        private static void TestEvents()
        {
            System.Threading.AutoResetEvent ev1 = new System.Threading.AutoResetEvent( false );
            System.Threading.AutoResetEvent ev2 = new System.Threading.AutoResetEvent( false );
            object obj = new object();
            int    count = 0;

            System.Threading.Thread thread1 = new System.Threading.Thread( delegate()
            {
                for(int i = 0; i < 16; i++)
                {
                    ev1.WaitOne();
                    ev2.Set();

                    lock(obj)
                    {
                        System.Threading.Thread.Sleep( 10 );
                        count++;
                    }
                }
            } );

            System.Threading.Thread thread2 = new System.Threading.Thread( delegate()
            {
                for(int i = 0; i < 16; i++)
                {
                    ev2.WaitOne();
                    ev1.Set();

                    lock(obj)
                    {
                        System.Threading.Thread.Sleep( 10 );
                        count++;
                    }
                }
            } );

            thread1.Start();
            thread2.Start();

            ev1.Set();

            thread1.Join();
            thread2.Join();
        }

        private static void TestTime()
        {
            DateTime now;
            int      val;
            
            now = DateTime.Now;
            now = DateTime.Now;
            now = DateTime.Now;

            val = now.Year;
            val = now.Month;
            val = now.Day;
            val = now.Hour;
            val = now.Minute;
            val = now.Second;
            val = now.Millisecond;

            Microsoft.Zelig.Runtime.DateTimeImpl.SetUtcTime( TimeZone.CurrentTimeZone.ToUniversalTime( new DateTime( 2003, 1, 1 ) ) );

            now = DateTime.Now;
            now = DateTime.Now;
            now = DateTime.Now;

            val = now.Year;
            val = now.Month;
            val = now.Day;
            val = now.Hour;
            val = now.Minute;
            val = now.Second;
            val = now.Millisecond;
        }

        private static void TestTimers()
        {
            int                    count = 0;
            System.Threading.Timer timer = new System.Threading.Timer( delegate( object state )
            {
                count++;
            } );

            timer.Change( 20, System.Threading.Timeout.Infinite );

            System.Threading.Thread.Sleep( 100 );

            timer.Change( 20, 1 );

            System.Threading.Thread.Sleep( 100 );

            timer.Dispose();

            System.Threading.Thread.Sleep( 100 );
        }

        private static string[] TestToString()
        {
            string[] res = new string[32];
            int      i   = 0;

            res[i++] =   int   .MaxValue.ToString();
            res[i++] = ((int)0)         .ToString();
            res[i++] =   int   .MinValue.ToString();
            res[i++] =   123            .ToString( "00000" );

            res[i++] =   char  .MaxValue.ToString();
            res[i++] = ((char)0)        .ToString();
            res[i++] =   char  .MinValue.ToString();

            res[i++] =   long  .MaxValue.ToString();
            res[i++] = ((long)0)        .ToString();
            res[i++] =   long  .MinValue.ToString();

            res[i++] =   3.14f.ToString();
            res[i++] =   10f  .ToString();
            res[i++] =   0.1f .ToString();

            res[i++] =   float .MaxValue.ToString();
            res[i++] =   float .MinValue.ToString();
            res[i++] =   double.MaxValue.ToString();
            res[i++] =   double.MinValue.ToString();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendFormat( "Test {0:X8} {2} {1} End", 234, "<text>", -212 );
            res[i++] = sb.ToString();

            return res;
        }

        private static void TestGenericOpenClasses()
        {
            var obj = new GenericClassTest();

            for(int i = 0; i < 20; i++)
            {
                obj.Test1( i, i * 2 );
            }
        }

        private static int TestMethodInjection()
        {
            TargetForInjection obj = new TargetForInjection();
            int                c = 4;

            return obj.EmptyMethod( 2, 3, ref c );
        }

        struct TestTrackStack
        {
            public object a;
            public object b;
            public object c;
            public object d;
        }

        static TestTrackStack Start( object a ,
                                     object b ,
                                     object c ,
                                     object d )
        {
            TestTrackStack st = new TestTrackStack();

            st.a = a;
            st.b = b;
            st.c = c;
            st.d = d;

            return st;
        }

        private static void GCStress( ref object p, object s )
        {
            GC.Collect();

            p = s;
        }

        static unsafe void Main()
        {
#if INCLUDE_VERIFICATION

            mscorlib_UnitTest.Verification.Verify.RunVerification();

#endif

            //--//

#if GC_TEST_MULTIPOINTER_STRUCTS

            TestTrackStack st = Start( 1, 2, 3, 4 );

            Console.WriteLine( "{0}", st.a );
            GC.Collect();

            Console.WriteLine( "{0}", st.b );
            GC.Collect();

            Console.WriteLine( "{0}", st.c );
            GC.Collect();

            Console.WriteLine( "{0}", st.d );
            GC.Collect();

#elif GC_STRESS
            while(true)
            {
                TestFloatingPoint();

                object[] array = new object[120];

                array[3] = array;

                GCStress( ref array[5], array );
            }

#elif BITFIELD_TEST

            TestTrackStack st = Start( 1, 2, 3, 4 );

            st.d = 23;

            TestBitField();

#else

            TestFinalizers();

            TestSyncBlocks();

            TestArrayBoundChecks();
    
            TestCheckedOperations( 2, 3, 4 );
    
            DelegateTester.Run();
    
            TestArrayInitializers();
    
            TestToString();
    
            TestList();
    
            TestDictionary();
    
            TestFloatingPoint();
    
            TestExceptions();
    
            TestEvents();
    
            TestTime();
    
            TestTimers();
    
            TestGenericOpenClasses();
    
            TestMethodInjection();

#endif
        }
    }
}
