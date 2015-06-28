//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class Interop
    {
        [Flags]
        public enum CallbackResponse
        {
            DoNothing       = 0x00000000,
            StopExecution   = 0x00000001,
            NextInstruction = 0x00000002,
            RemoveDetour    = 0x00000004,
        }

        public delegate CallbackResponse Callback();

        public class Registration
        {
            //
            // State
            //

            public readonly uint     Address;
            public readonly bool     IsPostProcessing;
            public readonly Callback Target;

            //
            // Constructor Methods 
            //

            public Registration( uint     address          ,
                                 bool     isPostProcessing ,
                                 Callback target           )
            {
                this.Address          = address;
                this.IsPostProcessing = isPostProcessing;
                this.Target           = target;
            }
        }

        //--//

        //
        // Helper Methods
        //

        public abstract Registration SetInterop( uint     pc              ,
                                                 bool     fHead           ,
                                                 bool     fPostProcessing ,
                                                 Callback ftn             );

        public abstract void RemoveInterop( Registration reg );
    }
}
