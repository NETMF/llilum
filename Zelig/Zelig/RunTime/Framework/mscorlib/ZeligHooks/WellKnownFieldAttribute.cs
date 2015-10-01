//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Internals
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Internals_WellKnownFieldAttribute" )]
    [AttributeUsage(AttributeTargets.Field)]
    internal class WellKnownFieldAttribute : Attribute
    {
        //
        // State
        //

        internal readonly string FieldName;

        //
        // Constructor Methods
        //

        internal WellKnownFieldAttribute( string fieldName )
        {
            this.FieldName = fieldName;
        }
    }
}
