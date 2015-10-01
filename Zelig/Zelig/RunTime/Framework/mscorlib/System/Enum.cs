// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System
{
    using System;
    using System.Reflection;
////using System.Reflection.Emit;
////using System.Text;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
////using System.Runtime.Versioning;

    [Microsoft.Zelig.Internals.WellKnownType( "System_Enum" )]
    [Serializable]
    public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible
    {
        #region Private Static Data Members
////    private        const int    maxHashElements        = 100; // to trim the working set
////    private        const String enumSeperator          = ", ";
////    private static char[]       enumSeperatorCharArray = new char[] { ',' };
////    private static Type         intType                = typeof( int );
////    private static Type         stringType             = typeof( String );
////    private static Hashtable    fieldInfoHash          = Hashtable.Synchronized( new Hashtable() );
        #endregion

        #region Private Static Methods

////    private static void ValidateEnumType( Type enumType)
////    {
////        if(enumType == null)
////        {
////            throw new ArgumentNullException( "enumType" );
////        }
////
////        if(!(enumType is RuntimeType))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "enumType" );
////        }
////
////        if(!enumType.IsEnum)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeEnum" ), "enumType" );
////        }
////    }

////    private static FieldInfo GetValueField( Type type )
////    {
////        FieldInfo[] flds;
////
////        flds = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
////
////        if((flds == null) || (flds.Length != 1))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_EnumMustHaveUnderlyingValueField" ) );
////        }
////
////        return flds[0];
////    }
////
////    private static HashEntry GetHashEntry( Type enumType )
////    {
////        HashEntry hashEntry = (HashEntry)fieldInfoHash[enumType];
////
////        if(hashEntry == null)
////        {
////            // To reduce the workingset we clear the hashtable when a threshold number of elements are inserted.
////            if(fieldInfoHash.Count > maxHashElements)
////            {
////                fieldInfoHash.Clear();
////            }
////
////            ulong[] values = null;
////            String[] names = null;
////
////            BCLDebug.Assert( enumType.BaseType == typeof( Enum ), "Base type must of type Enum" );
////            if(enumType.BaseType == typeof( Enum ))
////            {
////                InternalGetEnumValues( enumType, ref values, ref names );
////            }
////            // If we switch over to EnumBuilder, this code path will be required.
////            else
////            {
////                // fall back on reflection for odd cases
////                FieldInfo[] flds = enumType.GetFields( BindingFlags.Static | BindingFlags.Public );
////
////                values = new ulong[flds.Length];
////                names = new String[flds.Length];
////                for(int i = 0; i < flds.Length; i++)
////                {
////                    names[i] = flds[i].Name;
////                    values[i] = ToUInt64( flds[i].GetValue( null ) );
////                }
////
////                // Insertion Sort these values in ascending order.
////                // We use this O(n^2) algorithm, but it turns out that most of the time the elements are already in sorted order and
////                // the common case performance will be faster than quick sorting this.
////                for(int i = 1; i < values.Length; i++)
////                {
////                    int    j         = i;
////                    String tempStr   = names[i];
////                    ulong  val       = values[i];
////                    bool   exchanged = false;
////
////                    // Since the elements are sorted we only need to do one comparision, we keep the check for j inside the loop.
////                    while(values[j - 1] > val)
////                    {
////                        names [j] = names [j - 1];
////                        values[j] = values[j - 1];
////
////                        j--;
////
////                        exchanged = true;
////                        if(j == 0)
////                        {
////                            break;
////                        }
////                    }
////
////                    if(exchanged)
////                    {
////                        names [j] = tempStr;
////                        values[j] = val;
////                    }
////                }
////            }
////
////            hashEntry = new HashEntry( names, values );
////
////            fieldInfoHash[enumType] = hashEntry;
////        }
////
////        return hashEntry;
////    }
////
////    private static String InternalGetValueAsString( Type enumType, Object value )
////    {
////        //Don't ask for the private fields.  Only .value is private and we don't need that.
////        HashEntry hashEntry = GetHashEntry     ( enumType );
////        Type      eT        = GetUnderlyingType( enumType );
////
////        // Lets break this up based upon the size.  We'll do part as an 64bit value
////        //  and part as the 32bit values.
////        if(eT == intType || eT == typeof( short ) || eT == typeof( long ) || eT == typeof( ushort ) || eT == typeof( byte ) || eT == typeof( sbyte ) || eT == typeof( uint ) || eT == typeof( ulong ))
////        {
////            ulong val   = ToUInt64( value );
////            int   index = BinarySearch( hashEntry.values, val );
////
////            if(index >= 0)
////            {
////                return hashEntry.names[index];
////            }
////        }
////
////        return null;
////    }
////
////    private static String InternalFormattedHexString( Object value )
////    {
////        TypeCode typeCode = Convert.GetTypeCode( value );
////
////        switch(typeCode)
////        {
////            case TypeCode.SByte:
////                {
////                    Byte result = (byte)(sbyte)value;
////
////                    return result.ToString( "X2", null );
////                }
////
////            case TypeCode.Byte:
////                {
////                    Byte result = (byte)value;
////
////                    return result.ToString( "X2", null );
////                }
////
////            case TypeCode.Int16:
////                {
////                    UInt16 result = (UInt16)(Int16)value;
////
////                    return result.ToString( "X4", null );
////                }
////
////            case TypeCode.UInt16:
////                {
////                    UInt16 result = (UInt16)value;
////
////                    return result.ToString( "X4", null );
////                }
////
////            case TypeCode.UInt32:
////                {
////                    UInt32 result = (UInt32)value;
////
////                    return result.ToString( "X8", null );
////                }
////
////            case TypeCode.Int32:
////                {
////                    UInt32 result = (UInt32)(int)value;
////
////                    return result.ToString( "X8", null );
////                }
////
////            case TypeCode.UInt64:
////                {
////                    UInt64 result = (UInt64)value;
////
////                    return result.ToString( "X16", null );
////                }
////
////            case TypeCode.Int64:
////                {
////                    UInt64 result = (UInt64)(Int64)value;
////
////                    return result.ToString( "X16", null );
////                }
////
////            // All unsigned types will be directly cast
////            default:
////                BCLDebug.Assert( false, "Invalid Object type in Format" );
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnknownEnumType" ) );
////        }
////    }
////
////    private static String InternalFormat( Type eT, Object value )
////    {
////        if(!eT.IsDefined( typeof( System.FlagsAttribute ), false )) // Not marked with Flags attribute
////        {
////            // Try to see if its one of the enum values, then we return a String back else the value
////            String retval = InternalGetValueAsString( eT, value );
////            if(retval == null)
////            {
////                return value.ToString();
////            }
////            else
////            {
////                return retval;
////            }
////        }
////        else // These are flags OR'ed together (We treat everything as unsigned types)
////        {
////            return InternalFlagsFormat( eT, value );
////        }
////    }
////
////    private static String InternalFlagsFormat( Type eT, Object value )
////    {
////        ulong     result    = ToUInt64( value );
////        HashEntry hashEntry = GetHashEntry( eT );
////
////        // These values are sorted by value. Don't change this
////        String[] names  = hashEntry.names;
////        ulong[]  values = hashEntry.values;
////
////        int           index      = values.Length - 1;
////        StringBuilder retval     = new StringBuilder();
////        bool          firstTime  = true;
////        ulong         saveResult = result;
////
////        // We will not optimize this code further to keep it maintainable. There are some boundary checks that can be applied
////        // to minimize the comparsions required. This code works the same for the best/worst case. In general the number of
////        // items in an enum are sufficiently small and not worth the optimization.
////        while(index >= 0)
////        {
////            if((index == 0) && (values[index] == 0))
////            {
////                break;
////            }
////
////            if((result & values[index]) == values[index])
////            {
////                result -= values[index];
////
////                if(!firstTime)
////                {
////                    retval.Insert( 0, enumSeperator );
////                }
////
////                retval.Insert( 0, names[index] );
////                firstTime = false;
////            }
////
////            index--;
////        }
////
////        // We were unable to represent this number as a bitwise or of valid flags
////        if(result != 0)
////        {
////            return value.ToString();
////        }
////
////        // For the case when we have zero
////        if(saveResult == 0)
////        {
////            if(values[0] == 0)
////            {
////                return names[0]; // Zero was one of the enum values.
////            }
////            else
////            {
////                return "0";
////            }
////        }
////        else
////        {
////            return retval.ToString(); // Return the string representation
////        }
////    }
////
////    private static ulong ToUInt64( Object value )
////    {
////        // Helper function to silently convert the value to UInt64 from the other base types for enum without throwing an exception.
////        // This is need since the Convert functions do overflow checks.
////        TypeCode typeCode = Convert.GetTypeCode( value );
////        ulong    result;
////
////        switch(typeCode)
////        {
////            case TypeCode.SByte:
////            case TypeCode.Int16:
////            case TypeCode.Int32:
////            case TypeCode.Int64:
////                result = (UInt64)Convert.ToInt64( value, CultureInfo.InvariantCulture );
////                break;
////
////            case TypeCode.Byte:
////            case TypeCode.UInt16:
////            case TypeCode.UInt32:
////            case TypeCode.UInt64:
////                result = Convert.ToUInt64( value, CultureInfo.InvariantCulture );
////                break;
////
////            default:
////                // All unsigned types will be directly cast
////                BCLDebug.Assert( false, "Invalid Object type in ToUInt64" );
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnknownEnumType" ) );
////        }
////        return result;
////    }
////
////    private static int BinarySearch( ulong[] array, ulong value )
////    {
////        int lo = 0;
////        int hi = array.Length - 1;
////
////        while(lo <= hi)
////        {
////            int   i    = (lo + hi) >> 1;
////            ulong temp = array[i];
////
////            if(value == temp) return i;
////
////            if(temp < value)
////            {
////                lo = i + 1;
////            }
////            else
////            {
////                hi = i - 1;
////            }
////        }
////
////        return ~lo;
////    }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern int InternalCompareTo( Object o1, Object o2 );

////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern Type InternalGetUnderlyingType( Type enumType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void InternalGetEnumValues( Type enumType, ref ulong[] values, ref String[] names );
        #endregion

        #region Public Static Methods

        public static Object Parse( Type enumType, String value )
        {
            return Parse( enumType, value, false );
        }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern Object Parse( Type enumType, String value, bool ignoreCase );
////    {
////        ValidateEnumType( enumType );
////
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        value = value.Trim();
////        if(value.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustContainEnumInfo" ) );
////        }
////
////        // We have 2 code paths here. One if they are values else if they are Strings.
////        // values will have the first character as as number or a sign.
////        ulong result = 0;
////
////        if(Char.IsDigit( value[0] ) || value[0] == '-' || value[0] == '+')
////        {
////            Type   underlyingType = GetUnderlyingType( enumType );
////            Object temp;
////
////            try
////            {
////                temp = Convert.ChangeType( value, underlyingType, CultureInfo.InvariantCulture );
////
////                return ToObject( enumType, temp );
////            }
////            catch(FormatException)
////            { // We need to Parse this a String instead. There are cases
////                // when you tlbimp enums that can have values of the form "3D".
////                // Don't fix this code.
////            }
////        }
////
////        String[] values = value.Split( enumSeperatorCharArray );
////
////        // Find the field.Lets assume that these are always static classes because the class is
////        //  an enum.
////        HashEntry hashEntry = GetHashEntry( enumType );
////        String[]  names     = hashEntry.names;
////
////        for(int i = 0; i < values.Length; i++)
////        {
////            values[i] = values[i].Trim(); // We need to remove whitespace characters
////
////            bool success = false;
////
////            for(int j = 0; j < names.Length; j++)
////            {
////                if(ignoreCase)
////                {
////                    if(String.Compare( names[j], values[i], StringComparison.OrdinalIgnoreCase ) != 0)
////                    {
////                        continue;
////                    }
////                }
////                else
////                {
////                    if(!names[j].Equals( values[i] ))
////                    {
////                        continue;
////                    }
////                }
////
////                ulong item = hashEntry.values[j];
////
////                result |= item;
////                success = true;
////                break;
////            }
////
////            if(!success)
////            {
////                // Not found, throw an argument exception.
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumValueNotFound" ), value ) );
////            }
////        }
////
////        return ToObject( enumType, result );
////    }

        public static Type GetUnderlyingType( Type enumType )
        {
            throw new NotImplementedException();
////        ////// Make this function working for EnumBuilder. JScript uses it.
////        ////if(enumType is EnumBuilder)
////        ////{
////        ////    return ((EnumBuilder)enumType).UnderlyingSystemType;
////        ////}
////
////        ValidateEnumType( enumType );
////
////        return InternalGetUnderlyingType( enumType );
        }

        public static Array GetValues( Type enumType )
        {
            throw new NotImplementedException();
////        ValidateEnumType( enumType );
////
////        // Get all of the values
////        ulong[] values = GetHashEntry( enumType ).values;
////
////        // Create a generic Array
////        Array ret = Array.CreateInstance( enumType, values.Length );
////
////        for(int i = 0; i < values.Length; i++)
////        {
////            Object val = ToObject( enumType, values[i] );
////
////            ret.SetValue( val, i );
////        }
////
////        return ret;
        }

////    public static String GetName( Type enumType, Object value )
////    {
////        ValidateEnumType( enumType );
////
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        Type valueType = value.GetType();
////
////        if(valueType.IsEnum || valueType == intType || valueType == typeof( short ) || valueType == typeof( ushort ) || valueType == typeof( byte ) || valueType == typeof( sbyte ) || valueType == typeof( uint ) || valueType == typeof( long ) || valueType == typeof( ulong ))
////        {
////            return InternalGetValueAsString( enumType, value );
////        }
////
////        throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeEnumBaseTypeOrEnum" ), "value" );
////    }
////
////    public static String[] GetNames( Type enumType )
////    {
////        ValidateEnumType( enumType );
////
////        // Get all of the Field names
////        String[] ret = GetHashEntry( enumType ).names;
////
////        // Make a copy since we can't hand out the same array since users can modify them
////        String[] retVal = new String[ret.Length];
////
////        Array.Copy( ret, retVal, ret.Length );
////
////        return retVal;
////    }
////
////    public static Object ToObject( Type enumType, Object value )
////    {
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        // Delegate rest of error checking to the other functions
////        TypeCode typeCode = Convert.GetTypeCode( value );
////
////        switch(typeCode)
////        {
////            case TypeCode.Int32:
////                return ToObject( enumType, (int)value );
////
////            case TypeCode.SByte:
////                return ToObject( enumType, (sbyte)value );
////
////            case TypeCode.Int16:
////                return ToObject( enumType, (short)value );
////
////            case TypeCode.Int64:
////                return ToObject( enumType, (long)value );
////
////            case TypeCode.UInt32:
////                return ToObject( enumType, (uint)value );
////
////            case TypeCode.Byte:
////                return ToObject( enumType, (byte)value );
////
////            case TypeCode.UInt16:
////                return ToObject( enumType, (ushort)value );
////
////            case TypeCode.UInt64:
////                return ToObject( enumType, (ulong)value );
////
////            default:
////                // All unsigned types will be directly cast
////                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeEnumBaseTypeOrEnum" ), "value" );
////        }
////    }
////
////    public static bool IsDefined( Type enumType, Object value )
////    {
////        ValidateEnumType( enumType );
////
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        // Check if both of them are of the same type
////        Type valueType = value.GetType();
////
////        if(!(valueType is RuntimeType))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "valueType" );
////        }
////
////        Type underlyingType = GetUnderlyingType( enumType );
////
////        // If the value is an Enum then we need to extract the underlying value from it
////        if(valueType.IsEnum)
////        {
////            Type valueUnderlyingType = GetUnderlyingType( valueType );
////
////            if(valueType != enumType)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumAndObjectMustBeSameType" ), valueType.ToString(), enumType.ToString() ) );
////            }
////
////            valueType = valueUnderlyingType;
////        }
////        else
////        {
////            // The value must be of the same type as the Underlying type of the Enum
////            if((valueType != underlyingType) && (valueType != stringType))
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumUnderlyingTypeAndObjectMustBeSameType" ), valueType.ToString(), underlyingType.ToString() ) );
////            }
////        }
////
////        // If String is passed in
////        if(valueType == stringType)
////        {
////            // Get all of the Fields
////            String[] names = GetHashEntry( enumType ).names;
////
////            for(int i = 0; i < names.Length; i++)
////            {
////                if(names[i].Equals( (string)value ))
////                {
////                    return true;
////                }
////            }
////
////            return false;
////        }
////
////        ulong[] values = GetHashEntry( enumType ).values;
////
////        // Look at the 8 possible enum base classes
////        if(valueType == intType || valueType == typeof( short ) || valueType == typeof( ushort ) || valueType == typeof( byte ) || valueType == typeof( sbyte ) || valueType == typeof( uint ) || valueType == typeof( long ) || valueType == typeof( ulong ))
////        {
////            ulong val = ToUInt64( value );
////
////            return (BinarySearch( values, val ) >= 0);
////        }
////
////        BCLDebug.Assert( false, "Unknown enum type" );
////        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnknownEnumType" ) );
////    }
////
////    public static String Format( Type enumType, Object value, String format )
////    {
////        ValidateEnumType( enumType );
////
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        if(format == null)
////        {
////            throw new ArgumentNullException( "format" );
////        }
////
////        // Check if both of them are of the same type
////        Type valueType = value.GetType();
////
////        if(!(valueType is RuntimeType))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "valueType" );
////        }
////
////        Type underlyingType = GetUnderlyingType( enumType );
////
////        // If the value is an Enum then we need to extract the underlying value from it
////        if(valueType.IsEnum)
////        {
////            Type valueUnderlyingType = GetUnderlyingType( valueType );
////
////            if(valueType != enumType)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumAndObjectMustBeSameType" ), valueType.ToString(), enumType.ToString() ) );
////            }
////
////            valueType =        valueUnderlyingType;
////            value     = ((Enum)value).GetValue();
////        }
////        // The value must be of the same type as the Underlying type of the Enum
////        else if(valueType != underlyingType)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType" ), valueType.ToString(), underlyingType.ToString() ) );
////        }
////
////        if(format.Length != 1)
////        {
////            // all acceptable format string are of length 1
////            throw new FormatException( Environment.GetResourceString( "Format_InvalidEnumFormatSpecification" ) );
////        }
////
////        char formatCh = format[0];
////
////        if(formatCh == 'D' || formatCh == 'd')
////        {
////            return value.ToString();
////        }
////
////        if(formatCh == 'X' || formatCh == 'x')
////        {
////            // Retrieve the value from the field.
////            return InternalFormattedHexString( value );
////        }
////
////        if(formatCh == 'G' || formatCh == 'g')
////        {
////            return InternalFormat( enumType, value );
////        }
////
////        if(formatCh == 'F' || formatCh == 'f')
////        {
////            return InternalFlagsFormat( enumType, value );
////        }
////
////        throw new FormatException( Environment.GetResourceString( "Format_InvalidEnumFormatSpecification" ) );
////    }
////
        #endregion

        #region Definitions
////    private class HashEntry
////    {
////        // Each entry contains a list of sorted pair of enum field names and values, sorted by values
////        public HashEntry( String[] names, ulong[] values )
////        {
////            this.names  = names;
////            this.values = values;
////        }
////
////        public String[] names;
////        public ulong[]  values;
////    }
        #endregion

        #region Private Methods
        private Object GetValue()
        {
            throw new NotImplementedException();
////        return InternalGetValue();
        }
    
////    private String ToHexString()
////    {
////        Type      eT        = this.GetType();
////        FieldInfo thisField = GetValueField( eT );
////
////        // Retrieve the value from the field.
////        return InternalFormattedHexString( ((RtFieldInfo)thisField).InternalGetValue( this, false ) );
////        //return  InternalFormattedHexString(((RuntimeFieldInfo)thisField).GetValue(this));
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern Object InternalGetValue();
////
        #endregion

        #region Object Overrides
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern override bool Equals( Object obj );
////    ////FCIMPL2(FC_BOOL_RET, ReflectionEnum::InternalEquals, Object *pRefThis, Object* pRefTarget)
////    ////{
////    ////    FCALL_CONTRACT;
////    ////
////    ////    VALIDATEOBJECT(pRefThis);
////    ////    BOOL ret = false;
////    ////    if (pRefTarget == NULL) {
////    ////        FC_RETURN_BOOL(ret);
////    ////    }
////    ////
////    ////    if( pRefThis == pRefTarget)
////    ////        FC_RETURN_BOOL(true);
////    ////
////    ////    //Make sure we are comparing same type.
////    ////    MethodTable* pMTThis = pRefThis->GetMethodTable();
////    ////    _ASSERTE(!pMTThis->IsArray());  // bunch of assumptions about arrays wrong.
////    ////    if ( pMTThis != pRefTarget->GetMethodTable()) {
////    ////        FC_RETURN_BOOL(ret);
////    ////    }
////    ////
////    ////    void * pThis = pRefThis->UnBox();
////    ////    void * pTarget = pRefTarget->UnBox();
////    ////    switch (pMTThis->GetNumInstanceFieldBytes()) {
////    ////    case 1:
////    ////        ret = (*(UINT8*)pThis == *(UINT8*)pTarget);
////    ////        break;
////    ////    case 2:
////    ////        ret = (*(UINT16*)pThis == *(UINT16*)pTarget);
////    ////        break;
////    ////    case 4:
////    ////        ret = (*(UINT32*)pThis == *(UINT32*)pTarget);
////    ////        break;
////    ////    case 8:
////    ////        ret = (*(UINT64*)pThis == *(UINT64*)pTarget);
////    ////        break;
////    ////    default:
////    ////        // should not reach here.
////    ////        UNREACHABLE_MSG("Incorrect Enum Type size!");
////    ////        break;
////    ////    }
////    ////
////    ////    FC_RETURN_BOOL(ret);
////    ////}
////    ////FCIMPLEND
////
////    public override int GetHashCode()
////    {
////        return GetValue().GetHashCode();
////    }
////
////    public override String ToString()
////    {
////        // Returns the value in a human readable format.  For PASCAL style enums who's value maps directly the name of the field is returned.
////        // For PASCAL style enums who's values do not map directly the decimal value of the field is returned.
////        // For BitFlags (indicated by the Flags custom attribute): If for each bit that is set in the value there is a corresponding constant
////        //(a pure power of 2), then the  OR string (ie "Red | Yellow") is returned. Otherwise, if the value is zero or if you can't create a string that consists of
////        // pure powers of 2 OR-ed together, you return a hex value
////        Type      eT        = this.GetType();
////        FieldInfo thisField = GetValueField( eT );
////
////        // Retrieve the value from the field.
////        Object value = ((RtFieldInfo)thisField).InternalGetValue( this, false );
////
////        //Object value = ((RuntimeFieldInfo)thisField).GetValueInternal(this);
////        return InternalFormat( eT, value );
////    }
        #endregion

        #region IFormattable
        [Obsolete( "The provider argument is not used. Please use ToString(String)." )]
        public String ToString( String format, IFormatProvider provider )
        {
            return ToString( format );
        }
        #endregion

        #region IComparable
        public int CompareTo( Object target )
        {
            const int retIncompatibleMethodTables = 2; // indicates that the method tables did not match
            const int retInvalidEnumType          = 3; // indicates that the enum was of an unknown/unsupported unerlying type

            if(this == null)
            {
                throw new NullReferenceException();
            }

            int ret = InternalCompareTo( this, target );

            if(ret < retIncompatibleMethodTables)
            {
                // -1, 0 and 1 are the normal return codes
                return ret;
            }
            else if(ret == retIncompatibleMethodTables)
            {
                Type thisType   = this.GetType();
                Type targetType = target.GetType();

#if EXCEPTION_STRINGS
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumAndObjectMustBeSameType" ), targetType.ToString(), thisType.ToString() ) );
#else
                throw new ArgumentException();
#endif
            }
            else
            {
                // assert valid return code (3)
                BCLDebug.Assert( ret == retInvalidEnumType, "Enum.InternalCompareTo return code was invalid" );

#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnknownEnumType" ) );
#else
                throw new InvalidOperationException();
#endif
            }
        }
        #endregion

        #region Public Methods
        public String ToString( String format )
        {
            return ToString();
////        if(format == null || format.Length == 0)
////        {
////            format = "G";
////        }
////
////        if(String.Compare( format, "G", StringComparison.OrdinalIgnoreCase ) == 0)
////        {
////            return ToString();
////            //              return InternalFormat(this.GetType(), this.GetValue());
////        }
////
////        if(String.Compare( format, "D", StringComparison.OrdinalIgnoreCase ) == 0)
////        {
////            return this.GetValue().ToString();
////        }
////
////        if(String.Compare( format, "X", StringComparison.OrdinalIgnoreCase ) == 0)
////        {
////            return this.ToHexString();
////        }
////
////        if(String.Compare( format, "F", StringComparison.OrdinalIgnoreCase ) == 0)
////        {
////            return InternalFlagsFormat( this.GetType(), this.GetValue() );
////        }
////
////        throw new FormatException( Environment.GetResourceString( "Format_InvalidEnumFormatSpecification" ) );
        }
    
        [Obsolete( "The provider argument is not used. Please use ToString()." )]
        public String ToString( IFormatProvider provider )
        {
            return ToString();
        }

        #endregion

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            Type enumType       = this.GetType();
            Type underlyingType = GetUnderlyingType( enumType );
    
            if(underlyingType == typeof( Int32 ))
            {
                return TypeCode.Int32;
            }
    
            if(underlyingType == typeof( sbyte ))
            {
                return TypeCode.SByte;
            }
    
            if(underlyingType == typeof( Int16 ))
            {
                return TypeCode.Int16;
            }
    
            if(underlyingType == typeof( Int64 ))
            {
                return TypeCode.Int64;
            }
    
            if(underlyingType == typeof( UInt32 ))
            {
                return TypeCode.UInt32;
            }
    
            if(underlyingType == typeof( byte ))
            {
                return TypeCode.Byte;
            }
    
            if(underlyingType == typeof( UInt16 ))
            {
                return TypeCode.UInt16;
            }
    
            if(underlyingType == typeof( UInt64 ))
            {
                return TypeCode.UInt64;
            }
    
            BCLDebug.Assert( false, "Unknown underlying type." );
#if EXCEPTION_STRINGS
            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnknownEnumType" ) );
#else
            throw new InvalidOperationException();
#endif
        }
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return Convert.ToBoolean( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
            return Convert.ToChar( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        sbyte IConvertible.ToSByte( IFormatProvider provider )
        {
            return Convert.ToSByte( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        byte IConvertible.ToByte( IFormatProvider provider )
        {
            return Convert.ToByte( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        short IConvertible.ToInt16( IFormatProvider provider )
        {
            return Convert.ToInt16( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        ushort IConvertible.ToUInt16( IFormatProvider provider )
        {
            return Convert.ToUInt16( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        int IConvertible.ToInt32( IFormatProvider provider )
        {
            return Convert.ToInt32( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        uint IConvertible.ToUInt32( IFormatProvider provider )
        {
            return Convert.ToUInt32( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        long IConvertible.ToInt64( IFormatProvider provider )
        {
            return Convert.ToInt64( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        ulong IConvertible.ToUInt64( IFormatProvider provider )
        {
            return Convert.ToUInt64( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        float IConvertible.ToSingle( IFormatProvider provider )
        {
            return Convert.ToSingle( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        double IConvertible.ToDouble( IFormatProvider provider )
        {
            return Convert.ToDouble( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        Decimal IConvertible.ToDecimal( IFormatProvider provider )
        {
            return Convert.ToDecimal( GetValue(), CultureInfo.CurrentCulture );
        }
    
        /// <internalonly/>
        DateTime IConvertible.ToDateTime( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Enum", "DateTime" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        Object IConvertible.ToType( Type type, IFormatProvider provider )
        {
            return Convert.DefaultToType( (IConvertible)this, type, provider );
        }
        #endregion

        #region ToObject
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern Object InternalBoxEnum( Type enumType, long value );
////    ////FCIMPL2_IV(Object*, ReflectionEnum::InternalBoxEnum, ReflectClassBaseObject* target, INT64 value) {
////    ////    FCALL_CONTRACT;
////    ////
////    ////    VALIDATEOBJECT(target);
////    ////    OBJECTREF ret = NULL;
////    ////
////    ////    MethodTable* pMT = target->GetType().AsMethodTable();
////    ////    HELPER_METHOD_FRAME_BEGIN_RET_0();
////    ////
////    ////    ret = pMT->Box(ArgSlotEndianessFixup((ARG_SLOT*)&value, pMT->GetNumInstanceFieldBytes()), FALSE);
////    ////
////    ////    HELPER_METHOD_FRAME_END();
////    ////    return OBJECTREFToObject(ret);
////    ////}
////    ////FCIMPLEND
////
////
////
////    [CLSCompliant( false )]
////    public static Object ToObject( Type enumType, sbyte value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    public static Object ToObject( Type enumType, short value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    public static Object ToObject( Type enumType, int value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    public static Object ToObject( Type enumType, byte value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    [CLSCompliant( false )]
////    public static Object ToObject( Type enumType, ushort value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    [CLSCompliant( false )]
////    public static Object ToObject( Type enumType, uint value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    public static Object ToObject( Type enumType, long value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, value );
////    }
////
////    [CLSCompliant( false )]
////    public static Object ToObject( Type enumType, ulong value )
////    {
////        ValidateEnumType( enumType );
////
////        return InternalBoxEnum( enumType, unchecked( (long)value ) );
////    }
        #endregion
    }
}
