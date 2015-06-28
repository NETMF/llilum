//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public sealed class DataRelocation : ImageAnnotation
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private DataRelocation() // Default constructor required by TypeSystemSerializer.
        {
        }

        public DataRelocation( SequentialRegion region ,
                               uint             offset ,
                               object           target ) : base( region, offset, sizeof(uint), target )
        {
        }

        //
        // Helper Methods
        //

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return true; }
        }

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = int.MinValue;
            upperBound = int.MaxValue;
        }

        public override bool ApplyRelocation()
        {
            uint val = m_region.Owner.Resolve( m_target );

            m_region.Write( m_offset, val );

            return true;
        }
    }
}
