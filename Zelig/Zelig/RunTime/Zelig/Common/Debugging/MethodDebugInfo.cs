//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugging
{
    using System;
    using System.Text;

    public sealed class MethodDebugInfo
    {
        //
        // State
        //

        public readonly String[] LocalVarNames;

        //
        // Constructor Methods
        //

        public MethodDebugInfo( String[] localVarNames )
        {
            this.LocalVarNames = localVarNames;
        }
    }
}
