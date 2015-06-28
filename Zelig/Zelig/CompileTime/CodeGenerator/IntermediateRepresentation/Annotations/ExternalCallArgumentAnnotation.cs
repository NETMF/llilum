//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ExternalCallArgumentAnnotation : Annotation
    {
        //
        // Constructor Methods
        //

        private ExternalCallArgumentAnnotation()
        {
        }

        public static ExternalCallArgumentAnnotation Create( TypeSystemForIR ts )
        {
            return (ExternalCallArgumentAnnotation)MakeUnique( ts, new ExternalCallArgumentAnnotation() );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is ExternalCallArgumentAnnotation)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override Annotation Clone( CloningContext context )
        {
            return this; // Nothing to change.
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return "<External Call Argument>";
        }
    }
}
