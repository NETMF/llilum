//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.AllowCompileTimeIntrospection]
    public enum ActivationRecordEvents
    {
        EnteringException  ,
        Constructing       ,
        ReadyForUse        ,
        ReadyForTearDown   ,
        ReturnToCaller     ,
        ReturnFromException,
        LongJump           ,
        NonReachable       ,
    }
}