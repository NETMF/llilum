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
        public static unsafe extern int GetANumber( );
        
        [DllImport( "C" )]
        public static unsafe extern void BreakWithTrap( );
        
        [DllImport( "C" )]
        public static unsafe extern void Breakpoint( uint n );
        
        internal static bool AlwaysTrueNonOptimizableCondition()
        {
            return GetANumber( ) >= 42;
        }

        internal static bool AlwaysFalseNonOptimizableCondition()
        {
            return GetANumber( ) < 42;
        }

        internal static void NonOptimizableCall()
        {
            SELFTEST_ASSERT( AlwaysTrueNonOptimizableCondition() );
        }

        //
        // Zelig Self Test - Early bootstrap, no heap:
        // integers, pointers and value types
        //
        [NoInline]
        internal static unsafe void SELFTEST_ASSERT( bool expression )
        {
            if( !expression )
            {
                BugCheck.Log( "!!! TEST FAILED !!!" );

                BreakWithTrap( ); 
            }
        }

        [NoInline]
        internal static unsafe void SELFTEST_LOG( string format )
        {
            BugCheck.Log( format );
        }

        [NoInline]
        internal static unsafe void SELFTEST_LOG( string format, int p1 )
        {
            BugCheck.Log( format, p1 );
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
            int seed = GetANumber() % mask;

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

        internal static void SelfTest__Memory()
        {
            SelfTest__Memory__BasicAllocation1();
            SelfTest__Memory__BasicAllocation2();
            SelfTest__Memory__BasicAllocation3();
            SelfTest__Memory__GapPlugTest1();
            SelfTest__Memory__GapPlugTest2();
            SelfTest__Memory__GapPlugTest3();
            SelfTest__Memory__GapPlugTest4();
            SelfTest__Memory__GapPlugTest5();
            SelfTest__Memory__GapPlugTest6();
            SelfTest__Memory__Random();

            SelfTest__Memory__AddrefReleaseSetup( );

            SelfTest__Memory__BasicAddrefRelease1( );
            SelfTest__Memory__BasicAddrefRelease2( );
            SelfTest__Memory__BasicAddrefRelease3( );
            SelfTest__Memory__BasicAddrefRelease4( );
            SelfTest__Memory__BasicAddrefRelease5( );
            SelfTest__Memory__BasicAddrefRelease6( );
            SelfTest__Memory__BasicAddrefRelease7( );

            BugCheck.Log("!!DONE!!");

            // Trap end of tests
            BreakWithTrap();

        }

        #region memory tests

        private const uint ArrayFixedSize = 3 * sizeof(uint); // 2 for ObjectHelper (MultiUseWord and VTable), 1 for length
        private const uint StandardAllocSize = 32;
        private const uint GapSize = (ArrayFixedSize - sizeof(uint)); // Not enough for a free block
        private const uint AllocSizeForGap = StandardAllocSize - GapSize;

        private static UIntPtr AllocHelper(uint size)
        {
            var allocSize = (size < ArrayFixedSize + 1) ? ArrayFixedSize + 1 : size;
            byte[] array = new byte[allocSize - ArrayFixedSize];
            
            for (uint i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(i & 0xff);
            }

            var oh = ObjectHeader.Unpack(array);
            var ptr = oh.ToPointer();
            BugCheck.Log("Allocated oh = 0x%x of total size %d", (int)ptr.ToUInt32(), (int)oh.Size);

            return ptr;
        }

        private static UIntPtr AllocHelperWithFakeGap(uint size)
        {
            var allocSize = (size < ArrayFixedSize + 1) ? ArrayFixedSize + 1 : size;
            uint[] array = new uint[(allocSize - ArrayFixedSize + sizeof(uint) - 1)/sizeof(uint)];

            for (uint i = 0; i < array.Length; i++)
            {
                array[i] = (uint)ObjectHeader.GarbageCollectorFlags.GapPlug;
            }

            var oh = ObjectHeader.Unpack(array);
            var ptr = oh.ToPointer();
            BugCheck.Log("Allocated oh = 0x%x of total size %d with fake gaps", (int)ptr.ToUInt32(), (int)oh.Size);

            return ptr;
        }

        // [Free][Obj1][*] // Alloc Obj1
        // [Free      ][*] // Free Obj1 <=== Merge free block with previous free block
        private static void SelfTest__Memory__BasicAllocation1()
        {
            BugCheck.Log("BasicAllocation1 Started...");

            var obj1 = AllocHelper(StandardAllocSize);

            MemoryManager.Instance.ConsistencyCheck();

            MemoryManager.Instance.Release(obj1);

            MemoryManager.Instance.ConsistencyCheck();

            BugCheck.Log("BasicAllocation1 Succeeded.");
        }

        //[Free][Obj2][Obj1][*] // Alloc Obj1, Obj2
        //[Free][Obj2][Free][*] // Free Obj1 <=== Merge free block alone
        //[Free            ][*] // Free Obj2 <=== Merge free block with previous and next free block
        private static void SelfTest__Memory__BasicAllocation2()
        {
            BugCheck.Log("BasicAllocation2 Started...");

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(StandardAllocSize);
            
            MemoryManager.Instance.Release(obj1);

            MemoryManager.Instance.ConsistencyCheck();

            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            BugCheck.Log("BasicAllocation2 Succeeded.");
        }

        //[Free][Obj3][Obj2][Obj1][*] // Alloc Obj1, Obj2, Obj3
        //[Free][Obj3][Obj2][Free][*] // Free Obj1
        //[Free][Obj3][Free      ][*] // Free Obj2 <=== Merge free block with next free block
        //[Free                  ][*] // Free Obj3
        private static void SelfTest__Memory__BasicAllocation3()
        {
            BugCheck.Log("BasicAllocation3 Started...");

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(StandardAllocSize);
            var obj3 = AllocHelper(StandardAllocSize);

            MemoryManager.Instance.Release(obj1);
            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            // Clean up
            MemoryManager.Instance.Release(obj3);

            MemoryManager.Instance.ConsistencyCheck();
            BugCheck.Log("BasicAllocation3 Succeeded.");
        }
        
        //[gap][Obj1][*] // Alloc Obj1 <== Alloc with gaps
        //[Free     ][*] // Free Obj1 <== Merge free block with previous gaps
        private static unsafe void SelfTest__Memory__GapPlugTest1()
        {
            BugCheck.Log("GapPlugTest1 Started...");

            MemoryManager.Instance.ConsistencyCheck();

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;
            
            var obj1 = AllocHelper(firstFreeBlockSize - GapSize);

            MemoryManager.Instance.ConsistencyCheck();

            MemoryManager.Instance.Release(obj1);

            MemoryManager.Instance.ConsistencyCheck();
            
            BugCheck.Log("GapPlugTest1 Succeeded.");
        }

        //[Obj2][Obj1     ][*] // Alloc Obj1, Obj2
        //[Obj2][Free     ][*] // Free Obj1
        //[Obj2][gap][Obj3][*] // Alloc Obj3
        //[Free     ][Obj3][*] // Free Obj2 <== 
        private static unsafe void SelfTest__Memory__GapPlugTest2()
        {
            BugCheck.Log("GapPlugTest2 Started...");

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(firstFreeBlockSize - StandardAllocSize); // Fill up the first block
            
            MemoryManager.Instance.Release(obj1);

            var obj3 = AllocHelper(AllocSizeForGap);

            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            // Cleanup
            MemoryManager.Instance.Release(obj3);

            MemoryManager.Instance.ConsistencyCheck();
            BugCheck.Log("GapPlugTest2 Succeeded.");
        }

        //[gap][Obj2][Obj1     ][*] // Alloc Obj1, Obj2
        //[gap][Obj2][Free     ][*] // Free Obj1
        //[gap][Obj2][gap][Obj3][*] // Alloc Obj3
        //[Free          ][Obj3][*] // Free Obj2 <== 
        private static unsafe void SelfTest__Memory__GapPlugTest3()
        {
            BugCheck.Log("GapPlugTest3 Started...");

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(firstFreeBlockSize - StandardAllocSize - GapSize);

            MemoryManager.Instance.Release(obj1);

            var obj3 = AllocHelper(AllocSizeForGap);

            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            // Cleanup
            MemoryManager.Instance.Release(obj3);
            MemoryManager.Instance.ConsistencyCheck();
            BugCheck.Log("GapPlugTest3 Succeeded.");
        }

        //[Obj2][Obj1     ][*] // Alloc Obj1, Obj2 (filled with fake gaps)
        //[Obj2][Free     ][*] // Free Obj1
        //[Obj2][gap][Obj3][*] // Alloc Obj3
        //[Obj2][Free     ][*] // Free Obj3 <== Merge with previous gaps following fake gaps
        private static unsafe void SelfTest__Memory__GapPlugTest4()
        {
            BugCheck.Log("GapPlugTest4 Started...");

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;

            var obj1 = AllocHelper(firstFreeBlockSize - StandardAllocSize);
            var obj2 = AllocHelperWithFakeGap(StandardAllocSize); // Fill up the first block

            MemoryManager.Instance.Release(obj1);

            var obj3 = AllocHelper(firstFreeBlockSize - StandardAllocSize - GapSize);

            MemoryManager.Instance.Release(obj3);

            MemoryManager.Instance.ConsistencyCheck();

            // Cleanup
            MemoryManager.Instance.Release(obj2);
            MemoryManager.Instance.ConsistencyCheck();
            BugCheck.Log("GapPlugTest4 Succeeded.");
        }

        //[gap][Obj2][Obj1     ][*] // Alloc Obj1, Obj2
        //[gap][Obj2][Free     ][*] // Free Obj1
        //[Free                ][*] // Free Obj2 <== Merge with previous gaps and next free block
        private static unsafe void SelfTest__Memory__GapPlugTest5()
        {
            BugCheck.Log("GapPlugTest5 Started...");

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(firstFreeBlockSize - StandardAllocSize - GapSize);

            MemoryManager.Instance.Release(obj1);
            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            BugCheck.Log("GapPlugTest5 Succeeded.");
        }

        //[Free][Obj2][Obj1     ][*] // Alloc Obj1, Obj2
        //[Free][Obj2][Free     ][*] // Free Obj1
        //[Free][Obj2][gap][Obj3][*] // Alloc Obj3
        //[Free           ][Obj3][*] // Free Obj2 <== Merge with previous free block and next gaps
        private static unsafe void SelfTest__Memory__GapPlugTest6()
        {
            BugCheck.Log("GapPlugTest6 Started...");

            var obj1 = AllocHelper(StandardAllocSize);
            var obj2 = AllocHelper(StandardAllocSize);

            MemoryManager.Instance.Release(obj1);

            var obj3 = AllocHelper(AllocSizeForGap);

            MemoryManager.Instance.Release(obj2);

            MemoryManager.Instance.ConsistencyCheck();

            // Cleanup
            MemoryManager.Instance.Release(obj3);
            MemoryManager.Instance.ConsistencyCheck();
            BugCheck.Log("GapPlugTest6 Succeeded.");
        }

        private static unsafe void SelfTest__Memory__Random()
        {
            BugCheck.Log("Random Started...");

            UIntPtr[] array = new UIntPtr[100];
            Random rand = new Random();

            for (uint i = 0; i < array.Length; i++)
            {
                array[i] = UIntPtr.Zero;
            }

            uint firstFreeBlockSize = MemoryManager.Instance.StartOfHeap->FirstFreeBlock->AvailableMemory;

            int maxAllocSize = (int)(firstFreeBlockSize / 2 / array.Length);
            maxAllocSize = maxAllocSize - maxAllocSize % sizeof(uint);

            
            for (uint i = 0; i < 1000; i++)
            {
                var target = rand.Next(array.Length);
                if (array[target] == UIntPtr.Zero)
                {
                    uint allocSize = (uint)rand.Next(maxAllocSize);
                    bool useFakeGap = (rand.Next(2) == 0);

                    if (useFakeGap)
                    {
                        BugCheck.Log("%04d>>>> array[%d] = AllocHelperWithFakeGap(%d);", (int)i, target, (int)allocSize);
                        array[target] = AllocHelperWithFakeGap(allocSize);
                    }
                    else
                    {
                        BugCheck.Log("%04d>>>> array[%d] = AllocHelper(%d);", (int)i, target, (int)allocSize);
                        array[target] = AllocHelper(allocSize);
                    }
                }
                else
                {
                    BugCheck.Log("%04d>>>> MemoryManager.Instance.Release(array[%d]); array[%d] = UIntPtr.Zero;", (int)i, target, target);
                    MemoryManager.Instance.Release(array[target]);
                    array[target] = UIntPtr.Zero;
                }

                MemoryManager.Instance.ConsistencyCheck();
            }

            // Cleanup
            for (uint i = 0; i < array.Length; i++)
            {
                MemoryManager.Instance.Release(array[i]);
            }
            MemoryManager.Instance.Release(ObjectHeader.Unpack(rand).ToPointer());
            MemoryManager.Instance.Release(ObjectHeader.Unpack(array).ToPointer());

            MemoryManager.Instance.ConsistencyCheck();

            BugCheck.Log("Random Succeeded.");
        }

        #endregion

        #region AddRef/Release Tests

        private class Node
        {
            public Node _node1;
            public Node _node2;
            public Node[] _nodeArray;

            private Node( Node node1, Node node2, Node[] nodeArray )
            {
                _node1 = node1;
                _node2 = node2;
                _nodeArray = nodeArray;
            }

            static public Node New( out UIntPtr ptr, Node node1, Node node2 )
            {
                return New( out ptr, node1, node2, null );
            }
            static public Node New( out UIntPtr ptr, Node node1, Node node2, Node[] nodeArray )
            {
                Node node = new Node(node1, node2, nodeArray);
                ObjectHeader.Unpack( node ).InitializeReferenceCount( );

                if(node1 != null)
                {
                    ObjectHeader.Unpack( node1 ).AddReference( );
                }
                if(node2 != null)
                {
                    ObjectHeader.Unpack( node2 ).AddReference( );
                }
                if(nodeArray != null)
                {
                    ObjectHeader.Unpack( nodeArray ).AddReference( );
                }

                ptr = ObjectHeader.Unpack( node ).ToPointer( );

                return node;
            }

            static public Node[] NewArray( out UIntPtr ptr, int length, Node node0 )
            {
                return NewArray( out ptr, length, node0, null, null );
            }

            static public Node[] NewArray( out UIntPtr ptr, int length, Node node0, Node node1 )
            {
                return NewArray( out ptr, length, node0, node1, null );
            }

            static public Node[] NewArray( out UIntPtr ptr, int length, Node node0, Node node1, Node node2 )
            {
                Node[] nodeArray = new Node[length];
                ObjectHeader.Unpack( nodeArray ).InitializeReferenceCount( );

                if(length > 0)
                {
                    nodeArray[ 0 ] = node0;
                    if(node0 != null)
                    {
                        ObjectHeader.Unpack( node0 ).AddReference( );
                    }

                    if(length > 1)
                    {
                        nodeArray[ 1 ] = node1;
                        if(node1 != null)
                        {
                            ObjectHeader.Unpack( node1 ).AddReference( );
                        }

                        if(length > 2)
                        {
                            nodeArray[ 2 ] = node2;
                            if(node2 != null)
                            {
                                ObjectHeader.Unpack( node2 ).AddReference( );
                            }
                        }
                    }
                }

                ptr = ObjectHeader.Unpack( nodeArray ).ToPointer( );

                return nodeArray;
            }
        }

        private struct NodeStruct
        {
            public Node _node1;
            public Node _node2;
            public Node[] _nodeArray;

            public void Set( Node node1, Node node2 )
            {
                Set( node1, node2, null );
            }

            public void Set( Node node1, Node node2, Node[] nodeArray )
            {
                _node1 = node1;
                _node2 = node2;
                _nodeArray = nodeArray;

                if(node1 != null)
                {
                    ObjectHeader.Unpack( node1 ).AddReference( );
                }
                if(node2 != null)
                {
                    ObjectHeader.Unpack( node2 ).AddReference( );
                }
                if(nodeArray != null)
                {
                    ObjectHeader.Unpack( nodeArray ).AddReference( );
                }
            }

            static public NodeStruct[] NewArray( out UIntPtr ptr, int length )
            {
                NodeStruct[] nodeArray = new NodeStruct[length];
                ObjectHeader.Unpack( nodeArray ).InitializeReferenceCount( );
                ptr = ObjectHeader.Unpack( nodeArray ).ToPointer( );

                return nodeArray;
            }
        }

        private static void SelfTest__Memory__AddrefReleaseSetup( )
        {
            // Ensure ReleaseReferenceHelper is allocated.
            ThreadImpl.CurrentThread.ReleaseReference.GetType( );
        }

        // Releasing 1 single reference type
        private static void SelfTest__Memory__BasicAddrefRelease1( )
        {
            BugCheck.Log( "BasicAddrefRelease1 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            UIntPtr nodePtr;
            Object node = Node.New(out nodePtr, null, null);

            ObjectHeader.ReleaseReference( ref node );

            BugCheck.Assert( node == null, BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( nodePtr ), BugCheck.StopCode.HeapCorruptionDetected );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease1 Succeeded." );
        }

        // Cascading releases
        //      [1]
        //     /   \
        //   [2]   [3]
        //   /      /\
        // [4]    [5][6]
        private static void SelfTest__Memory__BasicAddrefRelease2( )
        {
            BugCheck.Log( "BasicAddrefRelease2 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            UIntPtr node4Ptr;
            Object node4 = Node.New(out node4Ptr, null, null);
            UIntPtr node5Ptr;
            Object node5 = Node.New(out node5Ptr, null, null);
            UIntPtr node6Ptr;
            Object node6 = Node.New(out node6Ptr, null, null);
            UIntPtr node2Ptr;
            Object node2 = Node.New(out node2Ptr, (Node)node4, null);
            UIntPtr node3Ptr;
            Object node3 = Node.New(out node3Ptr, (Node)node5, (Node)node6);
            UIntPtr node1Ptr;
            Object node1 = Node.New(out node1Ptr, (Node)node2, (Node)node3);

            ObjectHeader.ReleaseReference( ref node2 );
            ObjectHeader.ReleaseReference( ref node3 );
            ObjectHeader.ReleaseReference( ref node4 );
            ObjectHeader.ReleaseReference( ref node5 );
            ObjectHeader.ReleaseReference( ref node6 );

            ObjectHeader.ReleaseReference( ref node1 );

            BugCheck.Assert( node1 == null, BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node2Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node3Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node4Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node5Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node6Ptr ), BugCheck.StopCode.HeapCorruptionDetected );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease2 Succeeded." );
        }

        // Do not free objects that are not ref-counted 
        private static void SelfTest__Memory__BasicAddrefRelease3( )
        {
            BugCheck.Log( "BasicAddrefRelease3 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            Exception node2 = new Exception("node2", null);
            UIntPtr node2Ptr = ObjectHeader.Unpack(node2).ToPointer();
            Object node1 = new Exception("node1", node2);
            UIntPtr node1Ptr = ObjectHeader.Unpack(node1).ToPointer();
            ObjectHeader.Unpack( node1 ).InitializeReferenceCount( );
            node2 = null;

            ObjectHeader.ReleaseReference( ref node1 );

            BugCheck.Assert( node1 == null, BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node2Ptr ), BugCheck.StopCode.HeapCorruptionDetected );

            MemoryManager.Instance.Release( node2Ptr );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease3 Succeeded." );
        }

        // Releasing a reference to an array of a reference type
        // node2 ----------------------\
        // nodeRoot --> [ 0 ] [null] [ 2 ]
        //               / \__________/
        //             [1]
        private static void SelfTest__Memory__BasicAddrefRelease4( )
        {
            BugCheck.Log( "BasicAddrefRelease4 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            UIntPtr node2Ptr;
            Object node2 = Node.New(out node2Ptr, null, null);
            UIntPtr node1Ptr;
            Object node1 = Node.New(out node1Ptr, null, null);
            UIntPtr node0Ptr;
            Object node0 = Node.New(out node0Ptr, (Node)node1, (Node)node2);
            UIntPtr nodeArrayPtr;
            Object nodeArray = Node.NewArray(out nodeArrayPtr, 3, (Node)node0, null, (Node)node2);
            UIntPtr nodeRootPtr;
            Object nodeRoot = Node.New(out nodeRootPtr, null, null, (Node[])nodeArray);

            ObjectHeader.ReleaseReference( ref node0 );
            ObjectHeader.ReleaseReference( ref node1 );
            ObjectHeader.ReleaseReference( ref nodeArray );

            ObjectHeader.ReleaseReference( ref nodeRoot );

            BugCheck.Assert( nodeRoot == null, BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( nodeRootPtr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( nodeArrayPtr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node0Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node2Ptr ), BugCheck.StopCode.HeapCorruptionDetected );

            ObjectHeader.ReleaseReference( ref node2 );

            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node2Ptr ), BugCheck.StopCode.HeapCorruptionDetected );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease4 Succeeded." );
        }

        // Releasing reference to an array of a struct (value type)
        // nodeStructArray --> [  ] [  ] [  ]
        //                      /\    |   ||
        //                   [0]  [1] |   [2]
        //                           \|
        //                          emptyArray
        private static void SelfTest__Memory__BasicAddrefRelease5( )
        {
            BugCheck.Log( "BasicAddrefRelease5 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            UIntPtr node2Ptr;
            Object node2 = Node.New(out node2Ptr, null, null);
            UIntPtr emptyArrayPtr;
            Object emptyArray = Node.NewArray(out emptyArrayPtr, 0, null);
            UIntPtr node1Ptr;
            Object node1 = Node.New(out node1Ptr, null, null, (Node[])emptyArray);
            UIntPtr node0Ptr;
            Object node0 = Node.New(out node0Ptr, (Node)node1, (Node)node2);

            UIntPtr nodeStructArrayPtr;
            NodeStruct[] nodeStructArray = NodeStruct.NewArray(out nodeStructArrayPtr, 3);
            Object nodeStructArrayObj = nodeStructArray;

            nodeStructArray[ 0 ].Set( (Node)node0, (Node)node1 );
            nodeStructArray[ 1 ].Set( null, null, (Node[])emptyArray );
            nodeStructArray[ 2 ].Set( (Node)node2, (Node)node2 );
            nodeStructArray = null;

            ObjectHeader.ReleaseReference( ref node0 );
            ObjectHeader.ReleaseReference( ref node2 );
            ObjectHeader.ReleaseReference( ref emptyArray );

            ObjectHeader.ReleaseReference( ref nodeStructArrayObj );

            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( nodeStructArrayPtr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node0Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node2Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( emptyArrayPtr ), BugCheck.StopCode.HeapCorruptionDetected ); // Still held by node1

            ObjectHeader.ReleaseReference( ref node1 );

            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( emptyArrayPtr ), BugCheck.StopCode.HeapCorruptionDetected );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease5 Succeeded." );
        }

        // Releasing reference to a primitive array
        private static void SelfTest__Memory__BasicAddrefRelease6( )
        {
            BugCheck.Log( "BasicAddrefRelease6 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            int[] intArray = new int[10];
            ObjectHeader.Unpack( intArray ).InitializeReferenceCount( );
            UIntPtr intArrayPtr = ObjectHeader.Unpack(intArray).ToPointer();
            for(int i = 0; i < intArray.Length; i++)
            {
                intArray[ i ] = i;
            }
            Object intArrayObj = intArray;
            intArray = null;
            ObjectHeader.ReleaseReference( ref intArrayObj );
            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( intArrayPtr ), BugCheck.StopCode.HeapCorruptionDetected );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease6 Succeeded." );
        }

        // Circular reference
        //       [0]
        //       / \
        //   ,-[1] [2a]-.
        //   \_/    /    \
        //        [2b]   |
        //           \__/
        private static void SelfTest__Memory__BasicAddrefRelease7( )
        {
            BugCheck.Log( "BasicAddrefRelease7 Started..." );

            uint availableMemoryBefore = MemoryManager.Instance.AvailableMemory;
            BugCheck.Log( "Available memory before = %d", (int)availableMemoryBefore );

            // Node 1 points to itself
            UIntPtr node1Ptr;
            Object node1 = Node.New(out node1Ptr, null, null);
            ( (Node)node1 )._node1 = (Node)node1;
            ObjectHeader.Unpack( node1 ).AddReference( );

            // Node 2a and 2b creates a circular reference
            UIntPtr node2aPtr;
            Object node2a = Node.New(out node2aPtr, null, null);
            UIntPtr node2bPtr;
            Object node2b = Node.New(out node2bPtr, (Node)node2a, null);
            ( (Node)node2a )._node2 = (Node)node2b;
            ObjectHeader.Unpack( node2b ).AddReference( );

            UIntPtr node0Ptr;
            Object node0 = Node.New(out node0Ptr, (Node)node1, (Node)node2a);

            ObjectHeader.ReleaseReference( ref node1 );
            ObjectHeader.ReleaseReference( ref node2a );
            ObjectHeader.ReleaseReference( ref node2b );

            ObjectHeader.ReleaseReference( ref node0 );

            BugCheck.Assert( !MemoryManager.Instance.IsObjectAlive( node0Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node1Ptr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node2aPtr ), BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( MemoryManager.Instance.IsObjectAlive( node2bPtr ), BugCheck.StopCode.HeapCorruptionDetected );

            MemoryManager.Instance.Release( node1Ptr );
            MemoryManager.Instance.Release( node2aPtr );
            MemoryManager.Instance.Release( node2bPtr );

            uint availableMemoryAfter = MemoryManager.Instance.AvailableMemory;

            BugCheck.Log( "Available memory after = %d", (int)availableMemoryAfter );
            BugCheck.Assert( availableMemoryBefore == availableMemoryAfter, BugCheck.StopCode.HeapCorruptionDetected );

            BugCheck.Log( "BasicAddrefRelease7 Succeeded." );
        }

        #endregion

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
            int a = GetANumber();
            int b = GetANumber();

            SELFTEST_ASSERT( a + b == PassByValue( a, b ) );
        }

        private static int PassByValue( int a, int b )
        {
            return a + b;
        }

        private static void SelfTest__Test__Integers_PassByRef( )
        {
            int a = GetANumber();
            int b = GetANumber();

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
