//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public class VectorHack_Initialize : VectorHack_Base
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private VectorHack_Initialize( Debugging.DebugInfo debugInfo ,
                                       int                 size      ) : base( debugInfo, size )
        {
        }

        //--//

        public static VectorHack_Initialize New( Debugging.DebugInfo debugInfo ,
                                                 int                 size      )
        {
            VectorHack_Initialize res = new VectorHack_Initialize( debugInfo, size );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new VectorHack_Initialize( m_debugInfo, this.Size ) );
        }

        //--//

        //
        // Access Methods
        //

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "VectorHack_Initialize(Size={0}", this.Size );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "VectorHack_Initialize(Size={0})", this.Size );
        }
    }
}