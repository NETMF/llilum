//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ExecutesWithInterruptsDisabled : Annotation
    {
        //
        // Constructor Methods
        //

        private ExecutesWithInterruptsDisabled( )
        {
        }

        public static ExecutesWithInterruptsDisabled Create( TypeSystemForIR ts )
        {
            return (ExecutesWithInterruptsDisabled)MakeUnique( ts, new ExecutesWithInterruptsDisabled( ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is ExecutesWithInterruptsDisabled)
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
            return "<Interrupts Disabled>";
        }
    }
}