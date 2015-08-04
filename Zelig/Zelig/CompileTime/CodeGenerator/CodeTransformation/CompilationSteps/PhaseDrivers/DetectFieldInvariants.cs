//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal static class DetectFieldInvariants
    {
        //
        // Helper Methods
        //

        internal static void Execute( TypeSystemForCodeTransformation typeSystem )
        {
            Transformations.LocateFieldsInCode lfic = new Transformations.LocateFieldsInCode( typeSystem );

            lfic.Run();

            foreach(FieldRepresentation fd in lfic.FieldReferences.Keys)
            {
                GrowOnlySet< Operator > set = lfic.FieldReferences[fd];

                StoreFieldOperator opStore    = null;
                bool               fCandidate = true;

                foreach(Operator op in set)
                {
                    if(op is StoreFieldOperator)
                    {
                        if(opStore != null)
                        {
                            //
                            // More than one store, let's give up.
                            //
                            // TODO: what if all the stores have the same shape?
                            //
                            fCandidate = false;
                            break;
                        }

                        opStore = (StoreFieldOperator)op;
                    }
                    else if(op is LoadAddressOperator)
                    {
                        //
                        // Address taken, not interesting.
                        //
                        fCandidate = false;
                        break;
                    }
                }

                if(fCandidate && opStore != null)
                {
                    fd.Flags |= FieldRepresentation.Attributes.HasSingleAssignment;

                    ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)opStore.BasicBlock.Owner;
                    MethodRepresentation                       md  = cfg.Method;

                    if(fd.OwnerType != md.OwnerType)
                    {
                        continue;
                    }

                    if(md is ConstructorMethodRepresentation       ||
                       md is StaticConstructorMethodRepresentation  )
                    {
                        Expression[]       rhs    = opStore.Arguments;
                        Expression         val    = rhs[rhs.Length - 1];
                        TypeRepresentation fdType = fd.FieldType;

                        if(val is ConstantExpression)
                        {
                            ConstantExpression exVal = (ConstantExpression)val;

                            if(fdType is ReferenceTypeRepresentation)
                            {
                                if(exVal.Value != null)
                                {
                                    fd.Flags |= FieldRepresentation.Attributes.NeverNull;
                                }

                                if(fdType is ArrayReferenceTypeRepresentation)
                                {
                                    Array array = exVal.Value as Array;
                                    if(array != null)
                                    {
                                        fd.FixedSize  = array.Length;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Operator singleDef = ControlFlowGraphState.CheckSingleDefinition( cfg.DataFlow_DefinitionChains, (VariableExpression)val );
                            if(singleDef != null)
                            {
                                if(fdType is ArrayReferenceTypeRepresentation)
                                {
                                    ArrayReferenceTypeRepresentation fdTypeArray = (ArrayReferenceTypeRepresentation)fdType;

                                    ArrayAllocationOperator opAlloc = singleDef as ArrayAllocationOperator;
                                    if(opAlloc != null)
                                    {
                                        fd.Flags |= FieldRepresentation.Attributes.NeverNull;

                                        ConstantExpression exSize = opAlloc.FirstArgument as ConstantExpression;
                                        long               iSize;

                                        if(exSize != null && exSize.GetAsSignedInteger( out iSize ))
                                        {
                                            int size = (int)iSize;

                                            fd.FixedSize = size;

                                            if(fd is StaticFieldRepresentation)
                                            {
                                                //
                                                // This is an array assigned to a static field and never changed.
                                                //
                                                // Convert it into a static array, remove the array allocation and the static field assignment.
                                                //

                                                InstanceFieldRepresentation  fdInstance = typeSystem.AddStaticFieldToGlobalRoot( (StaticFieldRepresentation)fd );
                                                DataManager.ObjectDescriptor odRoot     = typeSystem.GenerateGlobalRoot();
                                                var                          wkt        = typeSystem.WellKnownTypes;
                                                TypeRepresentation           tdElement  = fdTypeArray.ContainedType;
                                                Type                         tElement   = typeSystem.TryGetTypeFromTypeRepresentation( tdElement );
                                                Array                        array      = null;

                                                if(tElement != null)
                                                {
                                                    array = Array.CreateInstance( tElement, size );

                                                    if(typeSystem.PlatformAbstraction.PlatformName == "LLVM")
                                                    {
                                                        // BUGBUG: ColinA-MSFT: LLVM likes to optimize zero arrays away, and make them zero-length.
                                                        // However, we need these arrays to be their actual size in memory, so we must set at least
                                                        // one non-zero element. This workaround should be removed when we fix this on the LLVM side.
                                                        if (tElement.IsPrimitive && (size > 0))
                                                        {
                                                            array.SetValue(Convert.ChangeType(1, tElement), 0);
                                                        }
                                                    }
                                                }

                                                DataManager.Attributes flags = DataManager.Attributes.SuitableForConstantPropagation;

                                                if(size == 0)
                                                {
                                                    flags |= DataManager.Attributes.Constant;
                                                }
                                                else
                                                {
                                                    flags |= DataManager.Attributes.Mutable;
                                                }

                                                Abstractions.PlacementRequirements pr = typeSystem.GetPlacementRequirements( fd );

                                                //
                                                // For a non-zero array, make sure it's allocated in a writable data section.
                                                //
                                                if(size != 0)
                                                {
                                                    if(pr == null)
                                                    {
                                                        pr = new Abstractions.PlacementRequirements( sizeof(uint), 0 );
                                                    }

                                                    pr.AddConstraint( Runtime.MemoryUsage.DataRW );
                                                }

                                                DataManager.ArrayDescriptor ad = typeSystem.DataManagerInstance.BuildArrayDescriptor( fdTypeArray, flags, pr, array, size );

                                                odRoot.Set( fdInstance, ad );

                                                //
                                                // Remove the store and change the allocation with a load.
                                                //
                                                opAlloc.SubstituteWithOperator( LoadStaticFieldOperator.New( opAlloc.DebugInfo, fd, opAlloc.FirstResult ), Operator.SubstitutionFlags.Default );
                                                opStore.Delete();
                                            }
                                        }
                                    }
                                }
                                else if(fdType is ReferenceTypeRepresentation)
                                {
                                    ObjectAllocationOperator opAlloc = singleDef as ObjectAllocationOperator;
                                    if(opAlloc != null)
                                    {
                                        fd.Flags |= FieldRepresentation.Attributes.NeverNull;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
