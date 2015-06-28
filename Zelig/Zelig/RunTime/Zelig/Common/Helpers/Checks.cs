//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public static class CHECKS
    {
        public static bool ENABLED
        {
            get
            {
#if DEBUG || CHECKS_ASSERT
                return true;
#else
                return false;
#endif
            }
        }

        [System.Diagnostics.Conditional( "DEBUG"         )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        public static void ASSERT( bool   res  ,
                                   string text )
        {
            if(res == false)
            {
                throw AssertionViolationException.Create( text );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG"         )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        public static void ASSERT( bool   res  ,
                                   string text ,
                                   object arg1 )
        {
            if(res == false)
            {
                throw AssertionViolationException.Create( text, arg1 );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG"         )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        public static void ASSERT( bool   res  ,
                                   string text ,
                                   object arg1 ,
                                   object arg2 )
        {
            if(res == false)
            {
                throw AssertionViolationException.Create( text, arg1, arg2 );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG"         )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        public static void ASSERT( bool   res  ,
                                   string text ,
                                   object arg1 ,
                                   object arg2 ,
                                   object arg3 )
        {
            if(res == false)
            {
                throw AssertionViolationException.Create( text, arg1, arg2, arg3 );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG"         )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        public static void ASSERT(        bool     res  ,
                                          string   text ,
                                   params object[] args )
        {
            if(res == false)
            {
                throw AssertionViolationException.Create( text, args );
            }
        }
    }
}
