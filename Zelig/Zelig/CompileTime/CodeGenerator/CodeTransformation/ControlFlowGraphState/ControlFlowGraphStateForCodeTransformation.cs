//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation : ControlFlowGraphState
    {
        //
        // State
        //

        private TypeSystemForCodeTransformation               m_typeSystem;
        private GrowOnlyHashTable< Expression, Expression[] > m_lookupFragments;
        private GrowOnlyHashTable< string    , object       > m_propertyBag;

        //
        // Constructor Methods
        //

        public ControlFlowGraphStateForCodeTransformation() // Default constructor required by TypeSystemSerializer.
        {
            AllocateState();
        }

        public ControlFlowGraphStateForCodeTransformation( TypeSystemForCodeTransformation typeSystem ,
                                                           MethodRepresentation            md         ) : base( md )
        {
            m_typeSystem = typeSystem;

            AllocateState();
        }

        protected ControlFlowGraphStateForCodeTransformation( ControlFlowGraphState source ) : base( source )
        {
            ControlFlowGraphStateForCodeTransformation source2 = (ControlFlowGraphStateForCodeTransformation)source;

            m_typeSystem = source2.m_typeSystem;

            AllocateState();
        }

        private void AllocateState()
        {
            m_lookupFragments = HashTableFactory.NewWithReferenceEquality< Expression, Expression[] >();
            m_propertyBag     = HashTableFactory.New                     < string,     object       >();
        }

        //--//

        public ControlFlowGraphStateForCodeTransformation Clone( InstantiationContext ic )
        {
            ControlFlowGraphStateForCodeTransformation clonedCFG = new ControlFlowGraphStateForCodeTransformation( this );

            CloningContext context = new CloneForwardGraph( this, clonedCFG, ic );

            clonedCFG.CloneVariables( context, this );

            context.Clone( m_entryBasicBlock );

            return clonedCFG;
        }

        //--//

        protected override void CloneVariables( CloningContext        context ,
                                                ControlFlowGraphState source  )
        {
            ControlFlowGraphStateForCodeTransformation source2 = (ControlFlowGraphStateForCodeTransformation)source;

            base.CloneVariables( context, source2 );

            foreach(VariableExpression var in source2.DataFlow_SpanningTree_Variables)
            {
                CloneVariable( context, source2, var );
            }
        }

        private VariableExpression CloneVariable( CloningContext                             context ,
                                                  ControlFlowGraphStateForCodeTransformation source  ,
                                                  VariableExpression                         var     )
        {
            VariableExpression newVar = context.LookupRegistered( var );

            if(newVar == null)
            {
                TypeRepresentation           td           = var.Type;
                VariableExpression.DebugInfo debugInfo    = var.DebugName;
                VariableExpression           sourceVar    = null;
                uint                         sourceOffset = 0;

                if(var is LowLevelVariableExpression)
                {
                    LowLevelVariableExpression low = (LowLevelVariableExpression)var;

                    if(low.SourceVariable != null)
                    {
                        sourceVar    = CloneVariable( context, source, low.SourceVariable );
                        sourceOffset =                                 low.SourceOffset;
                    }
                }

                if(var is LocalVariableExpression)
                {
                    LocalVariableExpression loc = (LocalVariableExpression)var;

                    newVar = AllocateLocal( td, debugInfo );
                }
                else if(var is TemporaryVariableExpression)
                {
                    newVar = AllocateTemporary( td, debugInfo );
                }
                else if(var is ExceptionObjectVariableExpression)
                {
                    newVar = AllocateExceptionObjectVariable( td );
                }
                else if(var is PhiVariableExpression)
                {
                    PhiVariableExpression phiVar    = (PhiVariableExpression)var;
                    VariableExpression    newTarget = CloneVariable( context, source, phiVar.Target );

                    newVar = AllocatePhiVariable( newTarget );
                }
                else if(var is PseudoRegisterExpression)
                {
                    newVar = AllocatePseudoRegister( td, debugInfo, sourceVar, sourceOffset );
                }
                else if(var is TypedPhysicalRegisterExpression)
                {
                    TypedPhysicalRegisterExpression reg = (TypedPhysicalRegisterExpression)var;

                    newVar = AllocateTypedPhysicalRegister( td, reg.RegisterDescriptor, debugInfo, reg.SourceVariable, reg.SourceOffset );
                }
                else if(var is PhysicalRegisterExpression)
                {
                    PhysicalRegisterExpression reg = (PhysicalRegisterExpression)var;

                    newVar = AllocatePhysicalRegister( reg.RegisterDescriptor );
                }
                else if(var is StackLocationExpression)
                {
                    StackLocationExpression stack = (StackLocationExpression)var;

                    newVar = AllocateStackLocation( td, debugInfo, stack.Number, stack.StackPlacement, sourceVar, sourceOffset );
                }
                else if(var is ConditionCodeExpression)
                {
                    newVar = AllocateConditionCode();
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected expression '{0}' during cloning of '{1}'", var, source );
                }

                newVar.SkipReferenceCounting = var.SkipReferenceCounting;

                context.Register( var, newVar );
            }

            return newVar;
        }

        //--//

        internal VariableExpression[] AllocateVariables( TypeRepresentation[] localVars     ,
                                                         string[]             localVarNames )
        {
            if(m_md.ReturnType != m_typeSystem.WellKnownTypes.System_Void)
            {
                m_returnValue = AllocateTemporary( m_md.ReturnType, null );
            }

            //--//

            TypeRepresentation[] args    = m_md.ThisPlusArguments;
            int                  argsNum = args.Length;

            m_arguments = new VariableExpression[argsNum];

            for(int i = 0; i < argsNum; i++)
            {
                VariableExpression.DebugInfo debugInfo;
                string[]                     names = m_md.ArgumentNames;

                if(names != null)
                {
                    debugInfo = new VariableExpression.DebugInfo( m_md, names[i], i, false );
                }
                else
                {
                    debugInfo = null;
                }

                m_arguments[i] = new ArgumentVariableExpression( args[i], debugInfo, i );

                //
                // We want to have each argument variable assigned to on entry to the method,
                // so we create an ArgumentValueOperator (see GenerateVariableInitialization)
                //
                Operator op = GenerateVariableInitialization( null, m_arguments[i] );

                this.EntryBasicBlock.AddOperator( op );
            }

            //--//

            VariableExpression[] locals;

            if(localVars == null)
            {
                locals = VariableExpression.SharedEmptyArray;
            }
            else
            {
                int varsNum = localVars.Length;

                locals = new VariableExpression[varsNum];

                var bb = this.GetInjectionPoint( BasicBlock.Qualifier.PrologueEnd ); 

                for(int i = 0; i < varsNum; i++)
                {
                    VariableExpression.DebugInfo debugInfo;

                    if(localVarNames != null)
                    {
                        debugInfo = new VariableExpression.DebugInfo( m_md, localVarNames[i], i, true );
                    }
                    else
                    {
                        debugInfo = null;
                    }

                    locals[i] = AllocateLocal( localVars[i], debugInfo );

                    Operator op = GenerateVariableInitialization( null, locals[i] );

                    bb.AddOperator( op );
                }
            }

            return locals;
        }

        //--//

        //
        // Helper Methods
        //

        public PhiVariableExpression AllocatePhiVariable( VariableExpression target )
        {
            PhiVariableExpression newPhi = new PhiVariableExpression( target );

            TrackVariable( newPhi );

            return newPhi;
        }

        public PseudoRegisterExpression AllocatePseudoRegister( TypeRepresentation td )
        {
            return AllocatePseudoRegister( td, null, null, 0 );
        }

        public PseudoRegisterExpression AllocatePseudoRegister( TypeRepresentation           td        ,
                                                                VariableExpression.DebugInfo debugInfo )
        {
            return AllocatePseudoRegister( td, debugInfo, null, 0 );
        }

        internal PseudoRegisterExpression AllocatePseudoRegister( TypeRepresentation           td           ,
                                                                  VariableExpression.DebugInfo debugInfo    ,
                                                                  VariableExpression           sourceVar    ,
                                                                  uint                         sourceOffset )
        {
            CHECKS.ASSERT( m_typeSystem.PlatformAbstraction.CanFitInRegister( td ), "Type '{0}' is too large to fit in a pseudo register", td );

            PseudoRegisterExpression newPseudo = new PseudoRegisterExpression( td, debugInfo, sourceVar, sourceOffset );

            TrackVariable( newPseudo );

            return newPseudo;
        }

        public PhysicalRegisterExpression AllocatePhysicalRegister( Abstractions.RegisterDescriptor regDesc )
        {
            for(int pos = m_variablesCount; --pos >= 0; )
            {
                PhysicalRegisterExpression reg = m_variables[pos] as PhysicalRegisterExpression;

                if(reg != null && !(reg is TypedPhysicalRegisterExpression) && reg.Number == regDesc.Index)
                {
                    return reg;
                }
            }

            TypeRepresentation         td     = m_typeSystem.PlatformAbstraction.GetRuntimeType( m_typeSystem, regDesc );
            PhysicalRegisterExpression newReg = new PhysicalRegisterExpression( td, regDesc, null, null, 0 );

            TrackVariable( newReg );

            return newReg;
        }

        public TypedPhysicalRegisterExpression AllocateTypedPhysicalRegister( TypeRepresentation              td           ,
                                                                              Abstractions.RegisterDescriptor regDesc      ,
                                                                              VariableExpression.DebugInfo    debugInfo    ,
                                                                              VariableExpression              sourceVar    ,
                                                                              uint                            sourceOffset )
        {
            TypedPhysicalRegisterExpression newReg = new TypedPhysicalRegisterExpression( td, regDesc, debugInfo, sourceVar, sourceOffset );

            TrackVariable( newReg );

            return newReg;
        }

        internal StackLocationExpression AllocateLocalStackLocation( TypeRepresentation           td           ,
                                                                     VariableExpression.DebugInfo debugInfo    ,
                                                                     VariableExpression           sourceVar    ,
                                                                     uint                         sourceOffset )
        {
            int wordOffset = 0;

            for(int pos = m_variablesCount; --pos >= 0; )
            {
                StackLocationExpression stack = m_variables[pos]  as StackLocationExpression;

                if(stack != null && stack.StackPlacement == StackLocationExpression.Placement.Local)
                {
                    if(wordOffset <= stack.Number)
                    {
                        wordOffset = stack.Number + 1;
                    }
                }
            }

            return AllocateStackLocation( td, debugInfo, wordOffset, StackLocationExpression.Placement.Local, sourceVar, sourceOffset );
        }

        internal StackLocationExpression AllocateStackLocation( TypeRepresentation                td           ,
                                                                VariableExpression.DebugInfo      debugInfo    ,
                                                                int                               wordOffset   ,
                                                                StackLocationExpression.Placement placement    ,
                                                                VariableExpression                sourceVar    ,
                                                                uint                              sourceOffset )
        {
            StackLocationExpression newStack = new StackLocationExpression( td, debugInfo, wordOffset, placement, sourceVar, sourceOffset );

            TrackVariable( newStack );

            return newStack;
        }

        public ConditionCodeExpression AllocateConditionCode()
        {
            if(m_typeSystem.PlatformAbstraction.CanUseMultipleConditionCodes == false)
            {
                for(int pos = m_variablesCount; --pos >= 0; )
                {
                    var cc = m_variables[pos] as ConditionCodeExpression;
                    if(cc != null)
                    {
                        return cc;
                    }
                }
            }

            //--//

            var newCC = new ConditionCodeExpression( m_typeSystem.WellKnownTypes.System_UInt32, null, 0 );

            TrackVariable( newCC );

            return newCC;
        }

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            InnerApplyTransformation( context );

            context.Transform( ref m_typeSystem      );
            context.Transform( ref m_lookupFragments );
            context.Transform( ref m_propertyBag     );

            ApplyTransformation_MapToMachine( context );

            context.Pop();
        }

        //--//

        public override Operator GenerateVariableInitialization( Debugging.DebugInfo debugInfo       ,
                                                                 Expression          var             ,
                                                                 TypeRepresentation  td              ,
                                                                 bool                fThroughPointer )
        {
            Expression ex;

            if(var is ArgumentVariableExpression)
            {
                //
                // We want to have each argument variable assigned to on entry to the method, so we create an InvalidateExpressionOperator.
                //
                return InitialValueOperator.New( debugInfo, (VariableExpression)var );
            }

            if(td is ReferenceTypeRepresentation ||
               td is PointerTypeRepresentation    )
            {
                ex = m_typeSystem.CreateNullPointer( td );
            }
            else
            {
                if(td is EnumerationTypeRepresentation)
                {
                    td = td.UnderlyingType;
                }

                if(td is ScalarTypeRepresentation)
                {
                    object val;

                    switch(td.BuiltInType)
                    {
                        case TypeRepresentation.BuiltInTypes.BOOLEAN: val = (System.Boolean)false; break;
                        case TypeRepresentation.BuiltInTypes.CHAR   : val = (System.Char   )    0; break;
                        case TypeRepresentation.BuiltInTypes.I1     : val = (System.SByte  )    0; break;
                        case TypeRepresentation.BuiltInTypes.U1     : val = (System.Byte   )    0; break;
                        case TypeRepresentation.BuiltInTypes.I2     : val = (System.Int16  )    0; break;
                        case TypeRepresentation.BuiltInTypes.U2     : val = (System.UInt16 )    0; break;
                        case TypeRepresentation.BuiltInTypes.I4     : val = (System.Int32  )    0; break;
                        case TypeRepresentation.BuiltInTypes.U4     : val = (System.UInt32 )    0; break;
                        case TypeRepresentation.BuiltInTypes.I8     : val = (System.Int64  )    0; break;
                        case TypeRepresentation.BuiltInTypes.U8     : val = (System.UInt64 )    0; break;
                        case TypeRepresentation.BuiltInTypes.R4     : val = (System.Single )    0; break;
                        case TypeRepresentation.BuiltInTypes.R8     : val = (System.Double )    0; break;
                        case TypeRepresentation.BuiltInTypes.I      : val = (System.IntPtr )    0; break;
                        case TypeRepresentation.BuiltInTypes.U      : val = (System.UIntPtr)    0; break;

                        default:
                            throw IncorrectEncodingException.Create( "Unexpected type {0} for {1}", td.BuiltInType, var );
                    }

                    ex = m_typeSystem.CreateConstant( td, val );
                }
                else
                {
                    //
                    // TODO: expose a hook to allow arbitrary variable initialization policies.
                    //
                    CustomAttributeRepresentation ca;

                    if(m_typeSystem.MemoryMappedBitFieldPeripherals.TryGetValue( td, out ca ))
                    {
                        return GenerateVariableInitialization( debugInfo, var, ca.GetNamedArg< TypeRepresentation >( "PhysicalType" ), fThroughPointer );
                    }

                    ex = m_typeSystem.CreateConstant( td, null );
                }
            }

            if(fThroughPointer)
            {
                return StoreIndirectOperator.New( debugInfo, td, var, ex, null, 0, true );
            }
            else
            {
                return SingleAssignmentOperator.New( debugInfo, (VariableExpression)var, ex );
            }
        }

        public void DropDeadVariables()
        {
            m_variables      = this.DataFlow_SpanningTree_Variables;
            m_variablesCount = m_variables.Length;
        }

        public VariableExpression[] SortVariables()
        {
            VariableExpression[] variables = ArrayUtility.CopyNotNullArray( this.DataFlow_SpanningTree_Variables );

            Array.Sort( variables, VariableExpression.GetSorter() );

            return variables;
        }

        public override void RenumberVariables()
        {
            base.RenumberVariables();

            GrowOnlyHashTable < VariableExpression, int > phiVersions = null;

            int numPhi    = 0;
            int numPseudo = 0;
            int numCC     = 0;

            for(int pos = m_variablesCount; --pos >= 0; )
            {
                VariableExpression var = m_variables[pos] as VariableExpression;

                if(var is PhiVariableExpression)
                {
                    PhiVariableExpression varPhi = (PhiVariableExpression)var;
                    int                   version;

                    if(phiVersions == null)
                    {
                        phiVersions = HashTableFactory.NewWithReferenceEquality< VariableExpression, int >();
                    }

                    int maxVersion;
                    
                    if(phiVersions.TryGetValue( varPhi.Target, out maxVersion ))
                    {
                        version = maxVersion + 1;
                    }
                    else
                    {
                        version = 1;
                    }

                    varPhi.Version = version;

                    phiVersions[varPhi.Target] = version;

                    var.Number = numPhi++;
                }
                else if(var is PseudoRegisterExpression)
                {
                    var.Number = numPseudo++;
                }
                else if(var is StackLocationExpression)
                {
                }
                else if(var is ConditionCodeExpression)
                {
                    var.Number = numCC++;
                }
            }
        }

        //--//

        public override BasicBlock GetInjectionPoint( BasicBlock.Qualifier qualifier )
        {
            foreach(BasicBlock bb in this.DataFlow_SpanningTree_BasicBlocks)
            {
                if(bb.Annotation == qualifier)
                {
                    return bb;
                }
            }

            bool fAddAfter;

            switch(qualifier)
            {
                case BasicBlock.Qualifier.Entry:
                    return this.EntryBasicBlock;

                case BasicBlock.Qualifier.PrologueStart:
                case BasicBlock.Qualifier.PrologueEnd:
                case BasicBlock.Qualifier.EntryInjectionStart:
                case BasicBlock.Qualifier.EntryInjectionEnd:
                    fAddAfter = true;
                    break;

                case BasicBlock.Qualifier.ExitInjectionStart:
                case BasicBlock.Qualifier.ExitInjectionEnd:
                case BasicBlock.Qualifier.EpilogueStart:
                case BasicBlock.Qualifier.EpilogueEnd:
                    fAddAfter = false;
                    break;

                case BasicBlock.Qualifier.Exit:
                    return this.ExitBasicBlock;

                default:
                    return null;
            }

            BasicBlock res;

            if(fAddAfter)
            {
                var targetBB = GetInjectionPoint( qualifier - 1 );

                CHECKS.ASSERT( targetBB.Successors.Length == 1, "Internal failure: entry basic block has more than one successor in '{0}'", this );

                res = targetBB.InsertNewSuccessor( targetBB.FirstSuccessor );
            }
            else
            {
                var targetBB = GetInjectionPoint( qualifier + 1 );

                if(targetBB == null)
                {
                    return null;
                }

                res = targetBB.InsertNewPredecessor();
            }

            res.Annotation = qualifier;

            return res;
        }

        //--//

        public bool HasProperty( string key )
        {
            return m_propertyBag.ContainsKey( key );
        }

        public object GetPropertyValue( string key )
        {
            object res;

            m_propertyBag.TryGetValue( key, out res );

            return res;
        }

        public bool SetPropertyValue( string key   ,
                                      object value )
        {
            return m_propertyBag.Update( key, value );
        }

        public bool SetProperty( string key )
        {
            return SetPropertyValue( key, null );
        }

        //--//

        //
        // Access Methods
        //

        public override TypeSystemForIR TypeSystemForIR
        {
            get
            {
                return m_typeSystem;
            }
        }

        public TypeSystemForCodeTransformation TypeSystem
        {
            get
            {
                return m_typeSystem;
            }
        }
    }
}
