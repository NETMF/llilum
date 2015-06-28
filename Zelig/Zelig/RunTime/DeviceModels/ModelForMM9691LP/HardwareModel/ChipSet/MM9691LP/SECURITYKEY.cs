//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38090000U,Length=0x00000080U)]
    public class SECURITYKEY
    {
        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0004U)]
        public class BYTE
        {
            [Register(Offset=0x00000000U)] public byte Data8;
        }

        //--//

        [Register(Offset=0x00000000U,Instances=32)] public BYTE[] Key;

        //
        // Access Methods
        //

        public static extern SECURITYKEY Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}