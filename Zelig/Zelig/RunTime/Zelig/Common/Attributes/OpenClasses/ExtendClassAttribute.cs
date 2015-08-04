//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This attribute tells the system that the type under constructor is not a real type,
    // it should be used only add/change methods and fields to an existing type.
    //
    // The overriding type should either derive from the target one, derive from the same super type, or derive from object.
    //
    // There could be an issue with constructors, since their code points to the super type, which could be incompatible.
    // The system should automatically correct the obvious cases and abort on ambiguous ones.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ExtendClassAttribute" )]
    [AttributeUsage(AttributeTargets.Class  |
                    AttributeTargets.Struct )]
    public sealed class ExtendClassAttribute : Attribute
    {
        //
        // State
        //

        public Type     Target;
        public string   TargetByWellKnownName;
        public bool     NoConstructors;
        public Type     ProcessAfter;
        public string   PlatformFilter;

        //
        // Constructor Methods
        //

        public ExtendClassAttribute( Type target )
        {
            this.Target = target;
        }

        public ExtendClassAttribute( string target )
        {
            this.TargetByWellKnownName = target;
        }
    }
}
