//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    public sealed class NormalBasicBlock : BasicBlock
    {
        //
        // Constructor Methods
        //

        public NormalBasicBlock( ControlFlowGraphState owner ) : base( owner )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override BasicBlock Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new NormalBasicBlock( context.ControlFlowGraphDestination ) );
        }

        public static NormalBasicBlock CreateWithSameProtection( BasicBlock bb )
        {
            //
            // Create new basic block, copying the exception handler sets.
            //
            var res = new NormalBasicBlock( bb.Owner );

            res.m_protectedBy = ArrayUtility.CopyNotNullArray( bb.ProtectedBy );

            return res;
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToShortString()
        {
            return ToShortStringInner( "BasicBlock" );
        }
    }
}