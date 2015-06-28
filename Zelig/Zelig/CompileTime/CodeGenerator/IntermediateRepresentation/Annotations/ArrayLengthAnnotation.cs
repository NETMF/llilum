//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ArrayLengthAnnotation : Annotation
    {
        //
        // Constructor Methods
        //

        private ArrayLengthAnnotation()
        {
        }

        public static ArrayLengthAnnotation Create( TypeSystemForIR ts )
        {
            return (ArrayLengthAnnotation)MakeUnique( ts, new ArrayLengthAnnotation() );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is ArrayLengthAnnotation)
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
            return "<Array Length>";
        }
    }
}