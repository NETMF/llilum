//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class CodeCoverage
    {
        public delegate void EnumerationCallback( uint address, uint hits, uint cycles, uint waitStates );


        //
        // Helper Methods
        //

        public abstract void Reset();
        public abstract void Dump();
        public abstract void Enumerate( EnumerationCallback dlg );

        public abstract void SetSymbols( ArmProcessor.SymDef.SymbolToAddressMap symdef         ,
                                         ArmProcessor.SymDef.AddressToSymbolMap symdef_Inverse );


        //
        // Access Methods
        //

        public abstract bool Enable
        {
            get;
            set;
        }
    }
}
