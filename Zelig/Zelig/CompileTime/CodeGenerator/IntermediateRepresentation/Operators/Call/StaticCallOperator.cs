//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StaticCallOperator : CallOperator
    {
        //
        // Constructor Methods
        //

        private StaticCallOperator( Debugging.DebugInfo   debugInfo    ,
                                    OperatorCapabilities  capabilities ,
                                    CallOperator.CallKind callType     ,
                                    MethodRepresentation  md           ) : base( debugInfo, capabilities, OperatorLevel.ConcreteTypes_NoExceptions, callType, md )
        {
        }

        //--//

        public static StaticCallOperator New( Debugging.DebugInfo  debugInfo ,
                                              CallKind             callType  ,
                                              MethodRepresentation md        ,
                                              Expression[]         rhs       )
        {
            CHECKS.ASSERT( callType != CallKind.Virtual                                     , "Method '{0}' cannot be called as virtual"                                , md );
            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );

            StaticCallOperator res = Alloc( debugInfo, callType, md, rhs );

            res.SetRhsArray( rhs );

            return res;
        }

        public static StaticCallOperator New( Debugging.DebugInfo  debugInfo ,
                                              CallKind             callType  ,
                                              MethodRepresentation md        ,
                                              VariableExpression[] lhs       ,
                                              Expression[]         rhs       )
        {
            CHECKS.ASSERT( callType != CallKind.Virtual                                                        , "Method '{0}' cannot be called as virtual"                                , md );
            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID || lhs.Length != 0, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );
            CHECKS.ASSERT( md.ReturnType.BuiltInType != TypeRepresentation.BuiltInTypes.VOID || lhs.Length == 0, "Method '{0}' returns a value, it must be assigned to an Lvalue"          , md );

            StaticCallOperator res = Alloc( debugInfo, callType, md, rhs );

            res.SetLhsArray( lhs );
            res.SetRhsArray( rhs );

            return res;
        }

        private static StaticCallOperator Alloc( Debugging.DebugInfo  debugInfo ,
                                                 CallKind             callType  ,
                                                 MethodRepresentation md        ,
                                                 Expression[]         rhs       )
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

            return new StaticCallOperator( debugInfo, capabilities, callType, md );
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new StaticCallOperator( m_debugInfo, m_capabilities, m_callType, md ) );
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
            sb.Append( "StaticCallOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if(this.Arguments.Length > 0)
            {
                sb.Append( " " );
                for(int i = 0; i < this.Arguments.Length; i++)
                {
                    if(i != 0) sb.Append( ", " );

                    this.Arguments[i].InnerToString( sb );
                }
                sb.Append( " " );
            }

            if(this.Results.Length == 0)
            {
                return dumper.FormatOutput( "callStatic {0}({1})", this.TargetMethod, sb.ToString() );
            }
            else
            {
                return dumper.FormatOutput( "{2} = callStatic {0}({1})", this.TargetMethod, sb.ToString(), this.FirstResult );
            }
        }
    }
}