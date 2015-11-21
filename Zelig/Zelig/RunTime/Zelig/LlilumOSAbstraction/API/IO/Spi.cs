//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    //
    // !!!WARNING!!! This structure MUST be identical to the C structure LLOS_SPI_ControllerConfig in llos_spi.h
    // Also note that the boolean fields (loopbackMode, MSBTransferMode, etc.) are defined as uint values.  The
    // bool type gets reduced to 1 byte values which would not map to the underlying C/C++ structure where boolean
    // values are 32 bits. 
    //
    [StructLayout( LayoutKind.Sequential, Pack = 4 )]
    public struct SpiConfig
    {
        public uint chipSelect;
        public uint loopbackMode;
        public uint MSBTransferMode;
        public uint activeLow;
        public uint inversePolarity;             
        public uint clockIdleLevel;         
        public uint clockSamplingEdge;      
        public uint master;
        public uint phaseMode;
        public uint dataWidth;
        public uint clockRateHz;
        public uint chipSelectSetupCycles;
        public uint chipSelectHoldCycles;
        public uint busyPin;
    };

    public enum SpiAction
    {
        SpiWrite = 1,
        SpiRead,
        SpiTransceive,
        SpiError,
    };

    public unsafe delegate void LLOS_SPI_Callback(SpiContext* channel, UIntPtr callbackCtx, SpiAction edge);

    public static class Spi
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Initialize( uint mosi, uint miso, uint sclk, uint chipSelect, SpiContext** channel, SpiConfig** configuration );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_SPI_Uninitialize( SpiContext* channel );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Configure( SpiContext* channel, SpiConfig* config );

        public static unsafe uint LLOS_SPI_SetCallback( SpiContext* channel, LLOS_SPI_Callback callback, UIntPtr context )
        {
            UIntPtr callback_ptr = UIntPtr.Zero;

            if(callback != null)
            {
                DelegateImpl dlg = (DelegateImpl)(object)callback;

                callback_ptr = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            }

            return LLOS_SPI_SetCallback( channel, callback_ptr, context );
        }

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_SPI_SetCallback( SpiContext* channel, UIntPtr callback, UIntPtr context );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_SetFrequency( SpiContext* channel, uint frequencyHz );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Transfer( SpiContext* channel, byte* txBuffer, int txOffset, int txCount, byte* rxBuffer, int rxOffset, int rxCount, int rxStartOffset );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Write( SpiContext* channel, byte* txBuffer, int txOffset, int txCount );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Read( SpiContext* channel, byte* rxBuffer, int rxOffset, int rxCount, int rxStartOffset );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_IsBusy( SpiContext* channel, uint* isBusy );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Suspend( SpiContext* channel );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SPI_Resume( SpiContext* channel );
    }

    public unsafe struct SpiContext
    {
    }
}
