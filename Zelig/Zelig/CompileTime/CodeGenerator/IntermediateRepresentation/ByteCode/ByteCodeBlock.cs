//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal class ByteCodeBlock
    {
        [Flags]
        internal enum Flags
        {
            Reached          = 0x01,
            EntryPoint       = 0x02,
            JoinNode         = 0x04,
            BranchNode       = 0x08,
            BackEdge         = 0x10,
            ExceptionHandler = 0x20,

            NormalEntryPoint           = EntryPoint | JoinNode                   ,
            ExceptionHandlerEntryPoint = EntryPoint | JoinNode | ExceptionHandler,
        }

        internal static readonly ByteCodeBlock[] SharedEmptyArray = new ByteCodeBlock[0];

        //
        // State
        //

        private readonly Flags                             m_kind;
        private readonly int                               m_offsetOfFirstInstruction;
        private          int                               m_numberOfInstructions;
        private          ByteCodeBlock[]                   m_predecessors;
        private          ByteCodeBlock[]                   m_successors;
        private          ByteCodeBlock[]                   m_protectedBy;
        private          EHClause[]                        m_handlerFor;
        private          ExceptionObjectVariableExpression m_ehVariable;
        private          BasicBlock                        m_basicBlock;
        private          Expression[]                      m_exitStackModel;

        //
        // Constructor Methods
        //

        internal ByteCodeBlock( Flags kind                     ,
                                int   offsetOfFirstInstruction )
        {
            m_kind                     = kind;
            m_offsetOfFirstInstruction = offsetOfFirstInstruction;
            m_numberOfInstructions     = 0;

            m_predecessors = ByteCodeBlock.SharedEmptyArray;
            m_successors   = ByteCodeBlock.SharedEmptyArray;
            m_protectedBy  = ByteCodeBlock.SharedEmptyArray;
            m_handlerFor   = EHClause     .SharedEmptyArray;
        }

        //--//

        internal void IncrementInstructionCount()
        {
            m_numberOfInstructions++;
        }

        internal void SetPredecessor( ByteCodeBlock bcb )
        {
            m_predecessors = ArrayUtility.AddUniqueToNotNullArray( m_predecessors, bcb );
        }

        internal void SetSuccessor( ByteCodeBlock bcb )
        {
            m_successors = ArrayUtility.AddUniqueToNotNullArray( m_successors, bcb );
        }

        internal void SetProtectedBy( ByteCodeBlock bcb )
        {
            m_protectedBy = ArrayUtility.AddUniqueToNotNullArray( m_protectedBy, bcb );
        }

        internal void SetAsHandlerFor( EHClause eh )
        {
            m_handlerFor = ArrayUtility.AddUniqueToNotNullArray( m_handlerFor, eh );
        }

        //--//

        internal int OffsetOfFirstInstruction
        {
            get
            {
                return m_offsetOfFirstInstruction;
            }
        }

        internal int NumberOfInstructions
        {
            get
            {
                return m_numberOfInstructions;
            }
        }

        internal Flags Kind
        {
            get
            {
                return m_kind;
            }
        }

        internal ByteCodeBlock[] Predecessors
        {
            get
            {
                return m_predecessors;
            }
        }

        internal ByteCodeBlock[] Successors
        {
            get
            {
                return m_successors;
            }
        }

        internal ByteCodeBlock[] ProtectedBy
        {
            get
            {
                return m_protectedBy;
            }
        }

        internal EHClause[] HandlerFor
        {
            get
            {
                return m_handlerFor;
            }
        }

        internal ExceptionObjectVariableExpression EhVariable
        {
            get
            {
                return m_ehVariable;
            }

            set
            {
                m_ehVariable = value;
            }
        }

        internal BasicBlock BasicBlock
        {
            get
            {
                return m_basicBlock;
            }

            set
            {
                m_basicBlock = value;
            }
        }

        internal Expression[] ExitStackModel
        {
            get
            {
                return m_exitStackModel;
            }

            set
            {
                m_exitStackModel = value;
            }
        }
    }
}
