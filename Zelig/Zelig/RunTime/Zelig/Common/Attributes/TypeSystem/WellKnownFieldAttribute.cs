//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_WellKnownFieldAttribute" )]
    [AttributeUsage(AttributeTargets.Field)]
    public class WellKnownFieldAttribute : Attribute
    {
        //
        // State
        //

        public readonly string FieldName;

        //
        // Constructor Methods
        //

        public WellKnownFieldAttribute( string fieldName )
        {
            this.FieldName = fieldName;
        }
    }
}
