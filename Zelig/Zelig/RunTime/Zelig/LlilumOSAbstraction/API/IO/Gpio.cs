//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public enum GpioPinResistor
    {
        Default = 0, 
        Pullup,
        Pulldown,
        OpenDrain,
        Repeater,
    };

    public enum GpioPinEdge
    {
        None = 0,
        RisingEdge,
        FallingEdge,
        BothEdges,
        LevelLow,
        LevelHigh,
    }

    public enum GpioPinPolarity
    {
        Normal = 0,
        Inverted,
    }

    public enum GpioPinDirection
    {
        Input = 0,
        Output,
    }

    public unsafe delegate void LLOS_GPIO_InterruptCallback( GpioContext* pGpioCtx, UIntPtr callbackCtx, GpioPinEdge edge);

    public static class Gpio
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_AllocatePin( int pinNumber, GpioContext** ppGpioCtx );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_GPIO_FreePin( GpioContext* pGpioCtx );

        public static unsafe uint LLOS_GPIO_EnablePin( GpioContext* pin, GpioPinEdge edge, LLOS_GPIO_InterruptCallback callback, UIntPtr callbackCtx )
        {
            UIntPtr callback_ptr = UIntPtr.Zero;

            if(callback != null)
            {
                DelegateImpl dlg = (DelegateImpl)(object)callback;

                callback_ptr = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            }

            return LLOS_GPIO_EnablePin( pin, edge, callback_ptr, callbackCtx );
        }

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_GPIO_EnablePin( GpioContext* pGpioCtx, GpioPinEdge edge, UIntPtr callback, UIntPtr callbackCtx );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_DisablePin( GpioContext* pGpioCtx );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_SetPolarity( GpioContext* pin, GpioPinPolarity polarity );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_SetMode( GpioContext* pin, GpioPinResistor resistor );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_SetDirection( GpioContext* pin, GpioPinDirection direction );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_SetDebounce( GpioContext* pin, ulong debounce_time );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_GPIO_Write( GpioContext* pin, int value );

        [DllImport( "C" )]
        public static unsafe extern int LLOS_GPIO_Read( GpioContext* pin );
    }

    public unsafe struct GpioContext
    {
    };
}
