//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public static class CommonMethodRedundancyElimination
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            bool fModified = false;
            
            if(Transformations.MergeExtendedBasicBlocks.Execute( cfg, preserveInjectionSites: true ))
            {
                fModified = true;
            }

            if(Transformations.RemoveDeadCode.Execute( cfg, false ))
            {
                fModified = true;
            }

            if(Transformations.GlobalCopyPropagation.Execute( cfg ))
            {
                fModified = true;
            }

            if(Transformations.InlineScalars.Execute( cfg ))
            {
                fModified = true;
            }

            if(Transformations.RemoveSimpleIndirections.Execute( cfg ))
            {
                fModified = true;
            }

            if(Transformations.RemoveDeadCode.Execute( cfg, false ))
            {
                fModified = true;
            }

            return fModified;
        }
    }
}
