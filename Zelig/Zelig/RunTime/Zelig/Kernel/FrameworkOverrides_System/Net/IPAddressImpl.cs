//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;

    [ExtendClass(typeof(IPAddress), NoConstructors = true)]
    public abstract class IPAddressImpl
    {
        public static string GetDefaultLocalAddressImpl()
        {
            return NetworkInterfaceProvider.Instance.GetDefaultLocalAddress();
        }
    }
}
