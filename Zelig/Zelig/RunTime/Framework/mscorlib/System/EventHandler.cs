// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System
{
    using System;

    [Serializable]
    public delegate void EventHandler( Object sender, EventArgs e );

    [Serializable]
    public delegate void EventHandler<TEventArgs>( Object sender, TEventArgs e ) where TEventArgs : EventArgs;
}
