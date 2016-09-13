//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class LandingPadOperator : Operator
    {
        private const OperatorCapabilities cCapabilities =
            OperatorCapabilities.IsNonCommutative |
            OperatorCapabilities.DoesNotMutateExistingStorage |
            OperatorCapabilities.DoesNotAllocateStorage |
            OperatorCapabilities.DoesNotReadExistingMutableStorage |
            OperatorCapabilities.DoesNotThrow |
            OperatorCapabilities.DoesNotReadThroughPointerOperands |
            OperatorCapabilities.DoesNotWriteThroughPointerOperands |
            OperatorCapabilities.DoesNotCapturePointerOperands;

        private LandingPadOperator(Debugging.DebugInfo debugInfo, bool hasCleanupClause) :
            base(debugInfo, cCapabilities, OperatorLevel.Lowest)
        {
            HasCleanupClause = hasCleanupClause;
        }

        public bool HasCleanupClause
        {
            get;
        }

        public static LandingPadOperator New(
            Debugging.DebugInfo debugInfo,
            VariableExpression result,
            Expression[] handledTypes,
            bool hasCleanupClause)
        {
            LandingPadOperator res = new LandingPadOperator(debugInfo, hasCleanupClause);
            res.SetLhs(result);
            res.SetRhsArray(handledTypes);
            return res;
        }

        public override Operator Clone(CloningContext context)
        {
            return RegisterAndCloneState(context, new LandingPadOperator(m_debugInfo, HasCleanupClause));
        }

        protected override void CloneState(CloningContext context, Operator clone)
        {
            base.CloneState(context, clone);
        }

        //--//

        public override void ApplyTransformation(TransformationContextForIR context)
        {
            context.Push(this);
            base.ApplyTransformation(context);
            context.Pop();
        }

        public override bool CanPropagateCopy(Expression exOld, Expression exNew)
        {
            return false;
        }

        public override void InnerToString(System.Text.StringBuilder sb)
        {
            sb.Append("LandingPadOperator(");
            base.InnerToString(sb);
            sb.Append(")");
        }

        public override string FormatOutput(IIntermediateRepresentationDumper dumper)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int pos = 0; pos < this.Arguments.Length; pos++)
            {
                Expression ex = this.Arguments[pos];

                string format;
                if (pos == 0)
                {
                    format = "catch {0}";
                }
                else
                {
                    format = ", catch {0}";
                }

                sb.Append(dumper.FormatOutput(format, Arguments[pos]));
            }

            return dumper.FormatOutput("{0} = landingpad({1})", FirstResult, sb.ToString());
        }
    }
}
