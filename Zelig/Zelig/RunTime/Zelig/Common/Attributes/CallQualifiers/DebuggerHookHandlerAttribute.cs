//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.AllowCompileTimeIntrospection]
    public enum DebuggerHook
    {
        None                   ,

        FlushInstructionCache  ,
        FlushDataCache         ,

        GetFullProcessorContext,

        GetSoftBreakpointTable ,
    }

    public struct SoftBreakpointDescriptor
    {
        public UIntPtr Address;
        public uint    Value;
    }

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_DebuggerHookHandlerAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DebuggerHookHandlerAttribute : Attribute
    {
        //
        // State
        //

        public readonly DebuggerHook Kind;

        //
        // Constructor Methods
        //

        public DebuggerHookHandlerAttribute( DebuggerHook kind )
        {
            this.Kind = kind;
        }
    }
}
