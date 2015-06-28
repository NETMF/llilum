//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.IRViewer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

    public class Method
    {
        //
        // State
        //

        public string                 Name;
        public List< Variable       > Variables       = new List< Variable       >();
        public List< BasicBlock     > BasicBlocks     = new List< BasicBlock     >();
        public List< BasicBlockEdge > BasicBlockEdges = new List< BasicBlockEdge >();
    }

    public class Variable
    {
        //
        // State
        //

        public string Name;
        public string Type;
    }

    public class BasicBlock
    {
        //
        // State
        //

        public string                     Id;
        public string                     Index;
        public string                     Type;
        public List< Operator           > Operators           = new List< Operator           >();
        public List< ReachingDefinition > ReachingDefinitions = new List< ReachingDefinition >();
        public List< HandlerFor         > Handlers            = new List< HandlerFor         >();
    }

    public class ReachingDefinition
    {
        //
        // State
        //

        public string           Variable;
        public List< Operator > Definitions = new List< Operator >();
    }

    public class HandlerFor
    {
        //
        // State
        //

        public string Eh;
    }

    public class Operator
    {
        //
        // State
        //

        public int    Index;
        public string Value;
        public string Call;
        public Debug  Debug;
    }

    public class Debug
    {
        //
        // State
        //

        public string File;
        public int    BeginLine;
        public int    BeginColumn;
        public int    EndLine;
        public int    EndColumn;
    }

    public class BasicBlockEdge
    {
        //
        // State
        //

        public BasicBlock From;
        public BasicBlock To;
        public string     Kind;
    }

    public class Parser
    {
        //
        // State
        //

        private Dictionary< string, Method > m_methods = new Dictionary< string, Method >();

        //
        // Constructor Methods
        //

        public Parser( XmlNode node )
        {
            foreach(XmlNode subnode in node.SelectNodes( "Method" ))
            {
                Method res = ParseMethod( subnode );

                m_methods[ res.Name ] = res;
            }
        }

        public Dictionary< string, Method > Methods
        {
            get
            {
                return m_methods;
            }
        }

        //--//

        private Method ParseMethod( XmlNode node )
        {
            Method res = new Method();

            res.Name = GetAttribute( node, "Name" );

            foreach(XmlNode subnode in node.SelectNodes( "Variable" ))
            {
                res.Variables.Add( ParseVariable( subnode ) );
            }

            Dictionary< string, BasicBlock > lookupBasicBlock = new Dictionary< string, BasicBlock >();
            Dictionary< string, XmlNode    > lookupNode       = new Dictionary< string, XmlNode    >();
            Dictionary< int   , Operator   > lookupOperator   = new Dictionary< int   , Operator   >();

            foreach(XmlNode subnode in node.SelectNodes( "BasicBlock" ))
            {
                res.BasicBlocks.Add( ParseBasicBlock( subnode, lookupBasicBlock, lookupNode, lookupOperator ) );
            }

            foreach(string id in lookupNode.Keys)
            {
                BasicBlock bb      = lookupBasicBlock[ id ];
                XmlNode    subnode = lookupNode      [ id ];

                ParseBasicBlock( subnode, res, bb, lookupBasicBlock, lookupOperator );
            }

            return res;
        }

        private Variable ParseVariable( XmlNode node )
        {
            Variable res = new Variable();

            res.Name = GetAttribute( node, "Name" );
            res.Type = GetAttribute( node, "Type" );

            return res;
        }

        private BasicBlock ParseBasicBlock( XmlNode                          node             ,
                                            Dictionary< string, BasicBlock > lookupBasicBlock ,
                                            Dictionary< string, XmlNode    > lookupNode       ,
                                            Dictionary< int   , Operator   > lookupOperator   )
        {
            BasicBlock res = new BasicBlock();

            res.Id    = GetAttribute( node, "Id"    );
            res.Index = GetAttribute( node, "Index" );
            res.Type  = GetAttribute( node, "Type"  );

            lookupBasicBlock[ res.Id ] = res;
            lookupNode      [ res.Id ] = node;

            foreach(XmlNode subnode in node.SelectNodes( "Operator" ))
            {
                res.Operators.Add( ParseOperator( subnode, lookupOperator ) );
            }

            return res;
        }

        private Operator ParseOperator( XmlNode                     node           ,
                                        Dictionary< int, Operator > lookupOperator )
        {
            Operator res = new Operator();

            res.Index = int.Parse( GetAttribute( node, "Index" ) );
            res.Call  =            GetAttribute( node, "Call"  );

            res.Value =            node.InnerText;

            XmlNode subnode = node.SelectSingleNode( "Debug" );
            if(subnode != null)
            {
                res.Debug = ParseDebug( subnode );
            }

            lookupOperator[ res.Index ] = res;

            return res;
        }

        private Debug ParseDebug( XmlNode node )
        {
            Debug res = new Debug();

            res.File        =            GetAttribute( node, "File"        );
            res.BeginLine   = int.Parse( GetAttribute( node, "BeginLine"   ) );
            res.BeginColumn = int.Parse( GetAttribute( node, "BeginColumn" ) );
            res.EndLine     = int.Parse( GetAttribute( node, "EndLine"     ) );
            res.EndColumn   = int.Parse( GetAttribute( node, "EndColumn"   ) );

            return res;
        }

        private void ParseBasicBlock( XmlNode                          node             ,
                                      Method                           method           ,
                                      BasicBlock                       bb               ,
                                      Dictionary< string, BasicBlock > lookupBasicBlock ,
                                      Dictionary< int, Operator >      lookupOperator   )
        {
            foreach(XmlNode subnode in node.SelectNodes( "ReachingDefinition" ))
            {
                bb.ReachingDefinitions.Add( ParseReachingDefinition( subnode, lookupOperator ) );
            }

            foreach(XmlNode subnode in node.SelectNodes( "HandlerFor" ))
            {
                bb.Handlers.Add( ParseHandler( subnode ) );
            }

            foreach(XmlNode subnode in node.SelectNodes( "Edge" ))
            {
                method.BasicBlockEdges.Add( ParseBasicBlockEdge( subnode, lookupBasicBlock ) );
            }
        }

        private ReachingDefinition ParseReachingDefinition( XmlNode                     node           ,
                                                            Dictionary< int, Operator > lookupOperator )
        {
            ReachingDefinition res = new ReachingDefinition();

            res.Variable = GetAttribute( node, "Variable" );

            foreach(XmlNode subnode in node.SelectNodes( "Definition" ))
            {
                res.Definitions.Add( lookupOperator[ int.Parse( GetAttribute( subnode, "Index" ) ) ] );
            }

            return res;
        }

        private HandlerFor ParseHandler( XmlNode node )
        {
            HandlerFor res = new HandlerFor();

            res.Eh = GetAttribute( node, "Type" );

            return res;
        }

        private BasicBlockEdge ParseBasicBlockEdge( XmlNode                          node             ,
                                                    Dictionary< string, BasicBlock > lookupBasicBlock )
        {
            BasicBlockEdge res = new BasicBlockEdge();

            res.From = lookupBasicBlock[ GetAttribute( node, "From" ) ];
            res.To   = lookupBasicBlock[ GetAttribute( node, "To"   ) ];
            res.Kind =                   GetAttribute( node, "Kind" )  ;

            return res;
        }

        //--//

        private static string GetAttribute( XmlNode node ,
                                            string  name )
        {
            XmlAttribute attrib = node.Attributes.GetNamedItem( name ) as XmlAttribute ;

            if(attrib != null)
            {
                return attrib.Value;
            }

            return null;
        }
    }
}
