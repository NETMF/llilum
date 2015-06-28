//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class RefAnyValOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.MayThrow                           |
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

        private RefAnyValOperator( Debugging.DebugInfo debugInfo ,
                                   TypeRepresentation  td        ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented )
        {
            m_td = td;
        }

        //--//

        public static RefAnyValOperator New( Debugging.DebugInfo debugInfo ,
                                             TypeRepresentation  td        ,
                                             VariableExpression  lhs       ,
                                             Expression          rhs       )
        {
            RefAnyValOperator res = new RefAnyValOperator( debugInfo, td );

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

            return RegisterAndCloneState( context, new RefAnyValOperator( m_debugInfo, td ) );
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
            sb.Append( "RefAnyValOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_td );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = value from typedref {1}", this.FirstResult, this.FirstArgument );
        }
    }
}