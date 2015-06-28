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
        internal delegate bool Callback( MethodRepresentation md );

        //
        // State
        //

        private TypeSystemForCodeTransformation                     m_typeSystem;
        private GrowOnlyHashTable< MethodRepresentation, Callback > m_pending;

        //
        // Constructor Methods
        //

        public ImplementInternalMethods( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
            m_pending    = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, Callback >();
        }

        //
        // Helper Methods
        //

        public void Prepare()
        {
            WellKnownTypes       wkt                      = m_typeSystem.WellKnownTypes;
            WellKnownMethods     wkm                      = m_typeSystem.WellKnownMethods;
            TypeRepresentation   tdMulticastDelegate      = wkt.System_MulticastDelegate;
            MethodRepresentation mdObject_Equals          = wkm.Object_Equals;
            Callback             dlgImplementObjectEquals = ImplementObjectEquals;

            foreach(TypeRepresentation td in m_typeSystem.Types)
            {
                //
                // For all the user-defined value types, provide a system-generated implementation of Object.Equals and Obect.GetHashCode.
                //
                if(td is ValueTypeRepresentation && !(td is ScalarTypeRepresentation) && td.IsAbstract == false)
                {
                    if(td.IsOpenType == false && td.FindMatch( mdObject_Equals, null ) == null)
                    {
                        MethodRepresentation md = td.CreateOverride( m_typeSystem, (VirtualMethodRepresentation)mdObject_Equals );

                        m_pending[md] = dlgImplementObjectEquals;
                    }
                }

                //
                // Implement code for delegates.
                //
                if(td.IsSubClassOf( tdMulticastDelegate, null ) && wkm.MulticastDelegateImpl_MulticastDelegateImpl != null)
                {
                    foreach(MethodRepresentation md in td.Methods)
                    {
                        ImplementDelegateMethods( md );
                    }
                }

                //
                // Implement code for the compiler-generated methods on arrays.
                //
                if(td is ArrayReferenceTypeRepresentation)
                {
                    var tdHelper = FindArrayHelper( td );

                    foreach(MethodRepresentation md in td.Methods)
                    {
                        if(md is RuntimeMethodRepresentation)
                        {
                            if(TypeSystemForCodeTransformation.GetCodeForMethod( md ) == null)
                            {
                                var cfg = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );

                                if(tdHelper != null)
                                {
                                    LinkArrayImplementation( cfg, tdHelper );
                                }
                                else
                                {
                                    ProvideThrowingStubImplementation( cfg );
                                }
                            }
                        }
                    }
                }

                //LON: 2/16/09
                //
                // Implement code for external "C/ASM" calls that have been marked as imported references
                //
                foreach(MethodRepresentation md in td.Methods)
                {
                    if ((md.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Imported) != 0)
                    {
                        ImplementExternalMethodStub( md, this.m_typeSystem );
                    }
                }
            }

            //
            // Implement code for singleton factories.
            //
            foreach(MethodRepresentation md in m_typeSystem.SingletonFactories.Keys)
            {
                ImplementSingletonFactory( md, m_typeSystem.SingletonFactories[md] );
            }
        }

        private TypeRepresentation FindArrayHelper( TypeRepresentation td )
        {
            List< TypeRepresentation > lst;

            if(m_typeSystem.GenericTypeInstantiations.TryGetValue( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_SZArrayHelper, out lst ))
            {
                foreach(var tdHelper in lst)
                {
                    if(tdHelper.GenericParameters[0] == td.ContainedType)
                    {
                        return tdHelper;
                    }
                }
            }

            return null;
        }

        private void LinkArrayImplementation( ControlFlowGraphStateForCodeTransformation cfg      ,
                                              TypeRepresentation                         tdHelper )
        {
            MethodRepresentation md                = cfg.Method;
            TypeRepresentation[] thisPlusArguments = md.ThisPlusArguments;

            thisPlusArguments = ArrayUtility.InsertAtHeadOfNotNullArray( thisPlusArguments, tdHelper );
            
            foreach(var mdHelper in tdHelper.Methods)
            {
                if(mdHelper.MatchNameAndSignature( md.Name, md.ReturnType, thisPlusArguments, null ))
                {
                    var bb = cfg.CreateFirstNormalBasicBlock();

                    var rhs = m_typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdHelper, cfg.Arguments );
                    bb.AddOperator( StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdHelper, VariableExpression.ToArray( cfg.ReturnValue ), rhs ) );

                    cfg.AddReturnOperator();
                    return;
                }
            }

            //
            // No implementation found, fallback to throwing behavior.
            //
            ProvideThrowingStubImplementation( cfg );
        }

        private void ProvideThrowingStubImplementation( ControlFlowGraphStateForCodeTransformation cfg )
        {
            var bb = cfg.CreateFirstNormalBasicBlock();

            MethodRepresentation mdThrow = m_typeSystem.WellKnownMethods.ThreadImpl_ThrowNotImplementedException;
            Expression[]         rhs     = m_typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdThrow );
            bb.AddOperator( StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdThrow, rhs ) );

            cfg.ExitBasicBlock.FlowControl = DeadControlOperator.New( null );
        }

        public bool Complete()
        {
            MethodRepresentation[] mdArray  = m_pending.KeysToArray();
            Callback[]             dlgArray = m_pending.ValuesToArray();
            bool                   fRes     = true;

            m_pending.Clear();

            for(int i = 0; i < mdArray.Length; i++)
            {
                MethodRepresentation md = mdArray[i];

                if(m_typeSystem.EstimatedReachabilitySet.IsProhibited( md ) == false)
                {
                    fRes &= dlgArray[i]( mdArray[i] );
                }
            }

            return fRes;
        }
    }
}
