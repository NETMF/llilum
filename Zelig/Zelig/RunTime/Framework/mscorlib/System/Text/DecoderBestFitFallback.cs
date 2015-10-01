// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// DecoderBestFitFallback.cs
//
// This is used internally to create best fit behavior as per the original windows best fit behavior.
//
namespace System.Text
{
    using System;
    using System.Text;
    using System.Threading;

    [Serializable()]
    internal sealed class InternalDecoderBestFitFallback : DecoderFallback
    {
        // Our variables
        internal Encoding encoding = null;
        internal char[] arrayBestFit = null;
        internal char cReplacement = '?';

        internal InternalDecoderBestFitFallback( Encoding encoding )
        {
            // Need to load our replacement characters table.
            this.encoding = encoding;
            this.bIsMicrosoftBestFitFallback = true;
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer()
        {
            return new InternalDecoderBestFitFallbackBuffer( this );
        }

        // Maximum number of characters that this instance of this fallback could return
        public override int MaxCharCount
        {
            get
            {
                return 1;
            }
        }

        public override bool Equals( Object value )
        {
            InternalDecoderBestFitFallback that = value as InternalDecoderBestFitFallback;
            if(that != null)
            {
                return (this.encoding.CodePage == that.encoding.CodePage);
            }
            return (false);
        }

        public override int GetHashCode()
        {
            return this.encoding.CodePage;
        }
    }

    internal sealed class InternalDecoderBestFitFallbackBuffer : DecoderFallbackBuffer
    {
        // Our variables
        internal char cBestFit = '\0';
        internal int iCount = -1;
        internal int iSize;
        private InternalDecoderBestFitFallback oFallback;

        // Private object for locking instead of locking on a public type for SQL reliability work.
        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject
        {
            get
            {
                if(s_InternalSyncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
                }
                return s_InternalSyncObject;
            }
        }

        // Constructor
        public InternalDecoderBestFitFallbackBuffer( InternalDecoderBestFitFallback fallback )
        {
            this.oFallback = fallback;

            if(oFallback.arrayBestFit == null)
            {
                // Lock so we don't confuse ourselves.
                lock(InternalSyncObject)
                {
                    // Double check before we do it again.
                    if(oFallback.arrayBestFit == null)
                    {
                        oFallback.arrayBestFit = fallback.encoding.GetBestFitBytesToUnicodeData();
                    }
                }
            }
        }

        // Fallback methods
        public override bool Fallback( byte[] bytesUnknown, int index )
        {
            // We expect no previous fallback in our buffer
            BCLDebug.Assert( iCount < 1, "[DecoderReplacementFallbackBuffer.Fallback] Calling fallback without a previously empty buffer" );

            cBestFit = TryBestFit( bytesUnknown );
            if(cBestFit == '\0')
            {
                cBestFit = oFallback.cReplacement;
            }

            iCount = iSize = 1;

            return true;
        }

        // Default version is overridden in DecoderReplacementFallback.cs
        public override char GetNextChar()
        {
            // Just return cReturn, which is 0 if there's no best fit for it.
            return (iCount-- > 0) ? cBestFit : '\0';
        }

        public override bool MovePrevious()
        {
            // Exception fallback doesn't have anywhere to back up to.
            if(iCount >= 0)
            {
                iCount++;
            }

            // Return true if we could do it.
            return (iCount >= 0 && iCount <= iSize);
        }

        // How many characters left to output?
        public override int Remaining
        {
            get
            {
                return (iCount > 0) ? iCount : 0;
            }
        }

        // Clear the buffer
        public override unsafe void Reset()
        {
            iCount = -1;
            byteStart = null;
        }

        // This version just counts the fallback and doesn't actually copy anything.
        internal unsafe override int InternalFallback( byte[] bytes, byte* pBytes )
        // Right now this has both bytes and bytes[], which is silly, but we might have extra bytes, hence the
        // array, and we might need the index, hence the byte*
        {
            // return our replacement string Length (always 1 for InternalDecoderBestFitFallback, either
            // a best fit char or ?
            return 1;
        }

        // private helper methods
        private char TryBestFit( byte[] bytesCheck )
        {
            // Need to figure out our best fit character, low is beginning of array, high is 1 AFTER end of array
            int lowBound = 0;
            int highBound = oFallback.arrayBestFit.Length;
            int index;
            char cCheck;

            // Check trivial case first (no best fit)
            if(highBound == 0)
            {
                return '\0';
            }

            // If our array is too small or too big we can't check
            if(bytesCheck.Length == 0 || bytesCheck.Length > 2)
            {
                return '\0';
            }

            if(bytesCheck.Length == 1)
            {
                cCheck = unchecked( (char)bytesCheck[0] );
            }
            else
            {
                cCheck = unchecked( (char)((bytesCheck[0] << 8) + bytesCheck[1]) );
            }

            // Check trivial out of range case
            if(cCheck < oFallback.arrayBestFit[0] || cCheck > oFallback.arrayBestFit[highBound - 2])
            {
                return '\0';
            }

            // Binary search the array
            int iDiff;
            while((iDiff = (highBound - lowBound)) > 6)
            {
                // Look in the middle, which is complicated by the fact that we have 2 #s for each pair,
                // so we don't want index to be odd because it must be word aligned.
                // Also note that index can never == highBound (because diff is rounded down)
                index = ((iDiff / 2) + lowBound) & 0xFFFE;

                char cTest = oFallback.arrayBestFit[index];
                if(cTest == cCheck)
                {
                    // We found it
                    BCLDebug.Assert( index + 1 < oFallback.arrayBestFit.Length, "[InternalDecoderBestFitFallbackBuffer.TryBestFit]Expected replacement character at end of array" );
                    return oFallback.arrayBestFit[index + 1];
                }
                else if(cTest < cCheck)
                {
                    // We weren't high enough
                    lowBound = index;
                }
                else
                {
                    // We weren't low enough
                    highBound = index;
                }
            }

            for(index = lowBound; index < highBound; index += 2)
            {
                if(oFallback.arrayBestFit[index] == cCheck)
                {
                    // We found it
                    BCLDebug.Assert( index + 1 < oFallback.arrayBestFit.Length, "[InternalDecoderBestFitFallbackBuffer.TryBestFit]Expected replacement character at end of array" );
                    return oFallback.arrayBestFit[index + 1];
                }
            }

            // Char wasn't in our table            
            return '\0';
        }
    }
}

