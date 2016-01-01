////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public static class Math
    {

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public int Cos(int angle);
        [Obsolete( "", false )]
        static public int Cos(int angle)
        {
            return (int)(System.Math.Cos( (double)angle ) * 1000); 
        }


        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public int Sin(int angle);
        [Obsolete( "", false )]
        static public int Sin(int angle)
        {
            return (int)(System.Math.Sin( (double)angle ) * 1000); 
        }
    }
}


