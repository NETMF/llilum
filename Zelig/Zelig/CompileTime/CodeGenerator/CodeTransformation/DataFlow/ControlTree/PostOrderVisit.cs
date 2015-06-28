//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    public class PostOrderVisit : GenericDepthFirst
    {
        //
        // State
        //

        List< BasicBlock > m_basicBlocks;

        //
        // Constructor Methods
        //

        private PostOrderVisit()
        {
            m_basicBlocks = new List< BasicBlock >();
        }

        public static void Compute(     EntryBasicBlock entry       ,
                                    out BasicBlock[]    basicBlocks )
        {
            PostOrderVisit tree = new PostOrderVisit();

            tree.Visit( entry );

            basicBlocks = tree.m_basicBlocks.ToArray();
        }

        //--//

        protected override void ProcessAfter( BasicBlock bb )
        {
            m_basicBlocks.Add( bb );
        }
    }
}
