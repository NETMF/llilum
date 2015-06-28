//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CloneForwardGraphButLinkToExceptionHandlers : CloningContext
    {
        //
        // Constructor Methods
        //

        public CloneForwardGraphButLinkToExceptionHandlers( ControlFlowGraphState cfgSource      ,
                                                            ControlFlowGraphState cfgDestination ,
                                                            InstantiationContext  ic             ) : base( cfgSource, cfgDestination, ic )
        {
        }

        //--//

        protected override bool FoundInCache(     object from ,
                                              out object to   )
        {
            if(base.FoundInCache( from, out to )) return true;

            if(from is ExceptionHandlerBasicBlock)
            {
                to = from;
                return true;
            }

            return false;
        }
    }
}
