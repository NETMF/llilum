//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal abstract class ScanTypeSystem : TransformationContextForCodeTransformation
    {
        protected enum SubstitutionAction
        {
            Unknown   ,
            Substitute,
            Keep      ,
        }

        //
        // State
        //

        private   static GrowOnlyHashTable< Type, System.Reflection.MethodInfo > s_handlers;
        private   static DynamicTransform                                        s_dynamicTransform;

        protected TypeSystemForCodeTransformation                                m_typeSystem;
        protected object                                                         m_scanOriginator;
        private   GrowOnlySet< object         >                                  m_visited;
        private   object                                                         m_pending; // Transform( ref object ) has to go through Visit twice. Keep track of it.
        private   object[]                                                       m_contextStack;
        private   int                                                            m_contextStackSize;

        //
        // Constructor Methods
        //

        protected ScanTypeSystem( TypeSystemForCodeTransformation typeSystem     ,
                                  object                          scanOriginator )
        {
            m_typeSystem       = typeSystem;
            m_scanOriginator   = scanOriginator;
            m_visited          = SetFactory.NewWithReferenceEquality< object >();
            m_contextStack     = new object[16];
            m_contextStackSize = 0;
        }

#if TRANSFORMATIONCONTEXT__USE_EMIT
        public override bool IsReader
        {
            get { return false; }
        }
#endif

        //
        // Helper Methods
        //

        internal virtual void RefreshHashCodes()
        {
            m_visited.RefreshHashCodes();
        }

        internal virtual void ProcessTypeSystem()
        {
            this.Transform( ref m_typeSystem );
        }

        internal void Reset()
        {
            m_visited.Clear();
            m_pending = null;
        }

        //--//

        protected override void ClearPending()
        {
            m_pending = null;
        }

        protected virtual object ShouldSubstitute(     object             target ,
                                                   out SubstitutionAction result )
        {
            result = SubstitutionAction.Unknown;

            return null;
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        private SubstitutionAction Visit<T>( ref T target ) where T : class
        {
            return Visit( ref target, false );
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        private SubstitutionAction Visit<T>( ref T    target              ,
                                                 bool fSkipRecursiveCheck ) where T : class
        {
            if(m_pending != null)
            {
                CHECKS.ASSERT( m_pending == (object)target, "Internal error, ScanTypeSystem was expecting {0}, got {1}", m_pending, target );

                m_pending = null;

                return SubstitutionAction.Unknown;
            }

            if(target == null)
            {
                return SubstitutionAction.Keep;
            }

            SubstitutionAction res;
            object             substitute = ShouldSubstitute( target, out res );

            if(res == SubstitutionAction.Substitute)
            {
                target = (T)substitute;
            }
            else if(res == SubstitutionAction.Unknown)
            {
                //
                // 'target' is passed by reference, so ShouldSubstitute could change it.
                // So, even if we checked for null in the step above, we have to check for null again.
                //
                if(target == null)
                {
                    res = SubstitutionAction.Keep;
                }

                if(!fSkipRecursiveCheck && m_visited.Insert( target ))
                {
                    res = SubstitutionAction.Keep;
                }
            }

            return res;
        }

        //--//

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        private void ReplaceUnique<T>( ref T target ) where T : BaseRepresentation 
        {
            if(Visit( ref target ) == SubstitutionAction.Unknown)
            {
                target.ApplyTransformation( this );
            }
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        private void ReplaceUnique<T>( ref T[] target ) where T : BaseRepresentation 
        {
            if(Visit( ref target ) == SubstitutionAction.Unknown)
            {
                for(int i = 0; i < target.Length; i++)
                {
                    ReplaceUnique( ref target[i] );
                }
            }
        }

        private void ReplaceUnique<T>( ref List<T> target ) where T : BaseRepresentation
        {
            if(Visit( ref target ) == SubstitutionAction.Unknown)
            {
                for(int i = 0; i < target.Count; i++)
                {
                    T tmp = target[i];
                    ReplaceUnique( ref tmp );
                    target[i] = tmp;
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//

        //
        // TransformationContext
        //

        protected override bool ShouldTransform( object target )
        {
            SubstitutionAction res;

            ShouldSubstitute( target, out res );

            return res != SubstitutionAction.Keep;
        }

        public override void MarkAsVisited( object obj )
        {
            m_visited.Insert( obj );
        }

        public override void Push( object ctx )
        {
            if(m_contextStackSize >= m_contextStack.Length)
            {
                m_contextStack = ArrayUtility.IncreaseSizeOfNotNullArray( m_contextStack, 16 );
            }

            m_contextStack[m_contextStackSize++] = ctx;
        }

        public override void Pop()
        {
            m_contextStack[--m_contextStackSize] = null;

            if(m_contextStackSize == 0)
            {
                RunDelayedUpdates();
            }
        }

        public override object TopContext()
        {
            if(m_contextStackSize > 0)
            {
                return m_contextStack[m_contextStackSize-1];
            }

            return null;
        }

        public override object FindContext( Type ctx )
        {
            for(int i = m_contextStackSize; --i >= 0; )
            {
                object o = m_contextStack[i];

                if(ctx.IsInstanceOfType( o ))
                {
                    return o;
                }
            }

            return null;
        }

        public override object GetTransformInitiator()
        {
            return m_scanOriginator;
        }

        public override TypeSystem GetTypeSystem()
        {
            return m_typeSystem;
        }

        //--//

        public override void Transform( ref ITransformationContextTarget itf )
        {
            if(Visit( ref itf ) == SubstitutionAction.Unknown)
            {
                itf.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref bool val )
        {
            ClearPending();
        }

        public override void Transform( ref char val )
        {
            ClearPending();
        }

        public override void Transform( ref sbyte val )
        {
            ClearPending();
        }

        public override void Transform( ref byte val )
        {
            ClearPending();
        }

        public override void Transform( ref short val )
        {
            ClearPending();
        }

        public override void Transform( ref ushort val )
        {
            ClearPending();
        }

        public override void Transform( ref int val )
        {
            ClearPending();
        }

        public override void Transform( ref uint val )
        {
            ClearPending();
        }

        public override void Transform( ref long val )
        {
            ClearPending();
        }

        public override void Transform( ref ulong val )
        {
            ClearPending();
        }

        public override void Transform( ref float val )
        {
            ClearPending();
        }

        public override void Transform( ref double val )
        {
            ClearPending();
        }

        public override void Transform( ref IntPtr val )
        {
            ClearPending();
        }

        public override void Transform( ref UIntPtr val )
        {
            ClearPending();
        }

        public override void Transform( ref string val )
        {
            ClearPending();
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        public override void Transform( ref object val )
        {
            if(Visit( ref val ) == SubstitutionAction.Unknown)
            {
                m_pending = val;

                val = TransformGenericReference( val );
            }
        }

        public override void Transform( ref Type val )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref bool[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref char[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref sbyte[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref byte[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref short[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref ushort[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref int[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref uint[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref long[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref ulong[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref float[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref double[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref IntPtr[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref UIntPtr[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref string[] valArray )
        {
            ClearPending();
        }

        public override void Transform( ref object[] objArray )
        {
            if(Visit( ref objArray ) == SubstitutionAction.Unknown)
            {
                var array = objArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref List< string > strLst )
        {
            if(Visit( ref strLst ) == SubstitutionAction.Unknown)
            {
                List< string > lst = strLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    string str = lst[i];

                    Transform( ref str );

                    lst[i] = str;
                }
            }
        }

        //--//

        public override void Transform( ref Debugging.DebugInfo debugInfo )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref WellKnownTypes wkt )
        {
            if(Visit( ref wkt ) == SubstitutionAction.Unknown)
            {
                wkt.ApplyTransformation( this );
            }
        }

        public override void Transform( ref WellKnownMethods wkm )
        {
            if(Visit( ref wkm ) == SubstitutionAction.Unknown)
            {
                wkm.ApplyTransformation( this );
            }
        }

        public override void Transform( ref WellKnownFields wkf )
        {
            if(Visit( ref wkf ) == SubstitutionAction.Unknown)
            {
                wkf.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref AssemblyRepresentation asml )
        {
            ClearPending();
        }

        public override void Transform( ref List< AssemblyRepresentation > asmlLst )
        {
            ClearPending();
        }

        public override void Transform( ref AssemblyRepresentation.VersionData ver )
        {
            ClearPending();
        }

        public override void Transform( ref AssemblyRepresentation.VersionData.AssemblyFlags val )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref BaseRepresentation bd )
        {
            ReplaceUnique( ref bd );
        }

        //--//

        public override void Transform( ref TypeRepresentation td )
        {
            ReplaceUnique( ref td );
        }

        public override void Transform( ref ValueTypeRepresentation td )
        {
            ReplaceUnique( ref td );
        }

        public override void Transform( ref ArrayReferenceTypeRepresentation td )
        {
            ReplaceUnique( ref td );
        }

        public override void Transform( ref InterfaceTypeRepresentation itf )
        {
            ReplaceUnique( ref itf );
        }

        public override void Transform( ref TypeRepresentation[] tdArray )
        {
            ReplaceUnique( ref tdArray );
        }

        public override void Transform( ref InterfaceTypeRepresentation[] itfArray )
        {
            ReplaceUnique( ref itfArray );
        }

        public override void Transform( ref List< TypeRepresentation > tdLst )
        {
            if(Visit( ref tdLst ) == SubstitutionAction.Unknown)
            {
                List< TypeRepresentation > lst = tdLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    TypeRepresentation td = lst[i];

                    Transform( ref td );

                    lst[i] = td;
                }
            }
        }

        //--//

        public override void Transform( ref FieldRepresentation fd )
        {
            ReplaceUnique( ref fd );
        }

        public override void Transform( ref InstanceFieldRepresentation fd )
        {
            ReplaceUnique( ref fd );
        }

        public override void Transform( ref StaticFieldRepresentation fd )
        {
            ReplaceUnique( ref fd );
        }

        public override void Transform( ref FieldRepresentation[] fdArray )
        {
            ReplaceUnique( ref fdArray );
        }

        public override void Transform( ref InstanceFieldRepresentation[] fdArray )
        {
            ReplaceUnique( ref fdArray );
        }

        //--//

        public override void Transform( ref MethodRepresentation md )
        {
            ReplaceUnique( ref md );
        }

        public override void Transform( ref MethodRepresentation[] mdArray )
        {
            ReplaceUnique( ref mdArray );
        }

        public override void Transform( ref List<MethodRepresentation> resLst )
        {
            if(Visit( ref resLst ) == SubstitutionAction.Unknown)
            {
                List<MethodRepresentation> lst = resLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    MethodRepresentation res = lst[i];

                    Transform( ref res );

                    lst[i] = res;
                }
            }
        }

        public override void Transform( ref MethodImplRepresentation mi )
        {
            if(Visit( ref mi ) == SubstitutionAction.Unknown)
            {
                mi.ApplyTransformation( this );
            }
        }

        public override void Transform( ref MethodImplRepresentation[] miArray )
        {
            if(Visit( ref miArray ) == SubstitutionAction.Unknown)
            {
                var array = miArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref GenericParameterDefinition param )
        {
            ClearPending();

            Transform( ref param.Constraints );
        }

        public override void Transform( ref GenericParameterDefinition[] paramArray )
        {
            if(Visit( ref paramArray ) == SubstitutionAction.Unknown)
            {
                var array = paramArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref CustomAttributeRepresentation ca )
        {
            if(Visit( ref ca ) == SubstitutionAction.Unknown)
            {
                ca.ApplyTransformation( this );
            }
        }

        public override void Transform( ref CustomAttributeRepresentation[] caArray )
        {
            if(Visit( ref caArray ) == SubstitutionAction.Unknown)
            {
                var array = caArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref CustomAttributeAssociationRepresentation caa )
        {
            if(Visit( ref caa ) == SubstitutionAction.Unknown)
            {
                caa.ApplyTransformation( this );
            }
        }

        public override void Transform( ref CustomAttributeAssociationRepresentation[] caaArray )
        {
            if(Visit( ref caaArray ) == SubstitutionAction.Unknown)
            {
                var array = caaArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref ResourceRepresentation res )
        {
            if(Visit( ref res ) == SubstitutionAction.Unknown)
            {
                res.ApplyTransformation( this );
            }
        }

        public override void Transform( ref List< ResourceRepresentation > resLst )
        {
            if(Visit( ref resLst ) == SubstitutionAction.Unknown)
            {
                List< ResourceRepresentation > lst = resLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    ResourceRepresentation res = lst[i];

                    Transform( ref res );

                    lst[i] = res;
                }
            }
        }

        public override void Transform( ref ResourceRepresentation.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref ResourceRepresentation.Pair[] pairArray )
        {
            if(Visit( ref pairArray ) == SubstitutionAction.Unknown)
            {
                var array = pairArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    TransformGeneric( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref VTable vTable )
        {
            if(Visit( ref vTable ) == SubstitutionAction.Unknown)
            {
                vTable.ApplyTransformation( this );
            }
        }

        public override void Transform( ref VTable.InterfaceMap iMap )
        {
            ClearPending();

            Transform( ref iMap.Interface      );
            Transform( ref iMap.MethodPointers );
        }

        public override void Transform( ref GCInfo gi )
        {
            ClearPending();
        }

        public override void Transform( ref GCInfo.Kind giKind )
        {
            ClearPending();
        }

        public override void Transform( ref GCInfo.Pointer giPtr )
        {
            ClearPending();
        }

        public override void Transform( ref GCInfo.Pointer[] giPtrArray )
        {
            ClearPending();
        }

        public override void Transform( ref CodePointer cp )
        {
            ClearPending();
        }

        public override void Transform( ref CodePointer[] cpArray )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref TypeRepresentation.BuiltInTypes val )
        {
            ClearPending();
        }

        public override void Transform( ref TypeRepresentation.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref TypeRepresentation.BuildTimeAttributes val )
        {
            ClearPending();
        }

        public override void Transform( ref TypeRepresentation.GenericContext gc )
        {
            if(Visit( ref gc ) == SubstitutionAction.Unknown)
            {
                gc.ApplyTransformation( this );
            }
        }

        public override void Transform( ref TypeRepresentation.InterfaceMap map )
        {
            ClearPending();

            Transform( ref map.Interface );
            Transform( ref map.Methods   );
        }

        //--//

        public override void Transform( ref FieldRepresentation.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref GenericParameterDefinition.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref MethodRepresentation.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref MethodRepresentation.BuildTimeAttributes val )
        {
            ClearPending();
        }

        public override void Transform( ref MethodRepresentation.GenericContext gc )
        {
            if(Visit( ref gc ) == SubstitutionAction.Unknown)
            {
                gc.ApplyTransformation( this );
            }
        }

        public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension dim )
        {
            ClearPending();
        }

        public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension[] dimArray )
        {
            ClearPending();
        }

        public override void Transform( ref Runtime.ActivationRecordEvents val )
        {
            ClearPending();
        }

        //--//

        //
        // TransformationContextForIR
        //

        public override void Transform( ref ControlFlowGraphState cfg )
        {
            if(Visit( ref cfg ) == SubstitutionAction.Unknown)
            {
                ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

                cfg2.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref Operator op )
        {
            if(Visit( ref op ) == SubstitutionAction.Unknown)
            {
                op.ApplyTransformation( this );
            }
        }

        public override void Transform( ref Operator[] opArray )
        {
            if(Visit( ref opArray, true ) == SubstitutionAction.Unknown)
            {
                var array = opArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref Annotation an )
        {
            if(Visit( ref an ) == SubstitutionAction.Unknown)
            {
                an.ApplyTransformation( this );
            }
        }

        public override void Transform( ref Annotation[] anArray )
        {
            if(Visit( ref anArray, true ) == SubstitutionAction.Unknown)
            {
                var array = anArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        //--//

        public override void Transform( ref Expression ex )
        {
            if(Visit( ref ex ) == SubstitutionAction.Unknown)
            {
                ex.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ConstantExpression ex )
        {
            if(Visit( ref ex ) == SubstitutionAction.Unknown)
            {
                ex.ApplyTransformation( this );
            }
        }

        public override void Transform( ref VariableExpression ex )
        {
            if(Visit( ref ex ) == SubstitutionAction.Unknown)
            {
                ex.ApplyTransformation( this );
            }
        }

        public override void Transform( ref VariableExpression.DebugInfo val )
        {
            if(Visit( ref val ) == SubstitutionAction.Unknown)
            {
                val.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref Expression[] exArray )
        {
            if(Visit( ref exArray, true ) == SubstitutionAction.Unknown)
            {
                var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref VariableExpression[] exArray )
        {
            if(Visit( ref exArray, true ) == SubstitutionAction.Unknown)
            {
                var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref List< ConstantExpression > exLst )
        {
            if(Visit( ref exLst ) == SubstitutionAction.Unknown)
            {
                List< ConstantExpression > lst = exLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    ConstantExpression ex = lst[i];

                    Transform( ref ex );

                    lst[i] = ex;
                }
            }
        }

        //--//

        public override void Transform( ref BasicBlock bb )
        {
            if(Visit( ref bb ) == SubstitutionAction.Unknown)
            {
                bb.ApplyTransformation( this );
            }
        }

        public override void Transform( ref EntryBasicBlock bb )
        {
            if(Visit( ref bb ) == SubstitutionAction.Unknown)
            {
                bb.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ExitBasicBlock bb )
        {
            if(Visit( ref bb ) == SubstitutionAction.Unknown)
            {
                bb.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ExceptionHandlerBasicBlock bb )
        {
            if(Visit( ref bb ) == SubstitutionAction.Unknown)
            {
                bb.ApplyTransformation( this );
            }
        }

        public override void Transform( ref BasicBlock[] bbArray )
        {
            if(Visit( ref bbArray ) == SubstitutionAction.Unknown)
            {
                var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref ExceptionHandlerBasicBlock[] bbArray )
        {
            if(Visit( ref bbArray ) == SubstitutionAction.Unknown)
            {
                var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref BasicBlock.Qualifier val )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref ExceptionClause ec )
        {
            if(Visit( ref ec ) == SubstitutionAction.Unknown)
            {
                ec.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ExceptionClause[] ecArray )
        {
            if(Visit( ref ecArray ) == SubstitutionAction.Unknown)
            {
                var array = ecArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref ExceptionClause.ExceptionFlag val )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref CompilationConstraints val )
        {
            ClearPending();
        }

        public override void Transform( ref CompilationConstraints[] ccArray )
        {
            if(Visit( ref ccArray ) == SubstitutionAction.Unknown)
            {
                var array = ccArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref Operator.OperatorCapabilities val )
        {
            ClearPending();
        }

        public override void Transform( ref Operator.OperatorLevel val )
        {
            ClearPending();
        }

        public override void Transform( ref BinaryOperator.ALU val )
        {
            ClearPending();
        }

        public override void Transform( ref UnaryOperator.ALU val )
        {
            ClearPending();
        }

        public override void Transform( ref CallOperator.CallKind val )
        {
            ClearPending();
        }

        public override void Transform( ref CompareAndSetOperator.ActionCondition val )
        {
            ClearPending();
        }

        //--//

        //
        // TransformationContextForCodeTransformation
        //

        public override void Transform( ref TypeSystemForCodeTransformation typeSystem )
        {
            if(Visit( ref typeSystem ) == SubstitutionAction.Unknown)
            {
                typeSystem.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref StackLocationExpression.Placement val )
        {
            ClearPending();
        }

        public override void Transform( ref ConditionCodeExpression.Comparison val )
        {
            ClearPending();
        }

        public override void Transform( ref PiOperator.Relation val )
        {
            ClearPending();
        }

        //--//

        public override void Transform( ref DataManager dataManager )
        {
            if(Visit( ref dataManager ) == SubstitutionAction.Unknown)
            {
                dataManager.ApplyTransformation( this );
            }
        }

        public override void Transform( ref DataManager.Attributes val )
        {
            ClearPending();
        }

        public override void Transform( ref DataManager.ObjectDescriptor od )
        {
            if(Visit( ref od ) == SubstitutionAction.Unknown)
            {
                od.ApplyTransformation( this );
            }
        }

        public override void Transform( ref DataManager.ArrayDescriptor ad )
        {
            if(Visit( ref ad ) == SubstitutionAction.Unknown)
            {
                ad.ApplyTransformation( this );
            }
        }

        //--//

        public override void Transform( ref ImageBuilders.Core imageBuilder )
        {
            if(Visit( ref imageBuilder ) == SubstitutionAction.Unknown)
            {
                imageBuilder.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ImageBuilders.CompilationState cs )
        {
            if(Visit( ref cs ) == SubstitutionAction.Unknown)
            {
                cs.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ImageBuilders.SequentialRegion reg )
        {
            if(Visit( ref reg ) == SubstitutionAction.Unknown)
            {
                reg.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ImageBuilders.ImageAnnotation an )
        {
            if(Visit( ref an ) == SubstitutionAction.Unknown)
            {
                an.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ImageBuilders.CodeConstant cc )
        {
            if(Visit( ref cc ) == SubstitutionAction.Unknown)
            {
                cc.ApplyTransformation( this );
            }
        }

        public override void Transform( ref ImageBuilders.SequentialRegion[] regArray )
        {
            if(Visit( ref regArray ) == SubstitutionAction.Unknown)
            {
                var array = regArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref List< ImageBuilders.SequentialRegion > regLst )
        {
            if(Visit( ref regLst ) == SubstitutionAction.Unknown)
            {
                List< ImageBuilders.SequentialRegion > lst = regLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    ImageBuilders.SequentialRegion reg = lst[i];

                    Transform( ref reg );

                    lst[i] = reg;
                }
            }
        }

        public override void Transform( ref List< ImageBuilders.ImageAnnotation > anLst )
        {
            if(Visit( ref anLst ) == SubstitutionAction.Unknown)
            {
                List< ImageBuilders.ImageAnnotation > lst = anLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    ImageBuilders.ImageAnnotation an = lst[i];

                    Transform( ref an );

                    lst[i] = an;
                }
            }
        }

        public override void Transform( ref List< ImageBuilders.CodeConstant > ccLst )
        {
            if(Visit( ref ccLst ) == SubstitutionAction.Unknown)
            {
                List< ImageBuilders.CodeConstant > lst = ccLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    ImageBuilders.CodeConstant cc = lst[i];

                    Transform( ref cc );

                    lst[i] = cc;
                }
            }
        }

        public override void Transform( ref List< Runtime.Memory.Range > mrLst )
        {
            if(Visit( ref mrLst ) == SubstitutionAction.Unknown)
            {
                List< Runtime.Memory.Range > lst = mrLst;

                for(int i = 0; i < lst.Count; i++)
                {
                    Runtime.Memory.Range mr = lst[i];

                    TransformGeneric( ref mr );

                    lst[i] = mr;
                }
            }
        }

        public override void Transform( ref Runtime.MemoryAttributes val )
        {
            ClearPending();
        }

        public override void Transform( ref Runtime.MemoryAttributes[] maArray )
        {
            if(Visit( ref maArray ) == SubstitutionAction.Unknown)
            {
                var array = maArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref Runtime.MemoryUsage val )
        {
            ClearPending();
        }

        public override void Transform( ref Runtime.MemoryUsage[] muArray )
        {
            if(Visit( ref muArray ) == SubstitutionAction.Unknown)
            {
                var array = muArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref Abstractions.PlacementRequirements pr )
        {
            if(Visit( ref pr ) == SubstitutionAction.Unknown)
            {
                pr.ApplyTransformation( this );
            }
        }

        public override void Transform( ref Abstractions.RegisterDescriptor regDesc )
        {
            if(Visit( ref regDesc ) == SubstitutionAction.Unknown)
            {
                regDesc.ApplyTransformation( this );
            }
        }

        public override void Transform( ref Abstractions.RegisterDescriptor[] regDescArray )
        {
            if(Visit( ref regDescArray ) == SubstitutionAction.Unknown)
            {
                var array = regDescArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    Transform( ref array[i] );
                }
            }
        }

        public override void Transform( ref Abstractions.RegisterClass val )
        {
            ClearPending();
        }

        public override void Transform( ref Abstractions.CallingConvention.Direction val )
        {
            ClearPending();
        }

        //--//

        protected override void TransformArray( ref Array arrayIn )
        {
            if(Visit( ref arrayIn ) == SubstitutionAction.Unknown)
            {
                var array = arrayIn; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < array.Length; i++)
                {
                    object obj  = array.GetValue( i );
                    object obj2 = obj;

                    if(obj != null)
                    {
                        Type elemType = obj.GetType();

                        if(elemType.IsValueType || elemType == typeof(string)) break;
                    }

                    Transform( ref obj2 );

                    if(Object.ReferenceEquals( obj, obj2 ) == false)
                    {
                        array.SetValue( obj2, i );
                    }
                }
            }
        }

        //--//

        protected override GrowOnlyHashTable< Type, System.Reflection.MethodInfo > GetMethodInfoTable()
        {
            if(s_handlers == null)
            {
                s_handlers = BuildMethodInfoTable();
            }

            return s_handlers;
        }

        protected override DynamicTransform GetDynamicTransform()
        {
            if(s_dynamicTransform == null)
            {
                s_dynamicTransform = BuildDynamicTransform();
            }

            return s_dynamicTransform;
        }

        protected override object TransformThroughReflection( object obj )
        {
            if(Visit( ref obj ) == SubstitutionAction.Unknown)
            {
                TransformFields( obj, obj.GetType() );
            }

            return obj;
        }

        //--//

        //
        // Access Methods
        //

        internal GrowOnlySet< object > Visited
        {
            get
            {
                return m_visited;
            }
        }

        protected object[] FullContext
        {
            get
            {
                object[] res = new object[m_contextStackSize];

                for(int idxFwd = 0, idxBck = m_contextStackSize; --idxBck >= 0; idxFwd++)
                {
                    res[idxFwd] = m_contextStack[idxBck];
                }

                return res;
            }
        }

        //--//

        //
        // Debug Methods
        //

        internal string GetContextDump()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for(int i = 0; i < m_contextStackSize; i++)
            {
                if(i != 0)
                {
                    sb.Append( " => " );
                }

                sb.Append( m_contextStack[i] );
            }

            return sb.ToString();
        }
    }
}
