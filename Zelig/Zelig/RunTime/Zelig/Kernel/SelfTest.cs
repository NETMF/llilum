//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System.Runtime.InteropServices;

namespace Microsoft.Zelig.Runtime
{
    using System;
    
    internal static class SelfTest
    {
        
        [DllImport( "C" )]
        public static unsafe extern int get_a_number( );

        [DllImport( "C" )]
        public static unsafe extern void violent_breakpoint( );

        [DllImport( "C" )]
        public static unsafe extern void break_and_watch( uint n );


        //
        // Zelig Self Test - Early bootstrap, no heap:
        // integers, pointers and value types
        //
        [NoInline]
        internal static unsafe void SELFTEST_ASSERT( bool expression )
        {
            if( !expression )
            {
                violent_breakpoint( );
            }
        }
        
        //--//
        //--//
        //--//

        #region basic math, and flow controls
        private static int FibonacciInterative( int num )
        {
            int a = 0, b = 1, c = 0;

            if( num == 1 ) return 1;

            for( int i = 1; i < num; i++ )
            {
                c = a + b;
                a = b;
                b = c;
            }

            return c;
        }

        private static int FibonacciRecursive( int num )
        {
            if( num <= 1 )
            {
                return num;
            }

            return FibonacciRecursive( num - 1 ) + FibonacciRecursive( num - 2 );
        }

        private static void SelfTest__Test__Fibonacci( )
        {
            const int iter_0 = 0;
            const int iter_1 = 1;
            const int iter_2 = 1;
            const int iter_3 = 2;
            const int iter_4 = 3;
            const int iter_5 = 5;
            const int iter_6 = 8;
            const int iter_7 = 13;
            const int iter_8 = 21;
            const int iter_9 = 34;
            const int iter_10 = 55;
            const int iter_11 = 89;
            const int iter_12 = 144;
            const int iter_13 = 233;
            const int iter_14 = 377;
            const int iter_15 = 610;

            int iterations = 16;

            int count = -1;

            while( ++count < iterations )
            {
                int rec = FibonacciRecursive( count );

                switch( count )
                {
                    case 0:
                        SELFTEST_ASSERT( rec == iter_0 );
                        break;

                    case 1:
                        SELFTEST_ASSERT( rec == iter_1 );
                        break;

                    case 2:
                        SELFTEST_ASSERT( rec == iter_2 );
                        break;

                    case 3:
                        SELFTEST_ASSERT( rec == iter_3 );
                        break;

                    case 4:
                        SELFTEST_ASSERT( rec == iter_4 );
                        break;

                    case 5:
                        SELFTEST_ASSERT( rec == iter_5 );
                        break;

                    case 6:
                        SELFTEST_ASSERT( rec == iter_6 );
                        break;

                    case 7:
                        SELFTEST_ASSERT( rec == iter_7 );
                        break;

                    case 8:
                        SELFTEST_ASSERT( rec == iter_8 );
                        break;

                    case 9:
                        SELFTEST_ASSERT( rec == iter_9 );
                        break;

                    case 10:
                        SELFTEST_ASSERT( rec == iter_10 );
                        break;

                    case 11:
                        SELFTEST_ASSERT( rec == iter_11 );
                        break;

                    case 12:
                        SELFTEST_ASSERT( rec == iter_12 );
                        break;

                    case 13:
                        SELFTEST_ASSERT( rec == iter_13 );
                        break;

                    case 14:
                        SELFTEST_ASSERT( rec == iter_14 );
                        break;

                    case 15:
                        SELFTEST_ASSERT( rec == iter_15 );
                        break;
                    default:
                        SELFTEST_ASSERT( false );
                        break;

                }
            }
        }

        private static void SelfTest__Test__CallsAndControls( )
        {
            const int iterations = 25;

            int count = 0;

            while( count++ < iterations )
            {
                
                int rec = FibonacciRecursive( count );
                int ite = FibonacciInterative( count );

                SELFTEST_ASSERT( ite == rec );
            }
        }
        
        #endregion 

        #region native types, basic casts

        private static void SelfTest__Test__NativeTypes_Casts()
        {
            const int mask = ( 1 << 16 ) - 1;

            // get a number smaller than 16 bits
            int seed = get_a_number() % mask;

            SELFTEST_ASSERT( seed < mask );
            
            int i = seed << 16;

            // downcast
            short s = ( short )i;            
            SELFTEST_ASSERT( s == 0 );
        }

        #endregion

        #region Value Types, basic copy semantic

        struct ToCopy
        {
            public int i;
            public char c;
        }

        private static void SelfTest__Test__ValueTypesCopy_1()
        {
            //
            // Vanilla assignment between value types
            //
            ToCopy a;
            a.i = 1;
            a.c = 'a';

            SELFTEST_ASSERT( a.i == 1 && a.c == 'a' );

            ToCopy b;
            b.i = 2;
            b.c = 'b';
            
            SELFTEST_ASSERT( b.i == 2 && b.c == 'b' );


            ToCopy aa = a;

            a = b;
            
            // new values
            SELFTEST_ASSERT(  a.i == 2 &&  a.c == 'b' );
            // old values
            SELFTEST_ASSERT( aa.i == 1 && aa.c == 'a' );
        }
        
        private static void SelfTest__Test__ValueTypesCopy_2()
        {
            //
            // Vanilla assignment between value types
            //
            ToCopy a;
            a.i = 1;
            a.c = 'a';

            SELFTEST_ASSERT( a.i == 1 && a.c == 'a' );

            ToCopy aa = a;
            
            PassByValue( a );

            // new values
            SELFTEST_ASSERT(  a.i == 1 &&  a.c == 'a' );
            // old values
            SELFTEST_ASSERT( aa.i == 1 && aa.c == 'a' );
            // cross check values
            SELFTEST_ASSERT( aa.i == a.i && aa.c == a.c );
        }

        private static void PassByValue( ToCopy byValue )
        {
            byValue.i = 2;
            byValue.c = 'b';
        }
        
        private static void SelfTest__Test__ValueTypesCopy_3()
        {
            //
            // Vanilla assignment between value types
            //
            ToCopy a;
            a.i = 1;
            a.c = 'a';

            SELFTEST_ASSERT( a.i == 1 && a.c == 'a' );

            ToCopy aa = a;
            
            PassByRef( ref a );

            // new values
            SELFTEST_ASSERT(  a.i == 2 &&  a.c == 'b' );
            // old values
            SELFTEST_ASSERT( aa.i == 1 && aa.c == 'a' );
            // cross check values
            SELFTEST_ASSERT( aa.i + 1 == a.i );
        }

        private static void PassByRef( ref ToCopy byValue )
        {
            byValue.i = 2;
            byValue.c = 'b';
        }
        
        private static void SelfTest__Test__ValueTypesCopy_4()
        {
            //
            // Vanilla assignment between value types
            //
            ToCopy a;
            a.i = 1;
            a.c = 'a';

            SELFTEST_ASSERT( a.i == 1 && a.c == 'a' );

            ToCopy aa = a;

            a = ReturnByValue(); 

            // new values
            SELFTEST_ASSERT(  a.i == 2 &&  a.c == 'b' );
            // old values
            SELFTEST_ASSERT( aa.i == 1 && aa.c == 'a' );
        }

        private static ToCopy ReturnByValue(  )
        {
            ToCopy b;
            
            b.i = 2;
            b.c = 'b';

            return b;
        }

        #endregion

        //--//
        //--//
        //--//

        internal static void SelfTest__Bootstrap( )
        {

            //
            // Basic math and flow controls 
            //
            SelfTest__Test__Fibonacci( );
            SelfTest__Test__CallsAndControls( ); 

            //
            // Native types, basic casts 
            //
            SelfTest__Test__NativeTypes_Casts( );
            
            //
            // Value Types, basic copy semantic 
            //
            SelfTest__Test__ValueTypesCopy_1( );
            SelfTest__Test__ValueTypesCopy_2( );
            SelfTest__Test__ValueTypesCopy_3( );
            //SelfTest__Test__ValueTypesCopy_4(); KNOWN FAILURE

            //
            // pointers...
            //
            SelfTest__Test__Pointers_Conversions_1( );
            SelfTest__Test__Pointers_Conversions_2( );
            SelfTest__Test__Pointers_Conversions_3( );
            SelfTest__Test__Pointers_Conversions_4( );


            SelfTest__Test__RawPointers_Arithmetic_Pointer_1( ); 
            SelfTest__Test__RawPointers_Arithmetic_Pointer_2( ); 
            SelfTest__Test__RawPointers_Arithmetic_Values_1 ( ); 
            SelfTest__Test__RawPointers_Arithmetic_Values_2 ( ); 


            SelfTest__Test__Pointers_PassByValue( );
            SelfTest__Test__Pointers_PassByRef( );
            
            //--//
            //--// ...value types...
            //--//

            SelfTest__Test__ValueTypes_PassByValue( );
            SelfTest__Test__ValueTypes_PassByRef( );

            //
            // ...else...
            //

            SelfTest__Test__Integers_Conversions( );
            SelfTest__Test__Integers_PassByValue( );
            SelfTest__Test__Integers_PassByRef( );
            SelfTest__Test__Integers_BoxUnbox( );
            SelfTest__Test__Integers_BoxUnboxInverted( );

            //Trap end of tests
            SELFTEST_ASSERT( false );

        }

        //--//
        //--//
        //--//

      
        #region pointers basic 
        
        private static unsafe void SelfTest__Test__RawPointers_Arithmetic_Pointer_1( )
        {
            //
            // Pointer arithmetic, 1 value
            //
            int pointed = 42;
            
            int* a  = &pointed;
            int* aa = &pointed;

            a += 1;

            SELFTEST_ASSERT( a == aa + 1 );
        }

        private static unsafe void SelfTest__Test__RawPointers_Arithmetic_Pointer_2( )
        {
            //
            // Pointer arithmetic, 2 values
            //
            int pointed = 42;
            
            int* a  = &pointed;
            int* b  = &pointed;
            int* aa = &pointed;
            int* bb = &pointed;
            
            a += 1;
            b += 2;
            
            SELFTEST_ASSERT( a == aa + 1 );
            SELFTEST_ASSERT( b == bb + 2 );
        }

        private static unsafe void SelfTest__Test__RawPointers_Arithmetic_Values_1( )
        {
            //
            // Pointer arithmetic, 2 values
            //
                      int       pointed = 42;
            /*const*/ int const_pointed = 42;
            
            int* a  = &pointed;
            int* aa = &const_pointed;
            
            *a += 1;
            
            SELFTEST_ASSERT( *a == *aa + 1 );
        }

        private static unsafe void SelfTest__Test__RawPointers_Arithmetic_Values_2( )
        {
            //
            // Pointer arithmetic, 2 values
            //
                      int       pointed = 42;
            /*const*/ int const_pointed = 42;
            
            int* a  = &pointed;
            int* b  = &pointed;
            int* aa = &const_pointed;
            int* bb = &const_pointed;
            
            *a += 1;
            *b += 2;
            
            SELFTEST_ASSERT( *a == *aa + 3 );
            SELFTEST_ASSERT( *b == *bb + 3 );
        }
        
        private static void SelfTest__Test__Pointers_Conversions_1( )
        {
            IntPtr a = ( IntPtr )0x10004000;

            int aa = a.ToInt32( );

            SELFTEST_ASSERT( aa == 0x10004000 );
        }

        private static void SelfTest__Test__Pointers_Conversions_2( )
        {
            IntPtr a = ( IntPtr )0x10004000;
            IntPtr b = ( IntPtr )0x10004000;
            
            int aa = a.ToInt32( );
            int bb = b.ToInt32( );
            
            SELFTEST_ASSERT( a  == b  );
            SELFTEST_ASSERT( aa == bb );
        }
        
        private static void SelfTest__Test__Pointers_Conversions_3( )
        {
            UIntPtr a = ( UIntPtr )0x10004000;

            uint aa = a.ToUInt32( );

            SELFTEST_ASSERT( aa == 0x10004000 );
        }

        private static void SelfTest__Test__Pointers_Conversions_4( )
        {
            UIntPtr a = ( UIntPtr )0x10004000;
            UIntPtr b = ( UIntPtr )0x10004000;
            
            uint aa = a.ToUInt32( );
            uint bb = b.ToUInt32( );
            
            SELFTEST_ASSERT( a  == b  );
            SELFTEST_ASSERT( aa == bb );
        }

        private static void SelfTest__Test__Pointers_PassByValue( )
        {
            IntPtr a = ( IntPtr )10;
            IntPtr b = ( IntPtr )10;

            PassByValue( a, b );

            SELFTEST_ASSERT( a.ToInt32( ) == 10 );
            SELFTEST_ASSERT( b.ToInt32( ) == 10 ); 
            SELFTEST_ASSERT( ( a.ToInt32( ) == b.ToInt32( ) ) );
            SELFTEST_ASSERT( ( a.ToInt32( ) + b.ToInt32( ) ) == 20);
        }

        private static void PassByValue( IntPtr a, IntPtr b )
        {
            int val = 5;

            a = ( IntPtr )val;
            b = ( IntPtr )val;
        }

        private static void SelfTest__Test__Pointers_PassByRef( )
        {
            IntPtr a = ( IntPtr )10;
            IntPtr b = ( IntPtr )11;

            PassByRef( ref a, ref b );

            SELFTEST_ASSERT( a.ToInt32( ) == 5 );
            SELFTEST_ASSERT( b.ToInt32( ) == 5 ); 
            SELFTEST_ASSERT( ( a.ToInt32( ) == b.ToInt32( ) ) );
            SELFTEST_ASSERT( ( a.ToInt32( ) + b.ToInt32( ) ) == 10);
        }

        private static void PassByRef( ref IntPtr a, ref IntPtr b )
        {
            int val = 5;

            a = ( IntPtr )val;
            b = ( IntPtr )val;
        }

        #endregion pointers basic

        //--//
        //--//
        //--//

        #region pointers advanced

        private struct Foo
        {
            public IntPtr   barSigned;
            public int      dummyNativeInt;
            public byte     dummy;
            public Int32    dummyInt;
            public UIntPtr  barUnsigned;
        }

 
        private static void SelfTest__Test__ValueType_Pointer_ByteDummy( )
        {
            byte b = ( byte )0xFF;

            Foo foo1;
            foo1.barSigned = ( IntPtr )10;
            foo1.dummyNativeInt = 7;
            foo1.dummy = b;
            foo1.dummyInt = 15;
            foo1.barUnsigned = ( UIntPtr )20;

            Foo foo2;
            foo2.barSigned = ( IntPtr )5;
            foo2.dummyNativeInt = 7;
            foo2.dummy = b;
            foo2.dummyInt = 15;
            foo2.barUnsigned = ( UIntPtr )15;

            SELFTEST_ASSERT( ( ( int )foo1.barSigned ) == ( ( int )foo2.barSigned ) + 5 );
            SELFTEST_ASSERT( ( ( int )foo1.barUnsigned ) == ( ( int )foo2.barUnsigned ) + 5 );
            SELFTEST_ASSERT( foo1.dummy == foo2.dummy );
            SELFTEST_ASSERT( foo1.dummyInt == foo2.dummyInt );
            SELFTEST_ASSERT( foo1.dummyNativeInt == foo2.dummyNativeInt );

            PassByRef( ref foo1, ref foo2 );

            SELFTEST_ASSERT( ( ( int )foo1.barSigned ) == ( ( int )foo2.barSigned ) + 5 );
            SELFTEST_ASSERT( ( ( int )foo1.barUnsigned ) == ( ( int )foo2.barUnsigned ) + 5 );
            SELFTEST_ASSERT( foo1.dummy == foo2.dummy );
            SELFTEST_ASSERT( foo1.dummyInt == foo2.dummyInt + 5 );
            SELFTEST_ASSERT( foo1.dummyNativeInt == foo2.dummyNativeInt + 5 );
        }

        private static void PassByRef( ref Foo foo1, ref Foo foo2 )
        {
            foo1.barSigned = ( IntPtr )50;
            foo1.barUnsigned = ( UIntPtr )60;
            foo1.dummyInt = 65;
            foo1.dummyNativeInt = ( Int32 )75;

            foo2.barSigned = ( IntPtr )45;
            foo2.barUnsigned = ( UIntPtr )55;
            foo2.dummyInt = 60;
            foo2.dummyNativeInt = ( Int32 )70;
        }

        private unsafe struct FooBytePointer
        {
            public IntPtr   barSigned;
            public byte*    dummy;
            public UIntPtr  barUnsigned;
        }

        private static unsafe void SelfTest__Test__ValueType_Pointer_BytePointerDummy( )
        {
            byte b = ( byte )0xFF;

            FooBytePointer foo1;
            foo1.barSigned = ( IntPtr )10;
            foo1.dummy = &b;
            foo1.barUnsigned = ( UIntPtr )20;

            FooBytePointer foo2;
            foo2.barSigned = ( IntPtr )5;
            foo2.dummy = &b;
            foo2.barUnsigned = ( UIntPtr )15;

            SELFTEST_ASSERT( ( ( int )foo1.barSigned ) == ( ( int )foo2.barSigned ) + 5 );
            SELFTEST_ASSERT( ( ( int )foo1.barUnsigned ) == ( ( int )foo2.barUnsigned ) + 5 );
            SELFTEST_ASSERT( foo1.dummy == foo2.dummy );

            PassByRef( ref foo1, ref foo2 );

            SELFTEST_ASSERT( ( ( int )foo1.barSigned ) == ( ( int )foo2.barSigned ) + 5 );
            SELFTEST_ASSERT( ( ( int )foo1.barUnsigned ) == ( ( int )foo2.barUnsigned ) + 5 );
            SELFTEST_ASSERT( foo1.dummy == foo2.dummy );
        }

        private static void PassByRef( ref FooBytePointer foo1, ref FooBytePointer foo2 )
        {
            foo1.barSigned = ( IntPtr )50;
            foo1.barUnsigned = ( UIntPtr )60;

            foo2.barSigned = ( IntPtr )45;
            foo2.barUnsigned = ( UIntPtr )55;
        }

        #endregion pointers advanced

        //--//
        //--//
        //--//

        #region value types advanced

        private struct Mixed
        {
            public int     ii;
            public byte    bb;
            public long    ll;

            public static Mixed operator +( Mixed a, Mixed b )
            {
                Mixed c;

                c.ii = a.ii + b.ii;
                c.bb = ( byte )( a.bb + b.bb );
                c.ll = a.ll + b.ll;

                return c;
            }
        }

        private static void SelfTest__Test__ValueTypes_PassByValue( )
        {
            Mixed a;
            a.ii = 5;
            a.bb = 10;
            a.ll = 20;

            const int delta = 1;

            Mixed b;
            b.ii = 5 + delta;
            b.bb = 10 + delta;
            b.ll = 5 + delta;

            Mixed c = PassByValue( a, b );

            SELFTEST_ASSERT( c.ii == a.ii + b.ii );
            SELFTEST_ASSERT( c.bb == a.bb + b.bb );
            SELFTEST_ASSERT( c.ll == a.ll + b.ll );
        }

        private static Mixed PassByValue( Mixed a, Mixed b )
        {
            return a + b;
        }

        private static void SelfTest__Test__ValueTypes_PassByRef( )
        {
            Mixed a;
            a.ii = 5;
            a.bb = 10;
            a.ll = 20;

            const int delta = 1;

            Mixed b;
            b.ii = 5 + delta;
            b.bb = 10 + delta;
            b.ll = 5 + delta;

            Mixed aa = a;
            Mixed bb = b;

            Mixed c = PassByRef( ref a, ref b );

            Mixed d = aa + bb;

            SELFTEST_ASSERT( c.ii == a.ii + b.ii );
            SELFTEST_ASSERT( c.bb == a.bb + b.bb );
            SELFTEST_ASSERT( c.ll == a.ll + b.ll );

            SELFTEST_ASSERT( d.ii == aa.ii + bb.ii );
            SELFTEST_ASSERT( d.bb == aa.bb + bb.bb );
            SELFTEST_ASSERT( d.ll == aa.ll + bb.ll );
        }

        private static Mixed PassByRef( ref Mixed a, ref Mixed b )
        {
            a += b;
            b += a;

            return a + b;
        }

        #endregion value types advanced

        //--//
        //--//
        //--//
        

        #region integers

        private static void SelfTest__Test__Integers_Conversions( )
        {
            #region up

            #region signed

            //
            // up - int to long
            //
            int a3 = 10;
            long b3 = a3;
            SELFTEST_ASSERT( a3 == b3 );

            //
            // up - short to int
            //
            short a4 = 10;
            int b4 = a4;
            SELFTEST_ASSERT( a4 == b4 );

            //
            // up - sbyte to int
            //
            sbyte a5 = 10;
            int b5 = a5;
            SELFTEST_ASSERT( a5 == b5 );

            #endregion signed

            #region unsigned

            //
            // up - uint to ulong
            //
            uint a31 = 10;
            ulong b31 = a31;
            SELFTEST_ASSERT( a31 == b31 );

            //
            // up - ushort to uint
            //
            ushort a41 = 10;
            uint b41 = a41;
            SELFTEST_ASSERT( a41 == b41 );

            //
            // up - byte to uint
            //
            byte a51 = 10;
            uint b51 = a51;
            SELFTEST_ASSERT( a51 == b51 );

            #endregion unsigned

            #endregion up

            #region down

            #region signed

            //
            // down - long to int
            //
            long a = 10;
            int b = ( int )a;
            SELFTEST_ASSERT( a == b );

            //
            // down - int to short
            //
            int a0 = 10;
            short b0 = ( short )a;
            SELFTEST_ASSERT( a0 == b0 );

            //
            // down - int to byte
            //
            int a1 = 10;
            byte b1 = ( byte )a1;
            SELFTEST_ASSERT( a1 == b1 );

            #endregion signed

            #region unsigned

            //
            // down - ulong to uint
            //
            ulong a6 = 10;
            uint b6 = ( uint )a6;
            SELFTEST_ASSERT( a6 == b6 );

            //
            // down - uint to ushort
            //
            uint a7 = 10;
            ushort b7 = ( ushort )a7;
            SELFTEST_ASSERT( a7 == b7 );

            //
            // down - uint to byte
            //
            uint a8 = 10;
            byte b8 = ( byte )a8;
            SELFTEST_ASSERT( a8 == b8 );

            #endregion unsigned

            #endregion down
        }

        //--//
        //--//
        //--//

        private static void SelfTest__Test__Integers_PassByValue( )
        {
            int a = get_a_number();
            int b = get_a_number();

            SELFTEST_ASSERT( a + b == PassByValue( a, b ) );
        }

        private static int PassByValue( int a, int b )
        {
            return a + b;
        }

        private static void SelfTest__Test__Integers_PassByRef( )
        {
            int a = get_a_number();
            int b = get_a_number();

            int c = a + b;

            PassByRef( ref a, ref b );

            SELFTEST_ASSERT( c + 3 == a + b );
        }

        private static void PassByRef( ref int a, ref int b )
        {
            a += 1;
            b += 2;
        }

        //--//
        //--//
        //--//

        private static void SelfTest__Test__Integers_BoxUnbox( )
        {
            int a = 20;
            int b = 22;

            PassByRef( ref a, ref b ); SELFTEST_ASSERT( 45 == a + b );
            PassByValue( a, b ); SELFTEST_ASSERT( 45 == a + b );
            PassByRef( ref a, ref b ); SELFTEST_ASSERT( 48 == a + b );
        }

        private static void SelfTest__Test__Integers_BoxUnboxInverted( )
        {
            int a = 20;
            int b = 22;

            PassByValue( a, b ); SELFTEST_ASSERT( 42 == a + b );
            PassByRef( ref a, ref b ); SELFTEST_ASSERT( 45 == a + b );
            PassByValue( a, b ); SELFTEST_ASSERT( 45 == a + b );
        }

        #endregion integers
    }
}
