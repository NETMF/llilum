//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    public class BasicBlocksPostOrderVisit : PostOrderVisit< BasicBlock, ControlOperator >
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private BasicBlocksPostOrderVisit( ) : base( )
        {
        }

        public static void Compute(     EntryBasicBlock entry       ,
                                    out BasicBlock[ ]    basicBlocks )
        {
            var tree = new BasicBlocksPostOrderVisit();

            tree.Visit( entry );

            basicBlocks = tree.m_nodes.ToArray();
        }
    }
}
