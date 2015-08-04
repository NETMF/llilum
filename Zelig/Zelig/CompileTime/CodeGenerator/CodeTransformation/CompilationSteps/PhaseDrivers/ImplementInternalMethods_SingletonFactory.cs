//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed partial class ImplementInternalMethods
    {
        //
        // Helper Methods
        //

        private void ImplementSingletonFactory( MethodRepresentation          md ,
                                                CustomAttributeRepresentation ca )
        {
            if(TypeSystemForCodeTransformation.GetCodeForMethod( md ) == null)
            {
                ControlFlowGraphStateForCodeTransformation cfg    = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );
                VariableExpression                         dst    = cfg.ReturnValue;
                TypeRepresentation                         target = dst.Type;
                
                if((target.Flags & TypeRepresentation.Attributes.Sealed) == 0)
                {
                    TypeRepresentation targetDirect;

                    if(m_typeSystem.ForcedDevirtualizations.TryGetValue( target, out targetDirect ))
                    {
                        target = targetDirect;
                    }
                    else
                    {
                        //
                        // BUGBUG: This is valid only if we are in bounded application mode!!!
                        //
                        List< TypeRepresentation > lst = m_typeSystem.CollectConcreteImplementations( target );

                        switch(lst.Count)
                        {
                            case 0:
                                throw TypeConsistencyErrorException.Create( "Found singleton factory that refers to a type with no concrete subclasses: {0}", target.FullName );

                            case 1:
                                target = lst[ 0 ];
                                break;

                            default:
                                //
                                // if we have more than one
                                //
                                var mostDerived = new TypeSystemForCodeTransformation.LinearHierarchyBuilder( lst, m_typeSystem ).Build();

                                if( mostDerived != null )
                                {
                                    target = lst[ 0 ];
                                    break;
                                }

                                throw TypeConsistencyErrorException.Create( "Found singleton factory that refers to a type with multiple concrete subclasses: {0}", target.FullName );
                        }
                    }
                }

                //--//

                //
                // Prepare method body
                //
                var bb = cfg.CreateFirstNormalBasicBlock();

                md.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.NoInline;
                md.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.Inline;

                //
                // Create proper flow control for exit basic block.
                //
                cfg.AddReturnOperator();

                //--//

                DataManager.Attributes attrib;

                if(ca.HasNamedArg( "ReadOnly" ))
                {
                    attrib = DataManager.Attributes.Constant;
                }
                else
                {
                    attrib = DataManager.Attributes.Mutable;
                }

                attrib |= DataManager.Attributes.SuitableForConstantPropagation;

                //--//

                CustomAttributeRepresentation caMemoryMapped;
                Expression                    ex;
                Annotation                    an = null;

                if(m_typeSystem.MemoryMappedPeripherals.TryGetValue( target, out caMemoryMapped ))
                {
                    object baseObj = caMemoryMapped.GetNamedArg( "Base" );
                    if(baseObj == null)
                    {
                        throw TypeConsistencyErrorException.Create( "Found MemoryMappedPeripheral with singleton factory and no Base address: {0}", target.FullName );
                    }

                    uint baseAddress = (uint)baseObj;

                    ex = m_typeSystem.CreateConstant( target, baseAddress );

                    an = MemoryMappedPeripheralAnnotation.Create( m_typeSystem, caMemoryMapped );
                }
                else
                {
                    ex = m_typeSystem.GenerateConstantForSingleton( target, attrib );
                }

                var opNew = SingleAssignmentOperator.New( null, dst, ex );
                opNew.AddAnnotation( NotNullAnnotation.Create( m_typeSystem ) );
                if(an != null)
                {
                    opNew.AddAnnotation( an );
                }

                bb.AddOperator( opNew );
            }
        }
    }
}
