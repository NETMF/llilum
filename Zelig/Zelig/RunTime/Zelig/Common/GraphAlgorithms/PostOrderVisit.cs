//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections.Generic;

    public class PostOrderVisit< N, FC > : GenericDepthFirst< FC >
    {
        //
        // State
        //

        protected List< N > m_nodes;

        //
        // Constructor Methods
        //

        protected PostOrderVisit()
        {
            m_nodes = new List<N>();
        }

        //--//

        protected override void ProcessAfter( ITreeNode<FC> bb )
        {
            m_nodes.Add( (N)bb );
        }
    }
}
