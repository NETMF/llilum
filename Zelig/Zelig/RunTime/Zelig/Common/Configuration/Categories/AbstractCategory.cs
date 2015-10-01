//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class AbstractCategory
    {
        public class ValueChoices
        {
            //
            // State
            //

            GrowOnlyHashTable< System.Reflection.FieldInfo, List< object > > m_ht;

            //
            // Constructor Methods
            //

            public ValueChoices()
            {
                m_ht = HashTableFactory.New< System.Reflection.FieldInfo, List< object > >();
            }

            //
            // Helper Methods
            //

            public void Add( System.Reflection.FieldInfo fi  ,
                             object                      val )
            {
                HashTableWithListFactory.AddUnique( m_ht, fi, val );
            }

            //
            // Access Methods
            //

            public GrowOnlyHashTable< System.Reflection.FieldInfo, List< object > >.KeyEnumerable Keys
            {
                get
                {
                    return m_ht.Keys;
                }
            }

            public List< object > this[System.Reflection.FieldInfo fi]
            {
                get
                {
                    List< object > lst;

                    m_ht.TryGetValue( fi, out lst );

                    return lst;
                }
            }
        }

        public sealed class ValueContext
        {
            //
            // State
            //

            public AbstractCategory            Holder;
            public System.Reflection.FieldInfo Field;
            public int                         Index;
            public object                      Value;

            //
            // Equality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is ValueContext) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
                {
                    ValueContext other = (ValueContext)obj;

                    if(this.Holder == other.Holder &&
                       this.Field  == other.Field  &&
                       this.Index  == other.Index  &&
                       this.Value  == other.Value   )
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.Field.GetHashCode();
            }

            //
            // Helper Methods
            //

            public T GetAttribute< T >() where T : Attribute
            {
                return ReflectionHelper.GetAttribute< T >( this.Field, false );
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                if(this.Value == null)
                {
                    return "<null>";
                }
                else
                {
                    return this.Value.ToString();
                }
            }
        }

        //
        // State
        //

        private ValueChoices m_values;

        public Type Model;

        //
        // Constructor Methods
        //

        protected AbstractCategory()
        {
            m_values = new ValueChoices();
        }

        //
        // Helper Methods
        //

        public void ApplyDefaultValues()
        {
            GrowOnlySet< AbstractCategory > visited = SetFactory.NewWithReferenceEquality< AbstractCategory >();

            ApplyDefaultValues( visited );
        }

        private void ApplyDefaultValues( GrowOnlySet< AbstractCategory > visited )
        {
            if(visited.Insert( this ) == false)
            {
                foreach(System.Reflection.FieldInfo fi in m_values.Keys)
                {
                    List< object > lst = m_values[fi];

                    if(lst.Count == 1)
                    {
                        object val = lst[0];

                        IConvertible itf = val as IConvertible;
                        if(itf != null)
                        {
                            val = itf.ToType( fi.FieldType, null );
                        }

                        fi.SetValue( this, val );
                    }
                    else
                    {
                        Type t = fi.FieldType;

                        if(t.IsClass)
                        {
                            fi.SetValue( this, null );
                        }
                        else if(t.IsValueType)
                        {
                            fi.SetValue( this, Activator.CreateInstance( t ) );
                        }
                    }

                    foreach(object o in lst)
                    {
                        AbstractCategory sub = o as AbstractCategory;
                        if(sub != null)
                        {
                            sub.ApplyDefaultValues( visited );
                        }

                        AbstractCategory[] subArray = o as AbstractCategory[];
                        if(subArray != null)
                        {
                            foreach(AbstractCategory subElement in subArray)
                            {
                                subElement.ApplyDefaultValues( visited );
                            }
                        }
                    }
                }

                if(this.Model == null)
                {
                    if(this is EngineCategory)
                    {
                        this.Model = FindHardwareModel( this, HardwareModelAttribute.Kind.Engine );
                    }
                    else if(this is MemoryCategory)
                    {
                        this.Model = FindHardwareModel( this, HardwareModelAttribute.Kind.Memory );
                    }
                }
            }
        }

        //--//

        public List< ValueContext > SearchPossibleValues( Type type )
        {
            GrowOnlySet< AbstractCategory > visited = SetFactory.NewWithReferenceEquality< AbstractCategory >();
            List       < ValueContext     > lst     = new List< ValueContext >();

            SearchPossibleValues( visited, lst, type );

            return lst;
        }

        private void SearchPossibleValues( GrowOnlySet< AbstractCategory > visited ,
                                           List       < ValueContext >     lst     ,
                                           Type                            type    )
        {
            if(visited.Insert( this ) == false)
            {
                foreach(System.Reflection.FieldInfo fi in m_values.Keys)
                {
                    object val = fi.GetValue( this );

                    if(val != null)
                    {
                        ExpandPossibleValues( visited, lst, type, fi, val );
                    }
                    else
                    {
                        foreach(object o in m_values[fi])
                        {
                            ExpandPossibleValues( visited, lst, type, fi, o );
                        }
                    }
                }
            }
        }

        private void ExpandPossibleValues( GrowOnlySet< AbstractCategory > visited ,
                                           List       < ValueContext >     lst     ,
                                           Type                            type    ,
                                           System.Reflection.FieldInfo     fi      ,
                                           object                          o       )
        {
            if(type.IsInstanceOfType( o ))
            {
                ValueContext ctx = new ValueContext();

                ctx.Holder = this;
                ctx.Field  = fi;
                ctx.Value  = o;

                lst.Add( ctx );
            }

            AbstractCategory sub = o as AbstractCategory;
            if(sub != null)
            {
                sub.SearchPossibleValues( visited, lst, type );
            }
        }

        //--//

        public T SearchValue< T >()
        {
            List< ValueContext > lst = SearchValues( typeof(T) );

            if(lst.Count == 1)
            {
                return (T)lst[0].Value;
            }

            return default(T);
        }

        public T[] SearchValues< T >()
        {
            List< ValueContext > lst = SearchValues( typeof(T) );
            T[]                  res = new T[lst.Count];

            for(int i = 0; i < lst.Count; i++)
            {
                res[i] = (T)lst[i].Value;
            }

            return res;
        }

        public List< ValueContext > SearchValues( Type type )
        {
            GrowOnlySet< AbstractCategory > visited = SetFactory.NewWithReferenceEquality< AbstractCategory >();
            List       < ValueContext >     lst     = new List< ValueContext >();

            SearchValues( visited, lst, type, null );

            return lst;
        }

        //--//

        public List< ValueContext > SearchValuesWithAttributes( Type attributeType )
        {
            GrowOnlySet< AbstractCategory > visited = SetFactory.NewWithReferenceEquality< AbstractCategory >();
            List       < ValueContext >     lst     = new List< ValueContext >();

            SearchValues( visited, lst, null, attributeType );

            return lst;
        }

        private void SearchValues( GrowOnlySet< AbstractCategory > visited       ,
                                   List       < ValueContext >     lst           ,
                                   Type                            type          ,
                                   Type                            attributeType )
        {
            if(visited.Insert( this ) == false)
            {
                foreach(System.Reflection.FieldInfo fi in ReflectionHelper.GetAllInstanceFields( this.GetType() ))
                {
                    object val = fi.GetValue( this );
                    if(val != null)
                    {
                        Array valArray = val as Array;

                        if(valArray != null)
                        {
                            for(int i = 0; i < valArray.Length; i++)
                            {
                                SearchValues( visited, lst, type, attributeType, fi, i, valArray.GetValue( i ) );
                            }
                        }
                        else
                        {
                            SearchValues( visited, lst, type, attributeType, fi, -1, val );
                        }
                    }
                }
            }
        }

        private void SearchValues( GrowOnlySet< AbstractCategory > visited       ,
                                   List       < ValueContext >     lst           ,
                                   Type                            type          ,
                                   Type                            attributeType ,
                                   System.Reflection.FieldInfo     fi            ,
                                   int                             index         ,
                                   object                          val           )
        {
            bool fAdd = false;

            if(attributeType != null)
            {
                foreach(var attrib in fi.GetCustomAttributes( attributeType, false ))
                {
                    fAdd = true;
                }
            }

            if(type != null)
            {
                if(type.IsInstanceOfType( val ))
                {
                    fAdd = true;
                }
            }

            if(fAdd)
            {
                ValueContext ctx = new ValueContext();

                ctx.Holder = this;
                ctx.Field  = fi;
                ctx.Index  = index;
                ctx.Value  = val;

                lst.Add( ctx );
            }

            AbstractCategory sub = val as AbstractCategory;
            if(sub != null)
            {
                sub.SearchValues( visited, lst, type, attributeType );
            }
        }

        public static Type FindHardwareModel( object                      obj  ,
                                              HardwareModelAttribute.Kind kind )
        {
            foreach(HardwareModelAttribute attrib in ReflectionHelper.GetAttributes< HardwareModelAttribute >( obj, true ))
            {
                if(attrib.TargetKind == kind)
                {
                    return attrib.Target;
                }
            }

            return null;
        }

        public static Type FindHardwareModel( System.Reflection.MemberInfo mi   ,
                                              HardwareModelAttribute.Kind  kind )
        {
            foreach(HardwareModelAttribute attrib in ReflectionHelper.GetAttributes< HardwareModelAttribute >( mi, true ))
            {
                if(attrib.TargetKind == kind)
                {
                    return attrib.Target;
                }
            }

            return null;
        }

        //--//

        public T GetService<T>()
        {
            return (T)GetServiceInner( typeof(T) );
        }

        protected virtual object GetServiceInner( Type t )
        {
            return null;
        }

        //
        // Access Methods
        //

        public ValueChoices PossibleValues
        {
            get
            {
                return m_values;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            DisplayNameAttribute attrib = ReflectionHelper.GetAttribute< DisplayNameAttribute >( this, true );

            if(attrib != null)
            {
                return attrib.Value;
            }

            return base.ToString();
        }
    }
}
