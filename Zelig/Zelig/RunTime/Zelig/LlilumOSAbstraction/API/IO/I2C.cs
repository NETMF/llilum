//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System.Runtime.InteropServices;


    public static class I2C
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Initialize( int sdaPin, int sclPin, I2CContext** pChannel );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_I2C_Uninitialize( I2CContext* channel );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_SetFrequency( I2CContext* channel, uint frequencyHz );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Enable( I2CContext* channel );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Disable( I2CContext* channel );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Write( I2CContext* channel, uint address, byte* pBuffer, int offset, int* pLength, uint stop );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Read( I2CContext* channel, uint address, byte* pBuffer, int offset, int* pLength, uint stop );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_I2C_Reset( I2CContext* channel );
    }

    public unsafe struct I2CContext
    {
    };
}
