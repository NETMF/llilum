//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x380D0000U,Length=0x00000004U)]
    public class EDMAIF
    {
        [Register(Offset=0x00000000U)] public uint EMDAIF_Control;

        //
        // Access Methods
        //

        public static extern EDMAIF Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}