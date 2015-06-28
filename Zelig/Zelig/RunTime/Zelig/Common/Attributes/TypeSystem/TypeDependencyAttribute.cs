//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_TypeDependencyAttribute" )]
    [AttributeUsage( AttributeTargets.Struct |
                     AttributeTargets.Class  |
                     AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public sealed class TypeDependencyAttribute : Attribute
	{
        internal Type Type;

        public TypeDependencyAttribute( Type type )
		{
            this.Type = type;
        }
    }
}
