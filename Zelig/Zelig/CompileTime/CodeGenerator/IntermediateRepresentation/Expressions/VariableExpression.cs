//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class VariableExpression : Expression
    {
        [Flags]
        public enum Property
        {
            AddressTaken     = 0x00000001,
            Volatile         = 0x00000002,
            PhysicalRegister = 0x00000004,
        }

        public sealed class DebugInfo
        {
            //
            // State
            //

            private MethodRepresentation m_context;
            private string               m_name;
            private int                  m_number;
            private bool                 m_isLocal;

            //
            // Constructor Methods
            //

            public DebugInfo( MethodRepresentation context ,
                              string               name    ,
                              int                  number  ,
                              bool                 isLocal )
            {
                m_context = context;
                m_name    = name;
                m_number  = number;
                m_isLocal = isLocal;
            }

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContextForIR context )
            {
                context.Push( this );

                context.Transform( ref m_context );
                context.Transform( ref m_name    );
                context.Transform( ref m_number  );
                context.Transform( ref m_isLocal );

                context.Pop();
            }

            //
            // Access Methods
            //

            public MethodRepresentation Context
            {
                get
                {
                    return m_context;
                }
            }

            public string Name
            {
                get
                {
                    return m_name;
                }
            }

            public int Number
            {
                get
                {
                    return m_number;
                }
            }

            public bool IsLocal
            {
                get
                {
                    return m_isLocal;
                }
            }
        }

        //--//

        public static new readonly VariableExpression[] SharedEmptyArray = new VariableExpression[0];

        protected const int c_VariableKind_Argument  = 0;
        protected const int c_VariableKind_Local     = 1;
        protected const int c_VariableKind_Exception = 2;
        protected const int c_VariableKind_Temporary = 3;

        //
        // State
        //

        protected DebugInfo m_debugInfo;
        protected int       m_number;

        //
        // Constructor Methods
        //

        protected VariableExpression( TypeRepresentation type      ,
                                      DebugInfo          debugInfo ) : base( type )
        {
            m_debugInfo = debugInfo;
            m_number    = -1;
        }

        //--//

        //
        // Helper Methods
        //

        public static Comparison< VariableExpression > GetSorter()
        {
            return SortVariables;
        }

        private static int SortVariables( VariableExpression x ,
                                          VariableExpression y )
        {
            int xKind = x.GetVariableKind();
            int yKind = y.GetVariableKind();

            if(xKind < yKind) return -1;
            if(xKind > yKind) return  1;

            return x.Number - y.Number;
        }

        public abstract int GetVariableKind();

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_number    );
            context.Transform( ref m_debugInfo );

            context.Pop();
        }

        //--//

        public static VariableExpression ExtractAliased( Expression ex )
        {
            VariableExpression var = ex as VariableExpression;
            if(var != null)
            {
                return var.AliasedVariable;
            }

            return null;
        }
 
        public static VariableExpression[] ToArray( VariableExpression ex )
        {
            return ex != null ? new VariableExpression[] { ex } : SharedEmptyArray;
        }

        //
        // Access Methods
        //
        public IInliningPathAnnotation InliningPath { get; set; }

        public DebugInfo DebugName
        {
            get
            {
                return m_debugInfo;
            }
        }

        public int Number
        {
            get
            {
                return m_number;
            }

            set
            {
                m_number = value;
            }
        }

        public override CanBeNull CanBeNull
        {
            get
            {
                return CanBeNull.Unknown;
            }
        }

        public override bool CanTakeAddress
        {
            get
            {
                return true;
            }
        }

        public virtual VariableExpression AliasedVariable
        {
            get
            {
                return this;
            }
        }
 
        public bool SkipReferenceCounting
        {
            get; set;
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            string name;
            string fmt;

            if (m_number >= 0)
            {
                sb.Append( m_number );
            }

            if(m_index >= 0)
            {
                sb.AppendFormat( "_{0}", m_index );
            }

            AppendIdentity( sb );

            if(m_debugInfo != null)
            {
                name = m_debugInfo.Name;
            }
            else
            {
                name = null;
            }

            if(name != null)
            {
                fmt = "({0} {1})";
            }
            else
            {
                fmt = "({0})";
            }

            sb.AppendFormat( fmt, m_type.FullNameWithAbbreviation, name );
        }
    }
}