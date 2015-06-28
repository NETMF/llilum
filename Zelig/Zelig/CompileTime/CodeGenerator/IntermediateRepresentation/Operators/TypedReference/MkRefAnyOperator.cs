//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class MkRefAnyOperator : Operator
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

        private TypeRepresentation m_td;

        //
        // Constructor Methods
        //

        private MkRefAnyOperator( Debugging.DebugInfo debugInfo ,
                                  TypeRepresentation  td        ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented )
        {
            m_td = td;
        }

        //--//

        public static MkRefAnyOperator New( Debugging.DebugInfo debugInfo ,
                                            TypeRepresentation  td        ,
                                            VariableExpression  lhs       ,
                                            Expression          rhs       )
        {
            MkRefAnyOperator res = new MkRefAnyOperator( debugInfo, td );

            res.SetLhs( lhs );
            res.SetRhs( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            TypeRepresentation td = context.ConvertType( m_td );

            return RegisterAndCloneState( context, new MkRefAnyOperator( m_debugInfo, td ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            context.Transform( ref m_td );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public TypeRepresentation Type
        {
            get
            {
                return m_td;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "MkRefAnyOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_td );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = typedref {1} for {2}", this.FirstResult, m_td, this.FirstArgument );
        }
    }
}