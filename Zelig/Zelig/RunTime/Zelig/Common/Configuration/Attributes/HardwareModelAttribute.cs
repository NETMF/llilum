//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class |
                     AttributeTargets.Field , AllowMultiple=true )]
    public sealed class HardwareModelAttribute : Attribute
    {
        public enum Kind
        {
            Engine          ,
            Memory          ,
            Peripheral      ,
            PeripheralsGroup,
            Interop         ,
        }

        //
        // State
        //

        public Type Target;
        public Kind TargetKind;

        //
        // Constructor Methods
        //

        public HardwareModelAttribute( Type target ,
                                       Kind kind   )
        {
            this.Target     = target;
            this.TargetKind = kind;
        }
    }
}
