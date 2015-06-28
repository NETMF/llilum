//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PhiVariableExpression : VariableExpression
    {
        public static new readonly PhiVariableExpression[] SharedEmptyArray = new PhiVariableExpression[0];

        //
        // State
        //

        private VariableExpression m_target;
        private int                m_version;

        //
        // Constructor Methods
        //

        internal PhiVariableExpression( VariableExpression target ) : base( target.Type, target.DebugName )
        {
            m_target  = target;
            m_version = -1;
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

            return m_target.IsTheSamePhysicalEntity( ex );
        }

        public override bool IsTheSameAggregate( Expression ex )
        {
            if(base.IsTheSameAggregate( ex ))
            {
                return true;
            }

            return m_target.IsTheSameAggregate( ex );
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)context.ControlFlowGraphDestination;

            return RegisterAndCloneState( context, cfg.AllocatePhiVariable( null ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Expression     clone   )
        {
            PhiVariableExpression clone2 = (PhiVariableExpression)clone;

            clone2.m_target = (PhiVariableExpression)context.Clone( m_target );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_target );

            context.Pop();
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return m_target.GetLevel( helper );
        } 

        public override int GetVariableKind()
        {
            //
            // Just a heuristic to cluster together phi variables, no guarantee it's going to achieve the goal.
            //
            return 100 + (1024 * 1024) * m_target.GetVariableKind() + m_target.Number;
        }

        //--//

        //
        // Access Methods
        //

        public VariableExpression Target
        {
            get
            {
                return m_target;
            }
        }

        public int Version
        {
            get
            {
                return m_version;
            }

            internal set
            {
                m_version = value;
            }
        }

        public override CanBeNull CanBeNull
        {
            get
            {
                return m_target.CanBeNull;
            }
        }

        public override bool CanTakeAddress
        {
            get
            {
                return m_target.CanTakeAddress;
            }
        }

        public override VariableExpression AliasedVariable
        {
            get
            {
                return m_target;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "Phi'{0}", m_version );

            AppendIdentity( sb );

            sb.Append( "<" );

            m_target.InnerToString( sb );

            sb.Append( ">" );
        }
    }
}