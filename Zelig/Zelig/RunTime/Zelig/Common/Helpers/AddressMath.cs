//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    using RT = Microsoft.Zelig.Runtime;


    public static class AddressMath
    {
        [RT.Inline]
        public static int Compare( UIntPtr left  ,
                                   UIntPtr right )
        {
            return left.ToUInt32().CompareTo( right.ToUInt32() );
        }

        [RT.Inline]
        public static bool IsLessThan( UIntPtr left  ,
                                       UIntPtr right )
        {
            return left.ToUInt32() < right.ToUInt32();
        }

        [RT.Inline]
        public static bool IsLessThanOrEqual( UIntPtr left  ,
                                              UIntPtr right )
        {
            return left.ToUInt32() <= right.ToUInt32();
        }

        [RT.Inline]
        public static bool IsGreaterThan( UIntPtr left  ,
                                          UIntPtr right )
        {
            return left.ToUInt32() > right.ToUInt32();
        }

        [RT.Inline]
        public static bool IsGreaterThanOrEqual( UIntPtr left  ,
                                                 UIntPtr right )
        {
            return left.ToUInt32() >= right.ToUInt32();
        }

        [RT.Inline]
        public static bool IsInRange( UIntPtr value      ,
                                      UIntPtr rangeStart ,
                                      UIntPtr rangeEnd   )
        {
            return rangeStart.ToUInt32() <= value.ToUInt32() && value.ToUInt32() < rangeEnd.ToUInt32();
        }

        //--//

        [RT.Inline]
        public static UIntPtr Max( UIntPtr left  ,
                                   UIntPtr right )
        {
            return new UIntPtr( Math.Max( left.ToUInt32(), right.ToUInt32() ) );
        }


        [RT.Inline]
        public static UIntPtr Min( UIntPtr left  ,
                                   UIntPtr right )
        {
            return new UIntPtr( Math.Min( left.ToUInt32(), right.ToUInt32() ) );
        }

        //--//

        [RT.Inline]
        public static int Distance( UIntPtr start ,
                                    UIntPtr end   )
        {
            return (int)end.ToUInt32() - (int)start.ToUInt32();
        }

        [RT.Inline]
        public static uint RangeSize( UIntPtr start ,
                                      UIntPtr end   )
        {
            return end.ToUInt32() - start.ToUInt32();
        }

        //--//

        [RT.Inline]
        public unsafe static UIntPtr Increment( UIntPtr ptr    ,
                                                uint    offset )
        {
            return new UIntPtr( (byte*)ptr.ToPointer() + offset );
        }

        [RT.Inline]
        public unsafe static UIntPtr Decrement( UIntPtr ptr    ,
                                                uint    offset )
        {
            return new UIntPtr( (byte*)ptr.ToPointer() - offset );
        }

        //--//

        [RT.Inline]
        public static uint AlignToWordBoundary( uint value )
        {
            return (value + (sizeof(uint)-1)) & ~(uint)(sizeof(uint)-1);
        }

        public static uint AlignToBoundary( uint value     ,
                                            uint alignment )
        {
            uint off = value % alignment;

            return off != 0 ? value + alignment - off : value;
        }

        public static UIntPtr AlignToBoundary( UIntPtr value     ,
                                               uint    alignment )
        {
            uint offset = value.ToUInt32() % alignment;

            if(offset == 0)
            {
                return value;
            }

            return AddressMath.Increment( value, alignment - offset );
        }

        //--//

        [RT.Inline]
        public static UIntPtr AlignToLowerBoundary( UIntPtr value     ,
                                                    uint    alignment )
        {
            return new UIntPtr( value.ToUInt32() & ~(alignment - 1) );
        }

        [RT.Inline]
        public static uint IndexRelativeToLowerBoundary( UIntPtr value     ,
                                                         uint    alignment )
        {
            return value.ToUInt32() / alignment;
        }

        //--//--//

        [RT.Inline]
        public unsafe static bool IsAlignedTo64bits( void* ptr )
        {
            return IsAlignedTo64bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo64bits( uint ptr )
        {
            return IsAlignedTo64bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo64bits( UIntPtr ptr )
        {
            return (ptr.ToUInt32() & (sizeof(ulong)-1)) == 0;
        }

        //--//

        [RT.Inline]
        public unsafe static bool IsAlignedTo32bits( void* ptr )
        {
            return IsAlignedTo32bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo32bits( uint ptr )
        {
            return IsAlignedTo32bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo32bits( UIntPtr ptr )
        {
            return (ptr.ToUInt32() & (sizeof(uint)-1)) == 0;
        }

        //--//

        [RT.Inline]
        public unsafe static bool IsAlignedTo16bits( void* ptr )
        {
            return IsAlignedTo16bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo16bits( uint ptr )
        {
            return IsAlignedTo16bits( new UIntPtr( ptr ) );
        }

        [RT.Inline]
        public static bool IsAlignedTo16bits( UIntPtr ptr )
        {
            return (ptr.ToUInt32() & (sizeof(ushort)-1)) == 0;
        }

        //--//

        [RT.Inline]
        public static bool IsMultipleOf64bits( int value )
        {
            return (value & (sizeof(ulong)-1)) == 0;
        }

        [RT.Inline]
        public static bool IsMultipleOf32bits( int value )
        {
            return (value & (sizeof(uint)-1)) == 0;
        }

        [RT.Inline]
        public static bool IsMultipleOf16bits( int value )
        {
            return (value & (sizeof(ushort)-1)) == 0;
        }
    }
}
