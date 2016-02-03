//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ResumeUnwindOperator : ControlOperator
    {
        private const OperatorCapabilities cCapabilities =
            OperatorCapabilities.IsNonCommutative |
            OperatorCapabilities.DoesNotMutateExistingStorage |
            OperatorCapabilities.DoesNotAllocateStorage |
            OperatorCapabilities.DoesNotReadExistingMutableStorage |
            OperatorCapabilities.MayThrow |
            OperatorCapabilities.DoesNotReadThroughPointerOperands |
            OperatorCapabilities.DoesNotWriteThroughPointerOperands |
            OperatorCapabilities.DoesNotCapturePointerOperands;

        private ResumeUnwindOperator(Debugging.DebugInfo debugInfo) :
            base(debugInfo, cCapabilities, OperatorLevel.Lowest)
        {
        }

        public static ResumeUnwindOperator New(Debugging.DebugInfo debugInfo, Expression exception)
        {
            ResumeUnwindOperator res = new ResumeUnwindOperator(debugInfo);
            res.SetRhs(exception);
            return res;
        }

        protected override void UpdateSuccessorInformation()
        {
        }

        public override bool SubstituteTarget(BasicBlock oldBB, BasicBlock newBB)
        {
            return false;
        }

        public override Operator Clone(CloningContext context)
        {
            return RegisterAndCloneState(context, new ResumeUnwindOperator(m_debugInfo));
        }

        public override void InnerToString(System.Text.StringBuilder sb)
        {
            sb.Append("ResumeUnwindOperator(");
            base.InnerToString(sb);
            sb.Append(")");
        }

        public override string FormatOutput(IIntermediateRepresentationDumper dumper)
        {
            return dumper.FormatOutput("throw {0}", this.FirstArgument);
        }
    }
}
