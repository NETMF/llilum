//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    public sealed class EntryBasicBlock : BasicBlock
    {
        //
        // Constructor Methods
        //

        public EntryBasicBlock( ControlFlowGraphState owner ) : base( owner )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override BasicBlock Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new EntryBasicBlock( context.ControlFlowGraphDestination ) );
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToShortString()
        {
            return ToShortStringInner( "EntryBasicBlock" );
        }
    }
}