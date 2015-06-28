//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38050000U,Length=0x00010000U)]
    public class USART1 : USARTx
    {
        //
        // Helper Methods
        //

        public static USARTx Initialize( ref BaseSerialStream.Configuration cfg )
        {
            USART1 usart = Instance;

            return usart.Initialize( 1, ref cfg ) ? usart : null;
        }

        //
        // Access Methods
        //

        public static extern USART1 Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}