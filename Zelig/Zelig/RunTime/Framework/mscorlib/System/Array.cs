// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  Array
**
** Purpose: Base class which can be used to access any array
**
===========================================================*/
namespace System
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
////using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;

    [Microsoft.Zelig.Internals.WellKnownType( "System_Array" )]
    [Serializable]
    public abstract class Array : ICloneable, IList
    {
        /// <internalonly/>
        private Array()
        {
        }

////    public static ReadOnlyCollection<T> AsReadOnly<T>( T[] array )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        // T[] implements IList<T>.
////        return new ReadOnlyCollection<T>( array );
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static void Resize<T>( ref T[] array, int newSize )
////    {
////        if(newSize < 0)
////        {
////            throw new ArgumentOutOfRangeException( "newSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        T[] larray = array;
////        if(larray == null)
////        {
////            array = new T[newSize];
////            return;
////        }
////
////        if(larray.Length != newSize)
////        {
////            T[] newArray = new T[newSize];
////
////            Array.Copy( larray, 0, newArray, 0, larray.Length > newSize ? newSize : larray.Length );
////
////            array = newArray;
////        }
////    }

        // Create instance will create an array
        public unsafe static Array CreateInstance( Type elementType, int length )
        {
            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(elementType == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "elementType" );
#else
                throw new ArgumentNullException();
#endif
            }

            throw new NotImplementedException();
////        RuntimeType t = elementType.UnderlyingSystemType as RuntimeType;
////        if(t == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "elementType" );
////        }
////
////        return InternalCreate( (void*)t.TypeHandle.Value, 1, &length, null );
        }

////    public unsafe static Array CreateInstance( Type elementType, int length1, int length2 )
////    {
////        if(length1 < 0 || length2 < 0)
////        {
////            throw new ArgumentOutOfRangeException( (length1 < 0 ? "length1" : "length2"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        if(elementType == null)
////        {
////            throw new ArgumentNullException( "elementType" );
////        }
////
////        RuntimeType t = elementType.UnderlyingSystemType as RuntimeType;
////        if(t == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "elementType" );
////        }
////
////        int* pLengths = stackalloc int[2];
////        pLengths[0] = length1;
////        pLengths[1] = length2;
////
////        return InternalCreate( (void*)t.TypeHandle.Value, 2, pLengths, null );
////    }
////
////    public unsafe static Array CreateInstance( Type elementType, int length1, int length2, int length3 )
////    {
////        if(length1 < 0 || length2 < 0 || length3 < 0)
////        {
////            String arg = "length1";
////            if(length2 < 0) arg = "length2";
////            if(length3 < 0) arg = "length3";
////            throw new ArgumentOutOfRangeException( arg, Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        if(elementType == null)
////        {
////            throw new ArgumentNullException( "elementType" );
////        }
////
////        RuntimeType t = elementType.UnderlyingSystemType as RuntimeType;
////        if(t == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "elementType" );
////        }
////
////        int* pLengths = stackalloc int[3];
////        pLengths[0] = length1;
////        pLengths[1] = length2;
////        pLengths[2] = length3;
////
////        return InternalCreate( (void*)t.TypeHandle.Value, 3, pLengths, null );
////    }
////
////    public unsafe static Array CreateInstance( Type elementType, params int[] lengths )
////    {
////        if(lengths == null)
////        {
////            throw new ArgumentNullException( "lengths" );
////        }
////
////        if(lengths.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_NeedAtLeast1Rank" ) );
////        }
////
////        // Check to make sure the lenghts are all positive. Note that we check this here to give
////        // a good exception message if they are not; however we check this again inside the execution
////        // engine's low level allocation function after having made a copy of the array to prevent a
////        // malicious caller from mutating the array after this check.
////        for(int i = 0; i < lengths.Length; i++)
////        {
////            if(lengths[i] < 0)
////            {
////                throw new ArgumentOutOfRangeException( "lengths[" + i + ']', Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////        }
////
////        if(elementType == null)
////        {
////            throw new ArgumentNullException( "elementType" );
////        }
////
////        RuntimeType t = elementType.UnderlyingSystemType as RuntimeType;
////        if(t == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "elementType" );
////        }
////
////        fixed(int* pLengths = lengths)
////        {
////            return InternalCreate( (void*)t.TypeHandle.Value, lengths.Length, pLengths, null );
////        }
////    }
////
////    public unsafe static Array CreateInstance( Type elementType, int[] lengths, int[] lowerBounds )
////    {
////        if(lengths == null)
////        {
////            throw new ArgumentNullException( "lengths" );
////        }
////
////        if(lengths.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_NeedAtLeast1Rank" ) );
////        }
////
////        // Check to make sure the lenghts are all positive. Note that we check this here to give
////        // a good exception message if they are not; however we check this again inside the execution
////        // engine's low level allocation function after having made a copy of the array to prevent a
////        // malicious caller from mutating the array after this check.
////        for(int i = 0; i < lengths.Length; i++)
////        {
////            if(lengths[i] < 0)
////            {
////                throw new ArgumentOutOfRangeException( "lengths[" + i + ']', Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////        }
////
////        if(lowerBounds == null)
////        {
////            throw new ArgumentNullException( "lowerBounds" );
////        }
////
////        if(lengths.Length != lowerBounds.Length)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_RanksAndBounds" ) );
////        }
////
////
////        if(elementType == null)
////        {
////            throw new ArgumentNullException( "elementType" );
////        }
////
////        RuntimeType t = elementType.UnderlyingSystemType as RuntimeType;
////        if(t == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "elementType" );
////        }
////
////        fixed(int* pLengths = lengths)
////        {
////            fixed(int* pLowerBounds = lowerBounds)
////            {
////                return InternalCreate( (void*)t.TypeHandle.Value, lengths.Length, pLengths, pLowerBounds );
////            }
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private unsafe static extern Array InternalCreate( void* elementType, int rank, int* pLengths, int* pLowerBounds );

        // Copies length elements from sourceArray, starting at index 0, to
        // destinationArray, starting at index 0.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Copy( Array sourceArray, Array destinationArray, int length )
        {
            if(sourceArray == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "sourceArray" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(destinationArray == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "destinationArray" );
#else
                throw new ArgumentNullException();
#endif
            }

            Copy( sourceArray, sourceArray.GetLowerBound( 0 ), destinationArray, destinationArray.GetLowerBound( 0 ), length, false );
        }

        // Copies length elements from sourceArray, starting at sourceIndex, to
        // destinationArray, starting at destinationIndex.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Copy( Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length )
        {
            Copy( sourceArray, sourceIndex, destinationArray, destinationIndex, length, false );
        }

        // Reliability-wise, this method will either possibly corrupt your
        // instance & might fail when called from within a CER, or if the
        // reliable flag is true, it will either always succeed or always
        // throw an exception with no side effects.
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal static extern void Copy( Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable );

////    // Provides a strong exception guarantee - either it succeeds, or
////    // it throws an exception with no side effects.  The arrays must be
////    // compatible array types based on the array element type - this
////    // method does not support casting, boxing, or primitive widening.
////    // It will up-cast, assuming the array types are correct.
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static void ConstrainedCopy( Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length )
////    {
////        Copy( sourceArray, sourceIndex, destinationArray, destinationIndex, length, true );
////    }

        // Sets length elements in array to 0 (or null for Object arrays), starting
        // at index.
        //
////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Clear( Array array, int index, int length );

        // The various Get values...
////    public unsafe Object GetValue( params int[] indices )
////    {
////        if(indices == null)
////        {
////            throw new ArgumentNullException( "indices" );
////        }
////
////        if(Rank != indices.Length)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_RankIndices" ) );
////        }
////
////        TypedReference elemref = new TypedReference();
////
////        fixed(int* pIndices = indices)
////        {
////            InternalGetReference( &elemref, indices.Length, pIndices );
////        }
////
////        return TypedReference.InternalToObject( &elemref );
////    }

        public unsafe Object GetValue( int index )
        {
            throw new NotImplementedException();
////        if(Rank != 1)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need1DArray" ) );
////        }
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 1, &index );
////
////        return TypedReference.InternalToObject( &elemref );
        }

////    public unsafe Object GetValue( int index1, int index2 )
////    {
////        if(Rank != 2)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need2DArray" ) );
////        }
////
////        int* pIndices = stackalloc int[2];
////        pIndices[0] = index1;
////        pIndices[1] = index2;
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 2, pIndices );
////
////        return TypedReference.InternalToObject( &elemref );
////    }
////
////    public unsafe Object GetValue( int index1, int index2, int index3 )
////    {
////        if(Rank != 3)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need3DArray" ) );
////        }
////
////        int* pIndices = stackalloc int[3];
////        pIndices[0] = index1;
////        pIndices[1] = index2;
////        pIndices[2] = index3;
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 3, pIndices );
////
////        return TypedReference.InternalToObject( &elemref );
////    }

        public unsafe void SetValue( Object value, int index )
        {
            throw new NotImplementedException();
////        if(Rank != 1)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need1DArray" ) );
////        }
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 1, &index );
////
////        InternalSetValue( &elemref, value );
        }

////    public unsafe void SetValue( Object value, int index1, int index2 )
////    {
////        if(Rank != 2)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need2DArray" ) );
////        }
////
////        int* pIndices = stackalloc int[2];
////        pIndices[0] = index1;
////        pIndices[1] = index2;
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 2, pIndices );
////
////        InternalSetValue( &elemref, value );
////    }
////
////    public unsafe void SetValue( Object value, int index1, int index2, int index3 )
////    {
////        if(Rank != 3)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_Need3DArray" ) );
////        }
////
////        int* pIndices = stackalloc int[3];
////        pIndices[0] = index1;
////        pIndices[1] = index2;
////        pIndices[2] = index3;
////
////        TypedReference elemref = new TypedReference();
////
////        InternalGetReference( &elemref, 3, pIndices );
////
////        InternalSetValue( &elemref, value );
////    }
////
////    public unsafe void SetValue( Object value, params int[] indices )
////    {
////        if(indices == null)
////        {
////            throw new ArgumentNullException( "indices" );
////        }
////        if(Rank != indices.Length)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_RankIndices" ) );
////        }
////
////        TypedReference elemref = new TypedReference();
////
////        fixed(int* pIndices = indices)
////        {
////            InternalGetReference( &elemref, indices.Length, pIndices );
////        }
////
////        InternalSetValue( &elemref, value );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    // reference to TypedReference is banned, so have to pass result as pointer
////    private unsafe extern void InternalGetReference( void* elemRef, int rank, int* pIndices );
////
////    // Ideally, we would like to use TypedReference.SetValue instead. Unfortunately, TypedReference.SetValue
////    // always throws not-supported exception
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private unsafe extern static void InternalSetValue( void* target, Object value );

        public extern int Length
        {
////        [ResourceExposure( ResourceScope.None )]
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        private static int GetMedian( int low, int hi )
        {
            BCLDebug.Assert( hi - low >= 0, "Length overflow!" );
            return low + ((hi - low) >> 1);
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int GetLength( int dimension );

        public extern int Rank
        {
////        [ResourceExposure( ResourceScope.None )]
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int GetUpperBound( int dimension );

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int GetLowerBound( int dimension );

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetDataPtrOffsetInternal();

        // Number of elements in the Array.
        int ICollection.Count
        {
            get
            {
                return Length;
            }
        }


        // Returns an object appropriate for synchronizing access to this
        // Array.
        public Object SyncRoot
        {
            get
            {
                return this;
            }
        }

        // Is this Array read-only?
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        // Is this Array synchronized (i.e., thread-safe)?  If you want a synchronized
        // collection, you can use SyncRoot as an object to synchronize your
        // collection with.  You could also call GetSynchronized()
        // to get a synchronized wrapper around the Array.
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        Object IList.this[int index]
        {
            get
            {
                return GetValue( index );
            }

            set
            {
                SetValue( value, index );
            }
        }

        int IList.Add( Object value )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
#else
            throw new NotSupportedException();
#endif
        }

        bool IList.Contains( Object value )
        {
            return Array.IndexOf( this, value ) >= this.GetLowerBound( 0 );
        }

        void IList.Clear()
        {
            Array.Clear( this, 0, this.Length );
        }

        int IList.IndexOf( Object value )
        {
            return Array.IndexOf( this, value );
        }

        void IList.Insert( int index, Object value )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
#else
            throw new NotSupportedException();
#endif
        }

        void IList.Remove( Object value )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
#else
            throw new NotSupportedException();
#endif
        }

        void IList.RemoveAt( int index )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
#else
            throw new NotSupportedException();
#endif
        }

        // Make a new array which is a deep copy of the original array.
        //
        public Object Clone()
        {
            return MemberwiseClone();
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the
        // IComparable interface, which must be implemented by all elements
        // of the array and the given search value. This method assumes that the
        // array is already sorted according to the IComparable interface;
        // if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch( Array array, Object value )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return BinarySearch( array, lb, array.Length, value, null );
        }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the IComparable interface, which must be implemented by all
        // elements of the array and the given search value. This method assumes
        // that the array is already sorted according to the IComparable
        // interface; if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch( Array array, int index, int length, Object value )
        {
            return BinarySearch( array, index, length, value, null );
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the given
        // IComparer interface. If comparer is null, elements of the
        // array are compared to the search value using the IComparable
        // interface, which in that case must be implemented by all elements of the
        // array and the given search value. This method assumes that the array is
        // already sorted; if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch( Array array, Object value, IComparer comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return BinarySearch( array, lb, array.Length, value, comparer );
        }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the given IComparer interface. If comparer is null,
        // elements of the array are compared to the search value using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array and the given search value. This method
        // assumes that the array is already sorted; if this is not the case, the
        // result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch( Array array, int index, int length, Object value, IComparer comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );
            if(index < lb || length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < lb ? "index" : "length"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(array.Length - (index - lb) < length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }
            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new RankException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
#else
                throw new RankException();
#endif
            }

            if(comparer == null) comparer = Comparer.Default;

////        if(comparer == Comparer.Default)
////        {
////            int retval;
////            bool r = TrySZBinarySearch( array, index, length, value, out retval );
////            if(r)
////            {
////                return retval;
////            }
////        }

            int      lo       = index;
            int      hi       = index + length - 1;
            Object[] objArray = array as Object[];
            if(objArray != null)
            {
                while(lo <= hi)
                {
                    // i might overflow if lo and hi are both large positive numbers.
                    int i = GetMedian( lo, hi );
                    int c;

                    try
                    {
                        c = comparer.Compare( objArray[i], value );
                    }
                    catch(Exception e)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                        throw new InvalidOperationException(null, e);
#endif
                    }

                    if(c == 0) return i;

                    if(c < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
            }
            else
            {
                while(lo <= hi)
                {
                    int i = GetMedian( lo, hi );
                    int c;

                    try
                    {
                        c = comparer.Compare( array.GetValue( i ), value );
                    }
                    catch(Exception e)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                        throw new InvalidOperationException( null, e );
#endif
                    }

                    if(c == 0) return i;

                    if(c < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
            }

            return ~lo;
        }

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool TrySZBinarySearch( Array sourceArray, int sourceIndex, int count, Object value, out int retVal );

////    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch<T>( T[] array, T value )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            return BinarySearch<T>( array, 0, array.Length, value, null );
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch<T>( T[] array, T value, System.Collections.Generic.IComparer<T> comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            return BinarySearch<T>( array, 0, array.Length, value, comparer );
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch<T>( T[] array, int index, int length, T value )
        {
            return BinarySearch<T>( array, index, length, value, null );
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int BinarySearch<T>( T[] array, int index, int length, T value, System.Collections.Generic.IComparer<T> comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "length"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(array.Length - index < length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }
    
            return ArraySortHelper<T>.Default.BinarySearch( array, index, length, value, comparer );
        }


////    public static TOutput[] ConvertAll<TInput, TOutput>( TInput[] array, Converter<TInput, TOutput> converter )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(converter == null)
////        {
////            throw new ArgumentNullException( "converter" );
////        }
////
////        TOutput[] newArray = new TOutput[array.Length];
////        for(int i = 0; i < array.Length; i++)
////        {
////            newArray[i] = converter( array[i] );
////        }
////        return newArray;
////    }

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        //
        // This method is to support the ICollection interface, and calls
        // Array.Copy internally.  If you aren't using ICollection explicitly,
        // call Array.Copy to avoid an extra indirection.
        //
        public void CopyTo( Array array, int index )
        {
            if(array != null && array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }

            // Note: Array.Copy throws a RankException and we want a consistent ArgumentException for all the IList CopyTo methods.
            Array.Copy( this, GetLowerBound( 0 ), array, index, Length );
        }

////    public static bool Exists<T>( T[] array, Predicate<T> match )
////    {
////        return Array.FindIndex( array, match ) != -1;
////    }
////
////    public static T Find<T>( T[] array, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(match == null)
////        {
////            throw new ArgumentNullException( "match" );
////        }
////
////        for(int i = 0; i < array.Length; i++)
////        {
////            if(match( array[i] ))
////            {
////                return array[i];
////            }
////        }
////
////        return default( T );
////    }
////
////    public static T[] FindAll<T>( T[] array, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(match == null)
////        {
////            throw new ArgumentNullException( "match" );
////        }
////
////        List<T> list = new List<T>();
////
////        for(int i = 0; i < array.Length; i++)
////        {
////            if(match( array[i] ))
////            {
////                list.Add( array[i] );
////            }
////        }
////
////        return list.ToArray();
////    }
////
////    public static int FindIndex<T>( T[] array, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        return FindIndex( array, 0, array.Length, match );
////    }
////
////    public static int FindIndex<T>( T[] array, int startIndex, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        return FindIndex( array, startIndex, array.Length - startIndex, match );
////    }
////
////    public static int FindIndex<T>( T[] array, int startIndex, int count, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(startIndex < 0 || startIndex > array.Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(count < 0 || startIndex > array.Length - count)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        if(match == null)
////        {
////            throw new ArgumentNullException( "match" );
////        }
////
////        int endIndex = startIndex + count;
////
////        for(int i = startIndex; i < endIndex; i++)
////        {
////            if(match( array[i] )) return i;
////        }
////
////        return -1;
////    }
////
////    public static T FindLast<T>( T[] array, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(match == null)
////        {
////            throw new ArgumentNullException( "match" );
////        }
////
////        for(int i = array.Length - 1; i >= 0; i--)
////        {
////            if(match( array[i] ))
////            {
////                return array[i];
////            }
////        }
////
////        return default( T );
////    }
////
////    public static int FindLastIndex<T>( T[] array, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        return FindLastIndex( array, array.Length - 1, array.Length, match );
////    }
////
////    public static int FindLastIndex<T>( T[] array, int startIndex, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        return FindLastIndex( array, startIndex, startIndex + 1, match );
////    }
////
////    public static int FindLastIndex<T>( T[] array, int startIndex, int count, Predicate<T> match )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(match == null)
////        {
////            throw new ArgumentNullException( "match" );
////        }
////
////        if(array.Length == 0)
////        {
////            // Special case for 0 length List
////            if(startIndex != -1)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////        }
////        else
////        {
////            // Make sure we're not out of range
////            if(startIndex < 0 || startIndex >= array.Length)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////        }
////
////        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
////        if(count < 0 || startIndex - count + 1 < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        int endIndex = startIndex - count;
////
////        for(int i = startIndex; i > endIndex; i--)
////        {
////            if(match( array[i] ))
////            {
////                return i;
////            }
////        }
////
////        return -1;
////    }

        public static void ForEach<T>( T[] array, Action<T> action )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(action == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "action" );
#else
                throw new ArgumentNullException();
#endif
            }

            for(int i = 0; i < array.Length; i++)
            {
                action( array[i] );
            }
        }

        // GetEnumerator returns an IEnumerator over this Array.
        //
        // Currently, only one dimensional arrays are supported.
        //
        public IEnumerator GetEnumerator()
        {
            int lowerBound = GetLowerBound( 0 );
            if(Rank == 1 && lowerBound == 0)
            {
                return new SZArrayEnumerator( this );
            }
            else
            {
                throw new NotImplementedException();
////            return new ArrayEnumerator( this, lowerBound, Length );
            }
        }

        // Returns the index of the first occurrence of a given value in an array.
        // The array is searched forwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int IndexOf( Array array, Object value )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return IndexOf( array, value, lb, array.Length );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and ending at the last element of the array. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int IndexOf( Array array, Object value, int startIndex )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return IndexOf( array, value, startIndex, array.Length - startIndex + lb );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and upto count elements. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int IndexOf( Array array, Object value, int startIndex, int count )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new RankException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
#else
                throw new RankException();
#endif
            }

            int lb = array.GetLowerBound( 0 );
            if(startIndex < lb || startIndex > array.Length + lb)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(count < 0 || count > array.Length - startIndex + lb)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

////        // Try calling a quick native method to handle primitive types.
////        int retVal;
////        bool r = TrySZIndexOf( array, startIndex, count, value, out retVal );
////        if(r)
////        {
////            return retVal;
////        }

            Object[] objArray = array as Object[];
            int      endIndex = startIndex + count;

            if(objArray != null)
            {
                if(value == null)
                {
                    for(int i = startIndex; i < endIndex; i++)
                    {
                        if(objArray[i] == null) return i;
                    }
                }
                else
                {
                    for(int i = startIndex; i < endIndex; i++)
                    {
                        Object obj = objArray[i];

                        if(obj != null && obj.Equals( value )) return i;
                    }
                }
            }
            else
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    Object obj = array.GetValue( i );

                    if(obj == null)
                    {
                        if(value == null) return i;
                    }
                    else
                    {
                        if(obj.Equals( value )) return i;
                    }
                }
            }

            // Return one less than the lower bound of the array.  This way,
            // for arrays with a lower bound of -1 we will not return -1 when the
            // item was not found.  And for SZArrays (the vast majority), -1 still
            // works for them.
            return lb - 1;
        }

        public static int IndexOf<T>( T[] array, T value )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( array, value, 0, array.Length );
        }
    
        public static int IndexOf<T>( T[] array, T value, int startIndex )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( array, value, startIndex, array.Length - startIndex );
        }
    
        public static int IndexOf<T>( T[] array, T value, int startIndex, int count )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(startIndex < 0 || startIndex > array.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(count < 0 || count > array.Length - startIndex)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return EqualityComparer<T>.Default.IndexOf( array, value, startIndex, count );
        }

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool TrySZIndexOf( Array sourceArray, int sourceIndex, int count, Object value, out int retVal );


        // Returns the index of the last occurrence of a given value in an array.
        // The array is searched backwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int LastIndexOf( Array array, Object value )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return LastIndexOf( array, value, array.Length - 1 + lb, array.Length );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and ending at index 0. The elements of the array are
        // compared to the given value using the Object.Equals method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int LastIndexOf( Array array, Object value, int startIndex )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            int lb = array.GetLowerBound( 0 );

            return LastIndexOf( array, value, startIndex, startIndex + 1 - lb );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and counting uptocount elements. The elements of
        // the array are compared to the given value using the Object.Equals
        // method.
        //
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int LastIndexOf( Array array, Object value, int startIndex, int count )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new RankException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
#else
                throw new RankException();
#endif
            }

            int lb = array.GetLowerBound( 0 );
            if(array.Length == 0)
            {
                return lb - 1;
            }

            if(startIndex < lb || startIndex >= array.Length + lb)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count > startIndex - lb + 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "endIndex", Environment.GetResourceString( "ArgumentOutOfRange_EndIndexStartIndex" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

////        // Try calling a quick native method to handle primitive types.
////        int retVal;
////        bool r = TrySZLastIndexOf( array, startIndex, count, value, out retVal );
////        if(r)
////        {
////            return retVal;
////        }

            Object[] objArray = array as Object[];
            int      endIndex = startIndex - count + 1;
            if(objArray != null)
            {
                if(value == null)
                {
                    for(int i = startIndex; i >= endIndex; i--)
                    {
                        if(objArray[i] == null) return i;
                    }
                }
                else
                {
                    for(int i = startIndex; i >= endIndex; i--)
                    {
                        Object obj = objArray[i];

                        if(obj != null && obj.Equals( value )) return i;
                    }
                }
            }
            else
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    Object obj = array.GetValue( i );

                    if(obj == null)
                    {
                        if(value == null) return i;
                    }
                    else
                    {
                        if(obj.Equals( value )) return i;
                    }
                }
            }

            return lb - 1;  // Return lb-1 for arrays with negative lower bounds.
        }

////    public static int LastIndexOf<T>( T[] array, T value )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        return LastIndexOf( array, value, array.Length - 1, array.Length );
////    }
////
////    public static int LastIndexOf<T>( T[] array, T value, int startIndex )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        // if array is empty and startIndex is 0, we need to pass 0 as count
////        return LastIndexOf( array, value, startIndex, (array.Length == 0) ? 0 : (startIndex + 1) );
////    }
////
////    public static int LastIndexOf<T>( T[] array, T value, int startIndex, int count )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(array.Length == 0)
////        {
////            //
////            // Special case for 0 length List
////            // accept -1 and 0 as valid startIndex for compablility reason.
////            //
////            if(startIndex != -1 && startIndex != 0)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            // only 0 is a valid value for count if array is empty
////            if(count != 0)
////            {
////                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////            }
////            return -1;
////        }
////
////        // Make sure we're not out of range
////        if(startIndex < 0 || startIndex >= array.Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
////        if(count < 0 || startIndex - count + 1 < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        return EqualityComparer<T>.Default.LastIndexOf( array, value, startIndex, count );
////    }


////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool TrySZLastIndexOf( Array sourceArray, int sourceIndex, int count, Object value, out int retVal );


        // Reverses all elements of the given array. Following a call to this
        // method, an element previously located at index i will now be
        // located at index length - i - 1, where length is the
        // length of the array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Reverse( Array array )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            Reverse( array, array.GetLowerBound( 0 ), array.Length );
        }

        // Reverses the elements in a range of an array. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        // Reliability note: This may fail because it may have to box objects.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Reverse( Array array, int index, int length )
        {
            throw new NotImplementedException();
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////        if(array.Rank != 1)
////        {
////            throw new RankException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
////        }
////
////        int lb = array.GetLowerBound( 0 );
////        if(index < lb || length < 0)
////        {
////            throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "length"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////        if(array.Length - (index - lb) < length)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////        }
////
////        bool r = TrySZReverse( array, index, length );
////        if(r)
////        {
////            return;
////        }
////
////        int i = index;
////        int j = index + length - 1;
////        Object[] objArray = array as Object[];
////        if(objArray != null)
////        {
////            while(i < j)
////            {
////                Object temp = objArray[i];
////                objArray[i] = objArray[j];
////                objArray[j] = temp;
////                i++;
////                j--;
////            }
////        }
////        else
////        {
////            while(i < j)
////            {
////                Object temp = array.GetValue( i );
////                array.SetValue( array.GetValue( j ), i );
////                array.SetValue( temp, j );
////                i++;
////                j--;
////            }
////        }
        }

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool TrySZReverse( Array array, int index, int count );

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the IComparable interface, which must be implemented
        // by all elements of the array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array array )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort( array, null, array.GetLowerBound( 0 ), array.Length, null );
        }

        // Sorts the elements of two arrays based on the keys in the first array.
        // Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the IComparable interface, which must be
        // implemented by all elements of the keys array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array keys, Array items )
        {
            if(keys == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "keys" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort( keys, items, keys.GetLowerBound( 0 ), keys.Length, null );
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the IComparable interface, which
        // must be implemented by all elements in the given section of the array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array array, int index, int length )
        {
            Sort( array, null, index, length, null );
        }

        // Sorts the elements in a section of two arrays based on the keys in the
        // first array. Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the IComparable interface, which must be
        // implemented by all elements of the keys array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array keys, Array items, int index, int length )
        {
            Sort( keys, items, index, length, null );
        }

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the given IComparer interface. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array array, IComparer comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort( array, null, array.GetLowerBound( 0 ), array.Length, comparer );
        }

        // Sorts the elements of two arrays based on the keys in the first array.
        // Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements of the keys array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array keys, Array items, IComparer comparer )
        {
            if(keys == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "keys" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort( keys, items, keys.GetLowerBound( 0 ), keys.Length, comparer );
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements in the given section of the array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array array, int index, int length, IComparer comparer )
        {
            Sort( array, null, index, length, comparer );
        }

        // Sorts the elements in a section of two arrays based on the keys in the
        // first array. Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements of the given section of the keys array.
        //
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort( Array keys, Array items, int index, int length, IComparer comparer )
        {
            throw new NotImplementedException();
////        if(keys == null)
////        {
////            throw new ArgumentNullException( "keys" );
////        }
////        if(keys.Rank != 1 || (items != null && items.Rank != 1))
////        {
////            throw new RankException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
////        }
////
////        int lbK = keys .GetLowerBound( 0 );
////        int lbI = items.GetLowerBound( 0 );
////        if(items != null && lbK != lbI)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_LowerBoundsMustMatch" ) );
////        }
////        if(index < lbK || length < 0)
////        {
////            throw new ArgumentOutOfRangeException( (length < 0 ? "length" : "index"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////        if(keys.Length - (index - lbK) < length || (items != null && (index - lbI) > items.Length - length))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////        }
////
////
////        if(length > 1)
////        {
////            if(comparer == Comparer.Default || comparer == null)
////            {
////                bool r = TrySZSort( keys, items, index, index + length - 1 );
////                if(r)
////                {
////                    return;
////                }
////            }
////
////            Object[] objKeys = keys as Object[];
////            Object[] objItems = null;
////            if(objKeys != null)
////            {
////                objItems = items as Object[];
////            }
////
////            if(objKeys != null && (items == null || objItems != null))
////            {
////                SorterObjectArray sorter = new SorterObjectArray( objKeys, objItems, comparer );
////
////                sorter.QuickSort( index, index + length - 1 );
////            }
////            else
////            {
////                SorterGenericArray sorter = new SorterGenericArray( keys, items, comparer );
////
////                sorter.QuickSort( index, index + length - 1 );
////            }
////        }
        }

////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool TrySZSort( Array keys, Array items, int left, int right );
////
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<T>( T[] array )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort<T>( array, array.GetLowerBound( 0 ), array.Length, null );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<TKey, TValue>( TKey[] keys, TValue[] items )
        {
            if(keys == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "keys" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort<TKey, TValue>( keys, items, 0, keys.Length, null );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<T>( T[] array, int index, int length )
        {
            Sort<T>( array, index, length, null );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<TKey, TValue>( TKey[] keys, TValue[] items, int index, int length )
        {
            Sort<TKey, TValue>( keys, items, index, length, null );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<T>( T[] array, System.Collections.Generic.IComparer<T> comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort<T>( array, 0, array.Length, comparer );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<TKey, TValue>( TKey[] keys, TValue[] items, System.Collections.Generic.IComparer<TKey> comparer )
        {
            if(keys == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "keys" );
#else
                throw new ArgumentNullException();
#endif
            }

            Sort<TKey, TValue>( keys, items, 0, keys.Length, comparer );
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<T>( T[] array, int index, int length, System.Collections.Generic.IComparer<T> comparer )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (length < 0 ? "length" : "index"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(array.Length - index < length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }
    
            if(length > 1)
            {
////            // <STRIP>
////            // TrySZSort is still faster than the generic implementation.
////            // The reason is Int32.CompareTo is still expensive than just using "<" or ">".
////            // </STRIP>
////            if(comparer == null || comparer == Comparer<T>.Default)
////            {
////                if(TrySZSort( array, null, index, index + length - 1 ))
////                {
////                    return;
////                }
////            }
    
                ArraySortHelper<T>.Default.Sort( array, index, length, comparer );
            }
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static void Sort<TKey, TValue>( TKey[] keys, TValue[] items, int index, int length, System.Collections.Generic.IComparer<TKey> comparer )
        {
            if(keys == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "keys" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (length < 0 ? "length" : "index"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(keys.Length - index < length || (items != null && index > items.Length - length))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }
    
            if(length > 1)
            {
////            if(comparer == null || comparer == Comparer<TKey>.Default)
////            {
////                if(TrySZSort( keys, items, index, index + length - 1 ))
////                {
////                    return;
////                }
////            }
    
                ArraySortHelper<TKey>.Default.Sort( keys, items, index, length, comparer );
            }
        }

        public static void Sort<T>( T[] array, Comparison<T> comparison )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(comparison == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "comparison" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            IComparer<T> comparer = new FunctorComparer<T>( comparison );
    
            Array.Sort( array, comparer );
        }
    
        public static bool TrueForAll<T>( T[] array, Predicate<T> match )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(match == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "match" );
#else
                throw new ArgumentNullException();
#endif
            }

            for(int i = 0; i < array.Length; i++)
            {
                if(!match( array[i] ))
                {
                    return false;
                }
            }

            return true;
        }

        internal sealed class FunctorComparer<T> : IComparer<T>
        {
            Comparison<T> comparison;
////        Comparer<T>   c = Comparer<T>.Default;

            public FunctorComparer( Comparison<T> comparison )
            {
                this.comparison = comparison;
            }

            public int Compare( T x, T y )
            {
                return comparison( x, y );
            }
        }


////    // Private value type used by the Sort methods.
////    private struct SorterObjectArray
////    {
////        private Object[]  keys;
////        private Object[]  items;
////        private IComparer comparer;
////
////        internal SorterObjectArray( Object[] keys, Object[] items, IComparer comparer )
////        {
////            if(comparer == null) comparer = Comparer.Default;
////
////            this.keys     = keys;
////            this.items    = items;
////            this.comparer = comparer;
////        }
////
////        internal void SwapIfGreaterWithItems( int a, int b )
////        {
////            if(a != b)
////            {
////                try
////                {
////                    if(comparer.Compare( keys[a], keys[b] ) > 0)
////                    {
////                        Object temp = keys[a];
////                        keys[a] = keys[b];
////                        keys[b] = temp;
////
////                        if(items != null)
////                        {
////                            Object item = items[a];
////                            items[a] = items[b];
////                            items[b] = item;
////                        }
////                    }
////                }
////                catch(IndexOutOfRangeException)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", keys[b], keys[b].GetType().Name, comparer ) );
////                }
////                catch(Exception e)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
////                }
////            }
////        }
////
////
////        internal void QuickSort( int left, int right )
////        {
////            // Can use the much faster jit helpers for array access.
////            do
////            {
////                int i = left;
////                int j = right;
////
////                // pre-sort the low, middle (pivot), and high values in place.
////                // this improves performance in the face of already sorted data, or
////                // data that is made up of multiple sorted runs appended together.
////                int middle = GetMedian( i, j );
////
////                SwapIfGreaterWithItems( i     , middle ); // swap the low with the mid point
////                SwapIfGreaterWithItems( i     , j      ); // swap the low with the high
////                SwapIfGreaterWithItems( middle, j      ); // swap the middle with the high
////
////                Object x = keys[middle];
////                do
////                {
////                    // Add a try block here to detect IComparers (or their
////                    // underlying IComparables, etc) that are bogus.
////                    try
////                    {
////                        while(comparer.Compare( keys[i], x       ) < 0) i++;
////                        while(comparer.Compare( x      , keys[j] ) < 0) j--;
////                    }
////                    catch(IndexOutOfRangeException)
////                    {
////                        throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", x, x.GetType().Name, comparer ) );
////                    }
////                    catch(Exception e)
////                    {
////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
////                    }
////
////                    BCLDebug.Assert( i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?" );
////
////                    if(i > j) break;
////
////                    if(i < j)
////                    {
////                        Object key = keys[i];
////                        keys[i] = keys[j];
////                        keys[j] = key;
////
////                        if(items != null)
////                        {
////                            Object item = items[i];
////                            items[i] = items[j];
////                            items[j] = item;
////                        }
////                    }
////
////                    i++;
////                    j--;
////
////                } while(i <= j);
////
////                if(j - left <= right - i)
////                {
////                    if(left < j) QuickSort( left, j );
////                    left = i;
////                }
////                else
////                {
////                    if(i < right) QuickSort( i, right );
////                    right = j;
////                }
////
////            } while(left < right);
////        }
////    }
////
////    // Private value used by the Sort methods for instances of Array.
////    // This is slower than the one for Object[], since we can't use the JIT helpers
////    // to access the elements.  We must use GetValue & SetValue.
////    private struct SorterGenericArray
////    {
////        private Array     keys;
////        private Array     items;
////        private IComparer comparer;
////
////        internal SorterGenericArray( Array keys, Array items, IComparer comparer )
////        {
////            if(comparer == null) comparer = Comparer.Default;
////
////            this.keys     = keys;
////            this.items    = items;
////            this.comparer = comparer;
////        }
////
////        internal void SwapIfGreaterWithItems( int a, int b )
////        {
////            if(a != b)
////            {
////                try
////                {
////                    if(comparer.Compare( keys.GetValue( a ), keys.GetValue( b ) ) > 0)
////                    {
////                        Object key = keys.GetValue( a );
////                        keys.SetValue( keys.GetValue( b ), a );
////                        keys.SetValue( key               , b );
////
////                        if(items != null)
////                        {
////                            Object item = items.GetValue( a );
////                            items.SetValue( items.GetValue( b ), a );
////                            items.SetValue( item               , b );
////                        }
////                    }
////                }
////                catch(IndexOutOfRangeException)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", keys.GetValue( b ), keys.GetValue( b ).GetType().Name, comparer ) );
////                }
////                catch(Exception e)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
////                }
////            }
////        }
////
////        internal void QuickSort( int left, int right )
////        {
////            // Must use slow Array accessors (GetValue & SetValue)
////            do
////            {
////                int i = left;
////                int j = right;
////
////                // pre-sort the low, middle (pivot), and high values in place.
////                // this improves performance in the face of already sorted data, or
////                // data that is made up of multiple sorted runs appended together.
////                int middle = GetMedian( i, j );
////
////                SwapIfGreaterWithItems( i     , middle ); // swap the low with the mid point
////                SwapIfGreaterWithItems( i     , j      );      // swap the low with the high
////                SwapIfGreaterWithItems( middle, j      ); // swap the middle with the high
////
////                Object x = keys.GetValue( middle );
////                do
////                {
////                    // Add a try block here to detect IComparers (or their
////                    // underlying IComparables, etc) that are bogus.
////                    try
////                    {
////                        while(comparer.Compare( keys.GetValue( i ), x                  ) < 0) i++;
////                        while(comparer.Compare( x                 , keys.GetValue( j ) ) < 0) j--;
////                    }
////                    catch(IndexOutOfRangeException)
////                    {
////                        throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", x, x.GetType().Name, comparer ) );
////                    }
////                    catch(Exception e)
////                    {
////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
////                    }
////
////                    BCLDebug.Assert( i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?" );
////
////                    if(i > j) break;
////
////                    if(i < j)
////                    {
////                        Object key = keys.GetValue( i );
////                        keys.SetValue( keys.GetValue( j ), i );
////                        keys.SetValue( key               , j );
////
////                        if(items != null)
////                        {
////                            Object item = items.GetValue( i );
////                            items.SetValue( items.GetValue( j ), i );
////                            items.SetValue( item               , j );
////                        }
////                    }
////
////                    if(i != Int32.MaxValue) ++i;
////                    if(j != Int32.MinValue) --j;
////
////                } while(i <= j);
////
////                if(j - left <= right - i)
////                {
////                    if(left < j) QuickSort( left, j );
////                    left = i;
////                }
////                else
////                {
////                    if(i < right) QuickSort( i, right );
////                    right = j;
////                }
////
////            } while(left < right);
////        }
////    }

        [Serializable]
        private sealed class SZArrayEnumerator : IEnumerator, ICloneable
        {
            private Array m_array;
            private int   m_index;
            private int   m_endIndex; // cache array length, since it's a little slow.

            internal SZArrayEnumerator( Array array )
            {
                BCLDebug.Assert( array.Rank == 1 && array.GetLowerBound( 0 ) == 0, "SZArrayEnumerator only works on single dimension arrays w/ a lower bound of zero." );

                m_array    = array;
                m_index    = -1;
                m_endIndex = array.Length;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public bool MoveNext()
            {
                if(m_index < m_endIndex)
                {
                    m_index++;

                    return (m_index < m_endIndex);
                }

                return false;
            }

            public Object Current
            {
                get
                {
                    if(m_index < 0)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_index >= m_endIndex)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_array.GetValue( m_index );
                }
            }

            public void Reset()
            {
                m_index = -1;
            }
        }

////    [Serializable]
////    private sealed class ArrayEnumerator : IEnumerator, ICloneable
////    {
////        private Array m_array;
////        private int   m_index;
////        private int   m_endIndex;
////        private int   m_startIndex;    // Save for Reset.
////        private int[] m_indices;    // The current position in a multidim array
////        private bool  m_complete;
////
////        internal ArrayEnumerator( Array array, int index, int count )
////        {
////            m_array      = array;
////            m_index      = index - 1;
////            m_startIndex = index;
////            m_endIndex   = index + count;
////            m_indices    = new int[array.Rank];
////
////            int checkForZero = 1;  // Check for dimensions of size 0.
////
////            for(int i = 0; i < array.Rank; i++)
////            {
////                m_indices[i]  = array.GetLowerBound( i );
////                checkForZero *= array.GetLength( i );
////            }
////
////            // To make MoveNext simpler, decrement least significant index.
////            m_indices[m_indices.Length - 1]--;
////            m_complete = (checkForZero == 0);
////        }
////
////        private void IncArray()
////        {
////            // This method advances us to the next valid array index,
////            // handling all the multiple dimension & bounds correctly.
////            // Think of it like an odometer in your car - we start with
////            // the last digit, increment it, and check for rollover.  If
////            // it rolls over, we set all digits to the right and including
////            // the current to the appropriate lower bound.  Do these overflow
////            // checks for each dimension, and if the most significant digit
////            // has rolled over it's upper bound, we're done.
////            //
////            int rank = m_array.Rank;
////
////            m_indices[rank - 1]++;
////
////            for(int dim = rank - 1; dim >= 0; dim--)
////            {
////                if(m_indices[dim] > m_array.GetUpperBound( dim ))
////                {
////                    if(dim == 0)
////                    {
////                        m_complete = true;
////                        break;
////                    }
////
////                    for(int j = dim; j < rank; j++)
////                    {
////                        m_indices[j] = m_array.GetLowerBound( j );
////                    }
////
////                    m_indices[dim - 1]++;
////                }
////            }
////        }
////
////        public Object Clone()
////        {
////            return MemberwiseClone();
////        }
////
////        public bool MoveNext()
////        {
////            if(m_complete)
////            {
////                m_index = m_endIndex;
////                return false;
////            }
////
////            m_index++;
////
////            IncArray();
////
////            return !m_complete;
////        }
////
////        public Object Current
////        {
////            get
////            {
////                if(m_index < m_startIndex)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumNotStarted ) );
////                }
////
////                if(m_complete)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumEnded ) );
////                }
////
////                return m_array.GetValue( m_indices );
////            }
////        }
////
////        public void Reset()
////        {
////            m_index = m_startIndex - 1;
////
////            int checkForZero = 1;
////            for(int i = 0; i < m_array.Rank; i++)
////            {
////                m_indices[i]  = m_array.GetLowerBound( i );
////                checkForZero *= m_array.GetLength( i );
////            }
////
////            // To make MoveNext simpler, decrement least significant index.
////            m_indices[m_indices.Length - 1]--;
////            m_complete = (checkForZero == 0);
////        }
////    }


        // if this is an array of value classes and that value class has a default constructor
        // then this calls this default constructor on every elemen in the value class array.
        // otherwise this is a no-op.  Generally this method is called automatically by the compiler
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Initialize();
    }







//////----------------------------------------------------------------------------------------
////// ! READ THIS BEFORE YOU WORK ON THIS CLASS.
//////
////// The methods on this class must be written VERY carefully to avoid introducing security holes.
////// That's because they are invoked with special "this"! The "this" object
////// for all of these methods are not SZArrayHelper objects. Rather, they are of type U[]
////// where U[] is castable to T[]. No actual SZArrayHelper object is ever instantiated. Thus, you will
////// see a lot of expressions that cast "this" "T[]".
//////
////// This class is needed to allow an SZ array of type T[] to expose IList<T>,
////// IList<T.BaseType>, etc., etc. all the way up to IList<Object>. When the following call is
////// made:
//////
//////   ((IList<T>) (new U[n])).SomeIListMethod()
//////
////// the interface stub dispatcher treats this as a special case, loads up SZArrayHelper,
////// finds the corresponding generic method (matched simply by method name), instantiates
////// it for type <T> and executes it.
//////
////// The "T" will reflect the interface used to invoke the method. The actual runtime "this" will be
////// array that is castable to "T[]" (i.e. for primitivs and valuetypes, it will be exactly
////// "T[]" - for orefs, it may be a "U[]" where U derives from T.)
//////----------------------------------------------------------------------------------------
////sealed class SZArrayHelper
////{
////    // It is never legal to instantiate this class.
////    private SZArrayHelper()
////    {
////        BCLDebug.Assert( false, "Hey! How'd I get here?" );
////    }
////
////
////    // -----------------------------------------------------------
////    // ------- Implement IEnumerable<T> interface methods --------
////    // -----------------------------------------------------------
////    internal IEnumerator<T> GetEnumerator<T>()
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        return new SZGenericArrayEnumerator<T>( this as T[] );
////    }
////
////    // -----------------------------------------------------------
////    // ------- Implement ICollection<T> interface methods --------
////    // -----------------------------------------------------------
////    void CopyTo<T>( T[] array, int index )
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////
////        if(array != null && array.Rank != 1)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Rank_MultiDimNotSupported" ) );
////        }
////
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////
////        Array.Copy( _this, 0, array, index, _this.Length );
////    }
////
////    internal int get_Count<T>()
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////
////        return _this.Length;
////    }
////
////    // -----------------------------------------------------------
////    // ---------- Implement IList<T> interface methods -----------
////    // -----------------------------------------------------------
////    internal T get_Item<T>( int index )
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////        if((uint)index >= (uint)_this.Length)
////        {
////            ThrowHelper.ThrowArgumentOutOfRangeException();
////        }
////
////        return _this[index];
////    }
////
////    internal void set_Item<T>( int index, T value )
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////        if((uint)index >= (uint)_this.Length)
////        {
////            ThrowHelper.ThrowArgumentOutOfRangeException();
////        }
////
////        _this[index] = value;
////    }
////
////    void Add<T>( T value )
////    {
////        // Not meaningful for arrays.
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////    }
////
////    bool Contains<T>( T value )
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////
////        return Array.IndexOf( _this, value ) != -1;
////    }
////
////    bool get_IsReadOnly<T>()
////    {
////        return true;
////    }
////
////    void Clear<T>()
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////    }
////
////    int IndexOf<T>( T value )
////    {
////        //! Warning: "this" is an array, not an SZArrayHelper. See comments above
////        //! or you may introduce a security hole!
////        T[] _this = this as T[];
////        BCLDebug.Assert( _this != null, "this should be a T[]" );
////
////        return Array.IndexOf( _this, value );
////    }
////
////    void Insert<T>( int index, T value )
////    {
////        // Not meaningful for arrays
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////    }
////
////    bool Remove<T>( T value )
////    {
////        // Not meaningful for arrays
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////    }
////
////    void RemoveAt<T>( int index )
////    {
////        // Not meaningful for arrays
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////    }
////
////    // This is a normal generic Enumerator for SZ arrays. It doesn't have any of the "this" voodoo
////    // that SZArrayHelper does.
////    //
////    [Serializable]
////    private sealed class SZGenericArrayEnumerator<T> : IEnumerator<T>
////    {
////        private T[] m_array;
////        private int m_index;
////        private int m_endIndex; // cache array length, since it's a little slow.
////
////        internal SZGenericArrayEnumerator( T[] array )
////        {
////            BCLDebug.Assert( array.Rank == 1 && array.GetLowerBound( 0 ) == 0, "SZArrayEnumerator<T> only works on single dimension arrays w/ a lower bound of zero." );
////
////            m_array    = array;
////            m_index    = -1;
////            m_endIndex = array.Length;
////        }
////
////        public bool MoveNext()
////        {
////            if(m_index < m_endIndex)
////            {
////                m_index++;
////
////                return (m_index < m_endIndex);
////            }
////            return false;
////        }
////
////        public T Current
////        {
////            get
////            {
////                if(m_index < 0)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumNotStarted ) );
////                }
////
////                if(m_index >= m_endIndex)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumEnded ) );
////                }
////
////                return m_array[m_index];
////            }
////        }
////
////        object IEnumerator.Current
////        {
////            get
////            {
////                return Current;
////            }
////        }
////
////        void IEnumerator.Reset()
////        {
////            m_index = -1;
////        }
////
////        public void Dispose()
////        {
////        }
////    }
////}
}
