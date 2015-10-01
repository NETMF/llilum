// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ArraySortHelper
**
**
** Purpose: class to sort arrays
**
** 
===========================================================*/
namespace System.Collections.Generic
{

    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

////[TypeDependencyAttribute( "System.Collections.Generic.GenericArraySortHelper`1" )]
    [Microsoft.Zelig.Internals.TypeDependency( typeof(System.Collections.Generic.GenericArraySortHelper<>) )]
    internal class ArraySortHelper<T>
    {
        static ArraySortHelper<T> defaultArraySortHelper;

        public static ArraySortHelper<T> Default
        {
            get
            {
                ArraySortHelper<T> sorter = defaultArraySortHelper;
                if(sorter != null)
                {
                    return sorter;
                }
                return CreateArraySortHelper();
            }
        }

        private static ArraySortHelper<T> CreateArraySortHelper()
        {
////        if(typeof( IComparable<T> ).IsAssignableFrom( typeof( T ) ))
////        {
////            defaultArraySortHelper = (ArraySortHelper<T>)(typeof( GenericArraySortHelper<string> ).TypeHandle.CreateInstanceForAnotherGenericParameter( typeof( T ) ));
////        }
////        else
            {
                defaultArraySortHelper = new ArraySortHelper<T>();
            }
            return defaultArraySortHelper;
        }

        public void Sort( T[] items, int index, int length, IComparer<T> comparer )
        {
            QuickSort( items, (object[])null, index, index + (length - 1), comparer );
////        Sort<Object>( items, (object[])null, index, length, comparer );
        }

        //
        // ZELIG HACK: virtual generic methods are not supported yet.
        //
        public void Sort<TValue>( T[] keys, TValue[] values, int index, int length, IComparer<T> comparer )
////    public virtual void Sort<TValue>( T[] keys, TValue[] values, int index, int length, IComparer<T> comparer )
        {
////        BCLDebug.Assert( keys != null, "Check the arguments in the caller!" );
////        BCLDebug.Assert( index >= 0 && length >= 0 && (keys.Length - index >= length), "Check the arguments in the caller!" );
////
////        if(comparer == null || comparer == Comparer<T>.Default)
////        {
////            comparer = Comparer<T>.Default;
////        }
    
            QuickSort( keys, values, index, index + (length - 1), comparer );
        }


        private void SwapIfGreaterWithItems<TValue>( T[] keys, TValue[] values, IComparer<T> comparer, int a, int b )
        {
            if(a != b)
            {
                try
                {
                    if(comparer.Compare( keys[a], keys[b] ) > 0)
                    {
                        T key = keys[a];
                        keys[a] = keys[b];
                        keys[b] = key;
                        if(values != null)
                        {
                            TValue value = values[a];
                            values[a] = values[b];
                            values[b] = value;
                        }
                    }
                }
                catch(IndexOutOfRangeException)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", keys[b], keys[b].GetType().Name, comparer ) );
#else
                    throw new ArgumentException();
#endif
                }
                catch(Exception e)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                    throw new InvalidOperationException( null, e );
#endif
                }
            }
        }


        private void QuickSort<TValue>( T[] keys, TValue[] values, int left, int right, IComparer<T> comparer )
        {
            do
            {
                int i = left;
                int j = right;

                // pre-sort the low, middle (pivot), and high values in place.
                // this improves performance in the face of already sorted data, or 
                // data that is made up of multiple sorted runs appended together.
                int middle = i + ((j - i) >> 1);
                SwapIfGreaterWithItems( keys, values, comparer, i, middle );  // swap the low with the mid point
                SwapIfGreaterWithItems( keys, values, comparer, i, j );   // swap the low with the high
                SwapIfGreaterWithItems( keys, values, comparer, middle, j ); // swap the middle with the high

                T x = keys[middle];
                do
                {
                    // Add a try block here to detect IComparers (or their
                    // underlying IComparables, etc) that are bogus.
                    try
                    {
                        while(comparer.Compare( keys[i], x ) < 0) i++;
                        while(comparer.Compare( x, keys[j] ) < 0) j--;
                    }
                    catch(IndexOutOfRangeException)
                    {
#if EXCEPTION_STRINGS
                        throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", x, x.GetType().Name, comparer ) );
#else
                        throw new ArgumentException();
#endif
                    }
                    catch(Exception e)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                        throw new InvalidOperationException( null, e );
#endif
                    }
                    BCLDebug.Assert( i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?" );
                    if(i > j) break;
                    if(i < j)
                    {
                        T key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        if(values != null)
                        {
                            TValue value = values[i];
                            values[i] = values[j];
                            values[j] = value;
                        }
                    }
                    i++;
                    j--;
                } while(i <= j);
                if(j - left <= right - i)
                {
                    if(left < j) QuickSort( keys, values, left, j, comparer );
                    left = i;
                }
                else
                {
                    if(i < right) QuickSort( keys, values, i, right, comparer );
                    right = j;
                }
            } while(left < right);
        }

        public virtual int BinarySearch( T[] array, int index, int length, T value, IComparer<T> comparer )
        {
            BCLDebug.Assert( array != null, "Check the arguments in the caller!" );
            BCLDebug.Assert( index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!" );

            if(comparer == null)
            {
                comparer = System.Collections.Generic.Comparer<T>.Default;
            }

            int lo = index;
            int hi = index + length - 1;
            while(lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order;
                try
                {
                    order = comparer.Compare( array[i], value );
                }
                catch(Exception e)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                    throw new InvalidOperationException( null, e );
#endif
                }

                if(order == 0) return i;
                if(order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }
    }


    [Serializable()]
    internal class GenericArraySortHelper<T> : ArraySortHelper<T> where T : IComparable<T>
    {
        public override int BinarySearch( T[] array, int index, int length, T value, IComparer<T> comparer )
        {
            BCLDebug.Assert( array != null, "Check the arguments in the caller!" );
            BCLDebug.Assert( index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!" );

            if(comparer == null || comparer == Comparer<T>.Default)
            {
                return BinarySearch( array, index, length, value );
            }
            else
            {
                return base.BinarySearch( array, index, length, value, comparer );
            }
        }

////    public override void Sort<TValue>( T[] keys, TValue[] values, int index, int length, IComparer<T> comparer )
////    {
////        BCLDebug.Assert( keys != null, "Check the arguments in the caller!" );
////        BCLDebug.Assert( index >= 0 && length >= 0 && (keys.Length - index >= length), "Check the arguments in the caller!" );
////
////        if(comparer == null || comparer == Comparer<T>.Default)
////        {
////            // call the fatser version of QuickSort if the user doesn't provide a comparer
////            QuickSort( keys, values, index, index + length - 1 );
////        }
////        else
////        {
////            base.Sort( keys, values, index, length, comparer );
////        }
////    }

        // This function is called when the user doesn't specify any comparer.
        // Since T is constrained here, we can call IComparable<T>.CompareTo here.
        // We can avoid boxing for value type and casting for reference types.
        private int BinarySearch( T[] array, int index, int length, T value )
        {
            int lo = index;
            int hi = index + length - 1;
            while(lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order;
                try
                {
                    if(array[i] == null)
                    {
                        order = (value == null) ? 0 : -1;
                    }
                    else
                    {
                        order = array[i].CompareTo( value );
                    }
                }
                catch(Exception e)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                    throw new InvalidOperationException( null, e );
#endif
                }

                if(order == 0) return i;
                if(order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }


        private void SwapIfGreaterWithItems<TValue>( T[] keys, TValue[] values, int a, int b )
        {
            if(a != b)
            {
                try
                {
                    if(keys[a] == null || keys[a].CompareTo( keys[b] ) > 0)
                    {
                        T key = keys[a];
                        keys[a] = keys[b];
                        keys[b] = key;
                        if(values != null)
                        {
                            TValue value = values[a];
                            values[a] = values[b];
                            values[b] = value;
                        }
                    }
                }
                catch(IndexOutOfRangeException)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", keys[b], keys[b].GetType().Name, null ) );
#else
                    throw new ArgumentException();
#endif
                }
                catch(Exception e)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                    throw new InvalidOperationException( null, e );
#endif
                }
            }
        }

        private void QuickSort<TValue>( T[] keys, TValue[] values, int left, int right )
        {
            // The code in this function looks very similar to QuickSort in ArraySortHelper<T> class.
            // The difference is that T is constrainted to IComparable<T> here.
            // So the IL code will be different. This function is faster than the one in ArraySortHelper<T>.

            do
            {
                int i = left;
                int j = right;

                // pre-sort the low, middle (pivot), and high values in place.
                // this improves performance in the face of already sorted data, or 
                // data that is made up of multiple sorted runs appended together.
                int middle = i + ((j - i) >> 1);
                SwapIfGreaterWithItems( keys, values, i, middle ); // swap the low with the mid point
                SwapIfGreaterWithItems( keys, values, i, j );      // swap the low with the high
                SwapIfGreaterWithItems( keys, values, middle, j ); // swap the middle with the high

                T x = keys[middle];
                do
                {
                    // Add a try block here to detect IComparers (or their
                    // underlying IComparables, etc) that are bogus.
                    try
                    {
                        if(x == null)
                        {
                            // if x null, the loop to find two elements to be switched can be reduced.
                            while(keys[j] != null) j--;
                        }
                        else
                        {
                            while(x.CompareTo( keys[i] ) > 0) i++;
                            while(x.CompareTo( keys[j] ) < 0) j--;
                        }
                    }
                    catch(IndexOutOfRangeException)
                    {
#if EXCEPTION_STRINGS
                        throw new ArgumentException( Environment.GetResourceString( "Arg_BogusIComparer", x, x.GetType().Name, null ) );
#else
                        throw new ArgumentException();
#endif
                    }
                    catch(Exception e)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_IComparerFailed" ), e );
#else
                        throw new InvalidOperationException( null, e );
#endif
                    }
                    BCLDebug.Assert( i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?" );
                    if(i > j) break;
                    if(i < j)
                    {
                        T key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        if(values != null)
                        {
                            TValue value = values[i];
                            values[i] = values[j];
                            values[j] = value;
                        }
                    }
                    i++;
                    j--;
                } while(i <= j);
                if(j - left <= right - i)
                {
                    if(left < j) QuickSort( keys, values, left, j );
                    left = i;
                }
                else
                {
                    if(i < right) QuickSort( keys, values, i, right );
                    right = j;
                }
            } while(left < right);
        }
    }
}


