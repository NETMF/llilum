//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class ControlOperator : Operator
    {
        //
        // Constructor Methods
        //

        protected ControlOperator( Debugging.DebugInfo  debugInfo    ,
                                   OperatorCapabilities capabilities ,
                                   OperatorLevel        level        ) : base( debugInfo, capabilities, level )
        {
        }

        //--//

        //
        // Helper Methods
        //

        internal void UpdateSuccessorInformationInner()
        {
            UpdateSuccessorInformation();
        }

        protected abstract void UpdateSuccessorInformation();

        public virtual bool ShouldIncludeInScheduling( BasicBlock bbNext )
        {
            return (bbNext is ExceptionHandlerBasicBlock) == false;
        }

        //--//

        public abstract bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB );
    }
}