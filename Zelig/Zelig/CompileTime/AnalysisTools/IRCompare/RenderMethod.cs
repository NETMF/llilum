//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.IRCompare
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;

////using ARM = Microsoft.Zelig.Emulation.ArmProcessor;

    public class RenderMethod : IR.BaseIntermediateRepresentationDumper
    {
        public class OutputLine
        {
            public string      Text;
            public IR.Operator Operator;
        }

        //
        // State
        //

        IR.Operator        m_currentOperator;
        StringBuilder      m_writer;
        List< OutputLine > m_output;
        int                m_indent;
        bool               m_fIndented;

        //
        // Constructor Methods
        //

        public RenderMethod()
        {
            m_writer = new StringBuilder();
        }

        //--//

        private void AddLine()
        {
            OutputLine ol = new OutputLine();

            ol.Text     = m_writer.ToString();
            ol.Operator = m_currentOperator;

            m_output.Add( ol );

            m_writer.Length = 0;
        }

        public void WriteLine()
        {
            WriteIndentation();

            AddLine();

            m_fIndented = false;
        }

        public void WriteLine( string s )
        {
            WriteIndentation();

            m_writer.Append( s );
            AddLine();

            m_fIndented = false;
        }

        public void WriteLine( string s    ,
                               object arg1 )
        {
            WriteIndentedLine( s, arg1 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 )
        {
            WriteIndentedLine( s, arg1, arg2 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 ,
                               object arg3 )
        {
            WriteIndentedLine( s, arg1, arg2, arg3 );
        }

        public void WriteLine(        string   s    ,
                               params object[] args )
        {
            WriteIndentedLine( s, args );
        }

        //--//

        public List< OutputLine > DumpMethod( TS.MethodRepresentation md )
        {
            m_output = new List< OutputLine >();

            DumpGraph( IR.TypeSystemForCodeTransformation.GetCodeForMethod( md ) );

            return m_output;
        }

        public override void DumpGraph( IR.ControlFlowGraphState cfg )
        {
            IR.ControlFlowGraphStateForCodeTransformation cfg2 = (IR.ControlFlowGraphStateForCodeTransformation)cfg;

            base.DumpGraph( cfg2 );

            //--//

            if(m_sortedVariables.Length > 0)
            {
                m_indent += 3;

                foreach(IR.VariableExpression ex in m_sortedVariables)
                {
                    WriteLine( "{0} {1} as {2}", m_useChains[ex.SpanningTreeIndex].Length > 0 ? "       var" : "unused var", ex, ex.Type );
                }

                m_indent -= 3;

                WriteLine();
            }

            foreach(IR.BasicBlock bb in m_basicBlocks)
            {
                ProcessBasicBlock( bb );
            }
        }

        private void ProcessBasicBlock( IR.BasicBlock bb )
        {
            m_indent += 1;

            string label = CreateLabel( bb );
            if(label != null)
            {
                WriteLine( "{0}: [Index:{1}] [Annotation:{2}]", label, bb.SpanningTreeIndex, bb.Annotation );
            }

            m_indent += 4;

            if(bb is IR.ExceptionHandlerBasicBlock)
            {
                IR.ExceptionHandlerBasicBlock ehBB = (IR.ExceptionHandlerBasicBlock)bb;

                foreach(IR.ExceptionClause eh in ehBB.HandlerFor)
                {
                    WriteLine( ".handler for {0}", eh );
                }
            }

            foreach(IR.ExceptionHandlerBasicBlock bbEh in bb.ProtectedBy)
            {
                foreach(IR.ExceptionClause eh in bbEh.HandlerFor)
                {
                    WriteLine( ".protected by {0} at {1}", eh, CreateLabel( bbEh ) );
                }
            }

            foreach(IR.BasicBlockEdge edge in (IR.BasicBlockEdge[])bb.Predecessors)
            {
                WriteLine( ".edge {0} from {1}", edge.EdgeClass, CreateLabel( (IR.BasicBlock)edge.Predecessor ) );
            }

            foreach(IR.BasicBlockEdge edge in bb.Successors)
            {
                WriteLine( ".edge {0} to {1}", edge.EdgeClass, CreateLabel( (IR.BasicBlock)edge.Successor ) );
            }

            GrowOnlySet< IR.Expression > used = SetFactory.NewWithReferenceEquality< IR.Expression >();
            foreach(IR.Operator op in bb.Operators)
            {
                foreach(IR.Expression ex in op.Arguments)
                {
                    used.Insert( ex );
                }
            }

            foreach(IR.Expression ex in used)
            {
                bool fGot = false;

                foreach(int opIndex in m_reachingDefinitions[bb.Operators[0].SpanningTreeIndex])
                {
                    if(m_operators[opIndex].IsSourceOfExpression( ex ))
                    {
                        if(fGot == false)
                        {
                            WriteIndented( ".reaching {0} <=", ex );

                            fGot = true;
                        }
                        else
                        {
                            WriteIndented( "," );
                        }

                        WriteIndented( " Op_{0}", opIndex );
                    }
                }

                if(fGot)
                {
                    WriteLine();
                }
            }

            //--//

            foreach(IR.Operator op in bb.Operators)
            {
                DumpOperator( op );
            }

            m_indent -= 4;

            m_indent -= 1;

            WriteLine();
        }

        private void DumpOperator( IR.Operator op )
        {
            m_currentOperator = op;

            string s = op.FormatOutput( this );

            foreach(string s2 in s.Split( '\n' ))
            {
                WriteLine( "Op_{0,-4}:  {1}", op.SpanningTreeIndex, s2 );
            }

            m_currentOperator = null;
        }

        //--//

        private void WriteIndented(        string   s    ,
                                    params object[] args )
        {
            WriteIndentation();

            m_writer.Append( FormatOutputInternal( s, args ) );
        }

        private void WriteIndentedLine(        string   s    ,
                                        params object[] args )
        {
            WriteIndentation();

            m_writer.Append( FormatOutputInternal( s, args ) );
            AddLine();

            m_fIndented = false;
        }

        private void WriteIndentation()
        {
            if(m_fIndented == false)
            {
                m_fIndented = true;

                for(int i = 0; i < m_indent; i++)
                {
                    m_writer.Append( "  " );
                }
            }
        }
    }
}
