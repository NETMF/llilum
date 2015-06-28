//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class PhysicalRegisterExpression : LowLevelVariableExpression
    {
        //
        // State
        //

        protected Abstractions.RegisterDescriptor m_regDesc;

        //
        // Constructor Methods
        //

        internal PhysicalRegisterExpression( TypeRepresentation              type         ,
                                             Abstractions.RegisterDescriptor regDesc      ,
                                             VariableExpression.DebugInfo    debugInfo    ,
                                             VariableExpression              sourceVar    ,
                                             uint                            sourceOffset ) : base( type, debugInfo, sourceVar, sourceOffset )
        {
            m_number  = regDesc.Index;
            m_regDesc = regDesc;
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

            PhysicalRegisterExpression reg = Extract( ex );
            if(reg != null)
            {
                if(this.RegisterDescriptor == reg.RegisterDescriptor)
                {
                    //
                    // Different variables, but they are both assigned to the same physical register.
                    //
                    return true;
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

            PhysicalRegisterExpression reg = Extract( ex );
            if(reg != null)
            {
                if(this.RegisterDescriptor == reg.RegisterDescriptor)
                {
                    //
                    // Different variables, but they are both assigned to the same physical register.
                    //
                    return true;
                }

                //
                // Are they connected by interference?
                //
                foreach(var interference in this.RegisterDescriptor.InterfersWith)
                {
                    if(interference == reg.RegisterDescriptor)
                    {
                        return true;
                    }
                }

                foreach(var interference in reg.RegisterDescriptor.InterfersWith)
                {
                    if(interference == this.RegisterDescriptor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static PhysicalRegisterExpression Extract( Expression ex )
        {
            VariableExpression var = ex as VariableExpression;

            if(var != null)
            {
                return var.AliasedVariable as PhysicalRegisterExpression;
            }

            return null;
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)context.ControlFlowGraphDestination;

            return RegisterAndCloneState( context, cfg.AllocatePhysicalRegister( m_regDesc ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_regDesc );

            context2.Pop();
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return Operator.OperatorLevel.Registers;
        }

        public override int GetVariableKind()
        {
            return c_VariableKind_Physical;
        }

        //--//

        //
        // Access Methods
        //

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
                return false;
            }
        }

        public Abstractions.RegisterDescriptor RegisterDescriptor
        {
            get
            {
                return m_regDesc;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "$PhyReg__{0}__", m_regDesc.Mnemonic );

            base.InnerToString( sb );

            AppendOffsetInfo( sb );
        }
    }
}