//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public class VectorHack_LoadData : VectorHack_Base
    {
        //
        // State
        //

        private ZeligIR.Abstractions.RegisterDescriptor m_destinationBankBase;

        //
        // Constructor Methods
        //

        private VectorHack_LoadData( Debugging.DebugInfo                     debugInfo           ,
                                     int                                     size                ,
                                     ZeligIR.Abstractions.RegisterDescriptor destinationBankBase ) : base( debugInfo, size )
        {
            m_destinationBankBase = destinationBankBase;
        }

        //--//

        public static VectorHack_LoadData New( Debugging.DebugInfo                     debugInfo           ,
                                               int                                     size                ,
                                               ZeligIR.Abstractions.RegisterDescriptor destinationBankBase ,
                                               VariableExpression                      lhs                 ,
                                               Expression                              rhs                 )
        {
            VectorHack_LoadData res = new VectorHack_LoadData( debugInfo, size, destinationBankBase );

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
            return RegisterAndCloneState( context, new VectorHack_LoadData( m_debugInfo, this.Size, m_destinationBankBase ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_destinationBankBase );

            context2.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ZeligIR.Abstractions.RegisterDescriptor DestinationBankBase
        {
            get
            {
                return m_destinationBankBase;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "VectorHack_LoadData(Size={0},Base={1}", this.Size, m_destinationBankBase.Mnemonic );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = VectorHack_LoadData(Size={1},Base={2})[{3}]", this.FirstResult, this.Size, m_destinationBankBase.Mnemonic, this.FirstArgument );
        }
    }
}