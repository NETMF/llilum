//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;

    //--//

    // TODO: put right addresses, and fix code generation for LLVM that does not understand the attribute's constants
    //[MemoryMappedPeripheral(Base = 0x40D00000U, Length = 0x000000D0U)]
    public class SysTick
    {
        [Inline]
        public void SetMatch(int numTimer, uint val)
        {
            //if (numTimer < 4)
            //{
            //    this.OSMR0_3[numTimer] = val;
            //}
            //else if (numTimer < 12)
            //{
            //    this.OSMR4_11[numTimer - 4] = val;
            //}
        }

        [Inline]
        public void WriteCounter(int numTimer, uint val)
        {
        }

        [Inline]
        public uint ReadCounter(int numTimer)
        {
            return 0;
        }

        [Inline]
        public bool HasFired(int numTimer)
        {
            return false;
        }

        [Inline]
        public void ClearFired(int numTimer)
        {
        }

        [Inline]
        public void EnableInterrupt(int numTimer)
        {
        }

        [Inline]
        public void DisableInterrupt(int numTimer)
        {
        }

        //
        // Access Methods
        //

        public static extern SysTick Instance
        {
            [SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}