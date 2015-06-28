//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation
    {
        const string MappedToMachine = "VariablesMappedToMachine";

        public class ValueFragment : ITransformationContextTarget
        {
            //
            // State
            //

            private object m_target;
            private uint   m_offset;

            //
            // Constructor Methods
            //

            internal ValueFragment( object target ,
                                    uint   offset )
            {
                m_target = target;
                m_offset = offset;
            }

            //
            // Helper Methods
            //

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_target );
                context.Transform( ref m_offset );

                context.Pop();
            }

            //--//

            //
            // Access Methods
            //

            public uint Value
            {
                get
                {
                    return DataConversion.GetFragmentOfNumber( m_target, (int)m_offset );
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return string.Format( "<fragment {0} of {1}>", m_offset, m_target );
            }
        }
        
        //
        // State
        //

        private Abstractions.CallingConvention.CallState m_callState;

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

        private void ApplyTransformation_MapToMachine( TransformationContextForCodeTransformation context )
        {
            context.TransformGeneric( ref m_callState );
        }

        //--//--//--//--//--//--//--//

        internal void MapVariables()
        {
            if(SetProperty( MappedToMachine ) == false)
            {
                m_callState = m_typeSystem.CallingConvention.CreateCallState( Abstractions.CallingConvention.Direction.Callee );

                Operator insertionOp = this.GetInjectionPoint( BasicBlock.Qualifier.PrologueEnd ).FirstOperator;

                MapIncomingArgumentsBasedOnCallingConvention( insertionOp          );
                MapTheRestOfTheVariables                    ( insertionOp          );
                RemoveOldArgumentValueOperators             ( this.EntryBasicBlock );

                AddInvalidateOperatorsToExceptionHandlers();
            }
        }

        //--//--//--//--//--//--//--//

        private void MapIncomingArgumentsBasedOnCallingConvention( Operator insertionOp )
        {
            Abstractions.CallingConvention cc = m_typeSystem.CallingConvention;

            Expression[] fragments = cc.AssignReturnValue( this, insertionOp, m_returnValue, m_callState );
            if(fragments != null)
            {
                SetFragmentsForExpression( m_returnValue, fragments );
            }

            ///
            /// Do not add 'this' pointer as part of the incoming arguments for exported methods
            ///
            bool fAddCurrentArg = 0 == (this.Method.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Exported);

            foreach(VariableExpression exArg in m_arguments)
            {
                if(fAddCurrentArg)
                {
                    SetFragmentsForExpression( exArg, cc.AssignArgument( this, insertionOp, exArg, m_callState ) );
                }

                fAddCurrentArg = true;
            }
        }

        private void MapTheRestOfTheVariables( Operator insertingOp )
        {
            VariableExpression         [] vars     = this.DataFlow_SpanningTree_Variables;
            VariableExpression.Property[] varProps = this.DataFlow_PropertiesOfVariables;

            for(int i = 0; i < vars.Length; i++)
            {
                VariableExpression var = vars[i];

                if(m_typeSystem.GetLevel( var ) <= Operator.OperatorLevel.ScalarValues)
                {
                    //
                    // Only high-level expressions need to be mapped.
                    //
                    continue;
                }

                Expression[] fragments = GetFragmentsForExpression( var );
                Expression[] fragmentsNew;

                //
                // Any variable that is bigger than 64 bits or whose address is taken must be allocated on the stack.
                //
                if((varProps[i] & VariableExpression.Property.AddressTaken) != 0 || var.Type.SizeOfHoldingVariableInWords > 2)
                {
                    bool fAllocate;

                    if(fragments == null)
                    {
                        fAllocate = true;
                    }
                    else
                    {
                        fAllocate = false;

                        foreach(Expression fragment in fragments)
                        {
                            if(!(fragment is StackLocationExpression))
                            {
                                fAllocate = true;
                                break;
                            }
                        }
                    }

                    if(fAllocate)
                    {
                        fragmentsNew = MapExpressionToFragments( null, var.DebugName, var, m_callState, KindOfFragment.AllocateStackLocalLocation );
                    }
                    else
                    {
                        fragmentsNew = null;
                    }
                }
                else if(fragments == null)
                {
                    fragmentsNew = MapExpressionToFragments( null, var.DebugName, var, m_callState, KindOfFragment.AllocatePseudoRegister );
                }
                else
                {
                    fragmentsNew = null;
                }

                if(fragmentsNew != null)
                {
                    SetFragmentsForExpression( var, fragmentsNew );

                    if(fragments != null)
                    {
                        CHECKS.ASSERT( fragments.Length == fragmentsNew.Length, "Cannot copy between different-sized expressions" );

                        //
                        // Insert assignment ops for all the variables that have been reassigned.
                        //

                        for(int fragIdx = 0; fragIdx < fragments.Length; fragIdx++)
                        {
                            Expression src = fragments   [fragIdx];
                            Expression dst = fragmentsNew[fragIdx];

                            if(src != dst)
                            {
                                insertingOp.AddOperatorBefore( SingleAssignmentOperator.New( null, (VariableExpression)dst, src ) );
                            }
                        }
                    }
                }
            }
        }

        private void RemoveOldArgumentValueOperators( BasicBlock bb )
        {
            //
            // Arguments are passed through an invalidation operator, so that they appear always assigned to.
            // We need to remove these operators, since now we have equivalent ones for the fragments.
            //
            foreach(Operator op in bb.Operators)
            {
                if(op is InitialValueOperator)
                {
                    if(ArrayUtility.FindReferenceInNotNullArray( m_arguments, op.FirstResult ) >= 0)
                    {
                        op.Delete();
                    }
                }
            }
        }

        private void AddInvalidateOperatorsToExceptionHandlers()
        {
            foreach(BasicBlock bb in m_basicBlocks)
            {
                if(bb is ExceptionHandlerBasicBlock)
                {
                    var op = NopOperator.New( null );

                    bb.FirstOperator.AddOperatorBefore( op );

                    foreach(VariableExpression exToInvalidate in m_typeSystem.CallingConvention.CollectExpressionsToInvalidate( this, null, null ))
                    {
                        op.AddAnnotation( PostInvalidationAnnotation.Create( m_typeSystem, exToInvalidate ) );
                    }
                }
            }
        }

        //--//--//--//--//--//--//--//

        public enum KindOfFragment
        {
            CopyIncomingArgumentFromPhysicalToPseudoRegister,
            CopyIncomingArgumentFromStackToPseudoRegister   ,
            AllocatePhysicalRegisterForArgument             ,
            AllocatePhysicalRegisterForReturnValue          ,
            AllocatePseudoRegister                          ,
            AllocateStackInLocation                         ,
            AllocateStackLocalLocation                      ,
            AllocateStackOutLocation                        ,
        }

        public Expression[] MapExpressionToFragments( Operator                                 insertionOp ,
                                                      VariableExpression.DebugInfo             debugInfo   ,
                                                      VariableExpression                       sourceVar   ,
                                                      Abstractions.CallingConvention.CallState callState   ,
                                                      KindOfFragment                           kind        )
        {
            return MapExpressionToFragments( insertionOp, debugInfo, sourceVar, sourceVar.Type, callState, kind );
        }

        public Expression[] MapExpressionToFragments( Operator                                 insertionOp ,
                                                      VariableExpression.DebugInfo             debugInfo   ,
                                                      VariableExpression                       sourceVar   ,
                                                      TypeRepresentation                       sourceTd    ,
                                                      Abstractions.CallingConvention.CallState callState   ,
                                                      KindOfFragment                           kind        )
        {
            uint sourceFragments; 
            bool fDirect;

            m_typeSystem.PlatformAbstraction.ComputeNumberOfFragmentsForExpression( sourceTd, kind, out sourceFragments, out fDirect );

            Expression[]          res       = new Expression[sourceFragments];
            TypeRepresentation    genericTd = m_typeSystem.WellKnownTypes.System_UInt32;
            Abstractions.Platform pa        = m_typeSystem.PlatformAbstraction;


            for(uint offset = 0; offset < sourceFragments; offset++)
            {
                uint               sourceOffset = offset * sizeof(uint);
                TypeRepresentation fragmentTd;
                Expression         exFragment;
      
                if(fDirect)
                {
                    fragmentTd = sourceTd;
                }
                else if(sourceTd is ScalarTypeRepresentation && sourceFragments == 1)
                {
                    fragmentTd = sourceTd;
                }
                else
                {
                    fragmentTd = genericTd;

                    InstanceFieldRepresentation fd = sourceTd.FindFieldAtOffset( (int)sourceOffset );
                    if(fd != null)
                    {
                        TypeRepresentation fieldTd = fd.FieldType;

                        if(fieldTd.SizeOfHoldingVariable == genericTd.SizeOfHoldingVariable)
                        {
                            fragmentTd = fieldTd;
                        }
                    }
                }

                switch(kind)
                {
                    case KindOfFragment.CopyIncomingArgumentFromPhysicalToPseudoRegister:
                        {
                            exFragment = AllocatePseudoRegister( fragmentTd, debugInfo, sourceVar, sourceOffset );

                            //
                            // Invalidate the physical register and then copy it to pseudo one.
                            //
                            TypedPhysicalRegisterExpression exReg = AllocateTypedPhysicalRegister( fragmentTd, callState.GetNextRegister( pa, sourceTd, fragmentTd, kind ), debugInfo, sourceVar, sourceOffset );

                            var opNew = SingleAssignmentOperator.New( insertionOp.DebugInfo, (VariableExpression)exFragment, exReg );
                            insertionOp.AddOperatorBefore( opNew );

                            //
                            // Don't copy propagate the input registers.
                            //
                            opNew.AddAnnotation( BlockCopyPropagationAnnotation.Create( this.TypeSystem, 0, false, false ) );

                            //
                            // This pointer is always not null.
                            //
                            if(this.Method is InstanceMethodRepresentation && sourceVar == this.Arguments[0])
                            {
                                opNew.AddAnnotation( NotNullAnnotation.Create( this.TypeSystem ) );
                            }

                            var opTarget = GetTarget( insertionOp );
                            opTarget.AddOperatorBefore( InitialValueOperator.New( insertionOp.DebugInfo, exReg ) );
                        }
                        break;

                    case KindOfFragment.CopyIncomingArgumentFromStackToPseudoRegister:
                        {
                            exFragment = AllocatePseudoRegister( fragmentTd, debugInfo, sourceVar, sourceOffset );

                            StackLocationExpression exStack = AllocateStackLocation( fragmentTd, debugInfo, callState.GetNextIndex( pa, sourceTd, fragmentTd, kind ), StackLocationExpression.Placement.In, sourceVar, sourceOffset );

                            insertionOp.AddOperatorBefore( SingleAssignmentOperator.New( insertionOp.DebugInfo, (VariableExpression)exFragment, exStack ) );

                            Operator opTarget = GetTarget( insertionOp );
                            opTarget.AddOperatorBefore( InitialValueOperator.New( insertionOp.DebugInfo, exStack ) );
                        }
                        break;

                    case KindOfFragment.AllocatePhysicalRegisterForArgument:
                    case KindOfFragment.AllocatePhysicalRegisterForReturnValue:
                        exFragment = AllocateTypedPhysicalRegister( fragmentTd, callState.GetNextRegister( pa, sourceTd, fragmentTd, kind ), debugInfo, sourceVar, sourceOffset );
                        break;

                    case KindOfFragment.AllocatePseudoRegister:
                        exFragment = AllocatePseudoRegister( fragmentTd, debugInfo, sourceVar, sourceOffset );
                        break;

                    case KindOfFragment.AllocateStackInLocation:
                    case KindOfFragment.AllocateStackLocalLocation:
                    case KindOfFragment.AllocateStackOutLocation:
                        {
                            StackLocationExpression.Placement stackPlacement;

                            if     (kind == KindOfFragment.AllocateStackInLocation   ) stackPlacement = StackLocationExpression.Placement.In;
                            else if(kind == KindOfFragment.AllocateStackLocalLocation) stackPlacement = StackLocationExpression.Placement.Local;
                            else                                                       stackPlacement = StackLocationExpression.Placement.Out;

                            StackLocationExpression exStack = AllocateStackLocation( fragmentTd, debugInfo, callState.GetNextIndex( pa, sourceTd, fragmentTd, kind ), stackPlacement, sourceVar, sourceOffset );

                            if(kind == KindOfFragment.AllocateStackInLocation)
                            {
                                Operator opTarget = GetTarget( insertionOp );
                                opTarget.AddOperatorBefore( InitialValueOperator.New( insertionOp.DebugInfo, exStack ) );
                            }

                            exFragment = exStack;
                        }
                        break;

                    default:
                        exFragment = null;
                        break;
                }

                res[offset] = exFragment;
            }

            return res;
        }

        private static Operator GetTarget( Operator op )
        {
            return op.BasicBlock.Owner.EntryBasicBlock.GetFirstDifferentOperator( typeof(InitialValueOperator) );
        }

        //--//

        public Expression[][] GetFragmentsForExpressionArray( Expression[] exArray )
        {
            var res = new Expression[exArray.Length][];

            for(int i = 0; i < exArray.Length; i++)
            {
                res[i] = GetFragmentsForExpression( exArray[i] );
            }

            return res;
        }

        public Expression[] GetFragmentsForExpression( Expression ex )
        {
            Expression[] res;

            if(m_lookupFragments.TryGetValue( ex, out res ) == false)
            {
                if(ex is LowLevelVariableExpression)
                {
                    res = new Expression[] { ex };

                    SetFragmentsForExpression( ex, res );
                }
                else if(ex is ConstantExpression)
                {
                    ConstantExpression exConst = (ConstantExpression)ex;
                    TypeRepresentation td      = exConst.Type;
                    object             val     = exConst.Value;
                    uint               sourceFragments; 
                    bool               fDirect;

                    m_typeSystem.PlatformAbstraction.ComputeNumberOfFragmentsForExpression( td, KindOfFragment.AllocatePseudoRegister, out sourceFragments, out fDirect );

                    res = new Expression[sourceFragments];

                    if(sourceFragments == 1 && m_typeSystem.GetLevel( exConst ) <= Operator.OperatorLevel.ScalarValues)
                    {
                        res[0] = exConst;
                    }
                    else
                    {
                        TypeRepresentation genericTd = m_typeSystem.WellKnownTypes.System_UInt32;

                        for(uint offset = 0; offset < sourceFragments; offset++)
                        {
                            uint                        byteOffset = offset * sizeof(uint);
                            InstanceFieldRepresentation fd         = td.FindFieldAtOffset( (int)byteOffset );
                            TypeRepresentation          fragmentTd = genericTd;

                            if(fd != null)
                            {
                                TypeRepresentation fieldTd = fd.FieldType;

                                if(fieldTd.SizeOfHoldingVariable == fragmentTd.SizeOfHoldingVariable)
                                {
                                    fragmentTd = fieldTd;
                                }
                            }

                            object valFrag;
                            ulong  valInt;

                            if(exConst.IsValueInteger && exConst.GetAsRawUlong( out valInt ))
                            {
                                CHECKS.ASSERT( exConst.SizeOfValue == sourceFragments * sizeof(uint), "Mismatch between type and value of ConstantExpression: {0} <=> {1}", exConst.Type, exConst.Value );

                                if(offset == 0)
                                {
                                    fragmentTd = m_typeSystem.WellKnownTypes.System_UInt32;
                                    valFrag    = (uint)valInt;
                                }
                                else
                                {
                                    if(exConst.IsValueSigned)
                                    {
                                        fragmentTd = m_typeSystem.WellKnownTypes.System_Int32;
                                        valFrag    = (int)(valInt >> 32);
                                    }
                                    else
                                    {
                                        fragmentTd = m_typeSystem.WellKnownTypes.System_UInt32;
                                        valFrag    = (uint)(valInt >> 32);
                                    }
                                }
                            }
                            else
                            {
                                valFrag = new ValueFragment( val, offset );
                            }

                            res[offset] = new ConstantExpression( fragmentTd, valFrag );
                        }
                    }

                    SetFragmentsForExpression( exConst, res );
                }
            }

            return res;
        }

        internal void SetFragmentsForExpression( Expression   ex        ,
                                                 Expression[] fragments )
        {
            m_lookupFragments[ex] = fragments;
        }

        //--//

        //
        // Access Methods
        //

        public bool IsMappedToMachine
        {
            get
            {
                return HasProperty( MappedToMachine );
            }
        }

        //--//

        //
        // Debug Methods
        //
    }
}
