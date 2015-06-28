//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class LocateUsageInCode : ScanCode
    {
        //
        // Constructor Methods
        //

        internal LocateUsageInCode( TypeSystemForCodeTransformation typeSystem ) : base( typeSystem, typeof(LocateUsageInCode) )
        {
        }

        //
        // Helper Methods
        //

        internal void AddLookup( object src )
        {
            CreateEntry( src );
        }

        //--//

        protected override bool PerformAction( Operator op     ,
                                               object   target )
        {
            if(op != null)
            {
                List< Operator > lst = this[target];
                if(lst != null)
                {
                    RecordOccurence( lst, op );
                }
            }

            return true;
        }
    }
}
