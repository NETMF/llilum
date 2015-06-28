//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class DontRemoveAnnotation : Annotation
    {
        //
        // Constructor Methods
        //

        private DontRemoveAnnotation()
        {
        }

        public static DontRemoveAnnotation Create( TypeSystemForIR ts )
        {
            return (DontRemoveAnnotation)MakeUnique( ts, new DontRemoveAnnotation() );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is DontRemoveAnnotation)
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
            return "<Don't Remove>";
        }
    }
}