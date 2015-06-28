////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "zelig_interop.h"

int g_GlobalValue = 1;
int g_GlobalValue2 = 2;

static int s_StaticValue = 3;
static int s_StaticValue2 = 4;

int Foo::otherMethod(int arg1, int arg2)
{
    m_private = s_StaticValue++;                        // m_private      = 3, 4
    return arg1 + arg2 + m_private + g_GlobalValue2++;  // g_globalValue2 = 2, 3
}


INTEROP_INTERFACE g_InteropInterface = 
{    
    &FP_INTEROP_Driver::FP_Method
};

int FP_INTEROP_Driver::FP_Method( int cVal, const char* name, unsigned int* outParam )
{
    *outParam = (int)cVal;

    return cVal*cVal;
}


FP_INTEROP_INTERFACES g_AvailableInterfaces[] =
{
    { &g_InteropInterface },
};


extern "C"
{

int ImportedExternalFunction(int a, int b)
{
    int r = 1, p;
    Foo f;
    unsigned int outVal = 0;

    p = g_AvailableInterfaces[0].interopIface->FPInterop( 3, "Hello World", &outVal );

    s_StaticValue2++;     		     // s_StaticValue2          =  5

    b = f.otherMethod(a, b) + g_GlobalValue; // b = (3 + 7 + 3 + 2) + 1 = 16

    r = a + b + s_StaticValue2;		     // r = 3 + 16 + 5          = 24

    return f.otherMethod(r, b);              // ret =  24 + 16 + 4 + 3  = 47
}

}

//--// -a c:\sporoot\current\client_v4_2_dev\BuildOutput\ARM\RVDS4.0\le\FLASH\debug\iMXS_net_open\lib\zelig_interop.lib
