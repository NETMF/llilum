//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//TODO: Figure out why USE_EMIT is not working 
//#define TRANSFORMATIONCONTEXT__USE_EMIT
//#define TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

#if TRANSFORMATIONCONTEXT__USE_EMIT
    using System.Reflection;
    using System.Reflection.Emit;
#endif

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class TransformationContextForCodeTransformation : TransformationContextForIR
    {
        //
        // State
        //

#if !TRANSFORMATIONCONTEXT__USE_EMIT
        private GrowOnlyHashTable< Type, System.Reflection.MethodInfo > m_handlers;
#endif

        //
        // Helper Methods
        //

        public abstract void Transform( ref TypeSystemForCodeTransformation                   typeSystem   );
                                                                                                           
        public abstract void Transform( ref StackLocationExpression.Placement                 val          );
        public abstract void Transform( ref ConditionCodeExpression.Comparison                val          );
        public abstract void Transform( ref PiOperator.Relation                               val          );
                                                                                                           
        public abstract void Transform( ref DataManager                                       dataManager  );
        public abstract void Transform( ref DataManager.Attributes                            val          );
        public abstract void Transform( ref DataManager.ObjectDescriptor                      od           );
        public abstract void Transform( ref DataManager.ArrayDescriptor                       ad           );
                                                                                                
        public abstract void Transform( ref ImageBuilders.Core                                imageBuilder );
        public abstract void Transform( ref ImageBuilders.CompilationState                    cs           );
        public abstract void Transform( ref ImageBuilders.SequentialRegion                    reg          );
        public abstract void Transform( ref ImageBuilders.ImageAnnotation                     an           );
        public abstract void Transform( ref ImageBuilders.CodeConstant                        cc           );
        public abstract void Transform( ref ImageBuilders.SequentialRegion[]                  regArray     );
        public abstract void Transform( ref List< ImageBuilders.SequentialRegion >            regLst       );
        public abstract void Transform( ref List< ImageBuilders.ImageAnnotation >             anLst        );
        public abstract void Transform( ref List< ImageBuilders.CodeConstant >                ccLst        );

        public abstract void Transform( ref List< Runtime.Memory.Range >                      mrLst        );
        public abstract void Transform( ref       Runtime.MemoryAttributes                    val          );
        public abstract void Transform( ref       Runtime.MemoryAttributes[]                  maArray      );
        public abstract void Transform( ref       Runtime.MemoryUsage                         val          );
        public abstract void Transform( ref       Runtime.MemoryUsage[]                       muArray      );

        public abstract void Transform( ref Abstractions.PlacementRequirements                pr           );
        public abstract void Transform( ref Abstractions.RegisterDescriptor                   regDesc      );
        public abstract void Transform( ref Abstractions.RegisterDescriptor[]                 regDescArray );
        public abstract void Transform( ref Abstractions.RegisterClass                        val          );
        public abstract void Transform( ref Abstractions.CallingConvention.Direction          val          );

        //--//

        public void Transform( ref GrowOnlyHashTable< string, object > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< Expression, Expression[] > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< object, ConstantExpression > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< TypeRepresentation, GrowOnlyHashTable< object, ConstantExpression > > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< TypeRepresentation, List< ConstantExpression > > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< BaseRepresentation, InstanceFieldRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< TypeRepresentation, CustomAttributeRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< FieldRepresentation, CustomAttributeRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< MethodRepresentation, CustomAttributeRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< TypeRepresentation, TypeRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< object, DataManager.DataDescriptor > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< ControlFlowGraphStateForCodeTransformation, ImageBuilders.CompilationState > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< object, List< ImageBuilders.CodeConstant > > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable<ExternalDataDescriptor.IExternalDataContext, ImageBuilders.SequentialRegion> ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable<DataManager.DataDescriptor, ImageBuilders.SequentialRegion> ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< BaseRepresentation, Abstractions.PlacementRequirements > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< object, int > ht )
        {
            TransformValueTypeContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< MethodRepresentation, List<MethodRepresentation> > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< string, SourceCodeTracker.SourceCode > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< FieldRepresentation, BitFieldDefinition > ht )
        {
            TransformContents( ht );
        }

        //--//

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        protected override object TransformGenericReference( object obj )
        {
            if(obj is TypeSystemForCodeTransformation)
            {
                TypeSystemForCodeTransformation target = (TypeSystemForCodeTransformation)obj;
                
                Transform( ref target );
                
                return target;
            }

            return base.TransformGenericReference( obj );
        }

        //--//

        protected override DynamicTransform BuildDynamicTransform()
        {
            if(m_dynamicTransform == null)
            {
#if TRANSFORMATIONCONTEXT__USE_EMIT
                GrowOnlyHashTable< Type, System.Reflection.MethodInfo > ht = GetMethodInfoTable();

                List< Type > lst = new List< Type >();

                TreeNode tnClassType = new TreeNode( typeof(object   ) );
                TreeNode tnValueType = new TreeNode( typeof(ValueType) );

                foreach(var t in ht.Keys)
                {
                    TreeNode node = new TreeNode( t );

                    node.m_method = ht[t];

                    if(t.IsValueType)
                    {
                        tnValueType.Insert( node );
                    }
                    else
                    {
                        tnClassType.Insert( node );
                    }
                }

                tnClassType.Normalize();
                tnValueType.Normalize();

                //--//

                DynamicMethod methodBuilder = new DynamicMethod( "TransformDecoder", typeof(object), new Type[] { typeof(TransformationContext), typeof(object), typeof(Type) }, true );

                ILGenerator il = methodBuilder.GetILGenerator( 256 );

////            il.EmitWriteLine( "Starting check:" );
                if(tnValueType.GetNumberOfTerminalNodes() > 0)
                {
                    tnValueType.Emit( il, true );
                }

                if(tnClassType.GetNumberOfTerminalNodes() > 0)
                {
                    tnClassType.Emit( il, false );
                }

                m_dynamicTransform = (DynamicTransform)methodBuilder.CreateDelegate( typeof(DynamicTransform) );
#else
                m_dynamicTransform = TransformThroughInvoke;
#endif
            }

            return m_dynamicTransform;
        }

        //--//

#if TRANSFORMATIONCONTEXT__USE_EMIT

        class TreeNode
        {
            //
            // State
            //

            internal Type                         m_type;
            internal System.Reflection.MethodInfo m_method;
            internal List< TreeNode >             m_children = new List< TreeNode >();

            //
            // Constructor Methods
            //

            internal TreeNode( Type t )
            {
                m_type = t;
            }

            //
            // Helper Methods
            //

            internal bool IsSubclassOf( TreeNode node )
            {
                return m_type.IsSubclassOf( node.m_type );
            }

            internal void Insert( TreeNode newNode )
            {
                //
                // First, relocate all the node that are subclasses of the new node under it.
                //

                for(int i = 0; i < m_children.Count; i++)
                {
                    var child = m_children[i];

                    if(child.IsSubclassOf( newNode ))
                    {
                        newNode.m_children.Add( child );
                        m_children.RemoveAt( i-- );
                        return;
                    }
                }

                //
                // Then, add the node here.
                //

                m_children.Add( newNode );
            }

            internal void Normalize()
            {
                for(int i = 0; i < m_children.Count; i++)
                {
                    var child  = m_children[i];
                    var parent = child.m_type.BaseType;

                    if(parent != null && parent != m_type)
                    {
                        foreach(var child2 in m_children)
                        {
                            if(child2.m_type == parent)
                            {
                                child2.m_children.Add( child );
                                m_children.RemoveAt( i-- );
                                child = null;
                                break;
                            }
                        }

                        if(child != null)
                        {
                            TreeNode sub = new TreeNode( parent );

                            m_children[i] = sub;

                            sub.m_children.Add( child );
                        }
                    }
                }
            }

            internal int GetNumberOfTerminalNodes()
            {
                int num = m_method != null ? 1 : 0;

                foreach(var child in m_children)
                {
                    num += child.GetNumberOfTerminalNodes();
                }

                return num;
            }

            internal delegate Type GetTypeFromHandleDlg( RuntimeTypeHandle handle );

            internal void Emit( ILGenerator il            ,
                                bool        fPerformCheck )
            {
                if(fPerformCheck)
                {
                    if(m_children.Count == 1)
                    {
                        m_children[0].Emit( il, true );
                        return;
                    }
                }

                Label label = il.DefineLabel();

                if(fPerformCheck)
                {
////                il.EmitWriteLine( "Checking for: " + m_type.FullName );

                    if(m_type.IsArray || m_type == typeof(Array))
                    {
                        GetTypeFromHandleDlg dlg = Type.GetTypeFromHandle;

                        il.Emit( OpCodes.Ldtoken, m_type  );
                        il.Emit( OpCodes.Call, dlg.Method );
                        il.Emit( OpCodes.Ldarg_2          );
                        il.Emit( OpCodes.Bne_Un, label    );
                    }
                    else
                    {
                        il.Emit( OpCodes.Ldarg_1         );
                        il.Emit( OpCodes.Isinst , m_type );
                        il.Emit( OpCodes.Brfalse, label  );
                    }
                }

                foreach(var child in m_children)
                {
                    child.Emit( il, true );
                }

                if(m_method != null)
                {
                    System.Reflection.ParameterInfo[] parameters  = m_method.GetParameters();
                    System.Reflection.ParameterInfo   paramInfo   = parameters[0];
                    Type                              paramType   = paramInfo.ParameterType;
                    Type                              elementType = paramType.GetElementType();

                    LocalBuilder objectRef = il.DeclareLocal( elementType );

                    il.Emit( OpCodes.Ldarg_1 );
                    il.Emit( OpCodes.Unbox_Any, elementType );
                    il.Emit( OpCodes.Stloc, objectRef );

                    if(m_method.IsStatic == false)
                    {
                        il.Emit( OpCodes.Ldarg_0 );
                        il.Emit( OpCodes.Castclass, m_method.DeclaringType );
                    }
    
                    il.Emit( OpCodes.Ldloca, objectRef );
    
                    il.EmitCall( OpCodes.Callvirt, m_method, null );

                    il.Emit( OpCodes.Ldloc, objectRef );
                    if(elementType.IsValueType)
                    {
                        il.Emit( OpCodes.Box, elementType );
                    }
                }
                else
                {
                    //il.EmitWriteLine( "No method for: " + m_type.FullName );

                    //
                    // In case of failure to find a target transform, return the transform context, so the caller will know nothing got selected.
                    //
                    il.Emit( OpCodes.Ldarg_0 );
                }

                il.Emit( OpCodes.Ret );

                if(fPerformCheck)
                {
                    il.MarkLabel( label );
////                il.EmitWriteLine( "Check failed" );
                }
            }
        }

#else

        private static object TransformThroughInvoke( TransformationContext context ,
                                                      object                obj     ,
                                                      Type                  t       )
        {
            TransformationContextForCodeTransformation pThis = (TransformationContextForCodeTransformation)context;

            System.Reflection.MethodInfo mi;
            
            mi = pThis.GetAssociatedTransform( obj, true );
            if(mi != null)
            {
                return pThis.CallTransformMethod( mi, obj );
            }
    
            if(obj is Array)
            {
                Array array = (Array)obj;
                
                pThis.TransformArray( ref array );
    
                return array;
            }
    
            mi = pThis.GetAssociatedTransform( obj, false );
            if(mi != null)
            {
                return pThis.CallTransformMethod( mi, obj );
            }

            return context;
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        protected System.Reflection.MethodInfo GetAssociatedTransform( object obj    ,
                                                                       bool   fExact )
        {
            if(m_handlers == null)
            {
                m_handlers = GetMethodInfoTable();
            }

            System.Reflection.MethodInfo res;
            Type                         src = obj != null ? obj.GetType() : typeof(object);

            while(src != null)
            {
                if(m_handlers.TryGetValue( src, out res ))
                {
                    return res;
                }

                if(fExact) break;

                src = src.BaseType;
            }

            return null;
        }

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        protected object CallTransformMethod( System.Reflection.MethodInfo mi  ,
                                              object                       obj )
        {
            object[] parameters = new object[] { obj };

            mi.Invoke( this, parameters );

            return parameters[0];
        }

#endif
    }
}
