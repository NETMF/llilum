//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TYPESYSTEM_DEBUG_GENERICINSTANTIATIONCLOUSURE

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData.Normalized;

    internal class GenericInstantiationClosure
    {
        class Lookup : GrowOnlyHashTable< MetaDataTypeDefinitionGeneric, bool >
        {
        }

        class Roots : GrowOnlyHashTable< MetaDataTypeDefinitionPartiallyDelayedTypeParameter, Edges >
        {
        }

        class Edges : GrowOnlyHashTable< MetaDataTypeDefinitionPartiallyDelayedTypeParameter, Edge >
        {
        }

        class Edge
        {
            internal const bool Expanding    = true;
            internal const bool NotExpanding = false;

            //
            // State
            //

            private bool m_status;
            private int  m_visitIndex;

            //
            // Constructor Methods
            //

            internal Edge( bool status )
            {
                m_status     =  status;
                m_visitIndex = -1;
            }

            internal void Unvisit()
            {
                m_visitIndex = -1;
            }

            internal int VisitIndex
            {
                get
                {
                    return m_visitIndex;
                }

                set
                {
                    m_visitIndex = value;
                }
            }

            internal bool IsExpanding
            {
                get
                {
                    return m_status == Expanding;
                }
            }

            internal bool IsVisited
            {
                get
                {
                    return m_visitIndex != -1;
                }
            }
        }

        class Nodes : GrowOnlySet< MetaDataTypeDefinitionGenericInstantiation >
        {
        }

        class TypeSet : GrowOnlyHashTable< MetaDataTypeDefinitionAbstract, MetaDataTypeDefinitionAbstract >
        {
        }

        //
        // State
        //

        private readonly Lookup m_genericRecursionCheck = new Lookup();

        //--//

        internal bool VerifyFiniteInstantiationOfGenericType( MetaDataTypeDefinitionGeneric td )
        {
            bool res;

            if(m_genericRecursionCheck.TryGetValue( td, out res ) == false)
            {
                Roots   roots        = new Roots();
                Nodes   nodes        = new Nodes();
                TypeSet set          = new TypeSet();
                TypeSet setExpand    = new TypeSet();
                TypeSet setResN      = new TypeSet();
                TypeSet setResNplus1 = new TypeSet();
                TypeSet setResTmp;

////            Console.WriteLine();
////            Console.WriteLine();
////            Console.WriteLine();

                setResN[td] = td;

                while(setResN.Count > 0)
                {
                    setResNplus1.Clear();

                    foreach(MetaDataTypeDefinitionAbstract tdNext in setResN.Keys)
                    {
                        if(set.ContainsKey( tdNext ) == false)
                        {
                            set[tdNext] = tdNext;

                            setExpand.Clear();

                            ExpandIfNotPresent( tdNext, setExpand );

                            foreach(MetaDataTypeDefinitionAbstract tdGenNext in setExpand.Keys)
                            {
                                if(tdGenNext is MetaDataTypeDefinitionGeneric)
                                {
                                    CollectGenericInstantiations( setResNplus1, (MetaDataTypeDefinitionGeneric)tdGenNext );
                                }
                            }

                            foreach(MetaDataTypeDefinitionAbstract tdGenNext in setResNplus1.Values)
                            {
                                if(tdGenNext is MetaDataTypeDefinitionGenericInstantiation)
                                {
                                    MetaDataTypeDefinitionGenericInstantiation tdGenInst = (MetaDataTypeDefinitionGenericInstantiation)tdGenNext;

                                    nodes.Insert( tdGenInst );
                                }

                                if(tdGenNext is MetaDataTypeDefinitionGeneric)
                                {
                                    MetaDataTypeDefinitionGeneric tdGen = (MetaDataTypeDefinitionGeneric)tdGenNext;

                                    for(int i = 0; i < tdGen.GenericParams.Length; i++)
                                    {
                                        MetaDataTypeDefinitionPartiallyDelayedTypeParameter param = new MetaDataTypeDefinitionPartiallyDelayedTypeParameter( tdGen, i );

                                        roots[param] = new Edges();
                                    }
                                }
                            }
                        }
                    }

                    setResTmp    = setResN;
                    setResN      = setResNplus1;
                    setResNplus1 = setResTmp;
                }

                setResN.Clear();

                foreach(MetaDataTypeDefinitionGenericInstantiation tdGenInst in nodes)
                {
                    ComputeEdges( roots, setResN, tdGenInst );
                }

////            Console.WriteLine();
////            Dump( roots );

                res = (FindExpandingCycle( roots ) == false);

                m_genericRecursionCheck[td] = res;
            }

            return res;
        }

        //--//

        private void ComputeEdges( Roots                                      roots ,
                                   TypeSet                                    set   ,
                                   MetaDataTypeDefinitionGenericInstantiation td    )
        {
            if(set.ContainsKey( td ) == false)
            {
                set[td] = td;

                MetaDataTypeDefinitionGeneric tdGen = td.GenericType;

                for(int i = 0; i < tdGen.GenericParams.Length; i++)
                {
                    MetaDataTypeDefinitionPartiallyDelayedTypeParameter param = new MetaDataTypeDefinitionPartiallyDelayedTypeParameter( tdGen, i );

                    ComputeEdges( roots, set, param, td.InstantiationParams[i].Type, true );
                }
            }
        }

        private void ComputeEdges( Roots                                               roots       ,
                                   TypeSet                                             set         ,
                                   MetaDataTypeDefinitionPartiallyDelayedTypeParameter param       ,
                                   MetaDataTypeDefinitionAbstract                      td          ,
                                   bool                                                fFirstLevel )
        {
            if(td is MetaDataTypeDefinitionPartiallyDelayedTypeParameter)
            {
                MetaDataTypeDefinitionPartiallyDelayedTypeParameter param2 = (MetaDataTypeDefinitionPartiallyDelayedTypeParameter)td;
                Edges                                               edges;

                if(roots.TryGetValue( param2, out edges ) == false)
                {
                    edges = new Edges();

                    roots[param2] = edges;
                }

                if(fFirstLevel)
                {
                    //
                    // Add non-expanding edge.
                    //
                    // If an edge is already present, there are two cases:
                    //
                    //  1) It's a non-expanding edge => state is already correct.
                    //  2) It's an expanding edge    => state should not be changed.
                    //
                    // so only update if not present.
                    //
                    if(edges.ContainsKey( param ) == false)
                    {
////                    Console.WriteLine( "Add NonExpanding {0} -> {1}", param2.FullName, param.FullName );
                        edges[param] = new Edge( Edge.NotExpanding );
                    }
                }
                else
                {
                    //
                    // Add expanding edge.
                    //
                    // If an edge is already present, there are two cases:
                    //
                    //  1) It's a non-expanding edge => state should be changed to expanding.
                    //  2) It's an expanding edge    => state is already correct.
                    //
                    // so always update.
                    //
////                Console.WriteLine( "Add Expanding {0} -> {1}", param2.FullName, param.FullName );
                    edges[param] = new Edge( Edge.Expanding );
                }
            }
            else if(td is MetaDataTypeDefinitionPartiallyDelayedMethodParameter)
            {
                // BUGBUG: Not Implemented.
            }
            else if(td is MetaDataTypeDefinitionGenericInstantiation)
            {
                MetaDataTypeDefinitionGenericInstantiation td2 = (MetaDataTypeDefinitionGenericInstantiation)td;

                MetaDataTypeDefinitionGeneric tdGen = td2.GenericType;

                for(int i = 0; i < tdGen.GenericParams.Length; i++)
                {
                    ComputeEdges( roots, set, param, td2.InstantiationParams[i].Type, false );
                }

                ComputeEdges( roots, set, td2 );
            }
            else if(td is MetaDataTypeDefinitionByRef)
            {
                MetaDataTypeDefinitionByRef td2 = (MetaDataTypeDefinitionByRef)td;

                ComputeEdges( roots, set, param, td2.BaseType, false );
            }
            else if(td is MetaDataTypeDefinitionArray)
            {
                MetaDataTypeDefinitionArray td2 = (MetaDataTypeDefinitionArray)td;

                ComputeEdges( roots, set, param, td2.ObjectType, true );
            }
            else if(td is MetaDataTypeDefinition)
            {
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Internal error during ComputeEdges" );
            }
        }

        //--//

        private bool FindExpandingCycle( Roots roots )
        {
            foreach(MetaDataTypeDefinitionPartiallyDelayedTypeParameter param in roots.Keys)
            {
                if(FindExpandingCycle( roots, param, -1, 0 ))
                {
#if TYPESYSTEM_DEBUG_GENERICINSTANTIATIONCLOUSURE
                    Console.WriteLine( "Loop {0}", param.FullName );
#endif
                    return true;
                }
            }

            return false;
        }

        private bool FindExpandingCycle( Roots                                               roots         ,
                                         MetaDataTypeDefinitionPartiallyDelayedTypeParameter paramS        ,
                                         int                                                 expandingMark ,
                                         int                                                 currentMark   )
        {
            Edges edges;

            if(roots.TryGetValue( paramS, out edges ))
            {
                foreach(MetaDataTypeDefinitionPartiallyDelayedTypeParameter paramD in edges.Keys)
                {
                    Edge edge = edges[paramD];

                    if(edge.IsVisited)
                    {
                        int loopMark = edge.VisitIndex;

                        if(loopMark <= expandingMark)
                        {
#if TYPESYSTEM_DEBUG_GENERICINSTANTIATIONCLOUSURE
                            Console.WriteLine( "Found expanding cycle:" );
                            Dump( roots );
#endif

                            return true;
                        }

                        return false;
                    }

                    edge.VisitIndex = currentMark;

                    if(FindExpandingCycle( roots, paramD, edge.IsExpanding ? currentMark : expandingMark, currentMark+1 ))
                    {
#if TYPESYSTEM_DEBUG_GENERICINSTANTIATIONCLOUSURE
                        Console.WriteLine( "Loop {0}", paramD.FullName );
#endif
                        return true;
                    }

                    edge.Unvisit();
                }
            }

            return false;
        }

#if TYPESYSTEM_DEBUG_GENERICINSTANTIATIONCLOUSURE
        private void Dump( Roots roots )
        {
            foreach(MetaDataTypeDefinitionPartiallyDelayedTypeParameter paramS in roots.Keys)
            {
                Edges edges = roots[paramS];

                foreach(MetaDataTypeDefinitionPartiallyDelayedTypeParameter paramD in edges.Keys)
                {
                    Edge edge = edges[paramD];

                    Console.WriteLine( "{0} {1}{2} {3}", paramS.FullName, edge.IsVisited ? "*" : "", edge.IsExpanding ? "=>" : "->", paramD.FullName );
                }
            }
        }
#endif

        //--//

        private void ExpandIfNotPresent( MetaDataTypeDefinitionAbstract td  ,
                                         TypeSet                        set )
        {
            if(td.IsOpenType && set.ContainsKey( td ) == false)
            {
                set[td] = td;

                if(td is MetaDataTypeDefinitionGenericInstantiation)
                {
                    MetaDataTypeDefinitionGenericInstantiation td2 = (MetaDataTypeDefinitionGenericInstantiation)td;

                    ExpandIfNotPresent( td2.GenericType, set );

                    foreach(SignatureType td3 in td2.InstantiationParams)
                    {
                        ExpandIfNotPresent( td3.Type, set );
                    }
                }
                else if(td is MetaDataTypeDefinitionByRef)
                {
                    MetaDataTypeDefinitionByRef td2 = (MetaDataTypeDefinitionByRef)td;

                    ExpandIfNotPresent( td2.BaseType, set );
                }
                else if(td is MetaDataTypeDefinitionArray)
                {
                    MetaDataTypeDefinitionArray td2 = (MetaDataTypeDefinitionArray)td;

                    ExpandIfNotPresent( td2.ObjectType, set );
                }
            }
        }

        //--//

        private void CollectGenericInstantiations( TypeSet                       set ,
                                                   MetaDataTypeDefinitionGeneric td  )
        {
            CheckAndAddToGenericInstantiationsList( set, td, null, td.Extends );

            if(td.Interfaces != null)
            {
                foreach(MetaDataTypeDefinitionAbstract itf in td.Interfaces)
                {
                    CheckAndAddToGenericInstantiationsList( set, td, null, itf );
                }
            }

            if(td.Fields != null)
            {
                foreach(MetaDataField fd in td.Fields)
                {
                    CheckAndAddToGenericInstantiationsList( set, td, null, fd.FieldSignature.TypeSignature.Type );
                }
            }

            if(td.Methods != null)
            {
                foreach(MetaDataMethodBase md in td.Methods)
                {
                    SignatureMethod sig = md.Signature;

                    CheckAndAddToGenericInstantiationsList( set, td, md, sig.ReturnType.Type );

                    if(sig.Parameters != null)
                    {
                        foreach(SignatureType arg in sig.Parameters)
                        {
                            CheckAndAddToGenericInstantiationsList( set, td, md, arg.Type );
                        }
                    }

                    if(md.Locals != null)
                    {
                        foreach(SignatureType local in md.Locals)
                        {
                            CheckAndAddToGenericInstantiationsList( set, td, md, local.Type );
                        }
                    }

                    if(md.Instructions != null)
                    {
                        foreach(Instruction instr in md.Instructions)
                        {
                            object arg = instr.Argument;

                            if(arg != null)
                            {
                                if(arg is MetaDataTypeDefinitionAbstract)
                                {
                                    MetaDataTypeDefinitionAbstract tdArg = (MetaDataTypeDefinitionAbstract)arg;

                                    CheckAndAddToGenericInstantiationsList( set, td, md, tdArg );
                                }
                                else if(arg is MetaDataField)
                                {
                                    MetaDataField fdArg = (MetaDataField)arg;

                                    CheckAndAddToGenericInstantiationsList( set, td, md, fdArg.Owner );
                                }
                                else if(arg is MetaDataMethodBase)
                                {
                                    MetaDataMethodBase mdArg = (MetaDataMethodBase)arg;

                                    CheckAndAddToGenericInstantiationsList( set, td, md, mdArg.Owner );
                                }
                                else if(arg is MetaDataMethodGenericInstantiation)
                                {
                                    MetaDataMethodGenericInstantiation mdArg = (MetaDataMethodGenericInstantiation)arg;

                                    MetaDataMethodAbstract mdBase = mdArg.BaseMethod;

                                    if(mdBase is MetaDataMethodBase)
                                    {
                                        MetaDataMethodBase mdBase2 = (MetaDataMethodBase)mdBase;

                                        CheckAndAddToGenericInstantiationsList( set, td, mdBase2, mdBase2.Owner );
                                    }
                                    else if(mdBase is MetaDataMethodWithContext)
                                    {
                                        MetaDataMethodWithContext mdBase2 = (MetaDataMethodWithContext)mdBase;

                                        CheckAndAddToGenericInstantiationsList( set, td, md                , mdBase2.ContextType      );
                                        CheckAndAddToGenericInstantiationsList( set, td, mdBase2.BaseMethod, mdBase2.BaseMethod.Owner );
                                    }
                                }
                                else if(arg is MetaDataMethodWithContext)
                                {
                                    MetaDataMethodWithContext mdArg = (MetaDataMethodWithContext)arg;

                                    CheckAndAddToGenericInstantiationsList( set, td, md, mdArg.ContextType      );
                                    CheckAndAddToGenericInstantiationsList( set, td, md, mdArg.BaseMethod.Owner );
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckAndAddToGenericInstantiationsList( TypeSet                        set           ,
                                                             MetaDataTypeDefinitionGeneric  contextType   ,
                                                             MetaDataMethodBase             contextMethod ,
                                                             MetaDataTypeDefinitionAbstract td            )
        {
            if(ContainsGenericParameters( td ))
            {
                set[td] = Substitute( contextType, contextMethod as MetaDataMethodGeneric, td );
            }
        }

        private bool ContainsGenericParameters( MetaDataTypeDefinitionAbstract td )
        {
            return td != null && td.IsOpenType;
        }

        //--//

        private MetaDataTypeDefinitionAbstract Substitute( MetaDataTypeDefinitionGeneric  contextType   ,
                                                           MetaDataMethodGeneric          contextMethod ,
                                                           MetaDataTypeDefinitionAbstract td            )
        {
            if(td is MetaDataTypeDefinitionGenericInstantiation)
            {
                MetaDataTypeDefinitionGenericInstantiation td2 = (MetaDataTypeDefinitionGenericInstantiation)td;

                SignatureType[] instantiationParams = td2.InstantiationParams;
                SignatureType[] genericParameters   = new SignatureType[instantiationParams.Length];

                for(int i = 0; i < instantiationParams.Length; i++)
                {
                    genericParameters[i] = SignatureType.CreateUnique( Substitute( contextType, contextMethod, instantiationParams[i].Type ) );
                }

                return td2.Specialize( genericParameters );
            }
            else if(td is MetaDataTypeDefinitionByRef)
            {
                MetaDataTypeDefinitionByRef td2 = (MetaDataTypeDefinitionByRef)td;

                return Substitute( contextType, contextMethod, td2.BaseType );
            }
            else if(td is MetaDataTypeDefinitionArray)
            {
                MetaDataTypeDefinitionArray td2 = (MetaDataTypeDefinitionArray)td;

                return Substitute( contextType, contextMethod, td2.ObjectType );
            }
            else if(td is MetaDataTypeDefinitionDelayed)
            {
                MetaDataTypeDefinitionDelayed td2 = (MetaDataTypeDefinitionDelayed)td;

                if(td2.IsMethodParameter)
                {
                    return new MetaDataTypeDefinitionPartiallyDelayedMethodParameter( contextMethod, td2.ParameterNumber );
                }
                else
                {
                    return new MetaDataTypeDefinitionPartiallyDelayedTypeParameter( contextType, td2.ParameterNumber );
                }
            }
            else
            {
                return td;
            }
        }
    }
}
