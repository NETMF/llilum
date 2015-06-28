//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CompilationConstraintsOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      |
                                                           OperatorCapabilities.IsMetaOperator                     ;

        //
        // State
        //

        private CompilationConstraints[] m_ccArraySet;
        private CompilationConstraints[] m_ccArrayReset;

        //
        // Constructor Methods
        //

        private CompilationConstraintsOperator( Debugging.DebugInfo      debugInfo    ,
                                                CompilationConstraints[] ccArraySet   ,
                                                CompilationConstraints[] ccArrayReset ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_ccArraySet   = ccArraySet;
            m_ccArrayReset = ccArrayReset;
        }

        //--//

        public static CompilationConstraintsOperator New( Debugging.DebugInfo      debugInfo    ,
                                                          CompilationConstraints[] ccArraySet   ,
                                                          CompilationConstraints[] ccArrayReset )
        {
            CompilationConstraintsOperator res = new CompilationConstraintsOperator( debugInfo, ccArraySet, ccArrayReset );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new CompilationConstraintsOperator( m_debugInfo, m_ccArraySet, m_ccArrayReset ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_ccArraySet   );
            context.Transform( ref m_ccArrayReset );

            context.Pop();
        }

        //--//

        public CompilationConstraints[] TransformCompilationConstraints( CompilationConstraints[] ccArray )
        {
            foreach(CompilationConstraints cc in m_ccArrayReset)
            {
                ccArray = ControlFlowGraphState.RemoveCompilationConstraint( ccArray, cc );
            }

            foreach(CompilationConstraints cc in m_ccArraySet)
            {
                ccArray = ControlFlowGraphState.AddCompilationConstraint( ccArray, cc );
            }

            return ccArray;
        }


        //--//

        //
        // Access Methods
        //

        public CompilationConstraints[] Set
        {
            get
            {
                return m_ccArraySet;
            }
        }

        public CompilationConstraints[] Reset
        {
            get
            {
                return m_ccArrayReset;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "CompilationConstraintsOperator(" );

            base.InnerToString( sb );

            foreach(CompilationConstraints cc in m_ccArrayReset)
            {
                sb.AppendFormat( " Reset:{0}", cc );
            }

            foreach(CompilationConstraints cc in m_ccArraySet)
            {
                sb.AppendFormat( " Set:{0}", cc );
            }

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( "constraint" );

            foreach(CompilationConstraints cc in m_ccArrayReset)
            {
                sb.AppendFormat( " Reset:{0}", cc );
            }

            foreach(CompilationConstraints cc in m_ccArraySet)
            {
                sb.AppendFormat( " Set:{0}", cc );
            }

            return sb.ToString();
        }
    }
}