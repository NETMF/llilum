//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Internals
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Internals_WellKnownMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    internal class WellKnownMethodAttribute : Attribute
    {
        //
        // State
        //

        internal readonly string FieldName;

        //
        // Constructor Methods
        //

        internal WellKnownMethodAttribute( string fieldName )
        {
            this.FieldName = fieldName;
        }
    }
}
