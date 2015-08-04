//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DUMP_ALIVE_INFO


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //
    // Debug classes
    //

    public abstract class BaseIntermediateRepresentationDumper : IIntermediateRepresentationDumper
    {
        //
        // State
        //

        private   GrowOnlyHashTable< Expression, string > m_lookupName;
        private   GrowOnlyHashTable< BasicBlock, string > m_lookupLabel;

        //--//

        protected BasicBlock[]                            m_basicBlocks;
        protected BasicBlock[]                            m_ancestors;
        protected Operator[]                              m_operators;
        protected VariableExpression[]                    m_variables;
        protected VariableExpression[]                    m_sortedVariables;
        protected Operator[][]                            m_useChains;

                                                       
        protected BitVector[]                             m_reachingDefinitions;

#if DUMP_ALIVE_INFO
        protected BitVector[]                             m_liveness;
#endif


        //
        // Constructor Methods
        //

        protected BaseIntermediateRepresentationDumper()
        {
            m_lookupName  = HashTableFactory.NewWithReferenceEquality< Expression, string >();
            m_lookupLabel = HashTableFactory.NewWithReferenceEquality< BasicBlock, string >();
        }

        //--//

        public MethodRepresentation[] Sort( IEnumerable< MethodRepresentation > enumerable )
        {
            GrowOnlyHashTable< MethodRepresentation, string > htNames = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, string >();

            foreach(MethodRepresentation md in enumerable)
            {
                htNames[md] = md.ToShortString();
            }

            MethodRepresentation[] res = new MethodRepresentation[htNames.Count];
            int                    pos = 0;

            foreach(MethodRepresentation md in htNames.Keys)
            {
                res[pos++] = md;
            }

            Array.Sort( res, delegate( MethodRepresentation left, MethodRepresentation right )
            {
                return String.Compare( htNames[left], htNames[right] );
            } );

            return res;
        }

        public string FormatOutput( string s    ,
                                    object arg1 )
        {
            return FormatOutputInternal( s, arg1 );
        }

        public string FormatOutput( string s    ,
                                    object arg1 ,
                                    object arg2 )
        {
            return FormatOutputInternal( s, arg1, arg2 );
        }

        public string FormatOutput( string s    ,
                                    object arg1 ,
                                    object arg2 ,
                                    object arg3 )
        {
            return FormatOutputInternal( s, arg1, arg2, arg3 );
        }

        public string FormatOutput(        string   s    ,
                                    params object[] args )
        {
            return FormatOutputInternal( s, args );
        }

        //--//

        public virtual void DumpGraph( ControlFlowGraphState cfg )
        {
            ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

            m_lookupName .Clear();
            m_lookupLabel.Clear();

            //--//

            m_basicBlocks = cfg2.DataFlow_SpanningTree_BasicBlocks;
            m_ancestors   = cfg2.DataFlow_SpanningTree_Ancestors;
            m_operators   = cfg2.DataFlow_SpanningTree_Operators;
            m_variables   = cfg2.DataFlow_SpanningTree_Variables;
            m_useChains   = cfg2.DataFlow_UseChains;

            m_reachingDefinitions = cfg2.DataFlow_ReachingDefinitions;

#if DUMP_ALIVE_INFO
            m_liveness            = cfg2.LivenessAtOperator; // It's indexed as [<operator index>][<variable index>]
#endif

            cfg2.RenumberVariables();

            m_sortedVariables = cfg2.SortVariables();
        }

        public string CreateName( Expression ex )
        {
            string res;

            if(m_lookupName.TryGetValue( ex, out res ) == false)
            {
                res = ex.ToString();

                m_lookupName[ex] = res;
            }

            return res;
        }

        public string CreateLabel( BasicBlock bb )
        {
            string res;

            if(m_lookupLabel.TryGetValue( bb, out res ) == false)
            {
                res = bb.ToShortString();

                m_lookupLabel[bb] = res;
            }

            return res;
        }

        //--//

        protected string FormatOutputInternal(        string   s    ,
                                               params object[] args )
        {
            for(int i = 0; i < args.Length; i++)
            {
                object o = args[i];

                if(o is Expression)
                {
                    Expression ex = (Expression)o;

                    args[i] = CreateName( ex );
                }

                if(o is BasicBlock)
                {
                    BasicBlock bb = (BasicBlock)o;

                    args[i] = CreateLabel( bb );
                }
            }

            return string.Format( s, args );
        }
    }

    //--//

    public class TextIntermediateRepresentationDumper : BaseIntermediateRepresentationDumper, IDisposable
    {
        //
        // State
        //

        System.IO.TextWriter m_writer;
        bool                 m_closeWriter;
        int                  m_indent;
        bool                 m_fIndented;
        SourceCodeTracker    m_sourceCodeTracker;

        //
        // Constructor Methods
        //

        public TextIntermediateRepresentationDumper()
        {
            m_writer      = Console.Out;
            m_closeWriter = false;

            Init();
        }

        public TextIntermediateRepresentationDumper( string file )
        {
            m_writer      = new System.IO.StreamWriter( file, false, System.Text.Encoding.ASCII, 1024 * 1024 );
            m_closeWriter = true;

            Init();
        }

        public TextIntermediateRepresentationDumper( System.IO.StreamWriter writer )
        {
            m_writer      = writer;
            m_closeWriter = false;

            Init();
        }

        private void Init()
        {
            m_sourceCodeTracker = new SourceCodeTracker();
        }

        //--//

        public void WriteLine()
        {
            WriteIndentation();

            m_writer.WriteLine();

            m_fIndented = false;
        }

        public void WriteLine( string s )
        {
            WriteIndentation();

            m_writer.WriteLine( s );

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

        public override void DumpGraph( ControlFlowGraphState cfg )
        {
            ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

            base.DumpGraph( cfg2 );

            m_sourceCodeTracker.ExtraLinesToOutput = 3;

            //--//

            if(m_sortedVariables.Length > 0)
            {
                m_indent += 3;

                foreach(VariableExpression ex in m_sortedVariables)
                {
                    WriteLine( "{0} {1} as {2}", m_useChains[ex.SpanningTreeIndex].Length > 0 ? "       var" : "unused var", ex, ex.Type );
                }

                m_indent -= 3;

                WriteLine();
            }

            foreach(BasicBlock bb in m_basicBlocks)
            {
                ProcessBasicBlock( bb );
            }
        }

        private void ProcessBasicBlock( BasicBlock bb )
        {
            m_indent += 1;

            string label = CreateLabel( bb );
            if(label != null)
            {
                WriteLine( "{0}: [Index:{1}] [Annotation:{2}]", label, bb.SpanningTreeIndex, bb.Annotation );
            }

            //
            // Print context for every basic block, even if they are consecutive.
            //
            m_sourceCodeTracker.ResetContext();

            m_indent += 2;

            if(bb is ExceptionHandlerBasicBlock)
            {
                ExceptionHandlerBasicBlock ehBB = (ExceptionHandlerBasicBlock)bb;

                foreach(ExceptionClause eh in ehBB.HandlerFor)
                {
                    WriteLine( ".handler for {0}", eh );
                }
            }

            foreach(ExceptionHandlerBasicBlock bbEh in bb.ProtectedBy)
            {
                foreach(ExceptionClause eh in bbEh.HandlerFor)
                {
                    WriteLine( ".protected by {0} at {1}", eh, CreateLabel( bbEh ) );
                }
            }

            foreach(BasicBlockEdge edge in bb.Predecessors)
            {
                WriteLine( ".edge {0} from {1}", edge.EdgeClass, CreateLabel( edge.Predecessor ) );
            }

            foreach(BasicBlockEdge edge in bb.Successors)
            {
                WriteLine( ".edge {0} to {1}", edge.EdgeClass, CreateLabel( edge.Successor ) );
            }

            GrowOnlySet< Expression > used = SetFactory.NewWithReferenceEquality< Expression >();
            foreach(Operator op in bb.Operators)
            {
                foreach(Expression ex in op.Arguments)
                {
                    used.Insert( ex );
                }
            }

            foreach(Expression ex in used)
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

            foreach(Operator op in bb.Operators)
            {
                DumpOperator( op );
            }

            m_indent -= 2;

            m_indent -= 1;

            WriteLine();
        }

        private void DumpOperator( Operator op )
        {
            DumpSourceCode( op.DebugInfo );

#if DUMP_ALIVE_INFO
            if(op.IsSideEffect == false)
            {
                BitVector vec = m_liveness[op.SpanningTreeIndex];

                if(vec.Cardinality != 0)
                {
                    WriteLine();

                    foreach(int idx in vec)
                    {
                        VariableExpression ex = m_variables[idx];

                        WriteIndentedLine( ".alive {0}", ex );
                    }
                }
            }
#endif

            string s = op.FormatOutput( this );

            foreach(string s2 in s.Split( '\n' ))
            {
                WriteLine("Op_{0,-3} {1,-35}: {2}", op.SpanningTreeIndex, op.GetType().Name, s2);
            }

            foreach(Annotation an in op.Annotations)
            {
                WriteLine( "            {0}", an.FormatOutput( this ) );
            }
        }

        private void DumpSourceCode( Debugging.DebugInfo dbg )
        {
            m_sourceCodeTracker.Print( dbg, delegate ( string format, object[] args )
            {
                m_writer.Write    ( ";;;"        );
                m_writer.WriteLine( format, args );
            } );
        }

        //--//

        private void WriteIndented(        string   s    ,
                                    params object[] args )
        {
            WriteIndentation();

            m_writer.Write( FormatOutputInternal( s, args ) );
        }

        private void WriteIndentedLine(        string   s    ,
                                        params object[] args )
        {
            WriteIndentation();

            m_writer.WriteLine( FormatOutputInternal( s, args ) );

            m_fIndented = false;
        }

        private void WriteIndentation()
        {
            if(m_fIndented == false)
            {
                m_fIndented = true;

                for(int i = 0; i < m_indent; i++)
                {
                    m_writer.Write( "  " );
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(m_closeWriter)
            {
                m_writer.Flush();
                m_writer.Dispose();

                m_writer      = null;
                m_closeWriter = false;
            }
        }

        #endregion
    }

    //--//

    public class XmlIntermediateRepresentationDumper : BaseIntermediateRepresentationDumper
    {
        //
        // State
        //

        System.Xml.XmlDocument m_doc;
        System.Xml.XmlNode     m_root;

        //
        // Constructor Methods
        //

        public XmlIntermediateRepresentationDumper( System.Xml.XmlDocument doc  ,
                                                    System.Xml.XmlNode     root )
        {
            m_doc  = doc;
            m_root = root;
        }

        //--//

        public override void DumpGraph( ControlFlowGraphState cfg )
        {
            ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

            base.DumpGraph( cfg2 );

            //--//

            System.Xml.XmlNode node = XmlHelper.AddElement( m_root, "Method" );

            XmlHelper.AddAttribute( node, "Name", cfg.Method.ToString() );

            foreach(VariableExpression ex in cfg2.DataFlow_SpanningTree_Variables)
            {
                DumpVariable( node, ex );
            }

            foreach(BasicBlock bb in m_basicBlocks)
            {
                ProcessBasicBlock( node, bb );
            }
        }

        private void DumpVariable( System.Xml.XmlNode node ,
                                   VariableExpression ex   )
        {
            System.Xml.XmlNode subnode = XmlHelper.AddElement( node, "Variable" );

            XmlHelper.AddAttribute( subnode, "Name", CreateName( ex )   );
            XmlHelper.AddAttribute( subnode, "Type", ex.Type.ToString() );
        }

        private void ProcessBasicBlock( System.Xml.XmlNode node ,
                                        BasicBlock         bb   )
        {
            System.Xml.XmlNode subnode = XmlHelper.AddElement( node, "BasicBlock" );

            XmlHelper.AddAttribute( subnode, "Id"   , CreateLabel( bb ) );
            XmlHelper.AddAttribute( subnode, "Index", bb.SpanningTreeIndex.ToString() );
            XmlHelper.AddAttribute( subnode, "Type" , bb.GetType().Name );

            foreach(BasicBlockEdge edge in bb.Successors)
            {
                DumpEdge( subnode, edge );
            }

            //--//

            GrowOnlySet< Expression > used = SetFactory.NewWithReferenceEquality< Expression >();
            foreach(Operator op in bb.Operators)
            {
                foreach(Expression ex in op.Arguments)
                {
                    used.Insert( ex );
                }
            }

            foreach(Expression ex in used)
            {
                System.Xml.XmlNode reachingNode = null;

                foreach(int opIndex in m_reachingDefinitions[bb.Operators[0].SpanningTreeIndex])
                {
                    if(m_operators[opIndex].IsSourceOfExpression( ex ))
                    {
                        if(reachingNode == null)
                        {
                            reachingNode = XmlHelper.AddElement( subnode, "ReachingDefinition" );

                            XmlHelper.AddAttribute( reachingNode, "Variable", CreateName( ex ) );
                        }

                        System.Xml.XmlNode opNode = XmlHelper.AddElement( reachingNode, "Definition" );

                        XmlHelper.AddAttribute( opNode, "Index", opIndex.ToString() );
                    }
                }
            }

            if(bb is ExceptionHandlerBasicBlock)
            {
                ExceptionHandlerBasicBlock ehBB = (ExceptionHandlerBasicBlock)bb;

                foreach(ExceptionClause eh in ehBB.HandlerFor)
                {
                    System.Xml.XmlNode exNode = XmlHelper.AddElement( subnode, "HandlerFor" );

                    XmlHelper.AddAttribute( exNode, "Type", eh.ToString() );
                }
            }

            //--//

            foreach(Operator op in bb.Operators)
            {
                DumpOperator( subnode, op );
            }
        }

        private void DumpOperator( System.Xml.XmlNode node ,
                                   Operator           op   )
        {
            string s = op.FormatOutput( this );

            foreach(string s2 in s.Split( '\n' ))
            {
                System.Xml.XmlNode subnode = XmlHelper.AddElement( node, "Operator" );

                XmlHelper.AddAttribute( subnode, "Index", op.SpanningTreeIndex.ToString() );

                XmlHelper.AddAttribute(subnode, "Type", op.GetType().Name);

                if(op is CallOperator)
                {
                    CallOperator op2 = (CallOperator)op;

                    XmlHelper.AddAttribute( subnode, "Call", op2.TargetMethod.ToString() );
                }

                DumpDebug( subnode, op.DebugInfo );

                subnode.AppendChild( m_doc.CreateTextNode( s2 ) );
            }
        }

        private void DumpDebug( System.Xml.XmlNode  node ,
                                Debugging.DebugInfo dbg  )
        {
            if(dbg != null)
            {
                System.Xml.XmlNode subnode = XmlHelper.AddElement( node, "Debug" );

                XmlHelper.AddAttribute( subnode, "File"       , dbg.SrcFileName                );
                XmlHelper.AddAttribute( subnode, "MethodName" , dbg.MethodName                 );
                XmlHelper.AddAttribute( subnode, "BeginLine"  , dbg.BeginLineNumber.ToString() );
                XmlHelper.AddAttribute( subnode, "BeginColumn", dbg.BeginColumn    .ToString() );
                XmlHelper.AddAttribute( subnode, "EndLine"    , dbg.EndLineNumber  .ToString() );
                XmlHelper.AddAttribute( subnode, "EndColumn"  , dbg.EndColumn      .ToString() );
            }
        }

        private void DumpEdge( System.Xml.XmlNode node ,
                               BasicBlockEdge     edge )
        {
            System.Xml.XmlNode subnode = XmlHelper.AddElement( node, "Edge" );

            XmlHelper.AddAttribute( subnode, "From", CreateLabel( edge.Predecessor )        );
            XmlHelper.AddAttribute( subnode, "To"  , CreateLabel( edge.Successor   )        );
            XmlHelper.AddAttribute( subnode, "Kind",              edge.EdgeClass.ToString() );
        }
    }

    //--//

    public class PrettyDumper : BaseIntermediateRepresentationDumper
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        public PrettyDumper()
        {
        }

        //--//

        public override void DumpGraph( ControlFlowGraphState cfg )
        {
        }

        internal static string Dump( Operator op )
        {
            PrettyDumper dumper = new PrettyDumper();

            return op.FormatOutput( dumper );
        }
    }
}