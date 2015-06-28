//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class InstanceCallOperator : CallOperator
    {
        //
        // Constructor Methods
        //

        private InstanceCallOperator( Debugging.DebugInfo  debugInfo    ,
                                      OperatorCapabilities capabilities ,
                                      OperatorLevel        level        ,
                                      CallKind             callType     ,
                                      MethodRepresentation md           ) : base( debugInfo, capabilities, level, callType, md )
        {
        }

        //--//

        public static InstanceCallOperator New( Debugging.DebugInfo  debugInfo  ,
                                                CallKind             callType   ,
                                                MethodRepresentation md         ,
                                                Expression[]         rhs        ,
                                                bool                 fNullCheck )
        {
            if(callType == CallKind.Virtual && !(md is VirtualMethodRepresentation))
            {
                callType = CallKind.Direct;
            }

            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );

            InstanceCallOperator res = Alloc( debugInfo, callType, md, rhs, fNullCheck );

            res.SetRhsArray( rhs );

            return res;
        }

        public static InstanceCallOperator New( Debugging.DebugInfo  debugInfo  ,
                                                CallKind             callType   ,
                                                MethodRepresentation md         ,
                                                VariableExpression[] lhs        ,
                                                Expression[]         rhs        ,
                                                bool                 fNullCheck )
        {
            if(callType == CallKind.Virtual && !(md is VirtualMethodRepresentation))
            {
                callType = CallKind.Direct;
            }

            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID || lhs.Length != 0, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );
            CHECKS.ASSERT( md.ReturnType.BuiltInType != TypeRepresentation.BuiltInTypes.VOID || lhs.Length == 0, "Method '{0}' returns a value, it must be assigned to an Lvalue"          , md );

            InstanceCallOperator res = Alloc( debugInfo, callType, md, rhs, fNullCheck );

            res.SetLhsArray( lhs );
            res.SetRhsArray( rhs );

            return res;
        }

        private static InstanceCallOperator Alloc( Debugging.DebugInfo  debugInfo  ,
                                                   CallKind             callType   ,
                                                   MethodRepresentation md         ,
                                                   Expression[]         rhs        ,
                                                   bool                 fNullCheck )
        {
            CHECKS.ASSERT( CallOperator.MatchSignature( md, rhs, callType ), "Incompatible arguments for call to '{0}'", md );

            const OperatorCapabilities cCapabilitiesMay    = OperatorCapabilities.IsNonCommutative                   |
                                                             OperatorCapabilities.MayMutateExistingStorage           |
                                                             OperatorCapabilities.MayAllocateStorage                 |
                                                             OperatorCapabilities.MayReadExistingMutableStorage      |
                                                             OperatorCapabilities.MayThrow                           |
                                                             OperatorCapabilities.MayReadThroughPointerOperands      |
                                                             OperatorCapabilities.MayWriteThroughPointerOperands     |
                                                             OperatorCapabilities.MayCapturePointerOperands          ;

            const OperatorCapabilities cCapabilitiesDoesnt = OperatorCapabilities.IsNonCommutative                   |
                                                             OperatorCapabilities.MayMutateExistingStorage           |
                                                             OperatorCapabilities.MayAllocateStorage                 |
                                                             OperatorCapabilities.MayReadExistingMutableStorage      |
                                                             OperatorCapabilities.MayThrow                           |
                                                             OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                             OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                             OperatorCapabilities.DoesNotCapturePointerOperands      ;

            OperatorCapabilities capabilities = HasPointerArguments( rhs, 0 ) ? cCapabilitiesMay : cCapabilitiesDoesnt;
            OperatorLevel        level;
            
            if(callType == CallKind.Virtual)
            {
                level = OperatorLevel.ObjectOriented;
            }
            else if(fNullCheck)
            {
                level = OperatorLevel.ConcreteTypes;
            }
            else
            {
                level = OperatorLevel.ConcreteTypes_NoExceptions;
            }

            return new InstanceCallOperator( debugInfo, capabilities, level, callType, md );
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new InstanceCallOperator( m_debugInfo, m_capabilities, m_level, m_callType, md ) );
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "InstanceCallOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if(this.Arguments.Length > 1)
            {
                sb.Append( " " );
                for(int i = 1; i < this.Arguments.Length; i++)
                {
                    if(i != 1) sb.Append( ", " );

                    this.Arguments[i].InnerToString( sb );
                }
                sb.Append( " " );
            }

            string call;

            switch(this.CallType)
            {
                default                        : call = "call"                 ; break;
                case CallKind.Indirect         : call = "callIndirect"         ; break;
                case CallKind.Virtual          : call = "callVirtual"          ; break;
                case CallKind.Overridden       : call = "callOverridden"       ; break;
                case CallKind.OverriddenNoCheck: call = "callOverriddenNoCheck"; break;
            }

            if(this.Results.Length == 0)
            {
                return dumper.FormatOutput( "{0} {1}.{2}({3})", call, this.FirstArgument, this.TargetMethod, sb.ToString() );
            }
            else
            {
                return dumper.FormatOutput( "{4} = {0} {1}.{2}({3})", call, this.FirstArgument, this.TargetMethod, sb.ToString(), this.FirstResult );
            }
        }
    }
}