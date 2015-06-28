//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ConditionalCompareOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;
        //
        // State
        //

        private ConditionCodeExpression.Comparison m_condition;

        //
        // Constructor Methods
        //

        private ConditionalCompareOperator( Debugging.DebugInfo                debugInfo ,
                                            ConditionCodeExpression.Comparison condition ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_condition = condition;
        }

        //--//

        public static ConditionalCompareOperator New( Debugging.DebugInfo                debugInfo ,
                                                      ConditionCodeExpression.Comparison condition ,
                                                      VariableExpression                 lhs       ,
                                                      Expression                         rhsLeft   ,
                                                      Expression                         rhsRight  ,
                                                      VariableExpression                 rhsCC     )
        {
            CHECKS.ASSERT( lhs  .AliasedVariable is ConditionCodeExpression, "Expecting a ConditionCodeExpression for the 'lhs' parameter, got '{0}'"  , lhs   );
            CHECKS.ASSERT( rhsCC.AliasedVariable is ConditionCodeExpression, "Expecting a ConditionCodeExpression for the 'rhsCC' parameter, got '{0}'", rhsCC );

            ConditionalCompareOperator res = new ConditionalCompareOperator( debugInfo, condition );

            res.SetLhs( lhs                      );
            res.SetRhs( rhsLeft, rhsRight, rhsCC );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ConditionalCompareOperator( m_debugInfo, m_condition ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_condition );

            context2.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ConditionCodeExpression.Comparison Condition
        {
            get
            {
                return m_condition;
            }

            set
            {
                m_condition = value;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "ConditionalCompareOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = compare {1} <-> {2} if {3} is {4}", this.FirstResult, this.FirstArgument, this.SecondArgument, this.ThirdArgument, m_condition );
        }
   }
}