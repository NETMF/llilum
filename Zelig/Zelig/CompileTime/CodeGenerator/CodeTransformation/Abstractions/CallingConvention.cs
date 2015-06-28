//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class CallingConvention : ITransformationContextTarget
    {
        public enum Direction
        {
            Caller,
            Callee,
        }

        public abstract class CallState : ITransformationContextTarget
        {
            //
            // State
            //

            private Direction m_direction;

            //
            // Constructor Methods
            //

            protected CallState() // Default constructor required by TypeSystemSerializer.
            {
            }

            protected CallState( Direction direction )
            {
                m_direction = direction;
            }

            //
            // Helper Methods
            //

            public abstract RegisterDescriptor GetNextRegister( Platform                                                  platform   ,
                                                                TypeRepresentation                                        sourceTd   ,
                                                                TypeRepresentation                                        fragmentTd ,
                                                                ControlFlowGraphStateForCodeTransformation.KindOfFragment kind       );

            public abstract int GetNextIndex( Platform                                                  platform   ,
                                              TypeRepresentation                                        sourceTd   ,
                                              TypeRepresentation                                        fragmentTd ,
                                              ControlFlowGraphStateForCodeTransformation.KindOfFragment kind       );

            public abstract bool CanMapToRegister( Platform           platform ,
                                                   TypeRepresentation td       );

            public abstract bool CanMapResultToRegister( Platform           platform ,
                                                         TypeRepresentation td       );

            public virtual void ApplyTransformation( TransformationContext context )
            {
                TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

                context2.Push( this );

                context2.Transform( ref m_direction );
    
                context2.Pop();
            }

            //
            // Access Methods
            //

            public Direction Direction
            {
                get
                {
                    return m_direction;
                }
            }
        }


        //
        // State
        //

        protected TypeSystemForCodeTransformation m_typeSystem;

        //
        // Constructor Methods
        //

        protected CallingConvention() // Default constructor required by TypeSystemSerializer.
        {
        }

        protected CallingConvention( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
        }

        //
        // Helper Methods
        //

        public virtual void ApplyTransformation( TransformationContext context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            context2.Transform( ref m_typeSystem );

            context2.Pop();
        }

        //--//

        public abstract void RegisterForNotifications( TypeSystemForCodeTransformation  ts    ,
                                                       CompilationSteps.DelegationCache cache );

        public virtual void ExpandCallsClosure( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
        }

        //--//

        public Expression[] AssignArgument(     ControlFlowGraphStateForCodeTransformation cfg         ,
                                                Operator                                   insertionOp ,
                                                Expression                                 ex          ,
                                                Abstractions.CallingConvention.CallState   callState   )
        {
            return AssignArgument( cfg, insertionOp, ex, ex.Type, callState );
        }

        public Expression[] AssignReturnValue( ControlFlowGraphStateForCodeTransformation cfg           ,
                                               Operator                                   insertionOp   ,
                                               VariableExpression                         exReturnValue ,
                                               Abstractions.CallingConvention.CallState   callState     )
        {
            if(exReturnValue != null)
            {
                return AssignReturnValue( cfg, insertionOp, exReturnValue, exReturnValue.Type, callState );
            }
            else
            {
                return null;
            }
        }

        //--//

        public abstract CallState CreateCallState( Direction direction );

        public abstract Expression[] AssignArgument( ControlFlowGraphStateForCodeTransformation cfg         ,
                                                     Operator                                   insertionOp ,
                                                     Expression                                 ex          ,
                                                     TypeRepresentation                         td          ,
                                                     Abstractions.CallingConvention.CallState   callState   );

        public abstract Expression[] AssignReturnValue( ControlFlowGraphStateForCodeTransformation cfg           ,
                                                        Operator                                   insertionOp   ,
                                                        VariableExpression                         exReturnValue ,
                                                        TypeRepresentation                         tdReturnValue ,
                                                        Abstractions.CallingConvention.CallState   callState     );

        public abstract VariableExpression[] CollectExpressionsToInvalidate( ControlFlowGraphStateForCodeTransformation cfg          ,
                                                                             CallOperator                               op           ,
                                                                             Expression[]                               resFragments );

        public abstract bool ShouldSaveRegister( RegisterDescriptor regDesc );

        //--//

        protected static VariableExpression[] MergeFragments( VariableExpression[] exDstArray ,
                                                              Expression[]         exSrcArray )
        {
            if(exSrcArray != null)
            {
                foreach(Expression exSrc in exSrcArray)
                {
                    VariableExpression var = exSrc as VariableExpression;
                    if(var != null)
                    {
                        exDstArray = AddUniqueToFragment( exDstArray, var );
                    }
                }
            }

            return exDstArray;
        }

        protected static VariableExpression[] AddUniqueToFragment( VariableExpression[] array ,
                                                                   VariableExpression   var   )
        {
            for(int i = array.Length; --i >= 0; )
            {
                if(array[i].IsTheSamePhysicalEntity( var ))
                {
                    return array;
                }
            }

            return ArrayUtility.AppendToNotNullArray( array, var );
        }

        protected static VariableExpression[] CollectAddressTakenExpressionsToInvalidate( ControlFlowGraphStateForCodeTransformation cfg     ,
                                                                                          VariableExpression[]                       res     ,
                                                                                          Operator                                   context )
        {
            VariableExpression[]          variables = cfg.DataFlow_SpanningTree_Variables;
            VariableExpression.Property[] varProps  = cfg.DataFlow_PropertiesOfVariables;

            for(int varIdx = 0; varIdx < variables.Length; varIdx++)
            {
                if((varProps[varIdx] & VariableExpression.Property.AddressTaken) != 0)
                {
                    VariableExpression var       = variables[varIdx];
                    Expression[]       fragments = cfg.GetFragmentsForExpression( var );

                    if(fragments != null)
                    {
                        res = MergeFragments( res, fragments );
                    }
                    else
                    {
                        res = AddUniqueToFragment( res, var );
                    }
                }
            }

            return res;
        }
    }
}
