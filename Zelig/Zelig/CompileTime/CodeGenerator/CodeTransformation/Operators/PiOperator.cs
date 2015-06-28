//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PiOperator : Operator
    {
        public enum Relation
        {
            Invalid                 ,

            Equal                   ,
            NotEqual                ,

            UnsignedHigherThanOrSame,
            UnsignedLowerThan       ,
            UnsignedHigherThan      ,
            UnsignedLowerThanOrSame ,

            SignedGreaterThanOrEqual,
            SignedLessThan          ,
            SignedGreaterThan       ,
            SignedLessThanOrEqual   ,
        }

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

        private Relation m_relation;

        //
        // Constructor Methods
        //

        private PiOperator( Debugging.DebugInfo debugInfo   ,
                            Relation            cmpRelation ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_relation = cmpRelation;
        }

        //--//

        public static PiOperator New( Debugging.DebugInfo debugInfo   ,
                                      VariableExpression  lhs         ,
                                      VariableExpression  rhs         ,
                                      Expression          cmpLeft     ,
                                      Expression          cmpRight    ,
                                      Relation            cmpRelation )
        {
            PiOperator res = new PiOperator( debugInfo, cmpRelation );

            res.SetLhs( lhs );
            res.SetRhs( rhs, cmpLeft, cmpRight );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new PiOperator( m_debugInfo, m_relation ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_relation );

            context2.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            //
            // We create Pi operators to say that input and output have some differences, so don't copy propagate or we'll lose that bit of knowledge!
            //
            return false;
        }

        //--//

        public static Relation ConvertRelation( ConditionCodeExpression.Comparison comparison )
        {
            switch(comparison)
            {
                case ConditionCodeExpression.Comparison.Equal                   : return Relation.Equal                   ;
                case ConditionCodeExpression.Comparison.NotEqual                : return Relation.NotEqual                ;
                case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: return Relation.UnsignedHigherThanOrSame;
                case ConditionCodeExpression.Comparison.UnsignedLowerThan       : return Relation.UnsignedLowerThan       ;
                case ConditionCodeExpression.Comparison.UnsignedHigherThan      : return Relation.UnsignedHigherThan      ;
                case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : return Relation.UnsignedLowerThanOrSame ;
                case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: return Relation.SignedGreaterThanOrEqual;
                case ConditionCodeExpression.Comparison.SignedLessThan          : return Relation.SignedLessThan          ;
                case ConditionCodeExpression.Comparison.SignedGreaterThan       : return Relation.SignedGreaterThan       ;
                case ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : return Relation.SignedLessThanOrEqual   ;
            }

            return Relation.Invalid;
        }

        public static Relation NegateRelation( Relation relation )
        {
            switch(relation)
            {
                case Relation.Equal                   : return Relation.NotEqual                ;
                case Relation.NotEqual                : return Relation.Equal                   ;
                case Relation.UnsignedHigherThanOrSame: return Relation.UnsignedLowerThan       ;
                case Relation.UnsignedLowerThan       : return Relation.UnsignedHigherThanOrSame;
                case Relation.UnsignedHigherThan      : return Relation.UnsignedLowerThanOrSame ;
                case Relation.UnsignedLowerThanOrSame : return Relation.UnsignedHigherThan      ;
                case Relation.SignedGreaterThanOrEqual: return Relation.SignedLessThan          ;
                case Relation.SignedLessThan          : return Relation.SignedGreaterThanOrEqual;
                case Relation.SignedGreaterThan       : return Relation.SignedLessThanOrEqual   ;
                case Relation.SignedLessThanOrEqual   : return Relation.SignedGreaterThan       ;
            }

            return Relation.Invalid;
        }

        public static bool SameAnnotation( PiOperator pi1 ,
                                           PiOperator pi2 )
        {
            if(pi1.LeftExpression   == pi2.LeftExpression   &&
               pi1.RightExpression  == pi2.RightExpression  &&
               pi1.RelationOperator == pi2.RelationOperator  )
            {
                return true;
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public Expression LeftExpression
        {
            get
            {
                return this.SecondArgument;
            }
        }

        public Expression RightExpression
        {
            get
            {
                return this.ThirdArgument;
            }
        }

        public Relation RelationOperator
        {
            get
            {
                return m_relation;
            }
        }

        //--//

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                VariableExpression left  = this.LeftExpression  as VariableExpression;
                VariableExpression right = this.RightExpression as VariableExpression;
                VariableExpression src   = this.FirstArgument   as VariableExpression; 

                //
                // If it's a relation between two variables, we should keep it.
                // If it's a ralation between a variable and a constant, we can remove it.
                //
                return (src == left  && right != null) ||
                       (src == right && left  != null);
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "PiOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = pi( {1} ) [{2} {3} {4}]", this.FirstResult, this.FirstArgument, this.LeftExpression, m_relation, this.RightExpression );
        }
    }
}