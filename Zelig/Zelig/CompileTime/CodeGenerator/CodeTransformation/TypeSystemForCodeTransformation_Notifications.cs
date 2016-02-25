//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class TypeSystemForCodeTransformation
    {
        [Runtime.ConfigurationOption( "ExcludeDebuggerHooks" )] public bool ExcludeDebuggerHooks;

        //--//

        [CompilationSteps.NewEntityNotification]
        private void Notify_NewBoxedType( TypeRepresentation td )
        {
            //
            // Make sure that there's a boxed representation for each value type.
            //
            if(td is ValueTypeRepresentation)
            {
                CreateBoxedValueType( td );
            }
        }

        [CompilationSteps.NewEntityNotification]
        private void Notify_CheckArrays( TypeRepresentation td )
        {
            if(td is SzArrayReferenceTypeRepresentation)
            {
                var elementType = td.ContainedType;

                //
                // For a non-generic array, we need to instantiate also the helper class.
                // It will provide the implementation for the runtime methods.
                //
                if(elementType.IsOpenType == false)
                {
                    CreateInstantiationOfGenericTemplate( this.WellKnownTypes.Microsoft_Zelig_Runtime_SZArrayHelper, elementType );
                }
            }
        }

        [CompilationSteps.NewEntityNotification]
        private void Notify_CheckForComparerInstantiation( MethodRepresentation md )
        {
            if(md.Name == "CreateComparer")
            {
                var td = md.OwnerType;

                if(td.IsGenericInstantiation)
                {
                    if(td.GenericTemplate == this.WellKnownTypes.System_Collections_Generic_Comparer_of_T)
                    {
                        var tdParam = td.GenericParameters[0];

                        ////    private static Comparer<T> CreateComparer()
                        ////    {
                        ////        Type t = typeof( T );
                        ////
                        ////        // If T implements IComparable<T> return a GenericComparer<T>
                        ////        if(typeof( IComparable<T> ).IsAssignableFrom( t ))
                        ////        {
                        ////            //return (Comparer<T>)Activator.CreateInstance(typeof(GenericComparer<>).MakeGenericType(t));
                        ////            return (Comparer<T>)(typeof( GenericComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( t ));
                        ////        }
                        ////
                        ////        // If T is a Nullable<U> where U implements IComparable<U> return a NullableComparer<U>
                        ////        if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof( Nullable<> ))
                        ////        {
                        ////            Type u = t.GetGenericArguments()[0];
                        ////            if(typeof( IComparable<> ).MakeGenericType( u ).IsAssignableFrom( u ))
                        ////            {
                        ////                //return (Comparer<T>)Activator.CreateInstance(typeof(NullableComparer<>).MakeGenericType(u));
                        ////                return (Comparer<T>)(typeof( NullableComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( u ));
                        ////            }
                        ////        }
                        ////
                        ////        // Otherwise return an ObjectComparer<T>
                        ////        return new ObjectComparer<T>();
                        ////    }

                        if(tdParam.FindInstantiationOfGenericInterface( (InterfaceTypeRepresentation)this.WellKnownTypes.System_IComparable_of_T, tdParam ) != null)
                        {
                            var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_GenericComparer_of_T, tdParam );

                            CreateComparerHelper( md, newTd );
                        }

                        if(tdParam.GenericTemplate == this.WellKnownTypes.System_Nullable_of_T)
                        {
                            var tdParam2 = tdParam.GenericParameters[0];

                            if(td.FindInstantiationOfGenericInterface( (InterfaceTypeRepresentation)this.WellKnownTypes.System_IComparable_of_T, tdParam2 ) != null)
                            {
                                var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_NullableComparer_of_T, tdParam2 );

                                CreateComparerHelper( md, newTd );
                                return;
                            }
                        }

                        {
                            var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_ObjectComparer_of_T, tdParam );

                            CreateComparerHelper( md, newTd );
                            return;
                        }
                    }
                }
            }
        }

        [CompilationSteps.NewEntityNotification]
        private void Notify_CheckForEqualityComparerInstantiation( MethodRepresentation md )
        {
            if(md.Name == "CreateComparer")
            {
                var td = md.OwnerType;

                if(td.IsGenericInstantiation)
                {
                    if(td.GenericTemplate == this.WellKnownTypes.System_Collections_Generic_EqualityComparer_of_T)
                    {
                        var tdParam = td.GenericParameters[0];

                        ////    private static EqualityComparer<T> CreateComparer()
                        ////    {
                        ////        Type t = typeof( T );
                        ////
                        ////        // Specialize type byte for performance reasons
                        ////        if(t == typeof( byte ))
                        ////        {
                        ////            return (EqualityComparer<T>)(object)(new ByteEqualityComparer());
                        ////        }
                        ////
                        ////        // If T implements IEquatable<T> return a GenericEqualityComparer<T>
                        ////        if(typeof( IEquatable<T> ).IsAssignableFrom( t ))
                        ////        {
                        ////            //return (EqualityComparer<T>)Activator.CreateInstance(typeof(GenericEqualityComparer<>).MakeGenericType(t));
                        ////            return (EqualityComparer<T>)(typeof( GenericEqualityComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( t ));
                        ////        }
                        ////
                        ////        // If T is a Nullable<U> where U implements IEquatable<U> return a NullableEqualityComparer<U>
                        ////        if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof( Nullable<> ))
                        ////        {
                        ////            Type u = t.GetGenericArguments()[0];
                        ////            if(typeof( IEquatable<> ).MakeGenericType( u ).IsAssignableFrom( u ))
                        ////            {
                        ////                //return (EqualityComparer<T>)Activator.CreateInstance(typeof(NullableEqualityComparer<>).MakeGenericType(u));
                        ////                return (EqualityComparer<T>)(typeof( NullableEqualityComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( u ));
                        ////            }
                        ////        }
                        ////
                        ////        // Otherwise return an ObjectEqualityComparer<T>
                        ////        return new ObjectEqualityComparer<T>();
                        ////    }

                        if(tdParam.FindInstantiationOfGenericInterface( (InterfaceTypeRepresentation)this.WellKnownTypes.System_IEquatable_of_T, tdParam ) != null)
                        {
                            var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_GenericEqualityComparer_of_T, tdParam );

                            CreateComparerHelper( md, newTd );
                            return;
                        }

                        if(tdParam.Extends == this.WellKnownTypes.System_Enum)
                        {
                            TypeRepresentation tdTemplate = null;
                            if(     tdParam.UnderlyingType == this.WellKnownTypes.System_SByte ||
                                    tdParam.UnderlyingType == this.WellKnownTypes.System_Byte   )
                            {
                                tdTemplate = this.WellKnownTypes.System_Collections_Generic_EqualityComparer_of_Enum_sbyte;
                            }
                            else if(tdParam.UnderlyingType == this.WellKnownTypes.System_Int16 ||
                                    tdParam.UnderlyingType == this.WellKnownTypes.System_UInt16 )
                            {
                                tdTemplate = this.WellKnownTypes.System_Collections_Generic_EqualityComparer_of_Enum_short;
                            }
                            else if(tdParam.UnderlyingType == this.WellKnownTypes.System_Int32 ||
                                    tdParam.UnderlyingType == this.WellKnownTypes.System_UInt32 )
                            {
                                tdTemplate = this.WellKnownTypes.System_Collections_Generic_EqualityComparer_of_Enum;
                            }
                            else if(tdParam.UnderlyingType == this.WellKnownTypes.System_Int64 ||
                                    tdParam.UnderlyingType == this.WellKnownTypes.System_UInt64 )
                            {
                                tdTemplate = this.WellKnownTypes.System_Collections_Generic_EqualityComparer_of_Enum_long;
                            }
                            else
                            {
                                throw TypeConsistencyErrorException.Create( "Enum {0} is not backed by a known underlying type", td );
                            }

                            var newTd = CreateInstantiationOfGenericTemplate( tdTemplate, tdParam );

                            CreateComparerHelper( md, newTd );
                            return;
                        }

                        if(tdParam.GenericTemplate == this.WellKnownTypes.System_Nullable_of_T)
                        {
                            var tdParam2 = tdParam.GenericParameters[0];

                            if(td.FindInstantiationOfGenericInterface( (InterfaceTypeRepresentation)this.WellKnownTypes.System_IEquatable_of_T, tdParam2 ) != null)
                            {
                                var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_NullableEqualityComparer_of_T, tdParam2 );

                                CreateComparerHelper( md, newTd );
                                return;
                            }
                        }

                        {
                            var newTd = CreateInstantiationOfGenericTemplate( this.WellKnownTypes.System_Collections_Generic_ObjectEqualityComparer_of_T, tdParam );

                            CreateComparerHelper( md, newTd );
                            return;
                        }
                    }
                }
            }
        }

        private void CreateComparerHelper( MethodRepresentation md    ,
                                           TypeRepresentation   newTd )
        {
            var cfg = (ControlFlowGraphStateForCodeTransformation)this.CreateControlFlowGraphState( md );
            var bb  = cfg.CreateFirstNormalBasicBlock();

            //
            // Allocate helper.
            //
            bb.AddOperator( ObjectAllocationOperator.New( null, newTd, cfg.ReturnValue ) );

            cfg.AddReturnOperator();
        }

        //--//--//

////    [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Internals_TypeDependencyAttribute"          )]
////    [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_TypeDependencyAttribute" )]
////    private void Notify_TypeDependencyAttribute( ref bool                          fKeep ,
////                                                     CustomAttributeRepresentation ca    ,
////                                                     TypeRepresentation            owner )
////    {
////    }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Internals_WellKnownFieldAttribute"          )]
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_WellKnownFieldAttribute" )]
        private void Notify_WellKnownFieldAttribute( ref bool                          fKeep ,
                                                         CustomAttributeRepresentation ca    ,
                                                         FieldRepresentation           owner )
        {
            fKeep = false;
            
            //
            // Filter methods that belong to types that do not belong to the platform compiled
            //            
            CustomAttributeRepresentation sfpf = owner.OwnerType.FindCustomAttribute( this.WellKnownTypes.Microsoft_Zelig_Runtime_ExtendClassAttribute );
            if(sfpf != null)
            {
                object obj = sfpf.GetNamedArg( "PlatformVersionFilter" );
                if(obj != null)
                {
                    uint filter = (uint)obj;

                    if((filter & this.PlatformAbstraction.PlatformVersion) != this.PlatformAbstraction.PlatformVersion)
                    {
                        // This type is not an allowed extension for the current platform
                        return;
                    }
                }
            }

            string              fieldName = (string)ca.FixedArgsValues[0];
            FieldRepresentation fdOld     = this.GetWellKnownFieldNoThrow( fieldName );

            if(fdOld != null && fdOld != owner)
            {
                throw TypeConsistencyErrorException.Create( "Found the well-known field '{0}' defined more than once: {1} and {2}", fieldName, fdOld, owner );
            }


            this.SetWellKnownField( fieldName, owner );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Internals_WellKnownMethodAttribute"          )]
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_WellKnownMethodAttribute" )]
        private void Notify_WellKnownMethodAttribute( ref bool                          fKeep ,
                                                          CustomAttributeRepresentation ca    ,
                                                          MethodRepresentation          owner )
        {
            fKeep = false;

            if(owner.IsGenericInstantiation)
            {
                owner = owner.GenericTemplate;
            }
            
            //
            // Filter methods that belong to types that do not belong to the platform compiled
            //            
            CustomAttributeRepresentation sfpf = owner.OwnerType.FindCustomAttribute( this.WellKnownTypes.Microsoft_Zelig_Runtime_ExtendClassAttribute );
            if(sfpf != null)
            {
                object obj = sfpf.GetNamedArg( "PlatformVersionFilter" );
                if(obj != null)
                {
                    uint filter = (uint)obj;

                    if((filter & this.PlatformAbstraction.PlatformVersion) != this.PlatformAbstraction.PlatformVersion)
                    {
                        // This type is not an allowed extension for the current platform
                        return;
                    }
                }
            }

            string               methodName = (string)ca.FixedArgsValues[0];
            MethodRepresentation mdOld      = this.GetWellKnownMethodNoThrow( methodName );

            if(mdOld != null && mdOld != owner)
            {
                throw TypeConsistencyErrorException.Create( "Found the well-known method '{0}' defined more than once: {1} and {2}", methodName, mdOld, owner );
            }

            this.SetWellKnownMethod( methodName, owner );
        }

        //--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_ForceDevirtualizationAttribute" )]
        private void Notify_ForceDevirtualizationAttribute( ref bool                          fKeep ,
                                                                CustomAttributeRepresentation ca    ,
                                                                TypeRepresentation            owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.ForceDevirtualization;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_ImplicitInstanceAttribute" )]
        private void Notify_ImplicitInstanceAttribute( ref bool                          fKeep ,
                                                           CustomAttributeRepresentation ca    ,
                                                           TypeRepresentation            owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.ImplicitInstance;
        }

        [CompilationSteps.CustomAttributeNotification( "System_FlagsAttribute" )]
        private void Notify_FlagsAttribute( ref bool                          fKeep ,
                                                CustomAttributeRepresentation ca    ,
                                                TypeRepresentation            owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.FlagsAttribute;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_InlineAttribute" )]
        private void Notify_InlineAttribute( ref bool                          fKeep ,
                                                 CustomAttributeRepresentation ca    ,
                                                 MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.Inline;
            owner.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.NoInline;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_NoInlineAttribute" )]
        private void Notify_NoInlineAttribute( ref bool                          fKeep ,
                                                   CustomAttributeRepresentation ca    ,
                                                   MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.NoInline;
            owner.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.Inline;
        }
    
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_BottomOfCallStackAttribute" )]
        private void Notify_BottomOfCallStackAttribute( ref bool                          fKeep ,
                                                            CustomAttributeRepresentation ca    ,
                                                            MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.BottomOfCallStack;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_SaveFullProcessorContextAttribute" )]
        private void Notify_SaveFullProcessorContextAttribute( ref bool                          fKeep ,
                                                                   CustomAttributeRepresentation ca    ,
                                                                   MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_NoReturnAttribute" )]
        private void Notify_NoReturnAttribute( ref bool                          fKeep ,
                                                   CustomAttributeRepresentation ca    ,
                                                   MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.NoReturn;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_CannotAllocateAttribute" )]
        private void Notify_CannotAllocateAttribute( ref bool                          fKeep ,
                                                         CustomAttributeRepresentation ca    ,
                                                         MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.CannotAllocate;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_CanAllocateOnReturnAttribute" )]
        private void Notify_CanAllocateOnReturnAttribute( ref bool                          fKeep ,
                                                              CustomAttributeRepresentation ca    ,
                                                              MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.CanAllocateOnReturn;
        }
                                                                                
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_StackNotAvailableAttribute" )]
        private void Notify_StackNotAvailableAttribute( ref bool                          fKeep ,
                                                            CustomAttributeRepresentation ca    ,
                                                            MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.StackNotAvailable;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_StackAvailableOnReturnAttribute" )]
        private void Notify_StackAvailableOnReturnAttribute( ref bool                          fKeep ,
                                                                 CustomAttributeRepresentation ca    ,
                                                                 MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.StackAvailableOnReturn;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_DisableBoundsChecksAttribute" )]
        private void Notify_DisableBoundsChecksAttribute( ref bool                          fKeep ,
                                                              CustomAttributeRepresentation ca    ,
                                                              MethodRepresentation          owner )
        {
            fKeep = false;

            if(ca.GetNamedArg( "ApplyRecursively" ) != null)
            {
                owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.DisableDeepBoundsChecks;
            }
            else
            {
                owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.DisableBoundsChecks;
            }
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_DisableNullChecksAttribute" )]
        private void Notify_DisableNullChecksAttribute( ref bool                          fKeep ,
                                                            CustomAttributeRepresentation ca    ,
                                                            MethodRepresentation          owner )
        {
            fKeep = false;

            if(ca.GetNamedArg( "ApplyRecursively" ) != null)
            {
                owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.DisableDeepNullChecks;
            }
            else
            {
                owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.DisableNullChecks;
            }
        }

        //LON: 2/16/09
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_ImportedMethodReferenceAttribute" )]
        private void Notify_ImportedMethodReferenceAttribute( ref bool                          fKeep ,
                                                                  CustomAttributeRepresentation ca    ,
                                                                  MethodRepresentation          owner )
        {
            fKeep = true;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.Imported;

            // for now disable inlining of these methods
            owner.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.NoInline;
            owner.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.Inline;
        }

        //LON: 2/16/09
        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_ExportedMethodAttribute" )]
        private void Notify_ExportedMethodAttribute( ref bool                          fKeep ,
                                                         CustomAttributeRepresentation ca    ,
                                                         MethodRepresentation          owner )
        {
            fKeep = false;

            owner.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.Exported;

            // for now disable inlining of these methods
            owner.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.NoInline;
            owner.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.Inline;

            if(this.PlatformAbstraction.PlatformVersion == TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv7M ||
               this.PlatformAbstraction.PlatformVersion == TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv6M ||
               this.PlatformAbstraction.PlatformVersion == TargetModel.Win32.InstructionSetVersion.Platform_Version__x86            )
            {
                CustomAttributeRepresentation cf = owner.FindCustomAttribute( this.WellKnownTypes.Microsoft_Zelig_Runtime_CapabilitiesFilterAttribute );
                if(cf != null)
                {
                    object obj = cf.GetNamedArg( "RequiredCapabilities" );
                    if(obj != null)
                    {
                        uint capabilities = (uint)obj;

                        uint reqFamily          = capabilities & TargetModel.ArmProcessor.InstructionSetVersion.Platform_Family__Mask;
                        uint reqVFP             = capabilities & TargetModel.ArmProcessor.InstructionSetVersion.Platform_VFP__Mask;
                        uint reqPlatformVersion = capabilities & TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__Mask;

                        if(reqFamily != 0 && reqFamily != this.PlatformAbstraction.PlatformFamily)
                        {
                            // This method does not conform to the capabilities of the platform we are compiling
                            return;
                        }
                        else if(reqVFP != 0 && reqVFP != this.PlatformAbstraction.PlatformVFP)
                        {
                            // This method does not conform to the capabilities of the platform we are compiling
                            return;
                        }
                        else if(reqPlatformVersion != 0 && reqPlatformVersion != this.PlatformAbstraction.PlatformVersion)
                        {
                            // This method does not conform to the capabilities of the platform we are compiling
                            return;
                        }
                    }
                }

                ExportedMethods.Add(owner);
            }
        }

        //--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_GarbageCollectionExtensionAttribute" )]
        private void Notify_GarbageCollectionExtensionAttribute( ref bool                          fKeep ,
                                                                     CustomAttributeRepresentation ca    ,
                                                                     TypeRepresentation            owner )
        {
            var td = (TypeRepresentation)ca.FixedArgsValues[0];

            m_garbageCollectionExtensions.Add( td, owner );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_SkipDuringGarbageCollectionAttribute" )]
        private void Notify_SkipDuringGarbageCollectionAttribute( ref bool                          fKeep ,
                                                                      CustomAttributeRepresentation ca    ,
                                                                      FieldRepresentation           owner )
        {
            m_garbageCollectionExclusions.Insert( owner );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_DisableReferenceCountingAttribute" )]
        private void Notify_EnableReferenceCountingAttribute( ref bool                          fKeep,
                                                                  CustomAttributeRepresentation ca,
                                                                  TypeRepresentation            owner )
        {
            m_referenceCountingExcludedTypes.Insert( owner );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_DisableAutomaticReferenceCountingAttribute" )]
        private void Notify_DisableAutomaticReferenceCountingAttribute( ref bool                          fKeep,
                                                                            CustomAttributeRepresentation ca,
                                                                            BaseRepresentation            owner )
        {
            if(owner is MethodRepresentation)
            {
                AddAutomaticReferenceCountingExclusion( (MethodRepresentation)owner );
            }
            else if(owner is TypeRepresentation)
            {
                var tr = (TypeRepresentation)owner;

                foreach(var mr in tr.Methods)
                {
                    AddAutomaticReferenceCountingExclusion( mr );
                }
            }
        }

        private void AddAutomaticReferenceCountingExclusion( MethodRepresentation md )
        {
            if(md.IsGenericInstantiation)
            {
                md = md.GenericTemplate;
            }

            var methodsList = m_automaticReferenceCountingExclusions.GetValue( md.Name );
            if(methodsList == null)
            {
                methodsList = new List<MethodRepresentation>( );
                m_automaticReferenceCountingExclusions.Add( md.Name, methodsList );
            }

            if(!methodsList.Contains( md ))
            {
                methodsList.Add( md );
            }
        }

        //--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_AlignmentRequirementsAttribute" )]
        private void Notify_AlignmentRequirementsAttribute( ref bool                          fKeep ,
                                                                CustomAttributeRepresentation ca    ,
                                                                BaseRepresentation            owner )
        {
            Abstractions.PlacementRequirements pr = CreatePlacementRequirements( owner );

            pr.Alignment       = (uint)ca.FixedArgsValues[0];
            pr.AlignmentOffset = (int )ca.FixedArgsValues[1];
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_MemoryRequirementsAttribute" )]
        private void Notify_MemoryRequirementsAttribute( ref bool                          fKeep ,
                                                             CustomAttributeRepresentation ca    ,
                                                             BaseRepresentation            owner )
        {
            Abstractions.PlacementRequirements pr = CreatePlacementRequirements( owner );

            pr.AddConstraint( (Runtime.MemoryAttributes)ca.FixedArgsValues[0] );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_MemoryUsageAttribute" )]
        private void Notify_MemoryUsageAttribute( ref bool                          fKeep ,
                                                      CustomAttributeRepresentation ca    ,
                                                      BaseRepresentation            owner )
        {
            Abstractions.PlacementRequirements pr = CreatePlacementRequirements( owner );

            pr.AddConstraint( (Runtime.MemoryUsage)ca.FixedArgsValues[0]                          );
            pr.AddConstraint(                      ca.GetNamedArg<string>( "SectionName", null  ) );

            pr.ContentsUninitialized   = ca.GetNamedArg( "ContentsUninitialized"  , false );
            pr.AllocateFromHighAddress = ca.GetNamedArg( "AllocateFromHighAddress", false );
        }

        //--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_HardwareExceptionHandlerAttribute" )]
        private void Notify_HardwareExceptionHandlerAttribute( ref bool                          fKeep ,
                                                                   CustomAttributeRepresentation ca    ,
                                                                   MethodRepresentation          owner )
        {
            m_hardwareExceptionHandlers[ owner ] = ca;
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_DebuggerHookHandlerAttribute" )]
        private void Notify_DebuggerHookHandlerAttribute( ref bool                          fKeep ,
                                                              CustomAttributeRepresentation ca    ,
                                                              MethodRepresentation          owner )
        {
            if(this.ExcludeDebuggerHooks == false)
            {
                m_debuggerHookHandlers[owner] = ca;
            }
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_SingletonFactoryAttribute" )]
        private void Notify_SingletonFactoryAttribute( ref bool                          fKeep ,
                                                           CustomAttributeRepresentation ca    ,
                                                           MethodRepresentation          owner )
        {
            m_singletonFactories[ owner ] = ca;

            var tdFallback = ca.GetNamedArg( "Fallback" ) as TypeRepresentation;
            if(tdFallback != null)
            {
                m_singletonFactoriesFallback[owner.OwnerType] = tdFallback;
            }
        }

        //--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_TypeSystem_NoVTableAttribute" )]
        private void Notify_NoVTableAttribute( ref bool                          fKeep ,
                                                   CustomAttributeRepresentation ca    ,
                                                   TypeRepresentation            owner )
        {
            owner.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.NoVTable;
        }

        //--//--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_MemoryMappedPeripheralAttribute" )]
        private void Notify_MemoryMappedPeripheralAttribute( ref bool                          fKeep ,
                                                                 CustomAttributeRepresentation ca    ,
                                                                 TypeRepresentation            owner )
        {
            m_memoryMappedPeripherals[ owner ] = ca;

            RegisterAsMemoryMappedPeripheral( owner );

            if(owner is ConcreteReferenceTypeRepresentation)
            {
                CheckRequiredFieldForMemoryMappedPeripheralAttribute( owner, "Base"   );
                CheckRequiredFieldForMemoryMappedPeripheralAttribute( owner, "Length" );
            }
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_RegisterAttribute" )]
        private void Notify_RegisterAttribute( ref bool                          fKeep ,
                                                   CustomAttributeRepresentation ca    ,
                                                   FieldRepresentation           owner )
        {
            CheckRequiredField( ca, owner, "Offset" );

            m_registerAttributes[ owner ] = ca;

            RegisterAsMemoryMappedPeripheral( owner.OwnerType );
        }

        private void RegisterAsMemoryMappedPeripheral( TypeRepresentation td )
        {
            for(var td2 = td; td2 != null && td2 != this.WellKnownTypes.System_Object; td2 = td2.Extends)
            {
                if(m_memoryMappedPeripherals.ContainsKey( td2 ) == false)
                {
                    if(td == td2)
                    {
                        throw TypeConsistencyErrorException.Create( "'{0}' is not marked as a memory-mapped class", td.FullName );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot have memory-mapped class derive from a non-memory-mapped one: '{0}' => '{1}'", td.FullName, td2.FullName );
                    }
                }
            }

            td.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.NoVTable;

            RegisterForTypeLayoutDelegation( td, Notify_LayoutMemoryMappedPeripheral );
        }

        //--//

        private bool Notify_LayoutMemoryMappedPeripheral( TypeRepresentation                td      ,
                                                          GrowOnlySet< TypeRepresentation > history )
        {
            uint maxSize = 0;

            {
                TypeRepresentation tdSize = td;

                while(tdSize != null)
                {
                    CustomAttributeRepresentation ca;
                    
                    if(m_memoryMappedPeripherals.TryGetValue( tdSize, out ca ))
                    {
                        object sizeObj = ca.GetNamedArg( "Length" );
                        if(sizeObj != null)
                        {
                            maxSize = (uint)sizeObj;
                            break;
                        }
                    }

                    tdSize = tdSize.Extends;
                }
            }

            foreach(FieldRepresentation fd in td.Fields)
            {
                if(fd is InstanceFieldRepresentation)
                {
                    CustomAttributeRepresentation caFd;
                    
                    if(m_registerAttributes.TryGetValue( fd, out caFd ) == false)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot have non-register field '{0}' in memory-mapped class '{1}'", fd.Name, td.FullNameWithAbbreviation );
                    }

                    var tdField = fd.FieldType;

                    EnsureTypeLayout( tdField, history, this.PlatformAbstraction.MemoryAlignment );

                    fd.Offset = (int)(uint)caFd.GetNamedArg( "Offset" );

                    maxSize = Math.Max( (uint)(fd.Offset + tdField.SizeOfHoldingVariable), maxSize );
                }
            }

            td.Size = maxSize;

            return true;
        }

        //--//--//

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_BitFieldPeripheralAttribute" )]
        private void Notify_BitFieldPeripheralAttribute( ref bool                          fKeep ,
                                                             CustomAttributeRepresentation ca    ,
                                                             TypeRepresentation            owner )
        {
            CheckRequiredField( ca, owner, "PhysicalType" );

            owner.BuildTimeFlags |= TypeRepresentation.BuildTimeAttributes.NoVTable;

            m_memoryMappedBitFieldPeripherals[ owner ] = ca;
    
            RegisterForTypeLayoutDelegation( owner, Layout_LayoutMemoryMappedBitFieldPeripheral );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_BitFieldRegisterAttribute" )]
        private void Notify_BitFieldRegisterAttribute( ref bool                          fKeep ,
                                                           CustomAttributeRepresentation ca    ,
                                                           FieldRepresentation           owner )
        {
            var sec = CreateSectionOfBitFieldDefinition( ca, owner );
            
            HandleBitFieldRegisterAttributeCommon( ref fKeep, ca, owner, sec );
        }

        [CompilationSteps.CustomAttributeNotification( "Microsoft_Zelig_Runtime_BitFieldSplitRegisterAttribute" )]
        private void Notify_BitFieldSplitRegisterAttribute( ref bool                          fKeep ,
                                                                CustomAttributeRepresentation ca    ,
                                                                FieldRepresentation           owner )
        {
            var sec = CreateSectionOfBitFieldDefinition( ca, owner );
            
            if(ca.TryToGetNamedArg( "Offset", out sec.Offset ) == false)
            {
                FailedRequiredField( ca, owner, "Offset" );
            }

            HandleBitFieldRegisterAttributeCommon( ref fKeep, ca, owner, sec );
        }

        private BitFieldDefinition.Section CreateSectionOfBitFieldDefinition( CustomAttributeRepresentation ca    ,
                                                                              FieldRepresentation           owner )
        {
            BitFieldDefinition.Section sec = new BitFieldDefinition.Section();

            if(ca.TryToGetNamedArg( "Position", out sec.Position ) == false)
            {
                FailedRequiredField( ca, owner, "Position" );
            }

            if(ca.TryToGetNamedArg( "Size", out sec.Size ) == false)
            {
                if(owner.FieldType != this.WellKnownTypes.System_Boolean)
                {
                    FailedRequiredField( ca, owner, "Size" );
                }

                sec.Size = 1;
            }

            ca.TryToGetNamedArg( "Modifiers", out sec.Modifiers );
            ca.TryToGetNamedArg( "ReadAs"   , out sec.ReadsAs   );
            ca.TryToGetNamedArg( "WriteAs"  , out sec.WritesAs  );

            return sec;
        }

        private void HandleBitFieldRegisterAttributeCommon( ref bool                          fKeep ,
                                                                CustomAttributeRepresentation ca    ,
                                                                FieldRepresentation           owner ,
                                                                BitFieldDefinition.Section    sec   )
        {
            fKeep = false;

            if(m_memoryMappedBitFieldPeripherals.ContainsKey( owner.OwnerType ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Containing type '{0}' of bitfield register '{1}' must be a bitfield peripheral", owner.OwnerType.FullName, owner );
            }

            BitFieldDefinition bfDef;

            if(m_bitFieldRegisterAttributes.TryGetValue( owner, out bfDef ) == false)
            {
                bfDef = new BitFieldDefinition();

                m_bitFieldRegisterAttributes[ owner ] = bfDef;
            }

            bfDef.AddSection( sec );
        }

        //--//

        private bool Layout_LayoutMemoryMappedBitFieldPeripheral( TypeRepresentation                td      ,
                                                                  GrowOnlySet< TypeRepresentation > history )
        {
            CustomAttributeRepresentation ca         = m_memoryMappedBitFieldPeripherals[td];
            TypeRepresentation            tdPhysical = ca.GetNamedArg< TypeRepresentation >( "PhysicalType" );

            EnsureTypeLayout( tdPhysical, history, this.PlatformAbstraction.MemoryAlignment );

            td.Size = tdPhysical.Size;

            foreach(FieldRepresentation fd in td.Fields)
            {
                if(fd is StaticFieldRepresentation)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot have static field '{0}' in memory-mapped bitfield class '{1}'", fd.Name, td.FullNameWithAbbreviation );
                }

                if(m_bitFieldRegisterAttributes.ContainsKey( fd ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot have non-bitfield register field '{0}' in memory-mapped bitfield class '{1}'", fd.Name, td.FullNameWithAbbreviation );
                }

                fd.Offset = 0;
            }

            return true;
        }

        //--//--//

        private void CheckRequiredFieldForMemoryMappedPeripheralAttribute( TypeRepresentation target ,
                                                                           string             name   )
        {
            for(TypeRepresentation td = target; td != null; td = td.Extends)
            {
                CustomAttributeRepresentation ca;

                if(m_memoryMappedPeripherals.TryGetValue( td, out ca ))
                {
                    if(ca.HasNamedArg( name ))
                    {
                        return;
                    }
                }
            }

            throw TypeConsistencyErrorException.Create( "Cannot find required '{0}' field for attribute MemoryMappedPeripheralAttribute on the hierarchy for {2}", name, target );
        }

        private static void CheckRequiredField( CustomAttributeRepresentation ca    ,
                                                object                        owner ,
                                                string                        name  )
        {
            if(ca.HasNamedArg( name ) == false)
            {
                FailedRequiredField( ca, owner, name );
            }
        }

        private static void FailedRequiredField( CustomAttributeRepresentation ca    ,
                                                 object                        owner ,
                                                 string                        name  )
        {
            throw TypeConsistencyErrorException.Create( "Missing required '{0}' field for attribute '{1}' on '{2}'", name, ca.Constructor.OwnerType.FullName, owner );
        }
    }
}
