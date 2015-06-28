//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using System.IO;

    public sealed class ExternalCallOperator : Operator
    {
        public interface IExternalCallGlobalContext
        {
            void Initialize();
        }

        public interface IExternalCallContext
        {
            Debugging.DebugInfo DebugInfo         { get;                                       }
            void                UpdateRelocation  ( object relocation                          );
            bool                PerformCodeLinkage( object region, object section, object core );
            Operator            Owner             { get; set;                                  }
            uint                OperatorOffset    { get;                                       }
        }

        private IExternalCallContext m_context;
        private static List<string> s_nativeImportDirs  = new List<string>();
        private static List<string> s_nativeImportFiles = new List<string>();

        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.MayMutateExistingStorage           |
                                                           OperatorCapabilities.MayAllocateStorage                 |
                                                           OperatorCapabilities.MayReadExistingMutableStorage      |
                                                           OperatorCapabilities.MayThrow                           |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // Constructor Methods
        //

        private ExternalCallOperator( IExternalCallContext ctx )
            : base( ctx.DebugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_context = ctx;
        }

        //--//

        public static ExternalCallOperator New( IExternalCallContext ctx )
        {
            return new ExternalCallOperator( ctx );
        }

        public override bool ShouldNotBeRemoved
        {
            get
            {
                return true;
            }
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ExternalCallOperator( m_context ) );
        }

        public bool Encode( object region, object section, object core )
        {
            return m_context.PerformCodeLinkage( region, section, core );
        }

        public static List<string> NativeImportDirectories
        {
            get { return s_nativeImportDirs ; }
            set { s_nativeImportDirs = value; }
        }

        public static List<string> NativeImportLibraries
        {
            get { return s_nativeImportFiles ; }
            set { s_nativeImportFiles = value; }
        }

        //--//

        //
        // Access Methods
        //

        public override bool PerformsNoActions
        {
            get
            {
                return false;
            }
        }

        public IExternalCallContext Context
        {
            get
            {
                return m_context;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "IExternalCallOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "ExternalCall" );
        }
    }
}
