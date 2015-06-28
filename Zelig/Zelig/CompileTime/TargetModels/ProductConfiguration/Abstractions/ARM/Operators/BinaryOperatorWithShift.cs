//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;

    public sealed class BinaryOperatorWithShift : Operator
    {
        //
        // State
        //

        AbstractBinaryOperator.ALU m_alu;
        bool                       m_fSigned;
        AbstractBinaryOperator.ALU m_aluShift;
        bool                       m_fSignedShift;
        
        //
        // Constructor Methods
        //

        private BinaryOperatorWithShift( Debugging.DebugInfo        debugInfo    ,
                                         OperatorCapabilities       capabilities ,
                                         AbstractBinaryOperator.ALU alu          ,
                                         bool                       fSigned      ,
                                         AbstractBinaryOperator.ALU aluShift     ,
                                         bool                       fSignedShift ) : base( debugInfo, capabilities, OperatorLevel.Lowest )
        {
            m_alu          = alu;
            m_fSigned      = fSignedShift;
            m_aluShift     = aluShift;
            m_fSignedShift = fSignedShift;
        }

        //--//

        public static BinaryOperatorWithShift New( Debugging.DebugInfo        debugInfo    ,
                                                   AbstractBinaryOperator.ALU alu          ,
                                                   bool                       fSigned      ,
                                                   AbstractBinaryOperator.ALU aluShift     ,
                                                   bool                       fSignedShift ,
                                                   VariableExpression         lhs          ,
                                                   Expression                 rhsLeft      ,
                                                   Expression                 rhsRight     ,
                                                   Expression                 rhsShift     )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      |
                                                OperatorCapabilities.DoesNotThrow                       ;

            switch(alu)
            {
                case AbstractBinaryOperator.ALU.ADD:
                case AbstractBinaryOperator.ALU.MUL:
                case AbstractBinaryOperator.ALU.AND:
                case AbstractBinaryOperator.ALU.OR:
                case AbstractBinaryOperator.ALU.XOR:
                    capabilities |= OperatorCapabilities.IsCommutative;
                    break;

                default:
                    capabilities |= OperatorCapabilities.IsNonCommutative;
                    break;
            }

            var res = new BinaryOperatorWithShift( debugInfo, capabilities, alu, fSigned, aluShift, fSignedShift );

            res.SetLhs( lhs                         );
            res.SetRhs( rhsLeft, rhsRight, rhsShift );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_alu          );
            context.Transform( ref m_fSigned      );
            context.Transform( ref m_aluShift     );
            context.Transform( ref m_fSignedShift );

            context.Pop();
        }

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new BinaryOperatorWithShift( m_debugInfo, m_capabilities, m_alu, m_fSigned, m_aluShift, m_fSignedShift ) );
        }

        //--//

        //
        // Access Methods
        //

        public AbstractBinaryOperator.ALU Alu
        {
            get
            {
                return m_alu;
            }
        }

        public bool Signed
        {
            get
            {
                return m_fSigned;
            }
        }

        public AbstractBinaryOperator.ALU AluShift
        {
            get
            {
                return m_aluShift;
            }
        }

        public bool SignedShift
        {
            get
            {
                return m_fSignedShift;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "BinaryOperatorWithShift(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1} {2} ({3} {4} {5})", this.FirstResult, this.FirstArgument, m_alu, this.SecondArgument, m_aluShift, this.ThirdArgument );
        }
    }
}