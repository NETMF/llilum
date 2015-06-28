//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using System.IO;
    using System.Runtime.CompilerServices;


    public interface IConfigurationProvider
    {
        bool GetValue(     string optionName ,
                       out object val        );
    }
}
