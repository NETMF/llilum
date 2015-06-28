//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class HalButtons
    {
        public const uint BUTTON_NONE = 0x00000000;
        public const uint BUTTON_B0   = 0x00000001;
        public const uint BUTTON_B1   = 0x00000002;
        public const uint BUTTON_B2   = 0x00000004;
        public const uint BUTTON_B3   = 0x00000008;
        public const uint BUTTON_B4   = 0x00000010;
        public const uint BUTTON_B5   = 0x00000020;

        //--//

        public abstract bool GetNextStateChange( out uint buttonsPressed  ,
                                                 out uint buttonsReleased );

        public abstract void QueueNextStateChange( uint buttonsPressed  ,
                                                   uint buttonsReleased );
    }
}
