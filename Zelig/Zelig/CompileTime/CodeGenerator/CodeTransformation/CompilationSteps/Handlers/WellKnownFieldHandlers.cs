//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class WellKnownFieldHandlers
    {
        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.PostPhaseHandler]
        private static void Handle_ResourceManagerImpl_s_resources( PhaseDriver                     host       ,
                                                                    TypeSystemForCodeTransformation typeSystem )
        {
            //
            // Only generate the table if the corresponding field is referenced by the application.
            // We can detect that by checking the "ImplementedBy" property of a static field:
            // it will be valid if there's an operator involving the field.
            //
            var fd = (StaticFieldRepresentation)typeSystem.WellKnownFields.ResourceManagerImpl_s_resources;
            if(fd != null)
            {
                InstanceFieldRepresentation fdReal = fd.ImplementedBy;
                if(fdReal != null)
                {
                    DataManager.ObjectDescriptor od = typeSystem.GetGlobalRoot();

                    if(od.Get( fdReal ) == null) // Create only once.
                    {
                        od.ConvertAndSet( fdReal, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, null, typeSystem.Resources.ToArray() );
                    }
                }
            }
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.ReduceTypeSystem) )]
        [CompilationSteps.PostPhaseHandler]
        private static void Handle_GarbageCollectionManager_m_extensionTargets( PhaseDriver                     host       ,
                                                                                TypeSystemForCodeTransformation typeSystem )
        {
            //
            // Only generate the table if the corresponding field is referenced by the application.
            // We can detect that by checking the "ImplementedBy" property of a static field:
            // it will be valid if there's an operator involving the field.
            //
            var fd = (InstanceFieldRepresentation)typeSystem.WellKnownFields.GarbageCollectionManager_m_extensionTargets;
            if(fd != null)
            {
                TypeRepresentation td = fd.OwnerType;
                TypeRepresentation tdNew;

                if(typeSystem.ForcedDevirtualizations.TryGetValue( td, out tdNew ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot devirtualize type '{0}'", td );
                }

                var od = typeSystem.GenerateSingleton( tdNew, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );

                if(od.Get( fd ) == null)
                {
                    var extensions = typeSystem.GarbageCollectionExtensions;
                    var vTables    = new VTable[extensions.Count];
                    int pos        = 0;

                    foreach(var tdSrc in extensions.Keys)
                    {
                        vTables[pos++] = tdSrc.VirtualTable;
                    }

                    od.ConvertAndSet( fd, DataManager.Attributes.Constant, null, vTables );
                }
            }
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.ReduceTypeSystem) )]
        [CompilationSteps.PostPhaseHandler]
        private static void Handle_GarbageCollectionManager_m_extensionHandlers( PhaseDriver                     host       ,
                                                                                 TypeSystemForCodeTransformation typeSystem )
        {
            //
            // Only generate the table if the corresponding field is referenced by the application.
            // We can detect that by checking the "ImplementedBy" property of a static field:
            // it will be valid if there's an operator involving the field.
            //
            var fd = (InstanceFieldRepresentation)typeSystem.WellKnownFields.GarbageCollectionManager_m_extensionHandlers;
            if(fd != null)
            {
                TypeRepresentation td = fd.OwnerType;
                TypeRepresentation tdNew;

                if(typeSystem.ForcedDevirtualizations.TryGetValue( td, out tdNew ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot devirtualize type '{0}'", td );
                }

                var od = typeSystem.GenerateSingleton( tdNew, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );

                if(od.Get( fd ) == null)
                {
                    var extensions = typeSystem.GarbageCollectionExtensions;
                    var handlers   = typeSystem.DataManagerInstance.BuildArrayDescriptor( (ArrayReferenceTypeRepresentation)fd.FieldType, DataManager.Attributes.Mutable, null, null, extensions.Count );
                    int pos        = 0;

                    foreach(var tdSrc in extensions.Keys)
                    {
                        handlers.Set( pos++, typeSystem.DataManagerInstance.BuildObjectDescriptor( extensions[tdSrc], DataManager.Attributes.Mutable, null ) );
                    }

                    od.Set( fd, handlers );
                }
            }
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.WellKnownFieldHandler( "Memory_m_availableMemory" )]
        private static void Handle_Memory_m_availableMemory( PhaseExecution.NotificationContext nc )
        {
            Operator                    op =                              nc.CurrentOperator;
            InstanceFieldRepresentation fd = (InstanceFieldRepresentation)nc.Value;
            TypeRepresentation          td = fd.OwnerType;
            TypeRepresentation          tdNew;

            if(nc.TypeSystem.ForcedDevirtualizations.TryGetValue( td, out tdNew ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Cannot devirtualize type '{0}'", td );
            }

            DataManager.ObjectDescriptor od = nc.TypeSystem.GenerateSingleton( tdNew, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );

            if(od.Get( fd ) == null)
            {
                //
                // This will be updated later in the compilation process, see Core.CreateAvailableMemoryTables()
                //
                od.ConvertAndSet( fd, DataManager.Attributes.Constant, null, new Runtime.Memory.Range[0] );
            }

            //
            // This is never null.
            //
            if(op.HasAnnotation< NotNullAnnotation >() == false)
            {
                op.AddAnnotation( NotNullAnnotation.Create( nc.TypeSystem ) );
            }
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations       ) )]
        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.WellKnownFieldHandler( "ArrayImpl_m_numElements" )]
        private static void Handle_ArrayImpl_m_numElements( PhaseExecution.NotificationContext nc )
        {
            Operator op = nc.CurrentOperator;

            if(op is LoadInstanceFieldOperator)
            {
                if(op.HasAnnotation< ArrayLengthAnnotation >() == false)
                {
                    op.AddAnnotation( ArrayLengthAnnotation.Create( nc.TypeSystem ) );
                }
            }
        }
    }
}
