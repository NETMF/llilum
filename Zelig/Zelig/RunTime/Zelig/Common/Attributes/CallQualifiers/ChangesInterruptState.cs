//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ChangesInterruptState" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public sealed class ChangesInterruptStateAttribute : Attribute
    {
        //
        // State
        //

        public readonly bool InterruptsEnabled;

        //
        // Constructor Methods
        //

        public ChangesInterruptStateAttribute( bool fEnablesInterrupts )
        {
            this.InterruptsEnabled = fEnablesInterrupts;
        }
    }
}
