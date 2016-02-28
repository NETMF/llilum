//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.AllowCompileTimeIntrospection]
    public enum HardwareException
    {
        None                ,
        VectorTable         , // Not really an exception, but a bootstrap for the exception table.
        Bootstrap           , // Not really an exception, but the entry point into the system at boot.
        LongJump            , // Not really an exception, but a marker for loading a full register context.

        Reset               ,

        UndefinedInstruction,
        PrefetchAbort       ,
        DataAbort           ,

        Interrupt           ,
        FastInterrupt       ,
        SoftwareInterrupt   ,

        NMI                 ,
        Fault               , 
        SysTick             , 
        PendSV              , 
        Service             , 
        Debug               , 
    }

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_HardwareExceptionHandlerAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HardwareExceptionHandlerAttribute : Attribute
    {
        //
        // State
        //

        public readonly HardwareException Origin;

        //
        // Constructor Methods
        //

        public HardwareExceptionHandlerAttribute( HardwareException origin )
        {
            this.Origin = origin;
        }
    }
}
