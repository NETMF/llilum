// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Threading
{
    using System;
////using System.Security.Permissions;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;

    // After much discussion, we decided the Interlocked class doesn't need
    // any HPA's for synchronization or external threading.  They hurt C#'s
    // codegen for the yield keyword, and arguably they didn't protect much.
    // Instead, they penalized people (and compilers) for writing threadsafe
    // code.
    public static class Interlocked
    {
        /******************************
         * Increment
         *   Implemented: int
         *                        long
         *****************************/

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern int Increment( ref int location );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern long Increment( ref long location );

        /******************************
         * Decrement
         *   Implemented: int
         *                        long
         *****************************/

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern int Decrement( ref int location );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern long Decrement( ref long location );

        /******************************
         * Exchange
         *   Implemented: int
         *                        long
         *                        float
         *                        double
         *                        Object
         *                        IntPtr
         *****************************/

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern int Exchange( ref int location1, int value );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern long Exchange( ref long location1, long value );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern float Exchange( ref float location1, float value );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern double Exchange( ref double location1, double value );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern Object Exchange( ref Object location1, Object value );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern IntPtr Exchange( ref IntPtr location1, IntPtr value );


////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern T Exchange<T>( ref T location1, T value ) where T : class;
////    {
////        _Exchange( __makeref(location1), __makeref(value) );
////        //Since value is a local we use trash its data on return
////        //  The Exchange replaces the data with new data
////        //  so after the return "value" contains the original location1
////        //See ExchangeGeneric for more details
////        return value;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    private static extern void _Exchange( TypedReference location1, TypedReference value );

        /******************************
         * CompareExchange
         *    Implemented: int
         *                         long
         *                         float
         *                         double
         *                         Object
         *                         IntPtr
         *****************************/

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern int CompareExchange( ref int location1, int value, int comparand );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern long CompareExchange( ref long location1, long value, long comparand );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern float CompareExchange( ref float location1, float value, float comparand );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern double CompareExchange( ref double location1, double value, double comparand );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern Object CompareExchange( ref Object location1, Object value, Object comparand );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern IntPtr CompareExchange( ref IntPtr location1, IntPtr value, IntPtr comparand );

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern T CompareExchange<T>( ref T location1, T value, T comparand ) where T : class;
////    {
////        _CompareExchange( __makeref(location1), __makeref(value), comparand );
////        //Since value is a local we use trash its data on return
////        //  The Exchange replaces the data with new data
////        //  so after the return "value" contains the original location1
////        //See CompareExchangeGeneric for more details
////        return value;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    private static extern void _CompareExchange( TypedReference location1, TypedReference value, Object comparand );

        /******************************
         * Add
         *    Implemented: int
         *                         long
         *****************************/

////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal static extern int ExchangeAdd( ref int location1, int value );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern long ExchangeAdd( ref long location1, long value );

        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern int Add( ref int location1, int value );
////    {
////        return ExchangeAdd( ref location1, value ) + value;
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern long Add( ref long location1, long value );
////    {
////        return ExchangeAdd( ref location1, value ) + value;
////    }

        /******************************
         * Read
         *****************************/

        public static long Read( ref long location )
        {
            return Interlocked.CompareExchange( ref location, 0, 0 );
        }
    }
}
