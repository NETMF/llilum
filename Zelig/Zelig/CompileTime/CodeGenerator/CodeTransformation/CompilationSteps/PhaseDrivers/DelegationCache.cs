//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class DelegationCache
    {
        class Entry
        {
            //
            // State
            //

            System.Reflection.MethodInfo                            m_source;
            Delegate                                                m_dlg;
            AbstractHandlerAttribute                                m_context;
                                                        
            PhaseFilterAttribute[]                                  m_phaseFilters;

            //
            // Constructor Methods
            //

            internal Entry( System.Reflection.MethodInfo source  ,
                            Delegate                     dlg     ,
                            AbstractHandlerAttribute     context )
            {
                m_source       = source;
                m_dlg          = dlg;
                m_context      = context;

                m_phaseFilters = ReflectionHelper.GetAttributes< PhaseFilterAttribute >( source, false );
            }

            //
            // Helper Methods
            //

            internal bool ShouldProcess( PhaseDriver phase ,
                                         int         pass  )
            {
                if(pass == 0)
                {
                    foreach(PhaseFilterAttribute pf in m_phaseFilters)
                    {
                        if(pf.Target == phase.GetType())
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if(m_phaseFilters.Length == 0)
                    {
                        //
                        // No filter => always on. But process after any entry that has a filter.
                        //
                        return true;
                    }
                }

                return false;
            }

            //
            // Access Methods
            //

            internal System.Reflection.MethodInfo Source
            {
                get
                {
                    return m_source;
                }
            }

            internal PhaseDriver.Notification DelegateForPhase
            {
                get
                {
                    return (PhaseDriver.Notification)m_dlg;
                }
            }

            internal PhaseExecution.NotificationOfTransformation DelegateForPhaseExecution
            {
                get
                {
                    return (PhaseExecution.NotificationOfTransformation)m_dlg;
                }
            }

            internal PhaseExecution.NotificationOfTransformationForAttribute DelegateForPhaseExecutionForAttribute
            {
                get
                {
                    return (PhaseExecution.NotificationOfTransformationForAttribute)m_dlg;
                }
            }

            internal ComputeCallsClosure.Notification DelegateForCallClosure
            {
                get
                {
                    return (ComputeCallsClosure.Notification)m_dlg;
                }
            }

            internal AbstractHandlerAttribute Context
            {
                get
                {
                    return m_context;
                }
            }
        }

        //
        // State
        //

        private readonly TypeSystemForCodeTransformation m_typeSystem;
        private          List< Entry >                   m_entries;

        //
        // Constructor Methods
        //

        public DelegationCache( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
            m_entries    = new List< Entry    >();
        }

        //
        // Helper Methods
        //

        public void Register( object instance )
        {
            Type t = instance.GetType();

            while(t != null)
            {
                foreach(var mi in t.GetMethods( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic ))
                {
                    HookCustomAttributeNotifications( mi, instance );
                    HookNewEntityNotifications      ( mi, instance );
                    CreateHandlers                  ( mi, instance );
                }

                var cfgProv = m_typeSystem.GetEnvironmentService< IConfigurationProvider >();
                if(cfgProv != null)
                {
                    foreach(var mi in ReflectionHelper.GetAllFields( t ))
                    {
                        foreach(var attr in ReflectionHelper.GetAttributes< Runtime.ConfigurationOptionAttribute >( mi, false ))
                        {
                            object val;

                            if(cfgProv.GetValue( attr.Name, out val ))
                            {
                                if(mi.IsStatic)
                                {
                                    mi.SetValue( null, val );
                                }
                                else
                                {
                                    mi.SetValue( instance, val );
                                }
                            }
                        }
                    }
                }

                t = t.BaseType;
            }
        }

        private void CreateHandlers( System.Reflection.MethodInfo mi       ,
                                     object                       instance )
        {
            foreach(AbstractHandlerAttribute attrib in ReflectionHelper.GetAttributes< AbstractHandlerAttribute >( mi, false ))
            {
                try
                {
                    Type type;

                    if(attrib is CallClosureHandlerAttribute)
                    {
                        type = typeof(ComputeCallsClosure.Notification);
                    }
                    else if(attrib is PrePhaseHandlerAttribute  ||
                            attrib is PostPhaseHandlerAttribute  )
                    {
                        type = typeof(PhaseDriver.Notification);
                    }
                    else if(attrib is CustomAttributeHandlerAttribute)
                    {
                        type = typeof(PhaseExecution.NotificationOfTransformationForAttribute);
                    }
                    else
                    {
                        type = typeof(PhaseExecution.NotificationOfTransformation);
                    }

                    Entry en = new Entry( mi, CreateNotification( type, mi, instance ), attrib );

                    m_entries.Add( en );
                }
                catch(Exception ex)
                {
                    throw TypeConsistencyErrorException.Create( "Got the following error while trying to create a notification handler for {0}:\n{1}", attrib, ex.ToString() );
                }
            }
        }

        private void HookNewEntityNotifications( System.Reflection.MethodInfo mi       ,
                                                 object                       instance )
        {
            if(ReflectionHelper.HasAttribute< CompilationSteps.NewEntityNotificationAttribute >( mi, false ))
            {
                System.Reflection.ParameterInfo[] parameters = mi.GetParameters();
                if(parameters != null && parameters.Length == 1)
                {
                    Type paramEntity = parameters[0].ParameterType;

                    if(paramEntity == typeof(TypeRepresentation))
                    {
                        var dlg = (TypeSystem.NotificationOfNewType)CreateNotification( typeof(TypeSystem.NotificationOfNewType), mi, instance );

                        m_typeSystem.RegisterForNewType( dlg );
                        return;
                    }

                    if(paramEntity == typeof(FieldRepresentation))
                    {
                        var dlg = (TypeSystem.NotificationOfNewField)CreateNotification( typeof(TypeSystem.NotificationOfNewField), mi, instance );

                        m_typeSystem.RegisterForNewField( dlg );
                        return;
                    }

                    if(paramEntity == typeof(MethodRepresentation))
                    {
                        var dlg = (TypeSystem.NotificationOfNewMethod)CreateNotification( typeof(TypeSystem.NotificationOfNewMethod), mi, instance );

                        m_typeSystem.RegisterForNewMethod( dlg );
                        return;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be used for new entity notification", mi );
            }
        }

        //--//

        private void HookCustomAttributeNotifications( System.Reflection.MethodInfo mi       ,
                                                       object                       instance )
        {
            foreach(var attrib in ReflectionHelper.GetAttributes< CompilationSteps.CustomAttributeNotificationAttribute >( mi, false ))
            {
                TypeRepresentation td = m_typeSystem.GetWellKnownTypeNoThrow( attrib.Target );
                if(td == null)
                {
                    throw TypeConsistencyErrorException.Create( "Failed to create custom attribute notification because type '{0}' is not well-known", attrib.Target );
                }

                System.Reflection.ParameterInfo[] parameters = mi.GetParameters();
                if(parameters != null && parameters.Length >= 3)
                {
                    Type paramTypeKeep  = parameters[0].ParameterType;
                    Type paramTypeCa    = parameters[1].ParameterType;
                    Type paramTypeOwner = parameters[2].ParameterType;

                    if(paramTypeKeep.IsByRef && paramTypeKeep.GetElementType() == typeof(bool)                          &&
                       paramTypeCa                                             == typeof(CustomAttributeRepresentation)  )
                    {
                        if(paramTypeOwner == typeof(BaseRepresentation) && parameters.Length == 3)
                        {
                            var dlg = (TypeSystem.NotificationOfAttributeOnGeneric)CreateNotification( typeof(TypeSystem.NotificationOfAttributeOnGeneric), mi, instance );

                            m_typeSystem.RegisterForNotificationOfAttributeOnGeneric( td, dlg );
                            continue;
                        }

                        if(paramTypeOwner == typeof(TypeRepresentation) && parameters.Length == 3)
                        {
                            var dlg = (TypeSystem.NotificationOfAttributeOnType)CreateNotification( typeof(TypeSystem.NotificationOfAttributeOnType), mi, instance );

                            m_typeSystem.RegisterForNotificationOfAttributeOnType( td, dlg );
                            continue;
                        }

                        if(paramTypeOwner == typeof(FieldRepresentation) && parameters.Length == 3)
                        {
                            var dlg = (TypeSystem.NotificationOfAttributeOnField)CreateNotification( typeof(TypeSystem.NotificationOfAttributeOnField), mi, instance );

                            m_typeSystem.RegisterForNotificationOfAttributeOnField( td, dlg );
                            continue;
                        }

                        if(paramTypeOwner == typeof(MethodRepresentation))
                        {
                            if(parameters.Length == 3)
                            {
                                var dlg = (TypeSystem.NotificationOfAttributeOnMethod)CreateNotification( typeof(TypeSystem.NotificationOfAttributeOnMethod), mi, instance );

                                m_typeSystem.RegisterForNotificationOfAttributeOnMethod( td, dlg );
                                continue;
                            }

                            if(parameters.Length == 4 && parameters[3].ParameterType == typeof(int))
                            {
                                var dlg = (TypeSystem.NotificationOfAttributeOnParam)CreateNotification( typeof(TypeSystem.NotificationOfAttributeOnParam), mi, instance );

                                m_typeSystem.RegisterForNotificationOfAttributeOnParam( td, dlg );
                                continue;
                            }
                        }
                    }
                }

                throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be used for custom attribute notification", mi );
            }
        }

        //--//

        public void HookNotifications( TypeSystemForCodeTransformation typeSystem ,
                                       PhaseDriver                     phase      )
        {
            for(int pass = 0; pass < 2; pass++)
            {
                foreach(Entry en in m_entries)
                {
                    if(en.ShouldProcess( phase, pass ))
                    {
                        if(en.Context is PrePhaseHandlerAttribute)
                        {
                            phase.RegisterForNotification( en.DelegateForPhase, true );
                        }

                        if(en.Context is PostPhaseHandlerAttribute)
                        {
                            phase.RegisterForNotification( en.DelegateForPhase, false );
                        }
                    }
                }
            }
        }

        public void HookNotifications( TypeSystemForCodeTransformation typeSystem ,
                                       PhaseExecution                  pe         ,
                                       PhaseDriver                     phase      )
        {
            for(int pass = 0; pass < 2; pass++)
            {
                foreach(Entry en in m_entries)
                {
                    if(en.ShouldProcess( phase, pass ))
                    {
                        if(en.Context is OptimizationHandlerAttribute)
                        {
                            OptimizationHandlerAttribute ca = (OptimizationHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOptimization( ca, en.DelegateForPhaseExecution );
                        }

                        if(en.Context is PreFlowGraphHandlerAttribute)
                        {
                            pe.RegisterForNotificationOfFlowGraph( en.DelegateForPhaseExecution, true );
                        }

                        if(en.Context is PostFlowGraphHandlerAttribute)
                        {
                            pe.RegisterForNotificationOfFlowGraph( en.DelegateForPhaseExecution, false );
                        }
                                
                        if(en.Context is OperatorHandlerAttribute)
                        {
                            OperatorHandlerAttribute ca = (OperatorHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOperatorsByType( ca.Target, en.DelegateForPhaseExecution );
                        }

                        if(en.Context is OperatorArgumentHandlerAttribute)
                        {
                            OperatorArgumentHandlerAttribute ca = (OperatorArgumentHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOperatorByTypeOfArguments( ca.Target, en.DelegateForPhaseExecution );
                        }

                        if(en.Context is WellKnownTypeHandlerAttribute)
                        {
                            WellKnownTypeHandlerAttribute ca = (WellKnownTypeHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOperatorByEntities( typeSystem.GetWellKnownTypeNoThrow( ca.Target ), en.DelegateForPhaseExecution );
                        }

                        if(en.Context is WellKnownFieldHandlerAttribute)
                        {
                            WellKnownFieldHandlerAttribute ca = (WellKnownFieldHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOperatorByEntities( typeSystem.GetWellKnownFieldNoThrow( ca.Target ), en.DelegateForPhaseExecution );
                        }

                        if(en.Context is WellKnownMethodHandlerAttribute)
                        {
                            WellKnownMethodHandlerAttribute ca = (WellKnownMethodHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfOperatorByEntities( typeSystem.GetWellKnownMethodNoThrow( ca.Target ), en.DelegateForPhaseExecution );
                        }

                        if(en.Context is CallToWellKnownMethodHandlerAttribute)
                        {
                            CallToWellKnownMethodHandlerAttribute ca = (CallToWellKnownMethodHandlerAttribute)en.Context;

                            pe.RegisterForNotificationOfCallOperatorByEntities( typeSystem.GetWellKnownMethodNoThrow( ca.Target ), en.DelegateForPhaseExecution );
                        }

                        if(en.Context is CustomAttributeHandlerAttribute)
                        {
                            CustomAttributeHandlerAttribute ca = (CustomAttributeHandlerAttribute)en.Context;

                            TypeRepresentation targetType = typeSystem.GetWellKnownTypeNoThrow( ca.FieldName );
                            if(targetType != null)
                            {
                                var dlg = en.DelegateForPhaseExecutionForAttribute;

                                typeSystem.EnumerateCustomAttributes( delegate( CustomAttributeAssociationRepresentation caa )
                                {
                                    if(caa.CustomAttribute.Constructor.OwnerType == targetType)
                                    {
                                        BaseRepresentation bdTarget = caa.Target;

                                        if(bdTarget != null)
                                        {
                                            pe.RegisterForNotificationOfOperatorByEntities( bdTarget, nc => dlg( nc, caa.CustomAttribute ) );
                                        }
                                    }
                                } );
                            }
                        }
                    }
                }
            }
        }

        internal void HookNotifications( TypeSystemForCodeTransformation typeSystem ,
                                         ComputeCallsClosure             cc         ,
                                         PhaseDriver                     phase      )
        {
            for(int pass = 0; pass < 2; pass++)
            {
                foreach(Entry en in m_entries)
                {
                    if(en.ShouldProcess( phase, pass ))
                    {
                        if(en.Context is CallClosureHandlerAttribute)
                        {
                            CallClosureHandlerAttribute ca = (CallClosureHandlerAttribute)en.Context;

                            cc.RegisterForNotification( ca.Target, en.DelegateForCallClosure );
                        }
                    }
                }
            }
        }

        //--//

        private static Delegate CreateNotification( Type                         type     ,
                                                    System.Reflection.MethodInfo mi       ,
                                                    object                       instance )
        {
            try
            {
                if(mi.IsStatic)
                {
                    return Delegate.CreateDelegate( type, mi, true );
                }
                else
                {
                    return Delegate.CreateDelegate( type, instance, mi, true );
                }
            }
            catch(Exception ex)
            {
                throw TypeConsistencyErrorException.Create( "Got the following error while trying to create a notification handler for {0}:\n{1}", mi, ex.ToString() );
            }
        }
    }
}
