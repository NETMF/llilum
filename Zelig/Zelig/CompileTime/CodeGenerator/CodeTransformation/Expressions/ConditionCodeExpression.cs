//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ConditionCodeExpression : LowLevelVariableExpression
    {
        public enum Comparison : byte
        {
            Equal                   ,
            NotEqual                ,

            Negative                ,
            PositiveOrZero          ,
            Overflow                ,
            NoOverflow              ,

            UnsignedHigherThanOrSame,
            UnsignedLowerThan       ,
            UnsignedHigherThan      ,
            UnsignedLowerThanOrSame ,

            SignedGreaterThanOrEqual,
            SignedLessThan          ,
            SignedGreaterThan       ,
            SignedLessThanOrEqual   ,

            Always                  ,
            NotValid                ,
        }

        internal class ConstantResult : ConstantExpression.DelayedValue,
            ITransformationContextTarget
        {
            //
            // State
            //

            object m_left;
            object m_right;

            //
            // Constructor Methods
            //

            internal ConstantResult( object left  ,
                                     object right )
            {
                m_left  = left;
                m_right = right;
            }

            //--//

            //
            // MetaDataEquality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is ConstantResult)
                {
                    ConstantResult other = (ConstantResult)obj;

                    if(m_left  == other.m_left  &&
                       m_right == other.m_right  )
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return m_left.GetHashCode();
            }

            //
            // Helper Methods
            //

            //--//

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.TransformGeneric( ref m_left  );
                context.TransformGeneric( ref m_right );

                context.Pop();
            }

            //--//

            //
            // Access Methods
            //

            public override bool CanEvaluate
            {
                get
                {
                    return false;
                }
            }

            public override object Value
            {
                get
                {
                    return null;
                }
            }

            public object LeftValue
            {
                get
                {
                    return m_left;
                }
            }

            public object RightValue
            {
                get
                {
                    return m_right;
                }
            }

            //--//
            
            //
            // Debug Methods
            //

            public override string ToString()
            {
                return string.Format( "<comparison of {0} and {1}>", m_left, m_right );
            }
        }

        //--//

        //
        // Constructor Methods
        //

        internal ConditionCodeExpression( TypeRepresentation type         ,
                                          VariableExpression sourceVar    ,
                                          uint               sourceOffset ) : base( type, null, sourceVar, sourceOffset )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)context.ControlFlowGraphDestination;

            return RegisterAndCloneState( context, cfg.AllocateConditionCode() );
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return Operator.OperatorLevel.Registers;
        }

        public override int GetVariableKind()
        {
            return c_VariableKind_Condition;
        }

        //--//

        public static Comparison NegateCondition( Comparison cond )
        {
            switch(cond)
            {
                case Comparison.Equal                   : cond = Comparison.NotEqual                ; break;
                case Comparison.NotEqual                : cond = Comparison.Equal                   ; break;
                                                                            
                case Comparison.Negative                : cond = Comparison.PositiveOrZero          ; break;
                case Comparison.PositiveOrZero          : cond = Comparison.Negative                ; break;
                case Comparison.Overflow                : cond = Comparison.NoOverflow              ; break;
                case Comparison.NoOverflow              : cond = Comparison.Overflow                ; break;
                                                                            
                case Comparison.UnsignedHigherThanOrSame: cond = Comparison.UnsignedLowerThan       ; break;
                case Comparison.UnsignedLowerThan       : cond = Comparison.UnsignedHigherThanOrSame; break;
                case Comparison.UnsignedHigherThan      : cond = Comparison.UnsignedLowerThanOrSame ; break;
                case Comparison.UnsignedLowerThanOrSame : cond = Comparison.UnsignedHigherThan      ; break;
                                                                            
                case Comparison.SignedGreaterThanOrEqual: cond = Comparison.SignedLessThan          ; break;
                case Comparison.SignedLessThan          : cond = Comparison.SignedGreaterThanOrEqual; break;
                case Comparison.SignedGreaterThan       : cond = Comparison.SignedLessThanOrEqual   ; break;
                case Comparison.SignedLessThanOrEqual   : cond = Comparison.SignedGreaterThan       ; break;
            }

            return cond;
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

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "$CC_" );

            base.InnerToString( sb );
        }
    }
}