////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "zelig_interop.h"

int s_StaticValue = 5;

typedef long long (*FP )(long long);
typedef unsigned char byte;

static FP  s_exportedFuncPtr = ExportedInternalFunction;

struct MyStruct
{
public:
    int a;
    double b;
    char c; 
};

extern "C"
{

int MyExternalFunction(int a)                 // a = 47
{
    int r = 1, b = 0;
    double d = 3.33;
    Foo f;
    long long time = 3;
    SYSTEMTIME sysTime;
    
    sysTime.wYear = 1;
    sysTime.wMonth = 2;
    sysTime.wDayOfWeek = 3;
    sysTime.wDay = 4;
    sysTime.wHour = 5;
    sysTime.wMinute = 6;
    sysTime.wSecond = 7;
    sysTime.wMilliseconds = 8;

    s_StaticValue++;     		      // s_StaticValue          =   6

    b = f.otherMethod(a, r) + g_GlobalValue;  // b = 47 + 1 + 5 + 4 + 1 =  58

    r = a + b + s_StaticValue;		      // r = 47 + 58 + 6        = 111

    r = f.otherMethod(r, b);                  // r = 111 + 58 + 6 + 5   = 180
 
    r = (int)s_exportedFuncPtr( r );          // r = 180 + 180          = 360

    r /= 43;                                  // r                      =   8

    r = (int)(r * d);                         // r                      =  26

    r += ExportedStructInt64( time, &sysTime ); // r = 27

    r += sysTime.wSecond;                       // r = 27 + 3 = 30

    return r;
}

double Multiply(double a, double b)
{
    double c = a * b;
    return c;   
}

void PassStringArgument( wchar_t* pChar, int len )
{
    int half = len / 2;
    len--;

    for(int i=0; i<half; i++)
    {
        wchar_t tmp  = pChar[i    ];
        pChar[i    ] = pChar[len-i];
        pChar[len-i] = tmp;
    }
}

void PassByteArrayArg( byte* data, int len )
{
    int half = len / 2;
    len--;

    for(int i=0; i<half; i++)
    {
        byte b      = data[i    ];
        data[i    ] = data[len-i];
        data[len-i] = b;
    }
}

void PassStructArg( MyStruct* pStruct )
{
   pStruct->a = 123;
   pStruct->b = 3.4775;
   pStruct->c = 'Z';
}

int StringLength( const wchar_t* pChar )
{
    int len = 0;

    while(*pChar++ != L'\0')
    {
       len++;
    }
    return len;
}

}

//--//
