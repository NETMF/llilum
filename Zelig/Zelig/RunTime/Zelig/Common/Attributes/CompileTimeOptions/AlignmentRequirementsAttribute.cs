//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_AlignmentRequirementsAttribute" )]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class AlignmentRequirementsAttribute : Attribute
    {
        //
        // State
        //

        public readonly uint Alignment;
        public readonly int  AlignmentOffset;


        //
        // Constructor Methods
        //

        public AlignmentRequirementsAttribute( uint alignment       ,
                                               int  alignmentOffset )
        {
            this.Alignment       = alignment;
            this.AlignmentOffset = alignmentOffset;
        }
    }
}
