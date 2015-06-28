//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public class MoveToCoprocessor : Operator
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

        private uint m_CpNum;
        private uint m_Op1;
        private uint m_CRn;
        private uint m_CRm;
        private uint m_Op2;

        //
        // Constructor Methods
        //

        private MoveToCoprocessor( Debugging.DebugInfo debugInfo ,
                                   uint                CpNum     ,
                                   uint                Op1       ,
                                   uint                CRn       ,
                                   uint                CRm       ,
                                   uint                Op2       ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_CpNum = CpNum;
            m_Op1   = Op1;
            m_CRn   = CRn;
            m_CRm   = CRm;
            m_Op2   = Op2;
        }

        //--//

        public static MoveToCoprocessor New( Debugging.DebugInfo debugInfo ,
                                             uint                CpNum     ,
                                             uint                Op1       ,
                                             uint                CRn       ,
                                             uint                CRm       ,
                                             uint                Op2       ,
                                             Expression          ex        )
        {
            MoveToCoprocessor res = new MoveToCoprocessor( debugInfo, CpNum, Op1, CRn, CRm, Op2 );

            res.SetRhs( ex );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new MoveToCoprocessor( m_debugInfo, m_CpNum, m_Op1, m_CRn, m_CRm, m_Op2 ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_CpNum );
            context2.Transform( ref m_Op1   );
            context2.Transform( ref m_CRn   );
            context2.Transform( ref m_CRm   );
            context2.Transform( ref m_Op2   );

            context2.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            if(exNew is ConstantExpression)
            {
                return false;
            }

            return true;
        }

        //
        // Access Methods
        //

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return true;
            }
        }

        public uint CpNum
        {
            get
            {
                return m_CpNum;
            }
        }

        public uint Op1
        {
            get
            {
                return m_Op1;
            }
        }

        public uint CRn
        {
            get
            {
                return m_CRn;
            }
        }

        public uint CRm
        {
            get
            {
                return m_CRm;
            }
        }

        public uint Op2
        {
            get
            {
                return m_Op2;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "MoveToCoprocessor(CpNum={0}, Op1={1}, CRn={2}, CRm={3}, Op2={4}", m_CpNum, m_Op1, m_CRn, m_CRm, m_Op2 );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "CoProc(CpNum={0}, Op1={1}, CRn={2}, CRm={3}, Op2={4}) = {5}", m_CpNum, m_Op1, m_CRn, m_CRm, m_Op2, this.FirstArgument );
        }
    }
}