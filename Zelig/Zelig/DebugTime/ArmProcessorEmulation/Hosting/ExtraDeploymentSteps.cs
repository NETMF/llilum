//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class ExtraDeploymentSteps
    {
        //
        // State
        //

        //
        // Helper Methods
        //

        public abstract List< Configuration.Environment.ImageSection > TransformImage( List< Configuration.Environment.ImageSection > image );

        public abstract void ExecuteToEntryPoint( Emulation.Hosting.AbstractHost owner             ,
                                                  uint                           entryPointAddress );
    }
}
