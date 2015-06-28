//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CloneForwardGraph : CloningContext
    {
        //
        // Constructor Methods
        //

        public CloneForwardGraph( ControlFlowGraphState cfgSource      ,
                                  ControlFlowGraphState cfgDestination ,
                                  InstantiationContext  ic             ) : base( cfgSource, cfgDestination, ic )
        {
        }
    }
}
