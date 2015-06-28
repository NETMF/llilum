//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class StoreFieldOperator : FieldOperator
    {
        //
        // Constructor Methods
        //

        protected StoreFieldOperator( Debugging.DebugInfo  debugInfo    ,
                                      OperatorCapabilities capabilities ,
                                      OperatorLevel        level        ,
                                      FieldRepresentation  field        ) : base( debugInfo, capabilities, level, field )
        {
        }

        //--//

        //
        // Helper Methods
        //

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //
    }
}