//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Internals
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Internals_TypeDependencyAttribute" )]
    [AttributeUsage( AttributeTargets.Struct |
                     AttributeTargets.Class  |
                     AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    internal sealed class TypeDependencyAttribute : Attribute
	{
        internal Type Type;

        public TypeDependencyAttribute( Type type )
		{
            this.Type = type;
        }
    }
}
