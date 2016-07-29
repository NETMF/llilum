//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class DoesNotAllocate : Annotation
    {
        //
        // Constructor Methods
        //

        private DoesNotAllocate( )
        {
        }

        public static DoesNotAllocate Create( TypeSystemForIR ts )
        {
            return (DoesNotAllocate)MakeUnique( ts, new DoesNotAllocate( ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is DoesNotAllocate)
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
            return "<Does Not Allocate>";
        }
    }
}