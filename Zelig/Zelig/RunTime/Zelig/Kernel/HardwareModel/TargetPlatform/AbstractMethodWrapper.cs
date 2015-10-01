//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class AbstractMethodWrapper
    {
        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_AddActivationRecordEvent" )]
        protected static extern void AddActivationRecordEvent( ActivationRecordEvents ev );

        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue" )]
        public abstract void Prologue( string                                      typeFullName   ,
                                       string                                      methodFullName ,
                                       TS.MethodRepresentation.BuildTimeAttributes attribs        );

        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue2" )]
        public abstract void Prologue( string                                      typeFullName   ,
                                       string                                      methodFullName ,
                                       TS.MethodRepresentation.BuildTimeAttributes attribs        ,
                                       HardwareException                           he             );

        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue" )]
        public abstract void Epilogue( string                                      typeFullName   ,
                                       string                                      methodFullName ,
                                       TS.MethodRepresentation.BuildTimeAttributes attribs        );

        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue2" )]
        public abstract void Epilogue( string                                      typeFullName   ,
                                       string                                      methodFullName ,
                                       TS.MethodRepresentation.BuildTimeAttributes attribs        ,
                                       HardwareException                           he             );
    }
}
