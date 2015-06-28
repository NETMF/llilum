//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public struct GenericParameterDefinition
    {
        //
        // This is just a copy of Microsoft.Zelig.MetaData.GenericParameterAttributes, needed to break the dependency of TypeSystem from MetaData.
        //
        [Flags]
        public enum Attributes : ushort
        {
            VarianceMask                   = 0x0003,
            NonVariant                     = 0x0000, // The generic parameter is non-variant
            Covariant                      = 0x0001, // The generic parameter is covariant
            Contravariant                  = 0x0002, // The generic parameter is contravariant

            SpecialConstraintMask          = 0x001C,
            ReferenceTypeConstraint        = 0x0004, // The generic parameter has the class special constraint
            NotNullableValueTypeConstraint = 0x0008, // The generic parameter has the valuetype special constraint
            DefaultConstructorConstraint   = 0x0010, // The generic parameter has the .ctor special constraint
        }

        //--//

        //
        // State
        //

        public Attributes           Flags;
        public String               Name;
        public TypeRepresentation[] Constraints;

        //--//

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Transform( ref Flags       );
            context.Transform( ref Name        );
            context.Transform( ref Constraints );
        }

        //--//

        public bool IsCompatible( ref GenericParameterDefinition paramDef )
        {
            return ArrayUtility.ArrayEqualsNotNull( this.Constraints, paramDef.Constraints, 0 );
        }
    }
}
