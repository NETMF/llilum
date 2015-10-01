//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class ArrayUtility
    {
        public static T[] EnsureSizeOfNotNullArray<T>( T[] array ,
                                                       int size  )
        {
            int diff = size - array.Length;

            if(diff > 0)
            {
                return IncreaseSizeOfNotNullArray( array, diff );
            }

            return array;
        }

        public static T[] IncreaseSizeOfNotNullArray<T>( T[] array ,
                                                         int extra )
        {
            int len = array.Length;
            T[] res = new T[len+extra];

            Array.Copy( array, res, len );

            return res;
        }

        //--//

        public static int FindInNotNullArray<T>( T[] array   ,
                                                 T   element )
        {
            int len = array.Length;

            for(int i = 0; i < len; i++)
            {
                if(array[i].Equals( element ) == true)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int FindReferenceInNotNullArray<T>( T[] array   ,
                                                          T   element ) where T : class
        {
            int len = array.Length;

            for(int i = 0; i < len; i++)
            {
                if(Object.ReferenceEquals( array[i], element ) == true)
                {
                    return i;
                }
            }

            return -1;
        }

        //--//

        public static T[] CopyNotNullArray<T>( T[] array )
        {
            int len = array.Length;
            T[] res = new T[len];

            Array.Copy( array, res, len );

            return res;
        }

        //--//

        public static T[] ExtractSliceFromNotNullArray<T>( T[] array  ,
                                                           int offset ,
                                                           int count  )
        {
            T[] res = new T[count];

            Array.Copy( array, offset, res, 0, count );

            return res;
        }

        //--//

        public static T[] TrimNullArray<T>( T[] array     ,
                                            int maxLength )
        {
            if(array.Length > maxLength)
            {
                return ExtractSliceFromNotNullArray( array, 0, maxLength );
            }
            else
            {
                return array;
            }
        }

        //--//

        public static T[] AppendToArray<T>( T[] array   ,
                                            T   element )
        {
            if(array == null)
            {
                return new T[] { element };
            }
            else
            {
                return AppendToNotNullArray( array, element );
            }
        }

        public static T[] AppendToNotNullArray<T>( T[] array   ,
                                                   T   element )
        {
            int len = array.Length;
            T[] res = new T[len+1];

            if(len > 0)
            {
                Array.Copy( array, res, len );
            }

            res[len] = element;

            return res;
        }

        public static T[] AppendArrayToArray<T>( T[] array  ,
                                                 T[] array2 )
        {
            if(array == null)
            {
                return array2;
            }

            if(array2 == null)
            {
                return array;
            }

            return AppendNotNullArrayToNotNullArray( array, array2 );
        }

        public static T[] AppendNotNullArrayToNotNullArray<T>( T[] array  ,
                                                               T[] array2 )
        {
            int len2 = array2.Length;
            if(len2 == 0)
            {
                return array;
            }

            int len = array.Length;
            if(len == 0)
            {
                return array2;
            }

            T[] res = new T[len+len2];

            Array.Copy( array , 0, res,   0, len  );
            Array.Copy( array2, 0, res, len, len2 );

            return res;
        }

        //--//

        public static T[] InsertAtHeadOfArray<T>( T[] array   ,
                                                  T   element )
        {
            if(array == null)
            {
                return new T[] { element };
            }
            else
            {
                return InsertAtHeadOfNotNullArray( array, element );
            }
        }

        public static T[] InsertAtHeadOfNotNullArray<T>( T[] array   ,
                                                         T   element )
        {
            int len = array.Length;
            T[] res = new T[len+1];

            if(len > 0)
            {
                Array.Copy( array, 0, res, 1, len );
            }

            res[0] = element;

            return res;
        }

        //--//

        public static T[] InsertAtPositionOfArray<T>( T[] array    ,
                                                      int position ,
                                                      T   element  )
        {
            if(array == null)
            {
                return new T[] { element };
            }
            else
            {
                return InsertAtPositionOfNotNullArray( array, position, element );
            }
        }

        public static T[] InsertAtPositionOfNotNullArray<T>( T[] array    ,
                                                             int position ,
                                                             T   element  )
        {
            int len = array.Length;
            if(len == position)
            {
                return AppendToNotNullArray( array, element );
            }

            T[] res = new T[len+1];

            Array.Copy( array, 0       , res, 0         ,       position );
            Array.Copy( array, position, res, position+1, len - position );

            res[position] = element;

            return res;
        }

        public static T[] InsertNotNullArrayAtPositionOfNotNullArray<T>( T[] array    ,
                                                                         int position ,
                                                                         T[] array2   )
        {
            int len2 = array2.Length;
            if(len2 == 0)
            {
                return array;
            }

            int len = array.Length;
            T[] res = new T[len+len2];

            Array.Copy( array , 0       , res, 0              ,       position );
            Array.Copy( array2, 0       , res, position       , len2           );
            Array.Copy( array , position, res, position + len2, len - position );

            return res;
        }

        //--//

        public static T[] AddUniqueToArray<T>( T[] array   ,
                                               T   element )
        {
            if(array == null)
            {
                return new T[] { element };
            }
            else
            {
                return AddUniqueToNotNullArray( array, element );
            }
        }

        public static T[] AddUniqueToNotNullArray<T>( T[] array   ,
                                                      T   element )
        {
            if(FindInNotNullArray( array, element ) < 0)
            {
                return AppendToNotNullArray( array, element );
            }
            else
            {
                return array;
            }
        }

        //--//

        public static T[] ReplaceAtPositionOfArray<T>( T[] array    ,
                                                       int position ,
                                                       T   element  )
        {
            if(array != null)
            {
                array = ReplaceAtPositionOfNotNullArray( array, position, element );
            }

            return array;
        }

        public static T[] ReplaceAtPositionOfNotNullArray<T>( T[] array    ,
                                                              int position ,
                                                              T   element  )
        {
            array = CopyNotNullArray( array );

            array[position] = element;

            return array;
        }

        //--//

        public static T[] RemoveAtPositionFromNotNullArray<T>( T[] array ,
                                                               int pos   )
        {
            return RemoveAtPositionFromNotNullArray( array, pos, 1 );
        }

        public static T[] RemoveAtPositionFromNotNullArray<T>( T[] array ,
                                                               int pos   ,
                                                               int count )
        {
            int len = array.Length;
            T[] res = new T[len-count];

            Array.Copy( array, 0          , res, 0  ,               pos );
            Array.Copy( array, pos + count, res, pos, len - count - pos );

            return res;
        }

        public static T[] RemoveUniqueFromNotNullArray<T>( T[] array   ,
                                                           T   element )
        {
            int pos = FindInNotNullArray( array, element );

            if(pos >= 0)
            {
                return RemoveAtPositionFromNotNullArray( array, pos );
            }
            else
            {
                return array;
            }
        }

        //--//

        public static bool ArraySameLength<T>( T[] s ,
                                               T[] d )
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            return(sLen == dLen);
        }

        public static bool ArrayReferenceEqualsNotNull<T>( T[] s      ,
                                                           T[] d      ,
                                                           int offset )
        {
            int sLen = s.Length;
            int dLen = d.Length;

            if(sLen == dLen)
            {
                for(int i = offset; i < sLen; i++)
                {
                    if(Object.ReferenceEquals( s[i], d[i] ) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool ArrayEqualsNotNull<T>( T[] s      ,
                                                  T[] d      ,
                                                  int offset )
        {
            int sLen = s.Length;
            int dLen = d.Length;

            if(sLen == dLen)
            {
                for(int i = offset; i < sLen; i++)
                {
                    if(Object.Equals( s[i], d[i] ) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool SameContents<T>( T[] s ,
                                            T[] d )
        {
            if(s != null)
            {
                if(d == null)
                {
                    return false;
                }

                foreach(T v in s)
                {
                    if(FindInNotNullArray( d, v ) < 0)
                    {
                        return false;
                    }
                }

                foreach(T v in d)
                {
                    if(FindInNotNullArray( s, v ) < 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if(d != null)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ArrayEquals<T>( T[] s ,
                                           T[] d )
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            if(sLen == dLen)
            {
                for(int i = 0; i < sLen; i++)
                {
                    if(Object.Equals( s[i], d[i] ) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool ByteArrayEquals( byte[] s ,
                                            byte[] d )
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            if(sLen == dLen)
            {
                for(int i = 0; i < sLen; i++)
                {
                    if(s[i] != d[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool ByteArrayEquals( byte[] s       ,
                                            int    sOffset ,
                                            byte[] d       ,
                                            int    dOffset ,
                                            int    count   )
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            while(--count >= 0)
            {
                if(sOffset >= sLen ||
                   dOffset >= dLen  )
                {
                    return false;
                }

                if(s[sOffset++] != d[dOffset++])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool UIntArrayEquals( uint[] s ,
                                            uint[] d )
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            if(sLen == dLen)
            {
                for(int i = 0; i < sLen; i++)
                {
                    if(s[i] != d[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
