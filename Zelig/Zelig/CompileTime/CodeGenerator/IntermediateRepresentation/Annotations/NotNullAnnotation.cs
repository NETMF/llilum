//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class NotNullAnnotation : Annotation
    {
        //
        // Constructor Methods
        //

        private NotNullAnnotation()
        {
        }

        public static NotNullAnnotation Create( TypeSystemForIR ts )
        {
            return (NotNullAnnotation)MakeUnique( ts, new NotNullAnnotation() );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is NotNullAnnotation)
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
            return "<Never Null>";
        }
    }
}