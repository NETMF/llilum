// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Globalization
{
    using System.Security.Permissions;
    using System.Runtime.Serialization;
    using System.Text;
    using System;
    //
    // Property             Default Description
    // PositiveSign           '+'   Character used to indicate positive values.
    // NegativeSign           '-'   Character used to indicate negative values.
    // NumberDecimalSeparator '.'   The character used as the decimal separator.
    // NumberGroupSeparator   ','   The character used to separate groups of
    //                              digits to the left of the decimal point.
    // NumberDecimalDigits    2     The default number of decimal places.
    // NumberGroupSizes       3     The number of digits in each group to the
    //                              left of the decimal point.
    // NaNSymbol             "NaN"  The string used to represent NaN values.
    // PositiveInfinitySymbol"Infinity" The string used to represent positive
    //                              infinities.
    // NegativeInfinitySymbol"-Infinity" The string used to represent negative
    //                              infinities.
    //
    //
    //
    // Property                  Default  Description
    // CurrencyDecimalSeparator  '.'      The character used as the decimal
    //                                    separator.
    // CurrencyGroupSeparator    ','      The character used to separate groups
    //                                    of digits to the left of the decimal
    //                                    point.
    // CurrencyDecimalDigits     2        The default number of decimal places.
    // CurrencyGroupSizes        3        The number of digits in each group to
    //                                    the left of the decimal point.
    // CurrencyPositivePattern   0        The format of positive values.
    // CurrencyNegativePattern   0        The format of negative values.
    // CurrencySymbol            "$"      String used as local monetary symbol.
    //

////[Serializable]
    sealed public class NumberFormatInfo : ICloneable, IFormatProvider
    {
        // invariantInfo is constant irrespective of your current culture.
        private static NumberFormatInfo invariantInfo;

        // READTHIS READTHIS READTHIS
        // This class has an exact mapping onto a native structure defined in COMNumber.cpp
        // DO NOT UPDATE THIS WITHOUT UPDATING THAT STRUCTURE. IF YOU ADD BOOL, ADD THEM AT THE END.
        // ALSO MAKE SURE TO UPDATE mscorlib.h in the VM directory to check field offsets.
        // READTHIS READTHIS READTHIS
        internal int[] numberGroupSizes = new int[] { 3 };
        internal int[] currencyGroupSizes = new int[] { 3 };
        internal int[] percentGroupSizes = new int[] { 3 };
        internal String positiveSign = "+";
        internal String negativeSign = "-";
        internal String numberDecimalSeparator = ".";
        internal String numberGroupSeparator = ",";
        internal String currencyGroupSeparator = ",";
        internal String currencyDecimalSeparator = ".";
        internal String currencySymbol = "\x00a4";  // U+00a4 is the symbol for International Monetary Fund.
        // The alternative currency symbol used in Win9x ANSI codepage, that can not roundtrip between ANSI and Unicode.
        // Currently, only ja-JP and ko-KR has non-null values (which is U+005c, backslash)
        internal String ansiCurrencySymbol = null;
        internal String nanSymbol = "NaN";
        internal String positiveInfinitySymbol = "Infinity";
        internal String negativeInfinitySymbol = "-Infinity";
        internal String percentDecimalSeparator = ".";
        internal String percentGroupSeparator = ",";
        internal String percentSymbol = "%";
        internal String perMilleSymbol = "\u2030";

        [OptionalField( VersionAdded = 2 )]
        internal String[] nativeDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        // an index which points to a record in Culture Data Table.
        // We shouldn't be persisting dataItem (since its useless & we weren't using it),
        // but since COMNumber.cpp uses it and since serialization isn't implimented, its stuck for now.
        internal int m_dataItem = 0;    // NEVER USED, DO NOT USE THIS! (Serialized in Everett)

        internal int numberDecimalDigits = 2;
        internal int currencyDecimalDigits = 2;
        internal int currencyPositivePattern = 0;
        internal int currencyNegativePattern = 0;
        internal int numberNegativePattern = 1;
        internal int percentPositivePattern = 0;
        internal int percentNegativePattern = 0;
        internal int percentDecimalDigits = 2;

        [OptionalField( VersionAdded = 2 )]
        internal int digitSubstitution = 1; // DigitShapes.None

        internal bool isReadOnly = false;
        // We shouldn't be persisting m_useUserOverride (since its useless & we weren't using it),
        // but since COMNumber.cpp uses it and since serialization isn't implimented, its stuck for now.
        internal bool m_useUserOverride = false;    // NEVER USED, DO NOT USE THIS! (Serialized in Everett)

        public NumberFormatInfo()
            : this( null )
        {
        }

////    #region Serialization
////    // The following fields are exist to keep the serialization compatibility with Everett and shouldn't 
////    // be used for any other purpose
////    internal bool validForParseAsNumber = true;
////    internal bool validForParseAsCurrency = true;
////
////    [OnSerializing]
////    private void OnSerializing( StreamingContext ctx )
////    {
////        if(numberDecimalSeparator != numberGroupSeparator)
////        {
////            validForParseAsNumber = true;
////        }
////        else
////        {
////            validForParseAsNumber = false;
////        }
////
////        if((numberDecimalSeparator != numberGroupSeparator) &&
////            (numberDecimalSeparator != currencyGroupSeparator) &&
////            (currencyDecimalSeparator != numberGroupSeparator) &&
////            (currencyDecimalSeparator != currencyGroupSeparator))
////        {
////            validForParseAsCurrency = true;
////        }
////        else
////        {
////            validForParseAsCurrency = false;
////        }
////    }
////
////
////    [OnDeserializing]
////    private void OnDeserializing( StreamingContext ctx )
////    {
////        nativeDigits = null;
////        digitSubstitution = -1;
////    }
////
////    [OnDeserialized]
////    private void OnDeserialized( StreamingContext ctx )
////    {
////        if(nativeDigits == null)
////        {
////            nativeDigits = new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
////        }
////
////        if(digitSubstitution < 0)
////        {
////            digitSubstitution = 1;
////        }
////    }
////
////    #endregion Serialization

////    private void VerifyDecimalSeparator( String decSep, String propertyName )
////    {
////        if(decSep == null)
////        {
////            throw new ArgumentNullException( propertyName,
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////        }
////
////        if(decSep.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_EmptyDecString" ) );
////        }
////
////    }
////
////    private void VerifyGroupSeparator( String groupSep, String propertyName )
////    {
////        if(groupSep == null)
////        {
////            throw new ArgumentNullException( propertyName,
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////        }
////    }
////
////    private void VerifyNativeDigits( String[] nativeDig, String propertyName )
////    {
////        if(nativeDig == null)
////        {
////            throw new ArgumentNullException( propertyName,
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////        }
////
////        if(nativeDig.Length != 10)
////        {
////            throw new ArgumentException( propertyName, Environment.GetResourceString( "Argument_InvalidNativeDigitCount" ) );
////        }
////
////        for(int i = 0; i < nativeDig.Length; i++)
////        {
////            if(nativeDig[i] == null)
////            {
////                throw new ArgumentNullException( propertyName,
////                        Environment.GetResourceString( "ArgumentNull_ArrayValue" ) );
////            }
////
////
////            if(nativeDig[i].Length != 1)
////            {
////                if(nativeDig[i].Length != 2)
////                {
////                    // Not 1 or 2 UTF-16 code points
////                    throw new ArgumentException( propertyName, Environment.GetResourceString( "Argument_InvalidNativeDigitValue" ) );
////                }
////                else if(!char.IsSurrogatePair( nativeDig[i][0], nativeDig[i][1] ))
////                {
////                    // 2 UTF-6 code points, but not a surrogate pair
////                    throw new ArgumentException( propertyName, Environment.GetResourceString( "Argument_InvalidNativeDigitValue" ) );
////                }
////            }
////
////            if(CharUnicodeInfo.GetDecimalDigitValue( nativeDig[i], 0 ) != i)
////            {
////                // Not the appropriate digit according to the Unicode data properties
////                // (Digit 0 must be a 0, etc.).
////                throw new ArgumentException( propertyName, Environment.GetResourceString( "Argument_InvalidNativeDigitValue" ) );
////            }
////        }
////    }
////
////    private void VerifyDigitSubstitution( DigitShapes digitSub, String propertyName )
////    {
////        switch(digitSub)
////        {
////            case DigitShapes.Context:
////            case DigitShapes.None:
////            case DigitShapes.NativeNational:
////                // Success.
////                break;
////
////            default:
////                throw new ArgumentException( propertyName, Environment.GetResourceString( "Argument_InvalidDigitSubstitution" ) );
////        }
////    }


        // We aren't persisting dataItem any more (since its useless & we weren't using it),
        // Ditto with m_useUserOverride.  Don't use them, we use a local copy of everything.
        internal NumberFormatInfo( CultureTableRecord cultureTableRecord )
        {
            if(cultureTableRecord != null)
            {
////            /*
////            We don't have information for the following four.  All cultures use
////            the same value set in the ctor of NumberFormatInfo.
////            PercentGroupSize
////            PercentDecimalDigits
////            PercentGroupSeparator
////            PerMilleSymbol
////            */
////
////            // We directly use fields here since these data is coming from data table or Win32, so we
////            // don't need to verify their values (except for invalid parsing situations).
////
////            cultureTableRecord.GetNFIOverrideValues( this );
////
////            // The Japanese and Korean fonts map a backslash (U+005c) to the correct currency symbol.
////            // And yes, we use the ANSI currency symbol in parsing, even for Unicode strings.
////            if((932 == cultureTableRecord.IDEFAULTANSICODEPAGE) ||
////               (949 == cultureTableRecord.IDEFAULTANSICODEPAGE))
////            {
////                // Legacy behavior for cultures that use Japanese/Korean default ANSI code pages
////                this.ansiCurrencySymbol = "\\";
////            }
////            this.negativeInfinitySymbol = cultureTableRecord.SNEGINFINITY;
////            this.positiveInfinitySymbol = cultureTableRecord.SPOSINFINITY;
////            this.nanSymbol = cultureTableRecord.SNAN;
            }
        }

        private void VerifyWritable()
        {
            if(isReadOnly)
            {
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ReadOnly" ) );
#else
                throw new InvalidOperationException();
#endif
            }
        }

        // Returns a default NumberFormatInfo that will be universally
        // supported and constant irrespective of the current culture.
        // Used by FromString methods.
        //

        public static NumberFormatInfo InvariantInfo
        {
            get
            {
                if(invariantInfo == null)
                {
                    // Lazy create the invariant info. This cannot be done in a .cctor because exceptions can
                    // be thrown out of a .cctor stack that will need this.
                    invariantInfo = ReadOnly( new NumberFormatInfo() );
                }
                return invariantInfo;
            }
        }


        public static NumberFormatInfo GetInstance( IFormatProvider formatProvider )
        {
////        // Fast case for a regular CultureInfo
////        NumberFormatInfo info;
////        CultureInfo cultureProvider = formatProvider as CultureInfo;
////        if(cultureProvider != null && !cultureProvider.m_isInherited)
////        {
////            info = cultureProvider.numInfo;
////            if(info != null)
////            {
////                return info;
////            }
////            else
////            {
////                return cultureProvider.NumberFormat;
////            }
////        }
////        // Fast case for an NFI;
////        info = formatProvider as NumberFormatInfo;
////        if(info != null)
////        {
////            return info;
////        }
////        if(formatProvider != null)
////        {
////            info = formatProvider.GetFormat( typeof( NumberFormatInfo ) ) as NumberFormatInfo;
////            if(info != null)
////            {
////                return info;
////            }
////        }
            return CurrentInfo;
        }



        public Object Clone()
        {
            NumberFormatInfo n = (NumberFormatInfo)MemberwiseClone();
            n.isReadOnly = false;
            return n;
        }


////    public int CurrencyDecimalDigits
////    {
////        get { return currencyDecimalDigits; }
////        set
////        {
////            VerifyWritable();
////            if(value < 0 || value > 99)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "CurrencyDecimalDigits",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                99 ) );
////            }
////            currencyDecimalDigits = value;
////        }
////    }
////
////
////    public String CurrencyDecimalSeparator
////    {
////        get { return currencyDecimalSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyDecimalSeparator( value, "CurrencyDecimalSeparator" );
////            currencyDecimalSeparator = value;
////        }
////    }


        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
        }

////    //
////    // Check the values of the groupSize array.
////    //
////    // Every element in the groupSize array should be between 1 and 9
////    // excpet the last element could be zero.
////    //
////    internal void CheckGroupSize( String propName, int[] groupSize )
////    {
////        for(int i = 0; i < groupSize.Length; i++)
////        {
////            if(groupSize[i] < 1)
////            {
////                if(i == groupSize.Length - 1 && groupSize[i] == 0)
////                    return;
////                throw new ArgumentException( propName, Environment.GetResourceString( "Argument_InvalidGroupSize" ) );
////            }
////            else if(groupSize[i] > 9)
////            {
////                throw new ArgumentException( propName, Environment.GetResourceString( "Argument_InvalidGroupSize" ) );
////            }
////        }
////    }
////
////
////    public int[] CurrencyGroupSizes
////    {
////        get
////        {
////            return ((int[])currencyGroupSizes.Clone());
////        }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "CurrencyGroupSizes",
////                    Environment.GetResourceString( "ArgumentNull_Obj" ) );
////            }
////
////            Int32[] inputSizes = (Int32[])value.Clone();
////            CheckGroupSize( "CurrencyGroupSizes", inputSizes );
////            currencyGroupSizes = inputSizes;
////        }
////
////    }
////
////
////
////    public int[] NumberGroupSizes
////    {
////        get
////        {
////            return ((int[])numberGroupSizes.Clone());
////        }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "NumberGroupSizes",
////                    Environment.GetResourceString( "ArgumentNull_Obj" ) );
////            }
////
////            Int32[] inputSizes = (Int32[])value.Clone();
////            CheckGroupSize( "NumberGroupSizes", inputSizes );
////            numberGroupSizes = inputSizes;
////        }
////    }
////
////
////    public int[] PercentGroupSizes
////    {
////        get
////        {
////            return ((int[])percentGroupSizes.Clone());
////        }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "PercentGroupSizes",
////                    Environment.GetResourceString( "ArgumentNull_Obj" ) );
////            }
////            Int32[] inputSizes = (Int32[])value.Clone();
////            CheckGroupSize( "PercentGroupSizes", inputSizes );
////            percentGroupSizes = inputSizes;
////        }
////
////    }
////
////
////    public String CurrencyGroupSeparator
////    {
////        get { return currencyGroupSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyGroupSeparator( value, "CurrencyGroupSeparator" );
////            currencyGroupSeparator = value;
////        }
////    }
////
////
////    public String CurrencySymbol
////    {
////        get { return currencySymbol; }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "CurrencySymbol",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            currencySymbol = value;
////        }
////    }

        // Returns the current culture's NumberFormatInfo.  Used by Parse methods.
        //

        public static NumberFormatInfo CurrentInfo
        {
            get
            {
                System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                if(!culture.m_isInherited)
                {
                    NumberFormatInfo info = culture.numInfo;
                    if(info != null)
                    {
                        return info;
                    }
                }
                return ((NumberFormatInfo)culture.GetFormat( typeof( NumberFormatInfo ) ));
            }
        }


        public String NaNSymbol
        {
            get
            {
                return nanSymbol;
            }
            set
            {
                VerifyWritable();
                if(value == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "NaNSymbol", Environment.GetResourceString( "ArgumentNull_String" ) );
#else
                    throw new ArgumentNullException();
#endif
                }
                nanSymbol = value;
            }
        }
    
    
    
////    public int CurrencyNegativePattern
////    {
////        get { return currencyNegativePattern; }
////        set
////        {
////            VerifyWritable();
////            if(value < 0 || value > 15)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "CurrencyNegativePattern",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                15 ) );
////            }
////            currencyNegativePattern = value;
////        }
////    }
////
////
////    public int NumberNegativePattern
////    {
////        get { return numberNegativePattern; }
////        set
////        {
////            //
////            // NOTENOTE: the range of value should correspond to negNumberFormats[] in vm\COMNumber.cpp.
////            //
////            VerifyWritable();
////            if(value < 0 || value > 4)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "NumberNegativePattern",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                4 ) );
////            }
////            numberNegativePattern = value;
////        }
////    }
////
////
////    public int PercentPositivePattern
////    {
////        get { return percentPositivePattern; }
////        set
////        {
////            //
////            // NOTENOTE: the range of value should correspond to posPercentFormats[] in vm\COMNumber.cpp.
////            //
////            VerifyWritable();
////            if(value < 0 || value > 3)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "PercentPositivePattern",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                3 ) );
////            }
////            percentPositivePattern = value;
////        }
////    }
////
////
////    public int PercentNegativePattern
////    {
////        get { return percentNegativePattern; }
////        set
////        {
////            //
////            // NOTENOTE: the range of value should correspond to posPercentFormats[] in vm\COMNumber.cpp.
////            //
////            VerifyWritable();
////            if(value < 0 || value > 11)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "PercentNegativePattern",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                11 ) );
////            }
////            percentNegativePattern = value;
////        }
////    }
    
    
        public String NegativeInfinitySymbol
        {
            get
            {
                return negativeInfinitySymbol;
            }
            set
            {
                VerifyWritable();
                if(value == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "NegativeInfinitySymbol", Environment.GetResourceString( "ArgumentNull_String" ) );
#else
                    throw new ArgumentNullException();
#endif
                }
                negativeInfinitySymbol = value;
            }
        }
    
    
////    public String NegativeSign
////    {
////        get { return negativeSign; }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "NegativeSign",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            negativeSign = value;
////        }
////    }
////
////
////    public int NumberDecimalDigits
////    {
////        get { return numberDecimalDigits; }
////        set
////        {
////            VerifyWritable();
////            if(value < 0 || value > 99)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "NumberDecimalDigits",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                99 ) );
////            }
////            numberDecimalDigits = value;
////        }
////    }
////
////
////    public String NumberDecimalSeparator
////    {
////        get { return numberDecimalSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyDecimalSeparator( value, "NumberDecimalSeparator" );
////            numberDecimalSeparator = value;
////        }
////    }
////
////
////    public String NumberGroupSeparator
////    {
////        get { return numberGroupSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyGroupSeparator( value, "NumberGroupSeparator" );
////            numberGroupSeparator = value;
////        }
////    }
////
////
////    public int CurrencyPositivePattern
////    {
////        get { return currencyPositivePattern; }
////        set
////        {
////            VerifyWritable();
////            if(value < 0 || value > 3)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "CurrencyPositivePattern",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                3 ) );
////            }
////            currencyPositivePattern = value;
////        }
////    }
    
    
        public String PositiveInfinitySymbol
        {
            get
            {
                return positiveInfinitySymbol;
            }
            set
            {
                VerifyWritable();
                if(value == null)
                {
                    throw new ArgumentNullException( "PositiveInfinitySymbol",
                        Environment.GetResourceString( "ArgumentNull_String" ) );
                }
                positiveInfinitySymbol = value;
            }
        }
    
    
////    public String PositiveSign
////    {
////        get { return positiveSign; }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "PositiveSign",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            positiveSign = value;
////        }
////    }
////
////
////    public int PercentDecimalDigits
////    {
////        get { return percentDecimalDigits; }
////        set
////        {
////            VerifyWritable();
////            if(value < 0 || value > 99)
////            {
////                throw new ArgumentOutOfRangeException(
////                            "PercentDecimalDigits",
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                                0,
////                                99 ) );
////            }
////            percentDecimalDigits = value;
////        }
////    }
////
////
////    public String PercentDecimalSeparator
////    {
////        get { return percentDecimalSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyDecimalSeparator( value, "PercentDecimalSeparator" );
////            percentDecimalSeparator = value;
////        }
////    }
////
////
////    public String PercentGroupSeparator
////    {
////        get { return percentGroupSeparator; }
////        set
////        {
////            VerifyWritable();
////            VerifyGroupSeparator( value, "PercentGroupSeparator" );
////            percentGroupSeparator = value;
////        }
////    }
////
////
////    public String PercentSymbol
////    {
////        get
////        {
////            return percentSymbol;
////        }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "PercentSymbol",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            percentSymbol = value;
////        }
////    }
////
////
////    public String PerMilleSymbol
////    {
////        get { return perMilleSymbol; }
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "PerMilleSymbol",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            perMilleSymbol = value;
////        }
////    }
////
////
////    public String[] NativeDigits
////    {
////        get { return nativeDigits; }
////        set
////        {
////            VerifyWritable();
////            VerifyNativeDigits( value, "NativeDigits" );
////            nativeDigits = value;
////        }
////    }
////
////
////    public DigitShapes DigitSubstitution
////    {
////        get { return (DigitShapes)digitSubstitution; }
////        set
////        {
////            VerifyWritable();
////            VerifyDigitSubstitution( value, "DigitSubstitution" );
////            digitSubstitution = (int)value;
////        }
////    }


        public Object GetFormat( Type formatType )
        {
            return formatType == typeof( NumberFormatInfo ) ? this : null;
        }


        public static NumberFormatInfo ReadOnly( NumberFormatInfo nfi )
        {
            if(nfi == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "nfi" );
#else
                throw new ArgumentNullException();
#endif
            }
            if(nfi.IsReadOnly)
            {
                return (nfi);
            }
            NumberFormatInfo info = (NumberFormatInfo)(nfi.MemberwiseClone());
            info.isReadOnly = true;
            return info;
        }

        // private const NumberStyles InvalidNumberStyles = unchecked((NumberStyles) 0xFFFFFC00);
        private const NumberStyles InvalidNumberStyles = ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                                                           | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign
                                                           | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint
                                                           | NumberStyles.AllowThousands | NumberStyles.AllowExponent
                                                           | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier);
    
        internal static void ValidateParseStyleInteger( NumberStyles style )
        {
            // Check for undefined flags
            if((style & InvalidNumberStyles) != 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidNumberStyles" ), "style" );
#else
                throw new ArgumentException();
#endif
            }
            if((style & NumberStyles.AllowHexSpecifier) != 0)
            { // Check for hex number
                if((style & ~NumberStyles.HexNumber) != 0)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Arg_InvalidHexStyle" ) );
#else
                    throw new ArgumentException();
#endif
                }
            }
        }
    
        internal static void ValidateParseStyleFloatingPoint( NumberStyles style )
        {
            // Check for undefined flags
            if((style & InvalidNumberStyles) != 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidNumberStyles" ), "style" );
#else
                throw new ArgumentException();
#endif
            }
            if((style & NumberStyles.AllowHexSpecifier) != 0)
            { // Check for hex number
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_HexStyleNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }
        }
    } // NumberFormatInfo
}









