//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Diagnostics;
    using Microsoft.Zelig.Runtime.TypeSystem;

    /// <summary>
    /// Inline common scalar operations and lower the imported wrapping struct to something better resembling native
    /// types. This entire set of optimizations assumes that all scalar types are created and owned by the runtime. If
    /// we ever open up the possibility of user-created scalars, some of the assumptions here may be invalidated.
    /// </summary>
    public static class InlineScalars
    {
        //
        // Helper Methods
        //

        public static bool Execute(ControlFlowGraphStateForCodeTransformation cfg)
        {
            cfg.TraceToFile("InlineScalars");

            using (new PerformanceCounters.ContextualTiming(cfg, "InlineScalars"))
            {
                bool fModified = false;

                fModified |= InlineScalarConstructors(cfg);
                fModified |= InlineScalarEquality(cfg);
                fModified |= RemoveScalarFieldLoads(cfg);

                if (fModified)
                {
                    cfg.UpdateFlowInformation();
                    RemoveUnnecessaryAddressLoads(cfg);
                    cfg.DropDeadVariables();
                }

                return fModified;
            }
        }

        private static bool InlineScalarConstructors(ControlFlowGraphStateForCodeTransformation cfg)
        {
            Operator[][] defChains = cfg.DataFlow_DefinitionChains;
            bool fModified = false;

            // Search for the following pattern:
            //    ScalarType::.ctor(&local, value)
            // Replace with:
            //    local = value
            foreach (var callOp in cfg.FilterOperators<InstanceCallOperator>())
            {
                var thisPtr = callOp.FirstArgument as VariableExpression;
                var thisType = callOp.FirstArgument.Type.UnderlyingType as ScalarTypeRepresentation;
                var targetMethod = callOp.TargetMethod as ConstructorMethodRepresentation;

                // Determine whether this is a scalar constructor.
                if ((thisType != null) && (targetMethod != null))
                {
                    Debug.Assert(callOp.Arguments.Length == 2, "Scalar constructors should always have exactly two arguments: 'this' and the value to initialize with.");

                    // Skip inlining if the source value can't be safely cast to the constructing type.
                    if (!thisType.CanBeAssignedFrom(targetMethod.ThisPlusArguments[1], null))
                    {
                        continue;
                    }

                    // We can only replace constructors on values that exist on the stack. If thisPtr is an argument
                    // or field, we should be assigning rather than constructing the value.
                    AddressAssignmentOperator addressOp = ControlFlowGraphState.CheckSingleDefinition(defChains, thisPtr) as AddressAssignmentOperator;
                    Debug.Assert(addressOp != null, "Couldn't find the constructing value.");

                    Expression initValue = callOp.SecondArgument;
                    var constructing = (VariableExpression)addressOp.FirstArgument;
                    var assignOp = SingleAssignmentOperator.New(callOp.DebugInfo, constructing, initValue);
                    callOp.SubstituteWithOperator(assignOp, Operator.SubstitutionFlags.Default);

                    fModified = true;
                }
            }

            return fModified;
        }

        private static bool InlineScalarEquality(ControlFlowGraphStateForCodeTransformation cfg)
        {
            bool fModified = false;

            // Patterns:
            // - Replace (scalar.Equals(scalar)) with (scalar == scalar)
            // - Replace (op_Equality(scalar, scalr)) with (scalar == scalar)
            // - Replace (op_Inequality(scalar, scalar)) with (scalar != scalar)
            foreach (var callOp in cfg.FilterOperators<CallOperator>())
            {
                var thisPtr = callOp.FirstArgument as VariableExpression;
                var thisType = callOp.FirstArgument.Type.UnderlyingType as ScalarTypeRepresentation;
                var targetMethod = callOp.TargetMethod;

                if (thisType != null)
                {
                    bool isStatic = targetMethod is StaticMethodRepresentation;
                    CompareAndSetOperator.ActionCondition condition;
                    switch (targetMethod.Name)
                    {
                    case "op_Equality":
                    case "Equals":
                        condition = CompareAndSetOperator.ActionCondition.EQ;
                        break;

                    case "op_Inequality":
                        condition = CompareAndSetOperator.ActionCondition.NE;
                        break;

                    default:
                        // Not an equality operator.
                        continue;
                    }

                    Expression left;
                    Expression right;

                    if (isStatic)
                    {
                        Debug.Assert(callOp.Arguments.Length == 3, "Static equality operators should always have exactly three arguments: null 'this', value, and the value to compare to.");
                        left = callOp.Arguments[1];
                        right = callOp.Arguments[2];
                    }
                    else
                    {
                        Debug.Assert(callOp.Arguments.Length == 2, "Instance equality operators should always have exactly two arguments: 'this' and the value to compare to.");

                        right = callOp.Arguments[1];
                        if (!(right.Type is ScalarTypeRepresentation))
                        {
                            // Comparand is not a scalar, so skip inlining.
                            continue;
                        }

                        left = LoadScalarAddress(cfg, thisPtr, callOp, true);
                    }

                    var compOperator = CompareAndSetOperator.New(callOp.DebugInfo, condition, false, callOp.FirstResult, left, right);
                    callOp.SubstituteWithOperator(compOperator, Operator.SubstitutionFlags.Default);
                    fModified = true;
                }
            }

            return fModified;
        }

        private static bool RemoveScalarFieldLoads(ControlFlowGraphStateForCodeTransformation cfg)
        {
            Operator[][] useChains = cfg.DataFlow_UseChains;
            bool fModified = false;

            // Pattern: Replace (scalarPtr->m_value) with (*scalarPtr)
            foreach (var loadFieldOp in cfg.FilterOperators<LoadInstanceFieldOperator>())
            {
                VariableExpression thisPtr = loadFieldOp.FirstArgument as VariableExpression;
                TypeRepresentation thisType = loadFieldOp.FirstArgument.Type.UnderlyingType as ScalarTypeRepresentation;

                if ((thisPtr != null) && (thisType != null))
                {
                    Expression loadedValue = LoadScalarAddress(cfg, thisPtr, loadFieldOp, loadFieldOp.MayThrow);

                    VariableExpression field = loadFieldOp.FirstResult;
                    foreach (Operator useOp in useChains[field.SpanningTreeIndex])
                    {
                        useOp.SubstituteUsage(field, loadedValue);
                    }

                    loadFieldOp.SubstituteWithOperator(NopOperator.New(loadFieldOp.DebugInfo), Operator.SubstitutionFlags.Default);
                    fModified = true;
                }
            }

            // Pattern: Replace (&scalarPtr->m_value) with (scalarPtr)
            foreach (var loadFieldOp in cfg.FilterOperators<LoadInstanceFieldAddressOperator>())
            {
                VariableExpression thisPtr = loadFieldOp.FirstArgument as VariableExpression;
                TypeRepresentation thisType = loadFieldOp.FirstArgument.Type.UnderlyingType as ScalarTypeRepresentation;

                if ((thisPtr != null) && (thisType != null))
                {
                    VariableExpression fieldPtr = loadFieldOp.FirstResult;
                    foreach (Operator useOp in useChains[fieldPtr.SpanningTreeIndex])
                    {
                        useOp.SubstituteUsage(fieldPtr, thisPtr);
                    }

                    loadFieldOp.SubstituteWithOperator(NopOperator.New(loadFieldOp.DebugInfo), Operator.SubstitutionFlags.Default);
                    fModified = true;
                }
            }

            return fModified;
        }

        private static Expression LoadScalarAddress(
            ControlFlowGraphStateForCodeTransformation cfg,
            VariableExpression pointer,
            Operator usingOp,
            bool checkForNull)
        {
            // Special case: If the pointer is an address assignment operator with a single definition, we can recover
            // the orginal expression without creating a new temporary and load operation.
            if (!(pointer is ArgumentVariableExpression))
            {
                var defOp = ControlFlowGraphState.CheckSingleDefinition(cfg.DataFlow_DefinitionChains, pointer);
                Debug.Assert(defOp != null, "Could not find source of scalar address.");

                if (defOp is AddressAssignmentOperator)
                {
                    return defOp.FirstArgument;
                }
            }

            TypeRepresentation loadedType = pointer.Type.UnderlyingType;
            var loadedValue = cfg.AllocateTemporary(loadedType, null);
            var loadOp = LoadIndirectOperator.New(usingOp.DebugInfo, loadedType, loadedValue, pointer, null, 0, false, checkForNull);
            usingOp.AddOperatorBefore(loadOp);

            return loadedValue;
        }

        private static bool RemoveUnnecessaryAddressLoads(ControlFlowGraphStateForCodeTransformation cfg)
        {
            Operator[][] useChains = cfg.DataFlow_UseChains;
            bool fModified = false;

            // Search for the following pattern where LoadIndirect is the only use of address:
            //    address = <LoadAddress>(expr)
            //    local = LoadIndirect(address)
            // Replace with:
            //    expr

            // Pattern: Replace (*&expr) with (expr)
            foreach (var addressOp in cfg.FilterOperators<AddressAssignmentOperator>())
            {
                // Remove orphan address loads.
                if (useChains[addressOp.FirstResult.SpanningTreeIndex].Length == 0)
                {
                    addressOp.Delete();
                    fModified = true;
                    continue;
                }

                var loadOp = ControlFlowGraphState.CheckSingleUse(useChains, addressOp.FirstResult) as LoadIndirectOperator;
                if (loadOp != null)
                {
                    Expression loaded = addressOp.FirstArgument;
                    foreach (Operator useOp in useChains[loaded.SpanningTreeIndex])
                    {
                        // Don't try to retarget the address assignment. The source value may not be addressable.
                        // Additionally, skip this substitution if the use is in an operator we just removed.
                        if ((useOp != addressOp) && (useOp.BasicBlock != null))
                        {
                            useOp.SubstituteUsage(loadOp.FirstResult, loaded);
                        }
                    }

                    // Delete instead of replacing with NOP. We don't want to save the debug info.
                    addressOp.Delete();
                    loadOp.Delete();
                    fModified = true;
                }
            }

            // Pattern: Replace (*&obj.Field) with (obj.Field)
            foreach (var addressOp in cfg.FilterOperators<LoadAddressOperator>())
            {
                var loadOp = ControlFlowGraphState.CheckSingleUse(useChains, addressOp.FirstResult) as LoadIndirectOperator;
                if (loadOp != null)
                {
                    LoadFieldOperator newLoadOp;
                    if (addressOp is LoadInstanceFieldAddressOperator)
                    {
                        newLoadOp = LoadInstanceFieldOperator.New(
                            loadOp.DebugInfo,
                            addressOp.Field,
                            loadOp.FirstResult,
                            addressOp.FirstArgument,
                            loadOp.MayThrow);
                    }
                    else
                    {
                        newLoadOp = LoadStaticFieldOperator.New(loadOp.DebugInfo, addressOp.Field, loadOp.FirstResult);
                    }

                    addressOp.Delete();
                    loadOp.SubstituteWithOperator(newLoadOp, Operator.SubstitutionFlags.Default);
                    fModified = true;
                }
            }

            // Pattern: Replace (*&array[index]) with (array[index])
            foreach (var addressOp in cfg.FilterOperators<LoadElementAddressOperator>())
            {
                var loadOp = ControlFlowGraphState.CheckSingleUse(useChains, addressOp.FirstResult) as LoadIndirectOperator;
                if (loadOp != null)
                {
                    var newLoadOp = LoadElementOperator.New(
                        loadOp.DebugInfo,
                        loadOp.FirstResult,
                        addressOp.FirstArgument,
                        addressOp.SecondArgument,
                        addressOp.AccessPath,
                        loadOp.MayThrow);

                    addressOp.Delete();
                    loadOp.SubstituteWithOperator(newLoadOp, Operator.SubstitutionFlags.Default);
                    fModified = true;
                }
            }

            return fModified;
        }
    }
}
