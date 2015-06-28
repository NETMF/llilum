//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public sealed class Manager
    {
        //
        // State
        //

        static GrowOnlyHashTable< string, Type >            s_allTypes;

        List                         < AbstractCategory >   m_allValues;
        List                         < Type             >   m_allOptions;
        GrowOnlyHashTable< Type, List< Type             > > m_allOptionsByType;

        //
        // Constructor Methods
        //

        public Manager()
        {
            m_allValues        = new                         List< AbstractCategory >  ();
            m_allOptions       = new                         List< Type             >  ();
            m_allOptionsByType = HashTableFactory.New< Type, List< Type             > >();
        }

        //
        // Helper Methods
        //

        public static void Serialize( System.Xml.XmlElement root     ,
                                      AbstractCategory      category )
        {
            GrowOnlyHashTable< object, int > visited = HashTableFactory.NewWithReferenceEquality< object, int >();

            Serialize( root, category, visited );
        }

        public static void Serialize( System.Xml.XmlElement            container ,
                                      AbstractCategory                 category  ,
                                      GrowOnlyHashTable< object, int > visited   )
        {
            System.Xml.XmlElement node = XmlHelper.AddElement( container, "Value" );
            int                   index;

            if(visited.TryGetValue( category, out index ))
            {
                XmlHelper.AddAttribute( node, "BackReference", index.ToString() );
            }
            else
            {
                visited[category] = visited.Count;

                Type t = category.GetType();

                XmlHelper.AddAttribute( node, "Type", t.AssemblyQualifiedName );

                foreach(System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( t ))
                {
                    System.Xml.XmlElement subNode = XmlHelper.AddElement( node, "Field" );

                    XmlHelper.AddAttribute( subNode, "Name", fi.Name                                );
////                XmlHelper.AddAttribute( subNode, "Site", fi.DeclaringType.AssemblyQualifiedName );

                    object val = fi.GetValue( category );

                    if(val is Array)
                    {
                        Serialize( subNode, (Array)val, visited );
                    }
                    else if(val is AbstractCategory)
                    {
                        Serialize( subNode, (AbstractCategory)val, visited );
                    }
                    else if(val is Type)
                    {
                        Type tVal = (Type)val;

                        XmlHelper.AddAttribute( subNode, "Value", tVal.AssemblyQualifiedName );
                    }
                    else if(val != null)
                    {
                        XmlHelper.AddAttribute( subNode, "Value", val.ToString() );
                    }
                }
            }
        }

        public static void Serialize( System.Xml.XmlElement            container ,
                                      Array                            array     ,
                                      GrowOnlyHashTable< object, int > visited   )
        {
            System.Xml.XmlElement node = XmlHelper.AddElement( container, "Array" );
            int                   index;

            if(visited.TryGetValue( array, out index ))
            {
                XmlHelper.AddAttribute( node, "BackReference", index.ToString() );
            }
            else
            {
                visited[array] = visited.Count;

                XmlHelper.AddAttribute( node, "Type"  , array.GetType().GetElementType().AssemblyQualifiedName );
                XmlHelper.AddAttribute( node, "Length", array.Length                                           );

                for(int i = 0; i < array.Length; i++)
                {
                    System.Xml.XmlElement subNode = XmlHelper.AddElement( node, "Element" );

                    object val = array.GetValue( i );

                    if(val is Array)
                    {
                        Serialize( subNode, (Array)val, visited );
                    }
                    else if(val is AbstractCategory)
                    {
                        Serialize( subNode, (AbstractCategory)val, visited );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot serialize configuration, unexpected array of type {0}", array.GetType() );
                    }
                }
            }
        }

        //--//

        public static AbstractCategory Deserialize( System.Xml.XmlNode root )
        {
            List< object > visited = new List< object >();

            return (AbstractCategory)Deserialize( root, visited, false );
        }

        public static object Deserialize( System.Xml.XmlNode container ,
                                          List< object >     visited   ,
                                          bool               fNullOk   )
        {
            System.Xml.XmlNode node;
            
            node = container.SelectSingleNode( "Value" );
            if(node != null)
            {
                return DeserializeObject( container, node, visited );
            }

            node = container.SelectSingleNode( "Array" );
            if(node != null)
            {
                return DeserializeArray( container, node, visited );
            }

            if(fNullOk)
            {
                return null;
            }

            throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, unknown tag {0}", container.Name );
        }

        private static AbstractCategory DeserializeObject( System.Xml.XmlNode container ,
                                                           System.Xml.XmlNode node      ,
                                                           List<object>       visited   )
        {
            System.Xml.XmlAttribute attrib;
            
            attrib = XmlHelper.FindAttribute( node, "BackReference" );
            if(attrib != null)
            {
                return (AbstractCategory)visited[int.Parse( attrib.Value )];
            }

            attrib = XmlHelper.FindAttribute( node, "Type" );
            if(attrib == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, missing 'Type' tag" );
            }

            Type             t        = ResolveType( attrib.Value );
            AbstractCategory category = (AbstractCategory)Activator.CreateInstance( t );

            visited.Add( category );

            foreach(System.Xml.XmlNode field in node.SelectNodes( "Field" ))
            {
                attrib = XmlHelper.FindAttribute( field, "Name" );
                if(attrib == null)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, missing 'Name' tag for 'Field' element" );
                }

                System.Reflection.FieldInfo fi = t.GetField( attrib.Value );
                object                      obj;

                attrib = XmlHelper.FindAttribute( field, "Value" );
                if(attrib != null)
                {
                    Type fieldType = fi.FieldType;

                    if(fieldType == typeof(bool))
                    {
                        obj = bool.Parse( attrib.Value );
                    }
                    else if(fieldType == typeof(uint))
                    {
                        obj = uint.Parse( attrib.Value );
                    }
                    else if(fieldType == typeof(ulong))
                    {
                        obj = ulong.Parse( attrib.Value );
                    }
                    else if(fieldType == typeof(int))
                    {
                        obj = int.Parse( attrib.Value );
                    }
                    else if(fieldType == typeof(string))
                    {
                        obj = attrib.Value;
                    }
                    else if(fieldType.IsSubclassOf( typeof(Enum )))
                    {
                        obj = Enum.Parse( fieldType, attrib.Value );
                    }
                    else if(fieldType == typeof(Type))
                    {
                        obj = ResolveType( attrib.Value );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, invalid value for {0}: {1}", fi, attrib.Value );
                    }
                }
                else
                {
                    obj = Deserialize( field, visited, true );
                }

                fi.SetValue( category, obj );
            }

            return category;
        }

        private static Type ResolveType( string name )
        {
            InitializeTypes();

            Type res;

            s_allTypes.TryGetValue( name, out res );

            return res;
        }

        private static Array DeserializeArray( System.Xml.XmlNode container ,
                                               System.Xml.XmlNode node      ,
                                               List<object>       visited   )
        {
            System.Xml.XmlAttribute attrib;
            
            attrib = XmlHelper.FindAttribute( node, "BackReference" );
            if(attrib != null)
            {
                return (Array)visited[int.Parse( attrib.Value )];
            }

            attrib = XmlHelper.FindAttribute( node, "Type" );
            if(attrib == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, missing 'Type' tag" );
            }

            Type t = ResolveType( attrib.Value );

            attrib = XmlHelper.FindAttribute( node, "Length" );
            if(attrib == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot deserialize configuration, missing 'Length' tag" );
            }

            int length = int.Parse( attrib.Value );

            //--//

            Array array = Array.CreateInstance( t, length );

            visited.Add( array );

            int pos = 0;

            foreach(System.Xml.XmlNode element in node.SelectNodes( "Element" ))
            {
                object val = Deserialize( element, visited, true );

                array.SetValue( val, pos++ );
            }

            return array;
        }

        //--//

        private static void InitializeTypes()
        {
            if(s_allTypes == null)
            {
                s_allTypes = HashTableFactory.New< string, Type >();

                foreach(System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach(Type t in assembly.GetTypes())
                    {
                        s_allTypes[ t.AssemblyQualifiedName ] = t;
                    }
                }
            }
        }

        public void AddAllAssemblies()
        {
            InitializeTypes();

            foreach(Type t in s_allTypes.Values)
            {
                if(t.IsSubclassOf( typeof(AbstractCategory) ) && t.IsAbstract == false)
                {
                    m_allOptions.Add( t );

                    for(Type lookup = t; lookup != null; lookup = lookup.BaseType)
                    {
                        HashTableWithListFactory.AddUnique( m_allOptionsByType, lookup, t );
                    }
                }
            }
        }

        public void ComputeAllPossibleValuesForFields()
        {
            List< Type > stack = new List< Type >();

            m_allValues.Clear();

            foreach(Type t in m_allOptions)
            {
                ComputeAllPossibleValuesForFields( stack, t, null );
            }
        }

        private AbstractCategory ComputeAllPossibleValuesForFields( List< Type >                                           stack                  ,
                                                                    Type                                                   t                      ,
                                                                    GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValuesInherited )
        {
            if(stack.IndexOf( t ) >= 0)
            {
                System.Text.StringBuilder loop   = new System.Text.StringBuilder();
                bool                      fFirst = true;

                foreach(Type t2 in stack)
                {
                    if(fFirst)
                    {
                        fFirst = false;
                    }
                    else
                    {
                        loop.Append( " -> " );
                    }

                    loop.AppendFormat( "{0}", t2.FullName );
                }

                throw TypeConsistencyErrorException.Create( "Detected loop in configuration definition: {0}", loop.ToString() );
            }

            stack.Add( t );

            //--//

            AbstractCategory option = (AbstractCategory)Activator.CreateInstance( t );

            return ComputeAllPossibleValuesForFields( stack, option, defaultValuesInherited );
        }

        private AbstractCategory ComputeAllPossibleValuesForFields( List< Type >                                           stack                  ,
                                                                    AbstractCategory                                       option                 ,
                                                                    GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValuesInherited )
        {
            Type t = option.GetType();

            m_allValues.Add( option );

            GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues = GatherDefaultValues( t, true );

            defaultValues = MergeDefaultValues( defaultValues, defaultValuesInherited );

            foreach(System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( t ))
            {
                GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValuesForField = GatherDefaultValues( fi, false );

                AllowedOptionsAttribute allowedOptions = ReflectionHelper.GetAttribute< AllowedOptionsAttribute >( fi, false );
                if(allowedOptions != null)
                {
                    foreach(Type allowedOption in allowedOptions.Targets)
                    {
                        Type allowedOptionType = ValidateOption( fi, allowedOption, "AllowedOption", true );

                        option.PossibleValues.Add( fi, ComputeAllPossibleValuesForFields( stack, allowedOptionType, defaultValuesForField ) );
                    }

                    continue;
                }

                //--//

                Type fiType = fi.FieldType;

                if(fiType.IsArray)
                {
                    Type fiSubType = fiType.GetElementType();
                    Type modelType;

                    if(fiSubType == typeof( PeripheralCategory ))
                    {
                        List< PeripheralCategory > lst = new List< PeripheralCategory >();

                        modelType = AbstractCategory.FindHardwareModel( fi, HardwareModelAttribute.Kind.Peripheral );
                        if(modelType != null)
                        {
                            CreatePeripheral( stack, lst, modelType, defaultValuesForField );
                        }

                        modelType = AbstractCategory.FindHardwareModel( fi, HardwareModelAttribute.Kind.PeripheralsGroup );
                        if(modelType != null)
                        {
                            CreatePeripheralsForNestedTypes( stack, lst, modelType, defaultValuesForField );
                        }

                        option.PossibleValues.Add( fi, lst.ToArray() );
                    }
                    else if(fiSubType == typeof(InteropCategory))
                    {
                        List< InteropCategory > lst = new List< InteropCategory >();

                        modelType = AbstractCategory.FindHardwareModel( fi, HardwareModelAttribute.Kind.Interop );
                        if(modelType != null)
                        {
                            CreateInteropsForNestedTypes( stack, lst, modelType, defaultValuesForField );
                        }

                        option.PossibleValues.Add( fi, lst.ToArray() );
                    }
                }
                else
                {
                    if(fiType.IsSubclassOf( typeof(AbstractCategory) ))
                    {
                        List< Type > lst;

                        if(m_allOptionsByType.TryGetValue( fiType, out lst ))
                        {
                            foreach(Type possibleOptionType in lst)
                            {
                                option.PossibleValues.Add( fi, ComputeAllPossibleValuesForFields( stack, possibleOptionType, defaultValuesForField ) );
                            }
                        }
                    }
                    else
                    {
                        AbstractDefaultsAttribute value;

                        if(defaultValues.TryGetValue( fi.Name, out value ))
                        {
                            option.PossibleValues.Add( fi, value.Value );
                        }
                    }
                }
            }

            //--//

            stack.Remove( t );

            return option;
        }

        private GrowOnlyHashTable< string, AbstractDefaultsAttribute > GatherDefaultValues( System.Reflection.MemberInfo mi       ,
                                                                                            bool                         fInherit )
        {
            GrowOnlyHashTable< string, AbstractDefaultsAttribute > values = HashTableFactory.New< string, AbstractDefaultsAttribute >();

            foreach(AbstractDefaultsAttribute attrib in ReflectionHelper.GetAttributes< AbstractDefaultsAttribute >( mi, fInherit ))
            {
                values[ attrib.Member ] = attrib;
            }

            return values;
        }

        private GrowOnlyHashTable< string, AbstractDefaultsAttribute > MergeDefaultValues( GrowOnlyHashTable< string, AbstractDefaultsAttribute > baseline  ,
                                                                                           GrowOnlyHashTable< string, AbstractDefaultsAttribute > extension )
        {
            if(extension == null)
            {
                return baseline;
            }
            else
            {
                GrowOnlyHashTable< string, AbstractDefaultsAttribute > values = baseline.Clone();

                foreach(string key in extension.Keys)
                {
                    AbstractDefaultsAttribute newAttrib = extension[key];
                    AbstractDefaultsAttribute oldAttrib;

                    if(values.TryGetValue( key, out oldAttrib ))
                    {
                        if(newAttrib.Merge)
                        {
                            if(newAttrib.Value is Runtime.MemoryAttributes &&
                               oldAttrib.Value is Runtime.MemoryAttributes  )
                            {
                                Runtime.MemoryAttributes newValue = (Runtime.MemoryAttributes)newAttrib.Value;
                                Runtime.MemoryAttributes oldValue = (Runtime.MemoryAttributes)oldAttrib.Value;

                                values[ key ] = new EnumDefaultsAttribute( key, oldValue | newValue );
                                continue;
                            }
                        }
                    }

                    values[ key ] = newAttrib;
                }

                return values;
            }
        }

        private void ApplyDefaultValue( List< Type >                                           stack         ,
                                        AbstractCategory                                       value         ,
                                        GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues )
        {
            ComputeAllPossibleValuesForFields( stack, value, defaultValues );
        }

        private void CreatePeripheral( List< Type >                                           stack         ,
                                       List< PeripheralCategory >                             lst           ,
                                       Type                                                   modelType     ,
                                       GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues )
        {
            PeripheralCategory value = new PeripheralCategory();

            value.Model = modelType;

            ApplyDefaultValue( stack, value, defaultValues );

            lst.Add( value );
        }

        private void CreatePeripheralsForNestedTypes( List< Type >                                           stack         ,
                                                      List< PeripheralCategory >                             lst           ,
                                                      Type                                                   modelType     ,
                                                      GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues )
        {
            Type[] array = modelType.GetNestedTypes( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic );

            foreach(Type t in array)
            {
                if(t.IsAbstract == false && t.IsSubclassOf( typeof(Emulation.ArmProcessor.Simulator.Peripheral) ))
                {
                    CreatePeripheral( stack, lst, t, defaultValues );
                }
            }
        }

        private void CreateInterop( List< Type >                                           stack         ,
                                    List< InteropCategory >                                lst           ,
                                    Type                                                   modelType     ,
                                    GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues )
        {
            InteropCategory value = new InteropCategory();

            value.Model = modelType;

            ApplyDefaultValue( stack, value, defaultValues );

            lst.Add( value );
        }

        private void CreateInteropsForNestedTypes( List< Type >                                           stack         ,
                                                   List< InteropCategory >                                lst           ,
                                                   Type                                                   modelType     ,
                                                   GrowOnlyHashTable< string, AbstractDefaultsAttribute > defaultValues )
        {
            Type[] array = modelType.GetNestedTypes( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic );

            foreach(Type t in array)
            {
                if(t.IsSubclassOf( typeof(Emulation.ArmProcessor.Simulator.InteropHandler) ))
                {
                    CreateInterop( stack, lst, t, defaultValues );
                }
            }
        }

        //--//

        private Type ValidateOption( System.Reflection.FieldInfo fi        ,
                                     Type                        t         ,
                                     string                      attrib    ,
                                     bool                        fRequired )
        {
            List< Type > lst;

            if(m_allOptionsByType.TryGetValue( t, out lst ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Field '{0}' is decorated with '{1} {2}', but '{2}' does not exist", fi, attrib, t.FullName );
            }

            if(lst.Count != 1)
            {
                if(fRequired)
                {
                    throw TypeConsistencyErrorException.Create( "Field '{0}' is decorated with '{1} {2}', but '{2}' is not unique", fi, attrib, t.FullName );
                }

                return null;
            }

            return lst[0];
        }

        //
        // Access Methods
        //

        public List< AbstractCategory > AllValues
        {
            get
            {
                return m_allValues;
            }
        }
    }
}
