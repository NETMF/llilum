//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    public sealed class ExitBasicBlock : BasicBlock
    {
        //
        // Constructor Methods
        //

        public ExitBasicBlock( ControlFlowGraphState owner ) : base( owner )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override BasicBlock Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ExitBasicBlock( context.ControlFlowGraphDestination ) );
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToShortString()
        {
            return ToShortStringInner( "ExitBasicBlock" );
        }
    }
}