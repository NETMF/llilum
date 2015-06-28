//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_WellKnownMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public class WellKnownMethodAttribute : Attribute
    {
        //
        // State
        //

        public readonly string FieldName;

        //
        // Constructor Methods
        //

        public WellKnownMethodAttribute( string fieldName )
        {
            this.FieldName = fieldName;
        }
    }
}
