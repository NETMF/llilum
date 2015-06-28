////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

extern "C" long long ExportedInternalFunction( long long arg1 );

extern int g_GlobalValue;
extern int g_GlobalValue2;

typedef int (*FP_INTEROP)( int /*cVal*/, const char*/*name*/, unsigned int* /*outParam*/ );

struct INTEROP_INTERFACE
{
    FP_INTEROP  FPInterop;
};

struct FP_INTEROP_Driver
{
    static int FP_Method( int cVal, const char* name, unsigned int* outParam );
};

struct FP_INTEROP_INTERFACES
{
    INTEROP_INTERFACE* interopIface;
};

struct Foo
{
private:
  int m_private;

public:
  int otherMethod(int arg1, int arg2);
};

typedef unsigned short WORD;

struct SYSTEMTIME
{
    WORD wYear;
    WORD wMonth;
    WORD wDayOfWeek;
    WORD wDay;
    WORD wHour;
    WORD wMinute;
    WORD wSecond;
    WORD wMilliseconds;
};

extern "C" int ExportedStructInt64( long long time, SYSTEMTIME* sysTime );
