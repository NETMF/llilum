//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public class MoveIntegerRegistersOperator : Operator
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
        private bool m_fRestoreSPSR;
        private uint m_registerMask;

        //
        // Constructor Methods
        //

        private MoveIntegerRegistersOperator( Debugging.DebugInfo debugInfo             ,
                                              bool                fLoad                 ,
                                              bool                fAddComputedRegisters ,
                                              bool                fRestoreSPSR          ,
                                              uint                registerMask          ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_fLoad                 = fLoad;
            m_fAddComputedRegisters = fAddComputedRegisters;
            m_fRestoreSPSR          = fRestoreSPSR;
            m_registerMask          = registerMask;
        }

        //--//

        public static MoveIntegerRegistersOperator New( Debugging.DebugInfo        debugInfo               ,
                                                        bool                       fLoad                   ,
                                                        bool                       fWriteBackIndexRegister ,
                                                        bool                       fAddComputedRegisters   ,
                                                        bool                       fRestoreSPSR            ,
                                                        uint                       registerMask            ,
                                                        PhysicalRegisterExpression ex                      )
        {
            var res = new MoveIntegerRegistersOperator( debugInfo, fLoad, fAddComputedRegisters, fRestoreSPSR, registerMask );

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
            return RegisterAndCloneState( context, new MoveIntegerRegistersOperator( m_debugInfo, m_fLoad, m_fAddComputedRegisters, m_fRestoreSPSR, m_registerMask ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_fLoad                 );
            context2.Transform( ref m_fAddComputedRegisters );
            context2.Transform( ref m_fRestoreSPSR          );
            context2.Transform( ref m_registerMask          );

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

        public bool ShouldRestoreSPSR
        {
            get
            {
                return m_fRestoreSPSR;
            }
        }

        public uint RegisterMask
        {
            get
            {
                return m_registerMask;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "MoveIntegerRegisters(Load={0}, AddComputedRegisters={1}, RestoreSPSR={2} RegisterMask={3:X8}", m_fLoad, m_fAddComputedRegisters, m_fRestoreSPSR, m_registerMask );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            string fmt;

            if(this.IsLoad)
            {
                if(this.ShouldRestoreSPSR)
                {
                    fmt = "<register set 0x{0:X8}{1}>,SPSR = [{3}]{2}";
                }
                else
                {
                    fmt = "<register set 0x{0:X8}{1}> = [{3}]{2}";
                }
            }
            else
            {
                fmt = "[{3}]{2} = <register set 0x{0:X8}{1}>";
            }

            return dumper.FormatOutput( fmt, this.RegisterMask, this.ShouldAddComputedRegisters ? " and scratched registers" : "", this.ShouldWriteBackIndexRegister ? "!" : "", this.FirstArgument );
        }
    }
}