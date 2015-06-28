//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DUMP_CONSTRAINTSYSTEM


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class ConstraintSystemCollector
    {
        public class GraphState
        {
            public Kind                                                           Flavor;
            public GrowOnlySet      < ConstraintInstance                        > Constraints;
            public GrowOnlyHashTable< object            ,              Operator > Vertices;
            public GrowOnlyHashTable< object            , GrowOnlySet< object > > IdentitySets;
            public GrowOnlyHashTable< object            ,              double   > ConstantsSet;
            public GrowOnlyHashTable< object            ,              string   > Labels;
            public List             < object                                    > ProofTests;

            //
            // Helper Methods
            //

            public bool WasReached( object obj )
            {
                if(this.ProofTests == null)
                {
                    return true;
                }

                foreach(object step in this.ProofTests)
                {
                    ConstraintInstance ci = step as ConstraintInstance;
                    if(ci != null)
                    {
                        if(ci.Destination == obj || ci.Source == obj)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public interface IVisualizer
        {
            void DisplayGraph( GraphState gs );
        }

        //--//

        public enum Kind
        {
            LessThanOrEqual   ,
            GreaterThanOrEqual,
        }

        public class ArrayLengthHolder
        {
            //
            // State
            //

            public readonly Expression Array;

            //
            // Constructor Methods
            //

            public ArrayLengthHolder( Expression array )
            {
                this.Array = array;
            }

            //
            // Equality Methods
            //

            public override bool Equals( Object obj )
            {
                if(obj is ArrayLengthHolder)
                {
                    ArrayLengthHolder other = (ArrayLengthHolder)obj;

                    if(this.Array == other.Array)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.Array.GetHashCode();
            }
        }

        public class ConstraintInstance
        {
            //
            // State
            //

            public readonly object Destination;
            public readonly object Source;
            public readonly double Weight;

            //
            // Constructor Methods
            //

            public ConstraintInstance( object destination ,
                                       object source      ,
                                       double weight      )
            {
                this.Destination = destination;
                this.Source      = source;
                this.Weight      = weight;
            }

            //
            // Equality Methods
            //

            public override bool Equals( Object obj )
            {
                if(obj is ConstraintInstance)
                {
                    ConstraintInstance other = (ConstraintInstance)obj;

                    if(this.Destination.Equals( other.Destination ) &&
                       this.Source     .Equals( other.Source      ) &&
                       this.Weight      ==      other.Weight         )
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.Destination.GetHashCode();
            }
        }

        public enum Lattice
        {
            False  ,
            Reduced,
            True   ,
        }

        private enum NormalizedValue
        {
            Invalid,
            Int    ,
            Long   ,
            Float  ,
            Double ,
        }

        private class ConstraintState
        {
            //
            // State
            //

            internal object                               m_destination;
            internal object                               m_source;
            internal GrowOnlyHashTable< double, Lattice > m_history;

            //
            // Constructor Methods
            //

            internal ConstraintState()
            {
            }

            internal ConstraintState( object destination ,
                                      object source      )
            {
                m_destination = destination;
                m_source      = source;
                m_history     = HashTableFactory.New< double, Lattice >();
            }

            //
            // Equality Methods
            //

            public override bool Equals( Object obj )
            {
                if(obj is ConstraintState)
                {
                    ConstraintState other = (ConstraintState)obj;

                    if(m_destination.Equals( other.m_destination ) &&
                       m_source     .Equals( other.m_source      )  )
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return m_destination.GetHashCode();
            }
        }

        //
        // State
        //

        private readonly ControlFlowGraphStateForCodeTransformation                                 m_cfg;
        private readonly Kind                                                                       m_kind;
        private readonly GrowOnlySet      < ConstraintInstance                                    > m_set;
        private readonly GrowOnlyHashTable< object            ,              Operator             > m_vertices;
        private readonly GrowOnlySet      < ArrayLengthHolder                                     > m_arrays;
        private readonly GrowOnlyHashTable< object            , List       < ConstraintInstance > > m_incomingEdges;
        private readonly GrowOnlyHashTable< VariableExpression, GrowOnlySet< object             > > m_assignments; // Multiple sources to one destination.
        private readonly GrowOnlyHashTable< object            , GrowOnlySet< object             > > m_identitySets;
        private readonly GrowOnlyHashTable< object            ,              double               > m_constantsSet;
                                                                                                 
        private readonly GrowOnlySet      < ConstraintState                                       > m_proveState;
        private readonly GrowOnlyHashTable< object, object                                        > m_proveActive;
        private readonly GrowOnlySet      < object                                                > m_proveAttempts;
        private readonly ConstraintState                                                            m_proveLookupHelper;

#if DUMP_CONSTRAINTSYSTEM
        List             < object                                         > m_debug_testHistory;
        GrowOnlyHashTable< object            , string                     > m_debug_labels;
        GrowOnlyHashTable< VariableExpression, List< VariableExpression > > m_debug_baseVariables;
        List             < ConstantExpression                             > m_debug_constants;
        int                                                                 m_debug_indent;
#endif

        //
        // Constructor Methods
        //

        public ConstraintSystemCollector( ControlFlowGraphStateForCodeTransformation cfg  ,
                                          Kind                                       kind )
        {
            m_cfg           = cfg;
            m_kind          = kind;
            m_set           = SetFactory      .New< ConstraintInstance                                    >();
            m_vertices      = HashTableFactory.New< object            ,              Operator             >();
            m_arrays        = SetFactory      .New< ArrayLengthHolder                                     >();
            m_incomingEdges = HashTableFactory.New< object            , List       < ConstraintInstance > >();
            m_assignments   = HashTableFactory.New< VariableExpression, GrowOnlySet< object             > >();
            m_identitySets  = HashTableFactory.New< object            , GrowOnlySet< object             > >();
            m_constantsSet  = HashTableFactory.New< object            ,              double               >();

            //--//

            m_proveState        = SetFactory      .New< ConstraintState >();
            m_proveActive       = HashTableFactory.New< object, object  >();
            m_proveAttempts     = SetFactory      .New< object          >();
            m_proveLookupHelper = new ConstraintState();

            //--//

            Collect();
        }

        //
        // Helper Methods
        //

        public bool Prove( Expression exDestination ,
                           Expression exSource      ,
                           double     weight        )
        {
            if(m_vertices.ContainsKey( exDestination ) &&
               m_vertices.ContainsKey( exSource      )  )
            {
                Debug_Print( "#############################################################" );

                m_proveAttempts.Clear();

                Debug_ResetReachability();

                if(ProveInternal( exDestination, exSource, weight ) != Lattice.False)
                {
                    Debug_Flush();
                    Debug_Print( "SUCCESS!" );
                    
                    //this.ShowGraph();
                    return true;
                }
            }

            Debug_Print( "FAILED!" );

            //this.ShowGraph();
            return false;
        }

        private Lattice ProveInternal( Expression exDestination ,
                                       object     source        ,
                                       double     weight        )
        {
            if(m_proveAttempts.Insert( source ) == false)
            {
                Debug_Print( "########" );
                Debug_PrintContext( exDestination, source, weight, "Round {0}, proving: ", m_proveAttempts.Count );

                m_proveState .Clear();
                m_proveActive.Clear();

                Debug_RecordReachabilityMarker();

                if(source is VariableExpression)
                {
                    //
                    // The algorithm doesn't work reliably with variables as the target of the proof.
                    //
                    // Of course it would be nice to be able to extend the algorithm to variable vs variable cases,
                    // but we need to prove if that is even theorically possible...
                    //
                }
                else
                {
                    Lattice res = ProveInternalDirect( exDestination, source, weight );
                    if(res != Lattice.False)
                    {
                        return res;
                    }
                }

                if(source is ArrayLengthHolder)
                {
                    ArrayLengthHolder     hld   = (ArrayLengthHolder)source;
                    VariableExpression    array = hld.Array as VariableExpression;
                    GrowOnlySet< object > set;

                    if(array != null && m_identitySets.TryGetValue( array, out set ))
                    {
                        foreach(object altSource in set)
                        {
                            foreach(object vertex in m_vertices.Keys)
                            {
                                ArrayLengthHolder hld2 = vertex as ArrayLengthHolder;
                                if(hld2 != null && hld != hld2 && hld2.Array == altSource)
                                {
                                    Lattice res = ProveInternal( exDestination, hld2, weight );
                                    if(res != Lattice.False)
                                    {
                                        return res;
                                    }
                                }
                            }
                        }
                    }
                }

                Operator opSource = GetDefinition( source );
    
                if(opSource is LoadIndirectOperator && opSource.HasAnnotation< ArrayLengthAnnotation >())
                {
                    object altSource = opSource.FirstArgument;
    
                    foreach(object vertex in m_vertices.Keys)
                    {
                        ArrayLengthHolder hld = vertex as ArrayLengthHolder;
                        if(hld != null && hld.Array == altSource)
                        {
                            Lattice res = ProveInternal( exDestination, hld, weight );
                            if(res != Lattice.False)
                            {
                                return res;
                            }
                        }
                    }
                }
                else if(opSource is SingleAssignmentOperator ||
                        opSource is PiOperator                )
                {
                    return ProveInternalIfVertex( exDestination, opSource.FirstArgument, weight );
                }
                else if(opSource is PhiOperator)
                {
                    Lattice res = Lattice.True;

                    foreach(Expression altSource in opSource.Arguments)
                    {
                        Lattice resSub = ProveInternalIfVertex( exDestination, altSource, weight );

                        if(res > resSub) res = resSub;
                    }

                    return res;
                }
            }

            return Lattice.False;
        }

        private Lattice ProveInternalIfVertex( Expression exDestination ,
                                               object     source        ,
                                               double     weight        )
        {
            if(m_vertices.ContainsKey( source ) == false)
            {
                return Lattice.False;
            }

            return ProveInternal( exDestination, source, weight );
        }

        //--//

        private Lattice ProveInternalDirect( object destination ,
                                             object source      ,
                                             double weight      )
        {
            Debug_RecordReachabilityStepIn( destination, source, weight );

            Debug_PrintFullContext( destination, source, weight, ">> Checking: {0}", IsMaxVertex( destination ) ? "MAX" : "MIN" );

            double valDestination;
            double valSource;

            if(m_constantsSet.TryGetValue( destination, out valDestination ) &&
               m_constantsSet.TryGetValue( source     , out valSource      )  )
            {
                Lattice res2 = IsComparisonAlwaysTrue( valDestination, valSource, weight ) ? Lattice.True : Lattice.False;

                Debug_Print( string.Format( " << {0} : Constant comparison.", res2 ) );

                return UpdateHistory( res2, destination, source, weight );
            }

            ConstraintState                      cs      = ExtractState( destination, source );
            GrowOnlyHashTable< double, Lattice > history = cs.m_history;

            //
            // Check for: same or stronger difference already proven.
            //
            foreach(double oldWeight in history.Keys)
            {
                if(IsLessThanOrEqual( oldWeight, weight ))
                {
                    if(history[oldWeight] == Lattice.True)
                    {
                        Debug_Print( "<< True : Same or stronger difference already proven." );

                        return UpdateHistory( Lattice.True, destination, source, weight );
                    }
                }
            }

            //
            // Check for: same or weaker difference already disproved.
            //
            foreach(double oldWeight in history.Keys)
            {
                if(IsGreaterThanOrEqual( oldWeight, weight ))
                {
                    if(history[oldWeight] == Lattice.False)
                    {
                        Debug_Print( "<< False : Same or weaker difference already disproved." );

                        return UpdateHistory( Lattice.False, destination, source, weight );
                    }
                }
            }

            //
            // Check for: "source" on a cycle that was reduced for same or stronger difference.
            //
            foreach(double oldWeight in history.Keys)
            {
                if(IsLessThanOrEqual( oldWeight, weight ))
                {
                    if(history[oldWeight] == Lattice.Reduced)
                    {
                        Debug_Print( "<< Reduced : source on a cycle that was reduced for same or stronger difference." );

                        return UpdateHistory( Lattice.Reduced, destination, source, weight );
                    }
                }
            }

            //
            // Check for: traversal reached the source vertex, success if "source - destination (== 0) <= weight".
            //
            if(source == destination)
            {
                if(IsGreaterThanOrEqual( weight, 0 ))
                {
                    Debug_Print( "<< True : Traversal reached the source vertex with lower weight." );

                    return UpdateHistory( Lattice.True, destination, source, weight );
                }
                else
                {
                    Debug_Print( "<< False : Traversal reached the source vertex with higher weight." );

                    return UpdateHistory( Lattice.False, destination, source, weight );
                }
            }

            object previousWeight;

            if(m_proveActive.TryGetValue( destination, out previousWeight ) && previousWeight is double)
            {
                //
                // Found cycle.
                //
                if(IsLessThan( weight, (double)previousWeight ))
                {
                    //
                    // weight < oldWeight, it's an amplifying cycle.
                    //
                    Debug_Print( "<< False : Amplifying cycle." );

                    return UpdateHistory( Lattice.False, destination, source, weight );
                }
                else
                {
                    //
                    // Harmless cycle.
                    //
                    Debug_Print( "<< Reduced : Harmless cycle." );

                    return UpdateHistory( Lattice.Reduced, destination, source, weight );
                }
            }

            List< ConstraintInstance > lst;

            if(m_incomingEdges.TryGetValue( destination, out lst ) == false)
            {
                Debug_Print( "<< False: No incoming edges found." );

                return UpdateHistory( Lattice.False, destination, source, weight );
            }

            //--//

            m_proveActive[destination] = weight;

            bool    fUseMeet = IsMaxVertex( destination );
            Lattice res      = fUseMeet ? Lattice.True : Lattice.False;

            foreach(ConstraintInstance ci in lst)
            {
                Lattice val = ProveInternalDirect( ci.Source, source, weight - ci.Weight );

                if(fUseMeet)
                {
                    if(res > val) res = val;
                }
                else
                {
                    if(res < val) res = val;
                }

                Debug_PrintContext( destination, source, weight, "   Step: {0} for ", res );
            }

            m_proveActive[destination] = null;

            //--//

            Debug_PrintContext( destination, source, weight, "<< Update: {0} for ", res );

            return UpdateHistory( res, destination, source, weight );
        }

        private bool IsLessThan( double left  ,
                                 double right )
        {
            if(m_kind == Kind.LessThanOrEqual)
            {
                return (left < right);
            }
            else
            {
                return (left > right);
            }
        }

        private bool IsLessThanOrEqual( double left  ,
                                        double right )
        {
            if(m_kind == Kind.LessThanOrEqual)
            {
                return (left <= right);
            }
            else
            {
                return (left >= right);
            }
        }

        private bool IsGreaterThanOrEqual( double left  ,
                                           double right )
        {
            if(m_kind == Kind.LessThanOrEqual)
            {
                return (left >= right);
            }
            else
            {
                return (left <= right);
            }
        }

        private bool IsComparisonAlwaysTrue( double valDestination ,
                                             double valSource      ,
                                             double weight         )
        {
            if(m_kind == Kind.LessThanOrEqual)
            {
                return (valDestination - valSource <= weight);
            }
            else
            {
                return (valDestination - valSource >= weight);
            }
        }

        private Operator GetDefinition( object obj )
        {
            Operator op;

            m_vertices.TryGetValue( obj, out op );

            return op;
        }

        private bool IsMaxVertex( object obj )
        {
            return GetDefinition( obj ) is PhiOperator;
        }

        private Lattice UpdateHistory( Lattice res         ,
                                       object  destination ,
                                       object  source      ,
                                       double  weight      )
        {
            Debug_RecordReachabilityStepOut( res );

            ConstraintState                      cs      = ExtractState( destination, source );
            GrowOnlyHashTable< double, Lattice > history = cs.m_history;

            history[weight] = res;

            return res;
        }

        private ConstraintState ExtractState( object destination ,
                                              object source      )
        {
            ConstraintState cs;

            m_proveLookupHelper.m_destination = destination;
            m_proveLookupHelper.m_source      = source;

            if(m_proveState.Contains( m_proveLookupHelper, out cs ) == false)
            {
                cs = new ConstraintState( destination, source );

                m_proveState.Insert( cs );
            }

            return cs;
        }

        //--//

        private void Collect()
        {
            CollectAssignments();

            ComputeEquivalenceSet();

            GrowOnlySet< PiOperator > set = CollectPiOperators();

            CollectConstraints( set );

            //AddConstraintsBetweenConstants();
        }

        private GrowOnlySet< PiOperator > CollectPiOperators()
        {
            GrowOnlySet< PiOperator > set = SetFactory.New< PiOperator >();

            foreach(var piOp in m_cfg.FilterOperators< PiOperator >())
            {
                set.Insert( piOp );
            }

            return set;
        }

        private void CollectAssignments()
        {
            foreach(Operator op in m_cfg.DataFlow_SpanningTree_Operators)
            {
                if(op is SingleAssignmentOperator)
                {
                    RecordAssignement( op.FirstResult, op.FirstArgument );
                }
                else if(op is PhiOperator)
                {
                    foreach(Expression ex in op.Arguments)
                    {
                        RecordAssignement( op.FirstResult, ex );
                    }
                }
                else if(op is PiOperator)
                {
                    RecordAssignement( op.FirstResult, op.FirstArgument );
                }
                else if(op is LoadIndirectOperator && op.HasAnnotation< ArrayLengthAnnotation >())
                {
                    ArrayLengthHolder hld = m_arrays.MakeUnique( new ArrayLengthHolder( op.FirstArgument ) );

                    RecordAssignement( op.FirstResult, hld );
                }
            }
        }

        private void CollectConstraints( GrowOnlySet< PiOperator > set )
        {
            foreach(Operator op in m_cfg.DataFlow_SpanningTree_Operators)
            {
                if(op is SingleAssignmentOperator)
                {
                    RecordConstraint( op.FirstResult, op.FirstArgument, 0 );

                    RecordDefinition( op );
                }
                else if(op is PhiOperator)
                {
                    foreach(Expression ex in op.Arguments)
                    {
                        RecordConstraint( op.FirstResult, ex, 0 );
                    }

                    RecordDefinition( op );
                }
                else if(op is PiOperator)
                {
                    RecordConstraint( op.FirstResult, op.FirstArgument, 0 );

                    PiOperator piOp = (PiOperator)op;
                    Expression exLeft;
                    Expression exRight;
                    long       val;

                    if(IsCompatible( set, piOp, out exLeft, out exRight, out val ))
                    {
                        RecordConstraint( exLeft, exRight, val );
                    }

                    RecordDefinition( op );
                }
                else if(op is BinaryOperator)
                {
                    BinaryOperator op2 = (BinaryOperator)op;

                    op2.EnsureConstantToTheRight();

                    Expression exLeft  = op2.FirstArgument;
                    Expression exRight = op2.SecondArgument;

                    if(!(exLeft  is ConstantExpression) &&
                        (exRight is ConstantExpression)  )
                    {
                        ConstantExpression exConst = (ConstantExpression)exRight;
                        bool               fOk;
                        bool               fNegate;

                        switch(op2.Alu)
                        {
                            case BinaryOperator.ALU.ADD:
                                fOk     = true;
                                fNegate = false;
                                break;

                            case BinaryOperator.ALU.SUB:
                                fOk     = true;
                                fNegate = true;
                                break;

                            default:
                                fOk     = false;
                                fNegate = false;
                                break;
                        }

                        if(fOk)
                        {
                            if(exConst.IsValueFloatingPoint)
                            {
                                double value;

                                if(exConst.GetFloatingPoint( out value ))
                                {
                                    if(fNegate)
                                    {
                                        value = -value;
                                    }

                                    RecordConstraint( op.FirstResult, exLeft, value );

                                    RecordDefinition( op );
                                }
                            }
                            else if(exConst.IsValueInteger)
                            {
                                long value;

                                if(exConst.GetAsSignedInteger( out value ))
                                {
                                    if(fNegate)
                                    {
                                        value = -value;
                                    }

                                    RecordConstraint( op.FirstResult, exLeft, value );

                                    RecordDefinition( op );
                                }
                            }
                        }
                    }
                }
                else if(op is LoadIndirectOperator && op.HasAnnotation< ArrayLengthAnnotation >())
                {
                    ArrayLengthHolder hld = m_arrays.MakeUnique( new ArrayLengthHolder( op.FirstArgument ) );

                    RecordConstraint( op.FirstResult, hld, 0 );

                    RecordDefinition( op );
                }
            }
        }

        private void RecordDefinition( Operator op )
        {
            foreach(var ex in op.Results)
            {
                m_vertices[ex] = op;
            }
        }

        private void AddConstraintsBetweenConstants()
        {
            foreach(ConstraintInstance ci in m_set.ToArray()) // Get a copy of the contents, since we're going to modify the hash table.
            {
                object destination = ci.Destination;
                object source      = ci.Source;

                if(m_constantsSet.ContainsKey( source ))
                {
                    double oldValue = m_constantsSet[source];

                    foreach(object constObj in m_constantsSet.Keys)
                    {
                        if(source != constObj)
                        {
                            double newValue = m_constantsSet[constObj];
                            double diff     = newValue - oldValue;

                            RecordConstraint( ci.Destination, constObj, ci.Weight - diff );
                        }
                    }
                }

                if(m_constantsSet.ContainsKey( destination ))
                {
                    double oldValue = m_constantsSet[destination];

                    foreach(object constObj in m_constantsSet.Keys)
                    {
                        if(destination != constObj)
                        {
                            double newValue = m_constantsSet[constObj];
                            double diff     = newValue - oldValue;

                            RecordConstraint( constObj, source, ci.Weight + diff );
                        }
                    }
                }
            }
        }

        private void ComputeEquivalenceSet()
        {
            GrowOnlySet< object > roots   = SetFactory.New< object >();
            GrowOnlySet< object > history = SetFactory.New< object >();

            foreach(VariableExpression var in m_assignments.Keys)
            {
                roots.Clear();

                foreach(object src in m_assignments[var])
                {
                    history.Clear();

                    FindRoots( roots, history, src );
                }

                if(roots.Count == 1)
                {
                    foreach(object src in m_assignments[var])
                    {
                        MergeEquivalenceSets( var, src );
                    }
                }
            }
        }

        private void FindRoots( GrowOnlySet< object > roots   ,
                                GrowOnlySet< object > history ,
                                object                obj     )
        {
            if(history.Insert( obj ) == false)
            {
                GrowOnlySet< object > set;

                if(obj is VariableExpression && m_assignments.TryGetValue( (VariableExpression)obj, out set ))
                {
                    foreach(object next in set)
                    {
                        FindRoots( roots, history, next );
                    }
                }
                else
                {
                    roots.Insert( obj );
                }
            }
        }

        private void MergeEquivalenceSets( object obj1 ,
                                           object obj2 )
        {
            GrowOnlySet< object > set1 = HashTableWithSetFactory.Create( m_identitySets, obj1 );
            GrowOnlySet< object > set2 = HashTableWithSetFactory.Create( m_identitySets, obj2 );
            GrowOnlySet< object > setM = set1.CloneSettings();

            setM.Merge ( set1 );
            setM.Merge ( set2 );
            setM.Insert( obj1 );
            setM.Insert( obj2 );

            foreach(object obj in setM)
            {
                m_identitySets[obj] = setM;
            }
        }

        //--//

        private void RecordAssignement( VariableExpression varDst ,
                                        object             source )
        {
            HashTableWithSetFactory.Add( m_assignments, varDst, source );
        }

        private bool IsCompatible(     GrowOnlySet< PiOperator > set     ,
                                       PiOperator                op      ,
                                   out Expression                exLeft  ,
                                   out Expression                exRight ,
                                   out long                      val     )
        {
            Expression left  = null;
            Expression right = null;
            long       res   = 0;

            switch(m_kind)
            {
                case Kind.LessThanOrEqual:
                    switch(op.RelationOperator)
                    {
                        case PiOperator.Relation.Equal                  :
                        case PiOperator.Relation.SignedLessThanOrEqual  :
                        case PiOperator.Relation.UnsignedLowerThanOrSame: 
                            left  = op.LeftExpression;
                            right = op.RightExpression;
                            res   = 0;
                            break;

                        case PiOperator.Relation.UnsignedHigherThanOrSame:
                        case PiOperator.Relation.SignedGreaterThanOrEqual:
                            left  = op.RightExpression;
                            right = op.LeftExpression;
                            res   = 0;
                            break;

                        case PiOperator.Relation.UnsignedLowerThan:
                        case PiOperator.Relation.SignedLessThan   :
                            left  = op.LeftExpression;
                            right = op.RightExpression;
                            res   = -1;
                            break;

                        case PiOperator.Relation.UnsignedHigherThan:
                        case PiOperator.Relation.SignedGreaterThan :
                            left  = op.RightExpression;
                            right = op.LeftExpression;
                            res   = -1;
                            break;
                    }
                    break;

                case Kind.GreaterThanOrEqual:
                    switch(op.RelationOperator)
                    {
                        case PiOperator.Relation.Equal                  :
                        case PiOperator.Relation.SignedLessThanOrEqual  :
                        case PiOperator.Relation.UnsignedLowerThanOrSame: 
                            left  = op.RightExpression;
                            right = op.LeftExpression;
                            res   = 0;
                            break;

                        case PiOperator.Relation.UnsignedHigherThanOrSame:
                        case PiOperator.Relation.SignedGreaterThanOrEqual:
                            left  = op.LeftExpression;
                            right = op.RightExpression;
                            res   = 0;
                            break;

                        case PiOperator.Relation.UnsignedLowerThan:
                        case PiOperator.Relation.SignedLessThan   :
                            left  = op.RightExpression;
                            right = op.LeftExpression;
                            res   = -1;
                            break;

                        case PiOperator.Relation.UnsignedHigherThan:
                        case PiOperator.Relation.SignedGreaterThan :
                            left  = op.LeftExpression;
                            right = op.RightExpression;
                            res   = -1;
                            break;
                    }
                    break;
            }

            exLeft  = FindDestination( set, op, left  );
            exRight = FindDestination( set, op, right );
            val     = res;

            return exLeft is VariableExpression;
        }

        private Expression FindDestination( GrowOnlySet< PiOperator > set ,
                                            PiOperator                op  ,
                                            Expression                ex  )
        {
            if(ex is VariableExpression)
            {
                foreach(PiOperator pi in set)
                {
                    if(PiOperator.SameAnnotation( op, pi ))
                    {
                        if(pi.FirstArgument == ex)
                        {
                            return pi.FirstResult;
                        }
                    }
                }
            }

            return ex;
        }

        private void RecordConstraint( object exLeft  ,
                                       object exRight ,
                                       double val     )
        {
            ConstraintInstance ci = new ConstraintInstance( exLeft, exRight, val );

            if(m_set.Insert( ci ) == false)
            {
                HashTableWithListFactory.AddUnique( m_incomingEdges, exLeft, ci );

                RecordVertex( exLeft  );
                RecordVertex( exRight );
            }
        }

        private void RecordVertex( object obj )
        {
            if(m_vertices.ContainsKey( obj ) == false)
            {
                m_vertices[obj] = null;

                if(obj is ConstantExpression)
                {
                    ConstantExpression exConst = (ConstantExpression)obj;

                    if(exConst.IsValueInteger)
                    {
                        if(exConst.IsValueSigned)
                        {
                            long val;

                            exConst.GetAsSignedInteger( out val );

                            m_constantsSet[obj] = (double)val;
                        }
                        else
                        {
                            ulong val;

                            exConst.GetAsUnsignedInteger( out val );

                            m_constantsSet[obj] = (double)val;
                        }
                    }
                    else if(exConst.IsValueFloatingPoint)
                    {
                        double val;

                        exConst.GetFloatingPoint( out val );

                        m_constantsSet[obj] = val;
                    }
                }
            }
        }

        //
        // Access Methods
        //

        public Kind SystemKind
        {
            get
            {
                return m_kind;
            }
        }

        //
        // Debug Methods
        //

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_Flush()
        {
#if DUMP_CONSTRAINTSYSTEM
            Console.Out.Flush();
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_Print(        char     prefix ,
                                         string   fmt    ,
                                  params object[] args   )
        {
#if DUMP_CONSTRAINTSYSTEM
            Console.Write    ( new String( prefix, m_debug_indent + 1 ) );
            Console.Write    ( " "                                      );
            Console.WriteLine( fmt, args                                );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_Print(        string   fmt  ,
                                  params object[] args )
        {
#if DUMP_CONSTRAINTSYSTEM
            Debug_Print( (char)('0' + (m_debug_indent % 10)), fmt, args );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_PrintContext(        object   destination ,
                                                object   source      ,
                                                double   weight      ,
                                                string   fmt         ,
                                         params object[] args        )
        {
#if DUMP_CONSTRAINTSYSTEM
            Debug_Print( "{0,-20} {1} - {2} {3} {4}", string.Format( fmt, args ), GenerateLabel( destination ), GenerateLabel( source ), m_kind == Kind.LessThanOrEqual ? "<=" : ">=", weight );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_PrintFullContext(        object   destination ,
                                                    object   source      ,
                                                    double   weight      ,
                                                    string   fmt         ,
                                             params object[] args        )
        {
#if DUMP_CONSTRAINTSYSTEM
            Debug_Print( "{0,-20} {1} - {2} {3} {4}", string.Format( fmt, args ), GenerateLabel( destination ), GenerateLabel( source ), m_kind == Kind.LessThanOrEqual ? "<=" : ">=", weight );

            Debug_Association( destination );
            Debug_Association( source      );
#endif
        }

#if DUMP_CONSTRAINTSYSTEM

        private void Debug_Association( object obj )
        {
            string   label = GenerateLabel( obj );
            Operator op    = GetDefinition( obj );

            if(op != null)
            {
                obj = op.ToPrettyString();
            }
            else if(obj is ArrayLengthHolder)
            {
                ArrayLengthHolder hld = (ArrayLengthHolder)obj;

                obj = hld.Array;
            }

            Debug_Print( "{0,-20} {1} is {2}", "", label, obj );
        }

#endif

        //--//

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        public void ShowGraph()
        {
#if DUMP_CONSTRAINTSYSTEM
            IVisualizer itf = m_cfg.TypeSystem.GetEnvironmentService< IVisualizer >();

            if(itf != null)
            {
                GenerateLabels();

                GraphState gs = new GraphState();

                gs.Flavor       = m_kind;;
                gs.Constraints  = m_set;
                gs.Vertices     = m_vertices;
                gs.IdentitySets = m_identitySets;
                gs.ConstantsSet = m_constantsSet;
                gs.Labels       = m_debug_labels;
                gs.ProofTests   = m_debug_testHistory;

                itf.DisplayGraph( gs );
            }
#endif
        }


        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        public void Dump()
        {
#if DUMP_CONSTRAINTSYSTEM
            SortedList< string, object > sortedList = new SortedList< string, object >();
            System.Text.StringBuilder    sb         = new System.Text.StringBuilder();

            GenerateLabels();

            Console.WriteLine( "#############################################################################" );

            Console.WriteLine( "Nodes:" );
            foreach(object obj in m_debug_labels.Keys)
            {
                sortedList.Add( m_debug_labels[obj], obj );
            }
            
            foreach(KeyValuePair< string, object > pair in sortedList)
            {
                Console.WriteLine( "   {0} => {1}", pair.Key, pair.Value );
            }
            Console.WriteLine();

            sortedList.Clear();

            Console.WriteLine( "Edges:" );
            foreach(ConstraintInstance ci in m_set)
            {
                sortedList.Add( string.Format( "{0} - {1} {2} {3}", GenerateLabel( ci.Destination ), GenerateLabel( ci.Source ), m_kind == Kind.LessThanOrEqual ? "<=" : ">=", ci.Weight ), null );
            }
            
            foreach(KeyValuePair< string, object > pair in sortedList)
            {
                Console.WriteLine( "{0}", pair.Key );
            }
            Console.WriteLine();

            sortedList.Clear();

            Console.WriteLine( "Identity Sets:" );
            foreach(object obj in m_identitySets.Keys)
            {
                sb.Length = 0;
                sb.AppendFormat( "{0} === ", GenerateLabel( obj ) );

                foreach(object obj2 in m_identitySets[obj])
                {
                    sb.AppendFormat( "{0} ", GenerateLabel( obj2 ) );
                }

                sortedList.Add( sb.ToString(), null );
            }
            
            foreach(KeyValuePair< string, object > pair in sortedList)
            {
                Console.WriteLine( "{0}", pair.Key );
            }
            Console.WriteLine();
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_RecordReachabilityMarker()
        {
#if DUMP_CONSTRAINTSYSTEM
            if(m_debug_testHistory == null)
            {
                m_debug_testHistory = new List< object >();
            }

            m_debug_testHistory.Add( "Reset" );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_RecordReachabilityStepIn( object destination ,
                                                     object source      ,
                                                     double weight      )
        {
#if DUMP_CONSTRAINTSYSTEM
            m_debug_indent++;

            m_debug_testHistory.Add( new ConstraintInstance( destination, source, weight ) );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_RecordReachabilityStepOut( Lattice res )
        {
#if DUMP_CONSTRAINTSYSTEM
            m_debug_indent--;

            m_debug_testHistory.Add( res );
#endif
        }

        [System.Diagnostics.Conditional( "DUMP_CONSTRAINTSYSTEM" )]
        private void Debug_ResetReachability()
        {
#if DUMP_CONSTRAINTSYSTEM
            m_debug_testHistory = null;
#endif
        }

        //--//

#if DUMP_CONSTRAINTSYSTEM

        private void GenerateLabels()
        {
            if(m_debug_labels == null)
            {
                m_debug_labels        = HashTableFactory.New< object            , string                     >();
                m_debug_baseVariables = HashTableFactory.New< VariableExpression, List< VariableExpression > >();
                m_debug_constants     = new List            < ConstantExpression                             >();

                foreach(ConstraintInstance ci in m_set)
                {
                    GenerateLabel( ci.Destination );
                    GenerateLabel( ci.Source      );
                }
            }
        }

        private string GenerateLabel( object obj )
        {
            GenerateLabels();

            string text;

            if(m_debug_labels.TryGetValue( obj, out text ) == false)
            {
                if(obj is VariableExpression)
                {
                    VariableExpression var     = (VariableExpression)obj;
                    VariableExpression baseVar = var.AliasedVariable;
                    int                num     = 0;
                    int                ver     = -1;

                    foreach(VariableExpression oldBaseVar in m_debug_baseVariables.Keys)
                    {
                        if(oldBaseVar == baseVar)
                        {
                            List< VariableExpression > lst = m_debug_baseVariables[oldBaseVar];

                            for(int i = 0; i < lst.Count; i++)
                            {
                                if(lst[i] == var)
                                {
                                    ver = i + 1;
                                    break;
                                }
                            }
                           
                            ver = lst.Count + 1;

                            lst.Add( var );
                            break;
                        }

                        num++;
                    }

                    if(ver == -1)
                    {
                        HashTableWithListFactory.Add( m_debug_baseVariables, baseVar, var );

                        ver = 1;
                    }

                    text = string.Format( "V{0}_{1}", num, ver );
                }
                else if(obj is ConstantExpression)
                {
                    ConstantExpression val = (ConstantExpression)obj;
                    int                num = -1;

                    for(int i = 0; i < m_debug_constants.Count; i++)
                    {
                        if(m_debug_constants[i] == val)
                        {
                            num = i;
                            break;
                        }
                    }

                    if(num == -1)
                    {
                        num = m_debug_constants.Count;

                        m_debug_constants.Add( val );
                    }

                    if(val.IsValueFloatingPoint || val.IsValueInteger)
                    {
                        text = string.Format( "C{0}({1})", num, val.Value );
                    }
                    else
                    {
                        text = string.Format( "C{0}", num );
                    }
                }
                else if(obj is ArrayLengthHolder)
                {
                    ArrayLengthHolder hld = (ArrayLengthHolder)obj;

                    text = string.Format( "<length of {0}>", GenerateLabel( hld.Array ) );
                }
                else
                {
                    text = obj.ToString();
                }

                m_debug_labels[obj] = text;
            }

            return text;
        }

#endif
    }
}
