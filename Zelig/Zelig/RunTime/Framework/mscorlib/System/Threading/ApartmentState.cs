// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ApartmentState
**
**
** Purpose: Enum to represent the different threading models
**
**
=============================================================================*/

namespace System.Threading
{
	[Serializable()]
    public enum ApartmentState
    {   
        /*=========================================================================
        ** Constants for thread apartment states.
        =========================================================================*/
        STA = 0,
        MTA = 1,
        Unknown = 2
    }
}
