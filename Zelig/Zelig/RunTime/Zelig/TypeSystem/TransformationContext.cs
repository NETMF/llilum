//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TRANSFORMATIONCONTEXT__USE_EMIT
//#define TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER


namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public interface ITransformationContextTarget
    {
        void ApplyTransformation( TransformationContext context );
    }

    public abstract class TransformationContext : IDisposable
    {
        private delegate void DelayedUpdate();

        protected delegate object DynamicTransform( TransformationContext pThis, object obj, Type t );

        //
        // State
        //

        private   GrowOnlyHashTable< Type, System.Reflection.FieldInfo[] > m_fields;
        private   List< DelayedUpdate >                                    m_delayedUpdates;
        protected DynamicTransform                                         m_dynamicTransform;

        //
        // Helper Methods
        //

        public virtual void Dispose()
        {
        }

        //--//

        protected abstract void ClearPending();

        protected abstract bool ShouldTransform( object target );

        protected virtual bool ShouldRefreshHashCodes()
        {
            return false;
        }

        public abstract void   MarkAsVisited( object obj );
        public abstract void   Push         ( object obj );
        public abstract void   Pop          (            );
        public abstract object TopContext   (            );
        public abstract object FindContext  ( Type   ctx );

        public abstract object GetTransformInitiator();

        public abstract TypeSystem GetTypeSystem();

        //--//

        public abstract void Transform( ref ITransformationContextTarget                      itf        );
                                                                                                           
        public abstract void Transform( ref bool                                              val        );
        public abstract void Transform( ref char                                              val        );
        public abstract void Transform( ref sbyte                                             val        );
        public abstract void Transform( ref byte                                              val        );
        public abstract void Transform( ref short                                             val        );
        public abstract void Transform( ref ushort                                            val        );
        public abstract void Transform( ref int                                               val        );
        public abstract void Transform( ref uint                                              val        );
        public abstract void Transform( ref long                                              val        );
        public abstract void Transform( ref ulong                                             val        );
        public abstract void Transform( ref float                                             val        );
        public abstract void Transform( ref double                                            val        );
        public abstract void Transform( ref IntPtr                                            val        );
        public abstract void Transform( ref UIntPtr                                           val        );
        public abstract void Transform( ref string                                            val        );
        public abstract void Transform( ref object                                            val        );
        public abstract void Transform( ref Type                                              val        );

        public abstract void Transform( ref bool[]                                            valArray   );
        public abstract void Transform( ref char[]                                            valArray   );
        public abstract void Transform( ref sbyte[]                                           valArray   );
        public abstract void Transform( ref byte[]                                            valArray   );
        public abstract void Transform( ref short[]                                           valArray   );
        public abstract void Transform( ref ushort[]                                          valArray   );
        public abstract void Transform( ref int[]                                             valArray   );
        public abstract void Transform( ref uint[]                                            valArray   );
        public abstract void Transform( ref long[]                                            valArray   );
        public abstract void Transform( ref ulong[]                                           valArray   );
        public abstract void Transform( ref float[]                                           valArray   );
        public abstract void Transform( ref double[]                                          valArray   );
        public abstract void Transform( ref IntPtr[]                                          valArray   );
        public abstract void Transform( ref UIntPtr[]                                         valArray   );
        public abstract void Transform( ref string[]                                          valArray   );
        public abstract void Transform( ref object[]                                          valArray   );

        public abstract void Transform( ref List< string >                                    strLst     );

        public abstract void Transform( ref Debugging.DebugInfo                               debugInfo  );

        public abstract void Transform( ref WellKnownTypes                                    wkt        );
        public abstract void Transform( ref WellKnownMethods                                  wkm        );
        public abstract void Transform( ref WellKnownFields                                   wkf        );

        public abstract void Transform( ref AssemblyRepresentation                            asml       );
        public abstract void Transform( ref List< AssemblyRepresentation >                    asmlLst    );
        public abstract void Transform( ref AssemblyRepresentation.VersionData                ver        );
        public abstract void Transform( ref AssemblyRepresentation.VersionData.AssemblyFlags  val        );

        public abstract void Transform( ref BaseRepresentation                                bd         );

        public abstract void Transform( ref TypeRepresentation                                td         );
        public abstract void Transform( ref ValueTypeRepresentation                           td         );
        public abstract void Transform( ref ArrayReferenceTypeRepresentation                  td         );
        public abstract void Transform( ref InterfaceTypeRepresentation                       itf        );
        public abstract void Transform( ref TypeRepresentation[]                              tdArray    );
        public abstract void Transform( ref InterfaceTypeRepresentation[]                     itfArray   );
        public abstract void Transform( ref List< TypeRepresentation >                        tdLst      );

        public abstract void Transform( ref FieldRepresentation                               fd         );
        public abstract void Transform( ref InstanceFieldRepresentation                       fd         );
        public abstract void Transform( ref StaticFieldRepresentation                         fd         );
        public abstract void Transform( ref FieldRepresentation[]                             fdArray    );
        public abstract void Transform( ref InstanceFieldRepresentation[]                     fdArray    );

        public abstract void Transform( ref MethodRepresentation                              md         );
        public abstract void Transform( ref MethodRepresentation[]                            mdArray    );
        public abstract void Transform( ref List<MethodRepresentation>                        mdList     );
        public abstract void Transform( ref MethodImplRepresentation                          mi         );
        public abstract void Transform( ref MethodImplRepresentation[]                        miArray    );

        public abstract void Transform( ref GenericParameterDefinition                        param      );
        public abstract void Transform( ref GenericParameterDefinition[]                      paramArray );

        public abstract void Transform( ref CustomAttributeRepresentation                     ca         );
        public abstract void Transform( ref CustomAttributeRepresentation[]                   caArray    );

        public abstract void Transform( ref CustomAttributeAssociationRepresentation          caa        );
        public abstract void Transform( ref CustomAttributeAssociationRepresentation[]        caaArray   );

        public abstract void Transform( ref ResourceRepresentation                            res        );
        public abstract void Transform( ref List< ResourceRepresentation >                    resLst     );
        public abstract void Transform( ref ResourceRepresentation.Attributes                 val        );
        public abstract void Transform( ref ResourceRepresentation.Pair[]                     pairArray  );

        public abstract void Transform( ref VTable                                            vTable     );
        public abstract void Transform( ref VTable.InterfaceMap                               iMap       );
        public abstract void Transform( ref GCInfo                                            gi         );
        public abstract void Transform( ref GCInfo.Kind                                       giKind     );
        public abstract void Transform( ref GCInfo.Pointer                                    giPtr      );
        public abstract void Transform( ref GCInfo.Pointer[]                                  giPtrArray );
        public abstract void Transform( ref CodePointer                                       cp         );
        public abstract void Transform( ref CodePointer[]                                     cpArray    );

        public abstract void Transform( ref TypeRepresentation.BuiltInTypes                   val        );
        public abstract void Transform( ref TypeRepresentation.Attributes                     val        );
        public abstract void Transform( ref TypeRepresentation.BuildTimeAttributes            val        );
        public abstract void Transform( ref TypeRepresentation.GenericContext                 gc         );
        public abstract void Transform( ref TypeRepresentation.InterfaceMap                   map        );

        public abstract void Transform( ref FieldRepresentation.Attributes                    val        );

        public abstract void Transform( ref GenericParameterDefinition.Attributes             val        );
                                                                                           
        public abstract void Transform( ref MethodRepresentation.Attributes                   val        );
        public abstract void Transform( ref MethodRepresentation.BuildTimeAttributes          val        );
        public abstract void Transform( ref MethodRepresentation.GenericContext               gc         );
                                                                                           
        public abstract void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension   dim        );
        public abstract void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension[] dimArray   );

        public abstract void Transform( ref ActivationRecordEvents                            val        );

        //--//

        public void Transform( ref BitVector vec )
        {
            ClearPending();

            if(vec != null && ShouldTransform( vec ))
            {
                this.Push( vec );

                uint[] bitArray = vec.ToDirectArray();

                Transform( ref bitArray );

                this.Pop();
            }
        }

        public void Transform( ref TypeSystem.Reachability reachability )
        {
            ClearPending();

            reachability.ApplyTransformation( this );
        }

        public void Transform( ref GrowOnlySet< object > set )
        {
            TransformContents( set );
        }

        public void Transform( ref GrowOnlySet< FieldRepresentation > set )
        {
            TransformContents( set );
        }

        //--//

        public void Transform( ref GrowOnlyHashTable< FieldRepresentation, object > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< InstanceFieldRepresentation, object > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< string, TypeRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< string, MethodRepresentation > ht )
        {
            TransformContents( ht );
        }

        public void Transform( ref GrowOnlyHashTable< string, FieldRepresentation > ht )
        {
            TransformContents( ht );
        }

        //--//

        public void TransformGeneric<T>( ref T obj )
        {
            object val = obj;

            Transform( ref val );

            obj = (T)val;
        }

        public void TransformFields<T>( T val )
        {
            this.Push( val );

            TransformFields( (object)val, typeof(T) );

            this.Pop();
        }

        //--//

        protected void TransformContents< K, V >( GrowOnlyHashTable< K, V > val ) where K : class where V : class
        {
            ClearPending();

            if(val != null && ShouldTransform( val ))
            {
                this.Push( val );

                K[] keys   = val.KeysToArray();
                V[] values = val.ValuesToArray();

                K[] keysWork   = val.KeysToArray();
                V[] valuesWork = val.ValuesToArray();

                object keysObj   = keysWork  ; Transform( ref keysObj   );
                object valuesObj = valuesWork; Transform( ref valuesObj );
                
                keysWork   = (K[])keysObj;
                valuesWork = (V[])valuesObj;

                if(ArrayUtility.ArrayReferenceEqualsNotNull( keys  , keysWork  , 0 ) == false ||
                   ArrayUtility.ArrayReferenceEqualsNotNull( values, valuesWork, 0 ) == false  )
                {
                    AddDelayedUpdate( delegate() { val.Load( keysWork, valuesWork ); } );
                }
                else if(ShouldRefreshHashCodes())
                {
                    val.RefreshHashCodes();
                }

                this.Pop();
            }
        }

        protected void TransformValueTypeContents< K, V >( GrowOnlyHashTable< K, V > val ) where K : class where V : struct
        {
            ClearPending();

            if(val != null && ShouldTransform( val ))
            {
                this.Push( val );

                K[] keys   = val.KeysToArray();
                V[] values = val.ValuesToArray();

                K[] keysWork   = val.KeysToArray();
                V[] valuesWork = val.ValuesToArray();

                object keysObj   = keysWork  ; Transform( ref keysObj   );
                object valuesObj = valuesWork; Transform( ref valuesObj );
                
                keysWork   = (K[])keysObj;
                valuesWork = (V[])valuesObj;

                if(ArrayUtility.ArrayReferenceEqualsNotNull( keys  , keysWork  , 0 ) == false ||
                   ArrayUtility.ArrayEqualsNotNull         ( values, valuesWork, 0 ) == false  )
                {
                    AddDelayedUpdate( delegate() { val.Load( keysWork, valuesWork ); } );
                }
                else if(ShouldRefreshHashCodes())
                {
                    val.RefreshHashCodes();
                }

                this.Pop();
            }
        }

        protected void TransformContents< K >( GrowOnlySet< K > val ) where K : class
        {
            ClearPending();

            if(val != null && ShouldTransform( val ))
            {
                this.Push( val );

                K[] keys     = val.ToArray();
                K[] keysWork = val.ToArray();

                object keysObj = keysWork; Transform( ref keysObj );
                
                keysWork = (K[])keysObj;

                if(ArrayUtility.ArrayReferenceEqualsNotNull( keys, keysWork, 0 ) == false)
                {
                    AddDelayedUpdate( delegate() { val.Load( keysWork ); } );
                }
                else if(ShouldRefreshHashCodes())
                {
                    val.RefreshHashCodes();
                }

                this.Pop();
            }
        }

        //--//

        protected abstract void TransformArray( ref Array array );

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        protected virtual object TransformGenericReference( object obj )
        {
            if(obj == null)
            {
                return obj;
            }

            if(obj is ITransformationContextTarget)
            {
                ITransformationContextTarget target = (ITransformationContextTarget)obj;
                
                Transform( ref target );

                return target;
            }
                     
            DynamicTransform dlg = GetDynamicTransform();
            Type             t   = obj.GetType();


#if TRANSFORMATIONCONTEXT__USE_EMIT
            if(this.IsReader && obj is MethodRepresentation)
            {
                if(ReflectionHelper.GetAttribute<AllowCompileTimeIntrospectionAttribute>( t, false ) != null)
                {
                    return TransformThroughReflection( obj );
                }
            }
#endif

            object obj2 = dlg( this, obj, t );
            if(Object.ReferenceEquals( obj2, this ) == false)
            {
                return obj2;
            }

            if(obj is Array)
            {
                Array array = (Array)obj;
                
                TransformArray( ref array );
    
                return array;
            }
    
            if(ReflectionHelper.GetAttribute< AllowCompileTimeIntrospectionAttribute >( t, false ) != null)
            {
                return TransformThroughReflection( obj );
            }

            throw TypeConsistencyErrorException.Create( "Unexpected type {0}", t );
        }

        protected abstract object TransformThroughReflection( object obj );

        //--//

        protected void TransformFields( object obj    ,
                                        Type   target )
        {
            while(target != null)
            {
                if(m_fields == null)
                {
                    m_fields = HashTableFactory.New< Type, System.Reflection.FieldInfo[] >();
                }

                System.Reflection.FieldInfo[] fiArray;

                if(m_fields.TryGetValue( target, out fiArray ) == false)
                {
                    fiArray = ReflectionHelper.GetAllInstanceFields( target );

                    m_fields[target] = fiArray;
                }

                foreach(System.Reflection.FieldInfo fi in fiArray)
                {
                    object val     = fi.GetValue( obj );
                    object valPost = val; Transform( ref valPost );

                    if(Object.ReferenceEquals( val, valPost ) == false)
                    {
                        fi.SetValue( obj, valPost );
                    }
                }

                target = target.BaseType;
            }
        }

        //--//

#if !TRANSFORMATIONCONTEXT_SHOWALLMETHODSTODEBUGGER
        [System.Diagnostics.DebuggerHidden]
#endif
        private void AddDelayedUpdate( DelayedUpdate dlg )
        {
            if(m_delayedUpdates == null)
            {
                m_delayedUpdates = new List< DelayedUpdate >();
            }

            m_delayedUpdates.Add( dlg );
        }

        protected void RunDelayedUpdates()
        {
            while(true)
            {
                List< DelayedUpdate > lst = m_delayedUpdates;
                if(lst == null)
                {
                    break;
                }

                m_delayedUpdates = null;

                foreach(DelayedUpdate dlg in lst)
                {
                    dlg();
                }
            }
        }

        //--//

        protected GrowOnlyHashTable< Type, System.Reflection.MethodInfo > BuildMethodInfoTable()
        {
            GrowOnlyHashTable< Type, System.Reflection.MethodInfo > handlers = HashTableFactory.NewWithReferenceEquality< Type, System.Reflection.MethodInfo >();

            Type t = this.GetType();

            while(t != null)
            {
                foreach(System.Reflection.MethodInfo mi in t.GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ))
                {
                    if(mi.Name == "Transform" && mi.IsAbstract == false)
                    {
                        System.Reflection.ParameterInfo[] parameters = mi.GetParameters();
                        if(parameters != null && parameters.Length == 1)
                        {
                            System.Reflection.ParameterInfo paramInfo = parameters[0];
                            Type                            paramType = paramInfo.ParameterType;

                            if(paramType.IsByRef)
                            {
                                Type elementType = paramType.GetElementType();

                                if(elementType != typeof(object))
                                {
                                    handlers[elementType] = mi;
                                }
                            }
                        }
                    }
                }

                t = t.BaseType;
            }

            return handlers;
        }

        protected abstract GrowOnlyHashTable< Type, System.Reflection.MethodInfo > GetMethodInfoTable();

        protected abstract DynamicTransform GetDynamicTransform();

        protected abstract DynamicTransform BuildDynamicTransform();

#if TRANSFORMATIONCONTEXT__USE_EMIT
        public abstract bool IsReader { get; }
#endif
    }
}
