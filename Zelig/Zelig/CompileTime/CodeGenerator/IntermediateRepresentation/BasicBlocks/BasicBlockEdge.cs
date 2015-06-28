//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;

    public sealed class BasicBlockEdge
    {
        public static readonly BasicBlockEdge[] SharedEmptyArray = new BasicBlockEdge[0];

        //
        // State
        //

        private readonly BasicBlock          m_predecessor;
        private readonly BasicBlock          m_successor;
        private          BasicBlockEdgeClass m_edgeClass;

        //
        // Constructor Methods
        //

        internal BasicBlockEdge( BasicBlock predecessor ,
                                 BasicBlock successor   )
        {
            m_predecessor = predecessor;
            m_successor   = successor;
            m_edgeClass   = BasicBlockEdgeClass.Unknown;
        }

        //--//

        //
        // Helper Methods
        //

        //--//

        //
        // Access Methods
        //

        public BasicBlock Predecessor
        {
            get
            {
                return m_predecessor;
            }
        }

        public BasicBlock Successor
        {
            get
            {
                return m_successor;
            }
        }

        public BasicBlockEdgeClass EdgeClass
        {
            get
            {
                return m_edgeClass;
            }

            set
            {
                m_edgeClass = value;
            }
        }
    }

}