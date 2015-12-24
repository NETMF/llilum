// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StringBuilder
**
**
** Purpose: implementation of the StringBuilder
** class.
**
===========================================================*/
namespace System.Text
{
    using System.Text;
    using System.Runtime.Serialization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Globalization;
    ////using System.Runtime.Versioning;

    // This class represents a mutable string.  It is convenient for situations in
    // which it is desirable to modify a string, perhaps by removing, replacing, or 
    // inserting characters, without creating a new String subsequent to
    // each modification. 
    // 
    // The methods contained within this class do not return a new StringBuilder
    // object unless specified otherwise.  This class may be used in conjunction with the String
    // class to carry out modifications upon strings.
    // 
    // When passing null into a constructor in VJ and VC, the null
    // should be explicitly type cast.
    // For Example:
    // StringBuilder sb1 = new StringBuilder((StringBuilder)null);
    // StringBuilder sb2 = new StringBuilder((String)null);
    // Console.WriteLine(sb1);
    // Console.WriteLine(sb2);
    // 
    [Serializable]
    public sealed class StringBuilder /*: ISerializable*/
    {


        //
        //
        //  CLASS VARIABLES
        //
        //
        internal Thread m_currentThread = Thread.CurrentThread;
        internal int m_MaxCapacity = 0;
        //
        // making m_StringValue volatile to guarantee reading order in some places. 
        //
        internal volatile String m_StringValue = null;


        //
        //
        // STATIC CONSTANTS
        //
        //
        internal const int DefaultCapacity = 16;
////    private const String CapacityField = "Capacity";
////    private const String MaxCapacityField = "m_MaxCapacity";
////    private const String StringValueField = "m_StringValue";
////    private const String ThreadIDField = "m_currentThread";

        //
        //
        //CONSTRUCTORS
        //
        //

        // Creates a new empty string builder (i.e., it represents String.Empty)
        // with the default capacity (16 characters).
        public StringBuilder()
            : this( DefaultCapacity )
        {
        }

        // Create a new empty string builder (i.e., it represents String.Empty)
        // with the specified capacity.
        public StringBuilder( int capacity )
            : this( String.Empty, capacity )
        {
        }


        // Creates a new string builder from the specified string.  If value
        // is a null String (i.e., if it represents String.NullString)
        // then the new string builder will also be null (i.e., it will also represent
        //  String.NullString).
        // 
        public StringBuilder( String value )
            : this( value, DefaultCapacity )
        {
        }

        // Creates a new string builder from the specified string with the specified 
        // capacity.  If value is a null String (i.e., if it represents 
        // String.NullString) then the new string builder will also be null 
        // (i.e., it will also represent String.NullString).
        // The maximum number of characters this string may contain is set by capacity.
        // 
        public StringBuilder( String value, int capacity )
            : this( value, 0, ((value != null) ? value.Length : 0), capacity )
        {
        }

        // Creates a new string builder from the specifed substring with the specified
        // capacity.  The maximum number of characters is set by capacity.
        // 

        public StringBuilder( String value, int startIndex, int length, int capacity )
        {
            if(capacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity",
                                                      String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_MustBePositive" ), "capacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length",
                                                      String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegNum" ), "length" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(value == null)
            {
                value = String.Empty;
            }

            if(startIndex > value.Length - length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_IndexLength" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_MaxCapacity = Int32.MaxValue;

            if(capacity == 0)
            {
                capacity = DefaultCapacity;
            }

            while(capacity < length)
            {
                capacity *= 2;
                // If we overflow, we should just use length as capacity. 
                // There is no reason we should throw an exception in this case if the system is able 
                // to allocate the string.
                if(capacity < 0)
                {
                    capacity = length;
                    break;
                }
            }

            m_StringValue = String.GetStringForStringBuilder( value, startIndex, length, capacity );
        }

        // Creates an empty StringBuilder with a minimum capacity of capacity
        // and a maximum capacity of maxCapacity.
        public StringBuilder( int capacity, int maxCapacity )
        {
            if(capacity > maxCapacity)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity", Environment.GetResourceString( "ArgumentOutOfRange_Capacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(maxCapacity < 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "maxCapacity", Environment.GetResourceString( "ArgumentOutOfRange_SmallMaxCapacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(capacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity",
                                                      String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_MustBePositive" ), "capacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(capacity == 0)
            {
                capacity = Math.Min( DefaultCapacity, maxCapacity );
            }

            m_StringValue = String.GetStringForStringBuilder( String.Empty, capacity );
            m_MaxCapacity = maxCapacity;

        }

////    private StringBuilder( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////            throw new ArgumentNullException( "info" );
////
////        int persistedCapacity = 0;
////        string persistedString = null;
////        int persistedMaxCapacity = Int32.MaxValue;
////        bool capacityPresent = false;
////
////        // Get the data
////        SerializationInfoEnumerator enumerator = info.GetEnumerator();
////        while(enumerator.MoveNext())
////        {
////            switch(enumerator.Name)
////            {
////                case MaxCapacityField:
////                    persistedMaxCapacity = info.GetInt32( MaxCapacityField );
////                    break;
////                case StringValueField:
////                    persistedString = info.GetString( StringValueField );
////                    break;
////                case CapacityField:
////                    persistedCapacity = info.GetInt32( CapacityField );
////                    capacityPresent = true;
////                    break;
////                default:
////                    // Note: deliberately ignore "m_currentThread" which earlier
////                    // verisions incorrectly persisted.
////                    // Ignore other fields for forward compatability.
////                    break;
////            }
////
////        }
////
////        // Check values and set defaults
////        if(persistedString == null)
////        {
////            persistedString = String.Empty;
////        }
////        if(persistedMaxCapacity < 1 || persistedString.Length > persistedMaxCapacity)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_StringBuilderMaxCapacity" ) );
////        }
////
////        if(!capacityPresent)
////        {
////            // StringBuilder in V1.X did not persist the Capacity, so this is a valid legacy code path.
////            persistedCapacity = DefaultCapacity;
////            if(persistedCapacity < persistedString.Length)
////            {
////                persistedCapacity = persistedString.Length;
////            }
////            if(persistedCapacity > persistedMaxCapacity)
////            {
////                persistedCapacity = persistedMaxCapacity;
////            }
////        }
////        if(persistedCapacity < 0 || persistedCapacity < persistedString.Length || persistedCapacity > persistedMaxCapacity)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_StringBuilderCapacity" ) );
////        }
////
////        // Assign
////        m_MaxCapacity = persistedMaxCapacity;
////        m_StringValue = String.GetStringForStringBuilder( persistedString, 0, persistedString.Length, persistedCapacity );
////        VerifyClassInvariant();
////    }
////
////    void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        VerifyClassInvariant();
////
////        info.AddValue( MaxCapacityField, m_MaxCapacity );
////        info.AddValue( CapacityField, Capacity );
////        info.AddValue( StringValueField, m_StringValue );
////        // Note: persist "m_currentThread" to be compatible with old versions
////        info.AddValue( ThreadIDField, 0 );
////    }
////
////    [System.Diagnostics.Conditional( "_DEBUG" )]
////    private void VerifyClassInvariant()
////    {
////        BCLDebug.Assert( m_MaxCapacity >= 1, "Invalid StringBuilder" );
////        BCLDebug.Assert( Capacity >= 0 && Capacity <= m_MaxCapacity, "Invalid StringBuilder" );
////        BCLDebug.Assert( m_StringValue != null && Capacity >= m_StringValue.Length, "Invalid StringBuilder" );
////    }

        private String GetThreadSafeString( out Thread th )
        {
            // Following two reads (m_StringValue, m_currentThread) needs to happen in order.
            // This is guaranteed by making the fields volatile.     
            // See ReplaceString method for details.

            String temp = m_StringValue;
            th = Thread.CurrentThread;
            if(m_currentThread == th)
                return temp;
            return String.GetStringForStringBuilder( temp, temp.Capacity );
        }

        public int Capacity
        {
            get { return m_StringValue.Capacity; } //-1 to account for terminating null.
            set
            {
                Thread th;
                String currentString = GetThreadSafeString( out th );

                if(value < 0)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCapacity" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                if(value < currentString.Length)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                if(value > MaxCapacity)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_Capacity" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                int currCapacity = currentString.Capacity;
                //If we already have the correct capacity, bail out early.
                if(value != currCapacity)
                {
                    //Allocate a new String with the capacity and copy all of our old characters
                    //into it.  We've already guaranteed that our String will fit within that capacity.
                    //We don't need to worry about the COW bit because we're always allocating a new String.
                    String newString = String.GetStringForStringBuilder( currentString, value );
                    ReplaceString( th, newString );
                }
            }
        }

        public int MaxCapacity
        {
            get { return m_MaxCapacity; }

        }

        // Read-Only Property 
        // Ensures that the capacity of this string builder is at least the specified value.  
        // If capacity is greater than the capacity of this string builder, then the capacity
        // is set to capacity; otherwise the capacity is unchanged.
        // 
        public int EnsureCapacity( int capacity )
        {
            if(capacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCapacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            Thread th;
            String currentString = GetThreadSafeString( out th );

            //If we need more space or the COW bit is set, copy the buffer.
            if(!NeedsAllocation( currentString, capacity ))
            {
                return currentString.Capacity;
            }

            String newString = GetNewString( currentString, capacity );
            ReplaceString( th, newString );
            return newString.Capacity;
        }

        public override String ToString()
        {
            //
            // We assume that their read of m_currentThread will always occur after read of m_StringValue.  
            // If these reads get re-ordered then it is possible to get a currentString owned by some other 
            // (mutating) thread and yet think, according to currentThread, that such was not the case.
            // This is acheived by marking m_StringValue as volatile. 
            //
            String currentString = m_StringValue;
            Thread currentThread = m_currentThread;

            //
            // Note calling ToString the second time or from a different thread will cause allocation of a new string.
            // If we do not make a copy if currentThread is IntPtr.Zero, we will have following race:
            //
            // (1) Thread T1 completes a mutation of the string and will become the owner.   
            // T1 then starts another mutation operation and 
            // A thread interleaving happens at this point.  
            // (2) Thread T2 starts a ToString operation.  T2 reads m_StringValue into its local currentString variable.  
            // A thread interleaving happens at this point.
            // (3) Thread T3 finshes a mutation of the string in the StringBuilder , performing the ReplaceString call.  
            // Thread T3 then starts a ToString operation.  Assuming the string is not wasting excessive space,  
            // T3 will proceeds to call ClearPostNullChar, registers NOBODY as the owner, and returns the string.  
            // A thread interleaving happens at this point.
            // (4) Thread T2 resumes execution.  T2 reads m_currentThread and sees that NOBODY is the registered owner
            //  Assuming its currentString is not wasting excessive space, T2 will return the same string that thread T1 is 
            //  in the middle of mutating.  
            //
            if(currentThread != Thread.CurrentThread)
            {
                return String.InternalCopy( currentString );
            }

            if((2 * currentString.Length) < currentString.ArrayLength)
            {
                return String.InternalCopy( currentString );
            }

            currentString.ClearPostNullChar();
            m_currentThread = null;
            return currentString;
        }

        // Converts a substring of this string builder to a String.
        public String ToString( int startIndex, int length )
        {
            // We here enforce the policy copying underlying String data in all cases, since StringBuilder
            // uses the the internal m_StringValue reference mutably.
            return m_StringValue.InternalSubStringWithChecks( startIndex, length, true );
        }

        // Sets the length of the String in this buffer.  If length is less than the current
        // instance, the StringBuilder is truncated.  If length is greater than the current 
        // instance, nulls are appended.  The capacity is adjusted to be the same as the length.

        public int Length
        {
            get
            {
                return m_StringValue.Length;
            }
            set
            {
                Thread th;
                String currentString = GetThreadSafeString( out th );

                if(value == 0)
                { //the user is trying to clear the string
                    currentString.SetLength( 0 );
                    ReplaceString( th, currentString );
                    return;
                }

                int currentLength = currentString.Length;
                int newlength = value;
                //If our length is less than 0 or greater than our Maximum capacity, bail.
                if(newlength < 0)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "newlength", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                if(newlength > MaxCapacity)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "capacity", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                //Jump out early if our requested length our currentlength.
                //This will be a pretty rare branch.
                if(newlength == currentLength)
                {
                    return;
                }


                //If the StringBuilder has never been converted to a string, simply set the length
                //without allocating a new string.
                if(newlength <= currentString.Capacity)
                {
                    if(newlength > currentLength)
                    {
                        for(int i = currentLength; i < newlength; i++) // This is a rare case anyway.
                            currentString.InternalSetCharNoBoundsCheck( i, '\0' );
                    }

                    currentString.InternalSetCharNoBoundsCheck( newlength, '\0' ); //Null terminate.
                    currentString.SetLength( newlength );
                    ReplaceString( th, currentString );

                    return;
                }

                // CopyOnWrite set we need to allocate a String
                int newCapacity = (newlength > currentString.Capacity) ? newlength : currentString.Capacity;
                String newString = String.GetStringForStringBuilder( currentString, newCapacity );

                //We know exactly how many characters we need, so embed that knowledge in the String.
                newString.SetLength( newlength );
                ReplaceString( th, newString );
            }
        }

        [System.Runtime.CompilerServices.IndexerName( "Chars" )]
        public char this[int index]
        {
            get
            {
                return m_StringValue[index];
            }
            set
            {
                Thread th;
                String currentString = GetThreadSafeString( out th );
                currentString.SetChar( index, value );
                ReplaceString( th, currentString );
            }
        }

        // Appends a character at the end of this string builder. The capacity is adjusted as needed.
        public StringBuilder Append( char value, int repeatCount )
        {
            if(repeatCount == 0)
            {
                return this;
            }
            if(repeatCount < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "repeatCount", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }


            Thread th;
            String currentString = GetThreadSafeString( out th );

            int currentLength = currentString.Length;
            int requiredLength = currentLength + repeatCount;

            if(requiredLength < 0)
                throw new OutOfMemoryException();

            if(!NeedsAllocation( currentString, requiredLength ))
            {
                currentString.AppendInPlace( value, repeatCount, currentLength );
                ReplaceString( th, currentString );
                return this;
            }

            String newString = GetNewString( currentString, requiredLength );
            newString.AppendInPlace( value, repeatCount, currentLength );
            ReplaceString( th, newString );
            return this;
        }

        // Appends an array of characters at the end of this string builder. The capacity is adjusted as needed. 
        public StringBuilder Append( char[] value, int startIndex, int charCount )
        {
            int requiredLength;

            if(value == null)
            {
                if(startIndex == 0 && charCount == 0)
                {
                    return this;
                }
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(charCount == 0)
            {
                return this;
            }

            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(charCount < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(charCount > value.Length - startIndex)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            Thread th;
            String currentString = GetThreadSafeString( out th );

            int currentLength = currentString.Length;
            requiredLength = currentLength + charCount;
            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.AppendInPlace( value, startIndex, charCount, currentLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.AppendInPlace( value, startIndex, charCount, currentLength );
                ReplaceString( th, currentString );
            }

            return this;
        }

        // Appends a copy of this string at the end of this string builder.
        public StringBuilder Append( String value )
        {
            //If the value being added is null, eat the null
            //and return.
            if(value == null)
            {
                return this;
            }

            Thread th;
            String currentString = GetThreadSafeString( out th );

            int currentLength = currentString.Length;

            int requiredLength = currentLength + value.Length;

            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.AppendInPlace( value, currentLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.AppendInPlace( value, currentLength );
                ReplaceString( th, currentString );
            }

            return this;
        }

        internal unsafe StringBuilder Append( char* value, int count )
        {
            //If the value being added is null, eat the null
            //and return.
            if(value == null)
            {
                return this;
            }


            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            int requiredLength = currentLength + count;

            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.AppendInPlace( value, count, currentLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.AppendInPlace( value, count, currentLength );
                ReplaceString( th, currentString );
            }

            return this;
        }

#if WIN64   // STUBS_AS_IL
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe extern void ReplaceBufferInternal(char* newBuffer, int newLength);

        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe extern void ReplaceBufferAnsiInternal(sbyte* newBuffer, int newLength);
#endif // WIN64 // STUBS_AS_IL

        private bool NeedsAllocation( String currentString, int requiredLength )
        {
            //<= accounts for the terminating 0 which we require on strings.
            return (currentString.ArrayLength <= requiredLength);
        }

        private String GetNewString( String currentString, int requiredLength )
        {
            int newCapacity;
            int maxCapacity = m_MaxCapacity;

            if(requiredLength < 0)
            {
                throw new OutOfMemoryException();
            }

            if(requiredLength > maxCapacity)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "requiredLength", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            newCapacity = (currentString.Capacity) * 2; // To force a predicatable growth of 160,320 etc. for testing purposes

            if(newCapacity < requiredLength)
            {
                newCapacity = requiredLength;
            }

            if(newCapacity > maxCapacity)
            {
                newCapacity = maxCapacity;
            }

            if(newCapacity <= 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "newCapacity", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCapacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return String.GetStringForStringBuilder( currentString, newCapacity );
        }

        private void ReplaceString( Thread th, String value )
        {
            BCLDebug.Assert( value != null, "[StringBuilder.ReplaceString]value!=null" );

            // Following two writes (m_currentThread, m_StringValue) needs to happen in order.
            // This is guaranteed by making the fields volatile.
            //
            // If two threads are modifying the same StringBuilder at the same time, 
            // we don't want them to use the same StringValue. This is done by comparing 
            // owner thread id against current thread id in GetThreadSafeString. 
            // Here we need to guarantee the ordering of writes to make sure if the another thread
            // see the change we have done to m_StringValue, it will see the change to m_currentThread as well.
            //  
            m_currentThread = th; // new owner
            m_StringValue = value;
        }

        // Appends a copy of the characters in value from startIndex to startIndex +
        // count at the end of this string builder.
        public StringBuilder Append( String value, int startIndex, int count )
        {
            //If the value being added is null, eat the null
            //and return.
            if(value == null)
            {
                if(startIndex == 0 && count == 0)
                {
                    return this;
                }
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(count <= 0)
            {
                if(count == 0)
                {
                    return this;
                }
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex < 0 || (startIndex > value.Length - count))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            int requiredLength = currentLength + count;

            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.AppendInPlace( value, startIndex, count, currentLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.AppendInPlace( value, startIndex, count, currentLength );
                ReplaceString( th, currentString );
            }

            return this;
        }

        public StringBuilder AppendLine()
        {
            return Append( Environment.NewLine );
        }

        public StringBuilder AppendLine( string value )
        {
            Append( value );
            return Append( Environment.NewLine );
        }

        public void CopyTo( int sourceIndex, char[] destination, int destinationIndex, int count )
        {
            if(destination == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "destination" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( Environment.GetResourceString( "Arg_NegativeArgCount" ), "count" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(destinationIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException(
                    Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegNum", "destinationIndex" ), "destinationIndex" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(destinationIndex > destination.Length - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "ArgumentOutOfRange_OffsetOut" ) );
#else
                throw new ArgumentException();
#endif
            }

            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            if(sourceIndex < 0 || sourceIndex > currentLength)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(sourceIndex > currentLength - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_LongerThanSrcString" ) );
#else
                throw new ArgumentException();
#endif
            }

            // following fixed statement will throw exception for empty array.
            if(count == 0)
            {
                return;
            }

            unsafe
            {
                fixed(char* dest = &destination[destinationIndex], tsrc = currentString)
                {
                    char* src = tsrc + sourceIndex;

                    Buffer.InternalMemoryCopy( src, dest, count );
                }
            }
        }

        // Inserts multiple copies of a string into this string builder at the specified position.
        // Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, this
        // string builder is not changed. 
        // 
        public unsafe StringBuilder Insert( int index, String value, int count )
        {
            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            //Range check the index.
            if(index < 0 || index > currentLength)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            //If value is null, empty or count is 0, do nothing. This is ECMA standard.
            if(value == null || value.Length == 0 || count == 0)
            {
                return this;
            }

            //Calculate the new length, ensure that we have the space and set the space variable for this buffer
            int requiredLength;
            try
            {
                requiredLength = checked( currentLength + (value.Length * count) );
            }
            catch(OverflowException)
            {
                throw new OutOfMemoryException();
            }

            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.InsertInPlace( index, value, count, currentLength, requiredLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.InsertInPlace( index, value, count, currentLength, requiredLength );
                ReplaceString( th, currentString );
            }
            return this;
        }



        // Property.
        // Removes the specified characters from this string builder.
        // The length of this string builder is reduced by 
        // length, but the capacity is unaffected.
        // 
        public StringBuilder Remove( int startIndex, int length )
        {
            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(length > currentLength - startIndex)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            currentString.RemoveInPlace( startIndex, length, currentLength );
            ReplaceString( th, currentString );

            return this;
        }


        //
        //
        // PUBLIC INSTANCE FUNCTIONS
        //
        //

        /*====================================Append====================================
        **
        ==============================================================================*/
        // Appends a boolean to the end of this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append( bool value )
        {
            return Append( value.ToString() );
        }

        // Appends an sbyte to this string builder.
        // The capacity is adjusted as needed. 
        [CLSCompliant( false )]
        public StringBuilder Append( sbyte value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends a ubyte to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append( byte value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends a character at the end of this string builder. The capacity is adjusted as needed.
        public StringBuilder Append( char value )
        {
            Thread th;
            String currentString = GetThreadSafeString( out th );

            int currentLength = currentString.Length;
            if(!NeedsAllocation( currentString, currentLength + 1 ))
            {
                currentString.AppendInPlace( value, currentLength );
                ReplaceString( th, currentString );
                return this;
            }

            String newString = GetNewString( currentString, currentLength + 1 );
            newString.AppendInPlace( value, currentLength );
            ReplaceString( th, newString );
            return this;
        }

        // Appends a short to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append( short value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends an int to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append( int value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends a long to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append( long value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends a float to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append( float value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends a double to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append( double value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        public StringBuilder Append( decimal value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends an ushort to this string builder. 
        // The capacity is adjusted as needed. 
        [CLSCompliant( false )]
        public StringBuilder Append( ushort value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends an uint to this string builder. 
        // The capacity is adjusted as needed. 
        [CLSCompliant( false )]
        public StringBuilder Append( uint value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends an unsigned long to this string builder. 
        // The capacity is adjusted as needed. 

        [CLSCompliant( false )]
        public StringBuilder Append( ulong value )
        {
            return Append( value.ToString( CultureInfo.CurrentCulture ) );
        }

        // Appends an Object to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append( Object value )
        {
            if(null == value)
            {
                //Appending null is now a no-op.
                return this;
            }
            return Append( value.ToString() );
        }

        // Appends all of the characters in value to the current instance.
        public StringBuilder Append( char[] value )
        {
            if(null == value)
            {
                return this;
            }

            int valueLength = value.Length;

            Thread th;
            String currentString = GetThreadSafeString( out th );

            int currentLength = currentString.Length;
            int requiredLength = currentLength + value.Length;
            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.AppendInPlace( value, 0, valueLength, currentLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.AppendInPlace( value, 0, valueLength, currentLength );
                ReplaceString( th, currentString );
            }
            return this;
        }

        /*====================================Insert====================================
        **
        ==============================================================================*/

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, String value )
        {
            if(value == null) // This is to do the index validation
                return Insert( index, value, 0 );
            else
                return Insert( index, value, 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, bool value )
        {
            return Insert( index, value.ToString(), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        [CLSCompliant( false )]
        public StringBuilder Insert( int index, sbyte value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, byte value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, short value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, char value )
        {
            return Insert( index, Char.ToString( value ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, char[] value )
        {
            if(null == value)
            {
                return Insert( index, value, 0, 0 );
            }
            return Insert( index, value, 0, value.Length );
        }

        // Returns a reference to the StringBuilder with charCount characters from 
        // value inserted into the buffer at index.  Existing characters are shifted
        // to make room for the new text and capacity is adjusted as required.  If value is null, the StringBuilder
        // is unchanged.  Characters are taken from value starting at position startIndex.
        public StringBuilder Insert( int index, char[] value, int startIndex, int charCount )
        {
            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            //Range check the index.
            if(index < 0 || index > currentLength)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            //If they passed in a null char array, just jump out quickly.
            if(value == null)
            {
                if(startIndex == 0 && charCount == 0)
                {
                    return this;
                }
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( Environment.GetResourceString( "ArgumentNull_String" ) );
#else
                throw new ArgumentNullException();
#endif
            }


            //Range check the array.
            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(charCount < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex > value.Length - charCount)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(charCount == 0)
            {
                // There is no data to insert, so just return.                   
                return this;
            }

            int requiredLength = currentLength + charCount;
            if(NeedsAllocation( currentString, requiredLength ))
            {
                String newString = GetNewString( currentString, requiredLength );
                newString.InsertInPlace( index, value, startIndex, charCount, currentLength, requiredLength );
                ReplaceString( th, newString );
            }
            else
            {
                currentString.InsertInPlace( index, value, startIndex, charCount, currentLength, requiredLength );
                ReplaceString( th, currentString );
            }
            return this;
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, int value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, long value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, float value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }


        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed. 
        // 
        public StringBuilder Insert( int index, double value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        public StringBuilder Insert( int index, decimal value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant( false )]
        public StringBuilder Insert( int index, ushort value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }


        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant( false )]
        public StringBuilder Insert( int index, uint value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant( false )]
        public StringBuilder Insert( int index, ulong value )
        {
            return Insert( index, value.ToString( CultureInfo.CurrentCulture ), 1 );
        }

        // Returns a reference to this string builder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the
        // new text.  The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed. No changes are made if value is null.
        // 
        public StringBuilder Insert( int index, Object value )
        {
            //If we get a null 
            if(null == value)
            {
                return this;
            }
            return Insert( index, value.ToString(), 1 );
        }

        public StringBuilder AppendFormat( String format, Object arg0 )
        {
            return AppendFormat( null, format, new Object[] { arg0 } );
        }

        public StringBuilder AppendFormat( String format, Object arg0, Object arg1 )
        {
            return AppendFormat( null, format, new Object[] { arg0, arg1 } );
        }

        public StringBuilder AppendFormat( String format, Object arg0, Object arg1, Object arg2 )
        {
            return AppendFormat( null, format, new Object[] { arg0, arg1, arg2 } );
        }

        public StringBuilder AppendFormat( String format, params Object[] args )
        {
            return AppendFormat( null, format, args );
        }

        private static void FormatError()
        {
#if EXCEPTION_STRINGS
            throw new FormatException( Environment.GetResourceString( "Format_InvalidString" ) );
#else
            throw new FormatException();
#endif
        }

        public StringBuilder AppendFormat( IFormatProvider provider, String format, params Object[] args )
        {
            if(format == null || args == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (format == null) ? "format" : "args" );
#else
                throw new ArgumentNullException();
#endif
            }
            char[] chars = format.ToCharArray( 0, format.Length );
            int pos = 0;
            int len = chars.Length;
            char ch = '\x0';

            ICustomFormatter cf = null;
            if(provider != null)
            {
                cf = (ICustomFormatter)provider.GetFormat( typeof( ICustomFormatter ) );
            }

            while(true)
            {
                int p = pos;
                int i = pos;
                while(pos < len)
                {
                    ch = chars[pos];

                    pos++;
                    if(ch == '}')
                    {
                        if(pos < len && chars[pos] == '}') // Treat as escape character for }}
                            pos++;
                        else
                            FormatError();
                    }

                    if(ch == '{')
                    {
                        if(pos < len && chars[pos] == '{') // Treat as escape character for {{
                            pos++;
                        else
                        {
                            pos--;
                            break;
                        }
                    }

                    chars[i++] = ch;
                }
                if(i > p) Append( chars, p, i - p );
                if(pos == len) break;
                pos++;
                if(pos == len || (ch = chars[pos]) < '0' || ch > '9') FormatError();
                int index = 0;
                do
                {
                    index = index * 10 + ch - '0';
                    pos++;
                    if(pos == len) FormatError();
                    ch = chars[pos];
                } while(ch >= '0' && ch <= '9' && index < 1000000);
                if(index >= args.Length) 
#if EXCEPTION_STRINGS
                    throw new FormatException( Environment.GetResourceString( "Format_IndexOutOfRange" ) );
#else
                    throw new FormatException();
#endif
                while(pos < len && (ch = chars[pos]) == ' ') pos++;
                bool leftJustify = false;
                int width = 0;
                if(ch == ',')
                {
                    pos++;
                    while(pos < len && chars[pos] == ' ') pos++;

                    if(pos == len) FormatError();
                    ch = chars[pos];
                    if(ch == '-')
                    {
                        leftJustify = true;
                        pos++;
                        if(pos == len) FormatError();
                        ch = chars[pos];
                    }
                    if(ch < '0' || ch > '9') FormatError();
                    do
                    {
                        width = width * 10 + ch - '0';
                        pos++;
                        if(pos == len) FormatError();
                        ch = chars[pos];
                    } while(ch >= '0' && ch <= '9' && width < 1000000);
                }

                while(pos < len && (ch = chars[pos]) == ' ') pos++;
                Object arg = args[index];
                String fmt = null;
                if(ch == ':')
                {
                    pos++;
                    p = pos;
                    i = pos;
                    while(true)
                    {
                        if(pos == len) FormatError();
                        ch = chars[pos];
                        pos++;
                        if(ch == '{')
                        {
                            if(pos < len && chars[pos] == '{')  // Treat as escape character for {{
                                pos++;
                            else
                                FormatError();
                        }
                        else if(ch == '}')
                        {
                            if(pos < len && chars[pos] == '}')  // Treat as escape character for }}
                                pos++;
                            else
                            {
                                pos--;
                                break;
                            }
                        }

                        chars[i++] = ch;
                    }
                    if(i > p) fmt = new String( chars, p, i - p );
                }
                if(ch != '}') FormatError();
                pos++;
                String s = null;
                if(cf != null)
                {
                    s = cf.Format( fmt, arg, provider );
                }

                if(s == null)
                {
                    if(arg is IFormattable)
                    {
                        s = ((IFormattable)arg).ToString( fmt, provider );
                    }
                    else if(arg != null)
                    {
                        s = arg.ToString();
                    }
                }

                if(s == null) s = String.Empty;
                int pad = width - s.Length;
                if(!leftJustify && pad > 0) Append( ' ', pad );
                Append( s );
                if(leftJustify && pad > 0) Append( ' ', pad );
            }
            return this;
        }

        // Returns a reference to the current StringBuilder with all instances of oldString 
        // replaced with newString.  If startIndex and count are specified,
        // we only replace strings completely contained in the range of startIndex to startIndex + 
        // count.  The strings to be replaced are checked on an ordinal basis (e.g. not culture aware).  If 
        // newValue is null, instances of oldValue are removed (e.g. replaced with nothing.).
        //
        public StringBuilder Replace( String oldValue, String newValue )
        {
            return Replace( oldValue, newValue, 0, Length );
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public extern StringBuilder Replace( String oldValue, String newValue, int startIndex, int count );

        public bool Equals( StringBuilder sb )
        {
            if(sb == null)
                return false;
            return ((this.Capacity == sb.Capacity) && (this.MaxCapacity == sb.MaxCapacity) && (this.m_StringValue.Equals( sb.m_StringValue )));
        }

        // Returns a StringBuilder with all instances of oldChar replaced with 
        // newChar.  The size of the StringBuilder is unchanged because we're only
        // replacing characters.  If startIndex and count are specified, we 
        // only replace characters in the range from startIndex to startIndex+count
        //
        public StringBuilder Replace( char oldChar, char newChar )
        {
            return Replace( oldChar, newChar, 0, Length );
        }

        public StringBuilder Replace( char oldChar, char newChar, int startIndex, int count )
        {
            Thread th;
            String currentString = GetThreadSafeString( out th );
            int currentLength = currentString.Length;

            if((uint)startIndex > (uint)currentLength)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count < 0 || startIndex > currentLength - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            BCLDebug.Assert( !NeedsAllocation( currentString, currentLength ), "New allocation should not happen!" );
            currentString.ReplaceCharInPlace( oldChar, newChar, startIndex, count, currentLength );
            ReplaceString( th, currentString );
            return this;
        }

        public StringBuilder Clear()
        {
            m_StringValue.SetLength( 0 ); return this;
        }
    }
}


 
