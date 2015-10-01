//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using System.Globalization;


    [ExtendClass(typeof(System.Number), NoConstructors = true)]
    public class NumberImpl
    {
        //private string ToString(char fmt, int digits, System.Globalization.NumberFormatInfo info)
        //{
        //    throw new NotImplementedException();
        //}

        internal unsafe static Int32 ParseInt32(String s,
                                                NumberStyles style,
                                                NumberFormatInfo info)
        {
            Int32 ret = 0;

            if (style == NumberStyles.HexNumber)
            {
                char[] chars = s.ToCharArray();
                int cnt = chars.Length;

                for (int i = 0; i < cnt; i++)
                {
                    char c = chars[i];

                    ret <<= 4;

                    if (c > 'Z')
                    {
                        c -= (char)('a' - 'A');
                    }

                    if (c >= 'A')
                    {
                        if (c <= 'F') ret += c - 'A';
                        else throw new ArgumentOutOfRangeException();
                    }
                    else if (c <= '9' && c >= '0')
                    {
                        ret += c - '0';
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return ret;
        }
    }
}
