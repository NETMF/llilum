//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//


namespace System.Net
{
    /// <summary>
    /// Network authentication type.
    /// Currently supports:
    /// Basic Authentication
    /// Microsoft Live Id Delegate Authentication
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// 
        /// </summary>
        Basic,
        /// <summary>
        /// 
        /// </summary>
        WindowsLive
    };
}
