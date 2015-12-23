//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace System.Net
{
    public class IPHostEntry
    {
        internal string hostName;
        internal IPAddress[] addressList;

        public string HostName
        {
            get
            {
                return hostName;
            }
        }

        public IPAddress[] AddressList
        {
            get
            {
                return addressList;
            }
        }
    } // class IPHostEntry
} // namespace System.Net

