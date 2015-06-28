//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StackLocationExpression : LowLevelVariableExpression
    {
        public enum Placement
        {
            In   ,
            Local,
            Out  ,
        }

        //--//

        //
        // State
        //

        private Placement m_placement;
        private uint      m_allocationOffset;

        //
        // Constructor Methods
        //

        internal StackLocationExpression( TypeRepresentation           type         ,
                                          VariableExpression.DebugInfo debugInfo    ,
                                          int                          number       ,
                                          Placement                    placement    ,
                                          VariableExpression           sourceVar    ,
                                          uint                         sourceOffset ) : base( type, debugInfo, sourceVar, sourceOffset )
        {
            m_number           = number;
            m_placement        = placement;
            m_allocationOffset = uint.MaxValue;
        }

        //
        // Equality Methods
        //

        public override bool IsTheSamePhysicalEntity( Expression ex )
        {
            if(base.IsTheSamePhysicalEntity( ex ))
            {
                return true;
            }

            VariableExpression var = ex as VariableExpression;
            if(var != null)
            {
                StackLocationExpression stack = var.AliasedVariable as StackLocationExpression;
                if(stack != null)
                {
                    if(this.m_number    == stack.m_number    &&
                       this.m_placement == stack.m_placement  )
                    {
                        //
                        // Different variables, but they are both assigned to the same stack location.
                        //
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool IsTheSameAggregate( Expression ex )
        {
            if(base.IsTheSameAggregate( ex ))
            {
                return true;
            }

            if(this.SourceVariable != null)
            {
                VariableExpression var = ex as VariableExpression;
                if(var != null)
                {
                    StackLocationExpression stack = var.AliasedVariable as StackLocationExpression;
                    if(stack != null)
                    {
                        if(this.SourceVariable == stack.SourceVariable)
                        {
                            //
                            // Different variables, but they are both assigned to the same stack location.
                            //
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            ControlFlowGraphStateForCodeTransformation cfg       = (ControlFlowGraphStateForCodeTransformation)context.ControlFlowGraphDestination;
            TypeRepresentation                         td        =                     context.ConvertType( m_type );
            VariableExpression                         sourceVar = (VariableExpression)context.Clone( m_sourceVar );

            return RegisterAndCloneState( context, cfg.AllocateStackLocation( td, m_debugInfo, m_number, m_placement, sourceVar, m_sourceOffset ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_placement        );
            context2.Transform( ref m_allocationOffset );

            context2.Pop();
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return Operator.OperatorLevel.StackLocations;
        }

        public override int GetVariableKind()
        {
            return c_VariableKind_StackLocation + (int)m_placement;
        }

        //--//

        //
        // Access Methods
        //

        public Placement StackPlacement
        {
            get
            {
                return m_placement;
            }
        }

        public uint AllocationOffset
        {
            get
            {
                return m_allocationOffset;
            }

            set
            {
                m_allocationOffset = value;
            }
        }


        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "$Stack{0}_", this.StackPlacement );

            base.InnerToString( sb );

            if(m_allocationOffset != uint.MaxValue)
            {
                sb.AppendFormat( "[0x{0:X8}]", m_allocationOffset );
            }

            AppendOffsetInfo( sb );
        }
    }
}
