//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;

    
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ProductFilterAttribute" )]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class ProductFilterAttribute : Attribute
    {
        //
        // State
        //

        public readonly String ProductFilter;

        //
        // Constructor Methods
        //

        public ProductFilterAttribute( String product )
        {
            this.ProductFilter = product;
        }
    }
}
