//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    // BrickTable is a helper used by the mark and sweep GC to map an internal pointer to a known 
    // heap object pointer. It works by dividing all the heaps into multiple pages of size c_PageSize, and
    // mapping each page to a short in m_bricks. The short value indicates the offset from the beginning
    // of the page to the first valid heap object in that page, or, in the case where the beginning of 
    // the page is at the middle of a valid heap object, the offset (which will be negative) to that heap
    // object.
    //
    // For example: 
    //   (Assume the page size is 10 for the purpose of this example and easy math)
    //
    //        0                   1                   2                   3
    // Addr:  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 
    // Heap:  [Free][HeapObj1][HeapObj2                    ][Free                            ]
    // Pages: [        3         ][        -2        ][       -12        ][     uninit       ]
    //
    public class BrickTable
    {
        private const uint c_PageSize                             = 2048;

        private const int  c_BrickEncoding_Uninitialized          = 0x33;
        private const int  c_BrickEncoding_MaxBacktrackingOffset  = short.MinValue;
        private const int  c_BrickEncoding_SafeBacktrackingOffset = c_BrickEncoding_MaxBacktrackingOffset / 2;


        //
        // State
        //

        private uint    m_lowestAddress;
        private uint    m_maxRelativeAddress;
        private short[] m_bricks;

        //
        // Helper Methods
        //

        public void Initialize( uint lowestAddress  ,
                                uint highestAddress )
        {
            uint numPages = ((highestAddress - lowestAddress) / c_PageSize);

            //
            // Allocate one extra page, to make it easier to deal with corner cases.
            //
            numPages = AddressMath.AlignToWordBoundary( numPages + 1 );

            //
            // Important: initialize the address range last, because the allocation of the array will cause an invocation of MarkObject.
            //
            m_bricks             = new short[numPages];
            m_lowestAddress      = lowestAddress;
            m_maxRelativeAddress = highestAddress - lowestAddress;
        }

        [TS.WellKnownMethod( "DebugBrickTable_Reset" )]
        public unsafe void Reset()
        {
            ArrayImpl array = ArrayImpl.CastAsArray( m_bricks );

            uint* start = array.GetDataPointer   ();
            uint* end   = array.GetEndDataPointer();
                          
            Memory.Fill( (byte*)start, (int)(end - start) * sizeof(uint), c_BrickEncoding_Uninitialized );
        }

        [DisableBoundsChecks(ApplyRecursively=true)]
        [DisableNullChecks  (ApplyRecursively=true)]
        [TS.WellKnownMethod( "DebugBrickTable_MarkObject" )]
        public unsafe void MarkObject( UIntPtr objectPtr  ,
                                       uint    objectSize )
        {
            uint address    = objectPtr.ToUInt32();
            uint relAddress = address - m_lowestAddress;

            if(relAddress < m_maxRelativeAddress)
            {
                int    startOffset = (int)(relAddress % c_PageSize);
                short* ptr         =      GetPagePointer( relAddress                  );
                short* endPagePtr  =      GetPagePointer( relAddress + objectSize - 1 );

                //
                // Only update if the new pointer is lower than the previous one.
                // This automatically covers the case of an uninitialized brick.
                //
                {
                    int value = *ptr;
                    if(value > startOffset)
                    {
                        *ptr = (short)startOffset;
                    }
                }

                //
                // If an object straddles more than one page, mark all the covered pages with a backtrack marker.
                //
                while(++ptr <= endPagePtr)
                {
                    startOffset -= (int)c_PageSize;

                    //
                    // Limit offset to fit in the brick slot.
                    //
                    if(startOffset <= c_BrickEncoding_MaxBacktrackingOffset)
                    {
                        startOffset = c_BrickEncoding_MaxBacktrackingOffset;
                    }

                    *ptr = (short)startOffset;
                }
            }
        }

        [DisableBoundsChecks(ApplyRecursively=true)]
        [DisableNullChecks  (ApplyRecursively=true)]
        [TS.WellKnownMethod( "DebugBrickTable_FindLowerBoundForObjectPointer" )]
        public unsafe UIntPtr FindLowerBoundForObjectPointer( UIntPtr interiorPtr )
        {
            uint address    = interiorPtr.ToUInt32();
            uint relAddress = address - m_lowestAddress;

            if(relAddress < m_maxRelativeAddress)
            {
                uint page = relAddress / c_PageSize;

                fixed(short* firstPagePtr = &m_bricks[0])
                {
                    short* ptr         = &firstPagePtr[page];
                    uint   pageAddress = m_lowestAddress + page * c_PageSize;

                    while(firstPagePtr <= ptr)
                    {
                        int value = *ptr;

                        //
                        // Offset too large, move by a few pages and retry.
                        //
                        if(value < c_BrickEncoding_SafeBacktrackingOffset)
                        {
                            ptr         += c_BrickEncoding_SafeBacktrackingOffset / c_PageSize;
                            pageAddress  = (uint)(pageAddress + c_BrickEncoding_SafeBacktrackingOffset);
                            continue;
                        }

                        if(value > c_PageSize)
                        {
                            break;
                        }

                        uint firstAddress = (uint)(pageAddress + value);
                        if(firstAddress <= address)
                        {
                            return new UIntPtr( firstAddress );
                        }

                        //
                        // The interior pointer is less than the first object's address,
                        // it could be pointing into an object from the previous page.
                        //
                        ptr         -= 1;
                        pageAddress -= c_PageSize;
                    }
                }
            }

            return UIntPtr.Zero;
        }

        [Inline]
        private unsafe short* GetPagePointer( uint relAddress )
        {
            uint startPage = relAddress / c_PageSize;

            fixed(short* startPagePtr = &m_bricks[startPage])
            {
                return startPagePtr;
            }
        }

        //
        // Access Methods
        //

        public static extern BrickTable Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
