//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Globalization.CompareInfo), NoConstructors=true)]
    public class CompareInfoImpl
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        unsafe private static int IndexOfString( String source, String value, int startIndex, int count, int options )
        {
            //
            // Most parameter validation on source performed already at the framework level, we can just work on the values here
            //
            if(value == null)
            {
                throw new ArgumentNullException( ); 
            }
            
            if(value == String.Empty)
            {
                return startIndex;
            }

            int sourceLen = source.Length;
            int valueLen  = value .Length;

            if(valueLen > sourceLen)
            {
                return -1;
            }


            //
            // Cannot find value when there is no longer enough space 
            //
            int end = Math.Min( startIndex + count, sourceLen - valueLen + 1 );

            if(startIndex > end)
            {
                return -1;
            }

            fixed(char* ptrS = (string)(object)source) fixed(char* ptrV = (string)(object)value)
            {
                for(int i = startIndex; i < end; i++)
                {
                    if(ptrS[i] == ptrV[0])
                    {
                        bool match = true;
                        for(int j = 1; j < valueLen; j++)
                        {
                            if(ptrS[i + j] != ptrV[j])
                            {
                                match = false;
                                break;
                            }
                        }
                        if(match)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        //
        // Access Methods
        //
    }
}
