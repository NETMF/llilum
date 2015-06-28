//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public sealed class GenericImageAnnotation : ImageAnnotation
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private GenericImageAnnotation() // Default constructor required by TypeSystemSerializer.
        {
        }

        public GenericImageAnnotation( SequentialRegion region ,
                                       uint             offset ,
                                       uint             size   ,
                                       object           target ) : base( region, offset, size, target )
        {
        }

        //
        // Helper Methods
        //

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return false; }
        }

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = int.MinValue;
            upperBound = int.MaxValue;
        }

        public override bool ApplyRelocation()
        {
            return true;
        }
    }
}
