//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public class MoveFloatingPointRegistersOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private bool m_fLoad;
        private bool m_fAddComputedRegisters;
        private uint m_registerLow;
        private uint m_registerHigh;

        //
        // Constructor Methods
        //

        private MoveFloatingPointRegistersOperator( Debugging.DebugInfo debugInfo             ,
                                                    bool                fLoad                 ,
                                                    bool                fAddComputedRegisters ,
                                                    uint                registerLow           ,
                                                    uint                registerHigh          ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_fLoad                 = fLoad;
            m_fAddComputedRegisters = fAddComputedRegisters;
            m_registerLow           = registerLow;
            m_registerHigh          = registerHigh;
        }

        //--//

        public static MoveFloatingPointRegistersOperator New( Debugging.DebugInfo        debugInfo               ,
                                                              bool                       fLoad                   ,
                                                              bool                       fWriteBackIndexRegister ,
                                                              bool                       fAddComputedRegisters   ,
                                                              uint                       registerLow             ,
                                                              uint                       registerHigh            ,
                                                              PhysicalRegisterExpression ex                      )
        {
            var res = new MoveFloatingPointRegistersOperator( debugInfo, fLoad, fAddComputedRegisters, registerLow, registerHigh );

            res.SetRhs( ex );

            if(fWriteBackIndexRegister)
            {
                res.SetLhs( ex );
            }

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new MoveFloatingPointRegistersOperator( m_debugInfo, m_fLoad, m_fAddComputedRegisters, m_registerLow, m_registerHigh ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_fLoad                 );
            context2.Transform( ref m_fAddComputedRegisters );
            context2.Transform( ref m_registerLow           );
            context2.Transform( ref m_registerHigh          );

            context2.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            return false;
        }

        //
        // Access Methods
        //

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return true;
            }
        }

        public bool IsLoad
        {
            get
            {
                return m_fLoad;
            }
        }

        public bool ShouldWriteBackIndexRegister
        {
            get
            {
                return this.Results.Length != 0;
            }
        }

        public bool ShouldAddComputedRegisters
        {
            get
            {
                return m_fAddComputedRegisters;
            }
        }

        public uint RegisterLow
        {
            get
            {
                return m_registerLow;
            }
        }

        public uint RegisterHigh
        {
            get
            {
                return m_registerHigh;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "MoveFloatingPointRegisters(Load={0}, AddComputedRegisters={1}, RegisterLow={2}, RegisterHigh={3}", m_fLoad, m_fAddComputedRegisters, m_registerLow, m_registerHigh );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            string fmt;

            if(this.IsLoad)
            {
                fmt = "<register set [{0} - {1}]{2}> = [{4}]{3}";
            }
            else
            {
                fmt = "[{4}]{3} = <register set [{0} - {1}]{2}>";
            }

            return dumper.FormatOutput( fmt, this.RegisterLow, this.RegisterHigh, this.ShouldAddComputedRegisters ? " and scratched registers" : "", this.ShouldWriteBackIndexRegister ? "!" : "", this.FirstArgument );
        }
    }
}