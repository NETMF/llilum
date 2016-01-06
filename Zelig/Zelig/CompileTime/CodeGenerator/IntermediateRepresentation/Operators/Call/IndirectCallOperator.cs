//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class IndirectCallOperator : CallOperator
    {
        //
        // Constructor Methods
        //

        private IndirectCallOperator( Debugging.DebugInfo  debugInfo    ,
                                      OperatorCapabilities capabilities ,
                                      OperatorLevel        level        ,
                                      MethodRepresentation md           ,
                                      bool                 isInstance   ) : base( debugInfo, capabilities, level, CallKind.Indirect, md )
        {
            IsInstanceCall = isInstance;
        }

        //--//

        public static IndirectCallOperator New( Debugging.DebugInfo  debugInfo  ,
                                                MethodRepresentation md         ,
                                                Expression[]         rhs        ,
                                                bool                 isInstance ,
                                                bool                 fNullCheck )
        {
            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );

            IndirectCallOperator res = Alloc( debugInfo, md, rhs, isInstance, fNullCheck );

            res.SetRhsArray( rhs );

            return res;
        }

        public static IndirectCallOperator New( Debugging.DebugInfo  debugInfo  ,
                                                MethodRepresentation md         ,
                                                VariableExpression[] lhs        ,
                                                Expression[]         rhs        ,
                                                bool                 isInstance ,
                                                bool                 fNullCheck )
        {
            CHECKS.ASSERT( md.ReturnType.BuiltInType == TypeRepresentation.BuiltInTypes.VOID || lhs.Length != 0, "Method '{0}' does not return a value, it cannot be assigned to an Lvalue", md );
            CHECKS.ASSERT( md.ReturnType.BuiltInType != TypeRepresentation.BuiltInTypes.VOID || lhs.Length == 0, "Method '{0}' returns a value, it must be assigned to an Lvalue"          , md );

            IndirectCallOperator res = Alloc( debugInfo, md, rhs, isInstance, fNullCheck );

            res.SetLhsArray( lhs );
            res.SetRhsArray( rhs );

            return res;
        }

        private static IndirectCallOperator Alloc( Debugging.DebugInfo  debugInfo  ,
                                                   MethodRepresentation md         ,
                                                   Expression[]         rhs        ,
                                                   bool                 isInstance ,
                                                   bool                 fNullCheck )
        {
            CHECKS.ASSERT( CallOperator.MatchSignature( md, rhs, CallKind.Indirect ), "Incompatible arguments for call to '{0}'", md );

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

            OperatorCapabilities capabilities = HasPointerArguments( rhs, 1 ) ? cCapabilitiesMay : cCapabilitiesDoesnt;
            OperatorLevel        level;

            if(fNullCheck)
            {
                level = OperatorLevel.ConcreteTypes;
            }
            else
            {
                level = OperatorLevel.ConcreteTypes_NoExceptions;
            }

            return new IndirectCallOperator(debugInfo, capabilities, level, md, isInstance);
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            var op = new IndirectCallOperator(m_debugInfo, m_capabilities, m_level, md, IsInstanceCall);
            return RegisterAndCloneState(context, op);
        }

        //--//

        //
        // Access Methods
        //

        public bool IsInstanceCall
        {
            get;
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "IndirectCallOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if(this.Arguments.Length > 2)
            {
                sb.Append( " " );
                for(int i = 2; i < this.Arguments.Length; i++)
                {
                    if(i != 2) sb.Append( ", " );

                    this.Arguments[i].InnerToString( sb );
                }
                sb.Append( " " );
            }

            if(this.Results.Length == 0)
            {
                return dumper.FormatOutput( "callIndirect[{0}] {1}.{2}({3})", this.FirstArgument, this.SecondArgument, this.TargetMethod, sb.ToString() );
            }
            else
            {
                return dumper.FormatOutput( "{4} = callIndirect[{0}] {1}.{2}({3})", this.FirstArgument, this.SecondArgument, this.TargetMethod, sb.ToString(), this.FirstResult );
            }
        }
    }
}
