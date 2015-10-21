//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class WellKnownMethodHandlers
    {
        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "Configuration_ExecuteApplication" )]
        private static void Handle_Configuration_ExecuteApplication( PhaseExecution.NotificationContext nc )
        {
            CallOperator                    op    = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation ts    = nc.TypeSystem;
            MethodRepresentation            md    = op.TargetMethod;
            MethodRepresentation            mdApp = ts.ApplicationEntryPoint;

            if(md.MatchSignature( mdApp, null ))
            {
                op.TargetMethod = mdApp;

                nc.MarkAsModified();
                return;
            }

            if(mdApp.ThisPlusArguments.Length == 2)
            {
                TypeRepresentation tdArg = mdApp.ThisPlusArguments[1];

                if(tdArg is ArrayReferenceTypeRepresentation)
                {
                    if(tdArg.ContainedType == ts.WellKnownTypes.System_String)
                    {
                        Expression[] rhs     = ts.AddTypePointerToArgumentsOfStaticMethod( mdApp, ts.CreateNullPointer( mdApp.ThisPlusArguments[1] ) );
                        CallOperator callNew = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, mdApp, rhs );
                        op.SubstituteWithOperator( callNew, Operator.SubstitutionFlags.Default );

                        nc.MarkAsModified();
                        return;
                    }
                }
            }

            throw TypeConsistencyErrorException.Create( "Cannot use '{0}' as an application entry point, incompatible signature", md );
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "RuntimeHelpers_InitializeArray" )]
        private static void Handle_RuntimeHelpers_InitializeArray( PhaseExecution.NotificationContext nc )
        {
            CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            ConstantExpression exConst = op.ThirdArgument as ConstantExpression;
            if(exConst != null)
            {
                DataManager.ObjectDescriptor    odHandle = (DataManager.ObjectDescriptor)exConst.Value;
                DataManager.ObjectDescriptor    odField  = (DataManager.ObjectDescriptor)odHandle.Get( (InstanceFieldRepresentation)nc.TypeSystem.WellKnownFields.RuntimeFieldHandleImpl_m_value );
                FieldRepresentation             fd       = (FieldRepresentation)odField.Source;
                TypeSystemForCodeTransformation ts       = nc.TypeSystem;

                //
                // Get the field, extract the default value, convert it to an array of the right type, then copy the array.
                //
                ArrayReferenceTypeRepresentation td        = (ArrayReferenceTypeRepresentation)op.SecondArgument.Type;
                ScalarTypeRepresentation         tdElement = td.ContainedType as ScalarTypeRepresentation;
                if(tdElement != null)
                {
                    byte[]               rawData = ts.GetDefaultValue( fd );
                    Type                 t       = tdElement.ConvertToRuntimeType();
                    Array                srcData = Array.CreateInstance( t, rawData.Length / tdElement.Size );
                    MethodRepresentation md      = ts.WellKnownMethods.RuntimeHelpers_InitializeArray2;

                    Buffer.BlockCopy( rawData, 0, srcData, 0, rawData.Length );

                    Expression[] rhs     = new Expression[] { op.FirstArgument, op.SecondArgument, ts.GenerateConstantArrayAccessor( op, td, srcData, null ) };
                    CallOperator callNew = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, rhs );

                    op.SubstituteWithOperator( callNew, Operator.SubstitutionFlags.Default );

                    nc.MarkAsModified();
                    return;
                }
            }

            throw TypeConsistencyErrorException.Create( "Unsupported array initializer in {0}", op );
        }
        
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "MarshalImpl_SizeOf__Type" )]
        private static void Handle_MarshalImpl_SizeOf__Type( PhaseExecution.NotificationContext nc )
        {
            CallOperator                               op  = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation            ts  = nc.TypeSystem;
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            while(true)
            {
                SingleAssignmentOperator opDef;

                VTable vTable = InferVirtualTableFromConstantTypeParameter( ts, cfg, op.SecondArgument, out opDef );
                if(vTable == null)
                {
                    break;
                }

                var opNew = SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, ts.CreateConstantForTypeSize( vTable.TypeInfo ) );

                if(cfg.FindSingleUse( opDef.FirstResult ) == op)
                {
                    opDef.Delete();
                }

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                nc.StopScan();
                return;
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "RuntimeHelpers_get_OffsetToStringData" )]
        private static void Handle_RuntimeHelpers_get_OffsetToStringData( PhaseExecution.NotificationContext nc )
        {
            CallOperator                    op = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation ts = nc.TypeSystem;

            // Return an adjusted offset, accounting for the header size.
            ConstantExpression offset = ts.CreateConstantForFieldOffset( ts.WellKnownFields.StringImpl_FirstChar );
            ConstantExpression headerSize = ts.CreateConstantForTypeSize( ts.WellKnownTypes.Microsoft_Zelig_Runtime_ObjectHeader );
            var opNew = BinaryOperator.New( op.DebugInfo, BinaryOperator.ALU.ADD, false, false, op.FirstResult, offset, headerSize );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "Object_NullCheck" )]
        private static void Handle_Object_NullCheck( PhaseExecution.NotificationContext nc )
        {
            CallOperator                    op = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation ts = nc.TypeSystem;

            NullCheckOperator opNew = NullCheckOperator.New( op.DebugInfo, op.SecondArgument );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_Runtime_AbstractMethodWrapper_AddActivationRecordEvent" )]
        private static void Handle_Microsoft_Zelig_Runtime_AbstractMethodWrapper_AddActivationRecordEvent( PhaseExecution.NotificationContext nc )
        {
            CallOperator                    op = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation ts = nc.TypeSystem;

            if(nc.IsParameterConstant( op, 1 ))
            {
                int ev = nc.ExtractConstantIntParameter( op, 1 );

                var opNew = AddActivationRecordEventOperator.New( op.DebugInfo, (Runtime.ActivationRecordEvents)ev );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CustomAttributeHandler( "Microsoft_Zelig_Runtime_TypeSystem_GenerateUnsafeCastAttribute" )]
        private static void Handle_GenerateUnsafeCast( PhaseExecution.NotificationContext nc ,
                                                       CustomAttributeRepresentation      ca )
        {
            CallOperator op = nc.GetOperatorAndThrowIfNotCall();
            int          offset;

            if(op.TargetMethod is StaticMethodRepresentation)
            {
                offset = 1;
            }
            else
            {
                offset = 0;
            }

            if(op.Arguments.Length != offset + 1)
            {
                throw TypeConsistencyErrorException.Create( "GenerateUnsafeCastAttribute use incompatible with signature for method '{0}'", op.TargetMethod );
            }

            var opNew = SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, op.Arguments[offset] );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ApplyConfigurationSettings) )]
        [CompilationSteps.CustomAttributeHandler( "Microsoft_Zelig_Runtime_ConfigurationOptionAttribute" )]
        private static void Handle_ConfigurationOption( PhaseExecution.NotificationContext nc ,
                                                        CustomAttributeRepresentation      ca )
        {
            var cfgProv = nc.TypeSystem.GetEnvironmentService< IConfigurationProvider >();
            var op      = nc.CurrentOperator;

            if(cfgProv != null)
            {
                string name = (string)ca.FixedArgsValues[0];
                object val;
                
                if(cfgProv.GetValue( name, out val ))
                {
                    ConstantExpression ex = nc.TypeSystem.CreateConstantFromObject( val );

                    if(op is CallOperator)
                    {
                        op.SubstituteWithOperator( SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, ex ), Operator.SubstitutionFlags.Default );

                        nc.MarkAsModified();
                        return;
                    }

                    if(op is LoadFieldOperator)
                    {
                        op.SubstituteWithOperator( SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, ex ), Operator.SubstitutionFlags.Default );

                        nc.MarkAsModified();
                        return;
                    }
                }
            }

            var call = op as CallOperator;
            if(call != null)
            {
                nc.Phase.CallsDataBase.QueueForForcedInlining( call );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "TypeSystemManager_CastToType"              )]
        [CompilationSteps.CallToWellKnownMethodHandler( "TypeSystemManager_CastToTypeNoThrow"       )]
        [CompilationSteps.CallToWellKnownMethodHandler( "TypeSystemManager_CastToSealedType"        )]
        [CompilationSteps.CallToWellKnownMethodHandler( "TypeSystemManager_CastToSealedTypeNoThrow" )]
        private static void Handle_CastToType( PhaseExecution.NotificationContext nc )
        {
            CallOperator       call     = nc.GetOperatorAndThrowIfNotCall();
            Expression         exSrc    = call.SecondArgument;
            ConstantExpression exVTable = call.ThirdArgument as ConstantExpression;

            if(exVTable != null)
            {
                DataManager.ObjectDescriptor od        = (DataManager.ObjectDescriptor)exVTable.Value;
                VTable                       vtable    = (VTable                      )od.Source;
                TypeRepresentation           tdTarget  =                               vtable.TypeInfo;
                TypeRepresentation           tdSource  =                               exSrc.Type;
                TypeRepresentation           tdSource2 =                               nc.TypeSystem.FindSingleConcreteImplementation( tdSource );

                if(tdSource2 != null && tdTarget.CanBeAssignedFrom( tdSource2, null ))
                {
                    var opNew = SingleAssignmentOperator.New( call.DebugInfo, call.FirstResult, exSrc );
                    call.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                    nc.MarkAsModified();
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "TypeImpl_GetTypeFromHandle" )]
        private static void Handle_TypeImpl_GetTypeFromHandle( PhaseExecution.NotificationContext nc )
        {
            CallOperator                               op  = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation            ts  = nc.TypeSystem;
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            ConstantExpression exTd = op.SecondArgument as ConstantExpression;
            if(exTd == null)
            {
                return;
            }

            DataManager.ObjectDescriptor odRuntimeTypeHandle = exTd.Value as DataManager.ObjectDescriptor;
            if(odRuntimeTypeHandle == null)
            {
                return;
            }

            DataManager.ObjectDescriptor odVTable = odRuntimeTypeHandle.Get( (InstanceFieldRepresentation)ts.WellKnownFields.RuntimeTypeHandleImpl_m_value ) as DataManager.ObjectDescriptor;
            if(odVTable == null)
            {
                return;
            }

            op.SubstituteWithOperator( SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, ts.CreateConstantForType( (VTable)odVTable.Source ) ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.CallToWellKnownMethodHandler( "ActivatorImpl_CreateInstanceInner" )]
        private static void Handle_ActivatorImpl_CreateInstanceInner( PhaseExecution.NotificationContext nc )
        {
            CallOperator                               op  = nc.GetOperatorAndThrowIfNotCall();
            TypeSystemForCodeTransformation            ts  = nc.TypeSystem;
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            //
            // We need to substitute the sequence:
            //
            //   opDef : $Temp_1[0](System.Type) = callStatic StaticMethodRepresentation(System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle))( <null value>, $Const(System.RuntimeTypeHandle $Object(System.RuntimeTypeHandle)) )
            //   op    : $Temp_2[1](object) = callStatic StaticMethodRepresentation(object System.Activator::CreateInstanceInner(System.Type))( <null value>, $Temp_1[0](System.Type) )
            //   opUse : <target>(T) = $Temp_2[1](object) cast as T
            //
            // with the sequence:
            //
            //   <target>(T) = new T;
            //   call <target>(T)T()
            //
            while(true)
            {
                SingleAssignmentOperator opDef;

                VTable vTable = InferVirtualTableFromConstantTypeParameter( ts, cfg, op.SecondArgument, out opDef );
                if(vTable == null)
                {
                    break;
                }

                TypeRepresentation tdTarget = vTable.TypeInfo;

                CastOperator opUse = cfg.FindSingleUse( op.FirstResult ) as CastOperator;
                if(opUse == null)
                {
                    break;
                }

                VariableExpression varTarget = opUse.FirstResult;
                if(varTarget.Type.CanBeAssignedFrom( tdTarget, null ) == false)
                {
                    break;
                }

                ConstructorMethodRepresentation mdTarget = tdTarget.FindDefaultConstructor();
                if(mdTarget == null)
                {
                    break;
                }

                op.AddOperatorBefore( ObjectAllocationOperator.New( op.DebugInfo,                               tdTarget,                    varTarget         ) );
                op.AddOperatorBefore( InstanceCallOperator    .New( op.DebugInfo, CallOperator.CallKind.Direct, mdTarget, new Expression[] { varTarget }, true ) );

                if(cfg.FindSingleUse( opDef.FirstResult ) == op)
                {
                    opDef.Delete();
                }

                op   .Delete();
                opUse.Delete();

                nc.StopScan();
                return;
            }

            throw TypeConsistencyErrorException.Create( "INTERNAL ERROR: Unrecognized use of {0}", op.TargetMethod );
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public static VTable InferVirtualTableFromConstantTypeParameter(     TypeSystemForCodeTransformation            ts    ,
                                                                             ControlFlowGraphStateForCodeTransformation cfg   ,
                                                                             Expression                                 ex    ,
                                                                         out SingleAssignmentOperator                   opDef )
        {
            opDef = cfg.FindSingleDefinition( ex ) as SingleAssignmentOperator;
            if(opDef == null)
            {
                return null;
            }

            var exType = opDef.FirstArgument as ConstantExpression;
            if(exType == null)
            {
                return null;
            }

            var dt = exType.Value as TypeSystemForCodeTransformation.DelayedType;
            if(dt != null)
            {
                return dt.VTable;
            }

            return null;
        }
    }
}
