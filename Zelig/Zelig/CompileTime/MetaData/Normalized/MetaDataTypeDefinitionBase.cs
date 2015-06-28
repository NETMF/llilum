//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataTypeDefinitionBase : MetaDataTypeDefinitionAbstract
    {
        //
        // State
        //

        internal TypeAttributes                   m_flags;
        internal string                           m_name;
        internal string                           m_nameSpace;

        internal MetaDataTypeDefinitionAbstract   m_enclosingClass;
        internal MetaDataTypeDefinitionAbstract[] m_nestedClasses;

        internal MetaDataTypeDefinitionAbstract[] m_interfaces;
        internal MetaDataField[]                  m_fields;
        internal MetaDataMethodBase[]             m_methods;
        internal MetaDataMethodImpl[]             m_methodImpls;

        internal MetaDataEvent[]                  m_events;
        internal MetaDataProperty[]               m_properties;
        internal MetaDataClassLayout              m_classLayout;

        //
        // Constructor Methods
        //

        protected MetaDataTypeDefinitionBase( MetaDataAssembly owner ,
                                              int              token ) : base( owner, token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        protected bool InnerEquals( MetaDataTypeDefinitionBase other )
        {
            // LT72: previous code used operator equality, which apparently differs in impplementation from 2.0 to 4.5
            //if(m_name           == other.m_name           &&
            //   m_nameSpace      == other.m_nameSpace      &&
            //   m_owner          == other.m_owner          &&
            //   m_enclosingClass == other.m_enclosingClass &&
            //   m_extends        == other.m_extends         )
            //{
            //    return true;
            //}
            if (                                                                 m_name          .Equals( other.m_name           )  &&
                                                                                 m_nameSpace     .Equals( other.m_nameSpace      )  &&
                                                                                 m_owner         .Equals( other.m_owner          )  &&
                ((m_enclosingClass == null && other.m_enclosingClass == null) || m_enclosingClass.Equals( other.m_enclosingClass )) &&
                ((m_extends        == null && other.m_extends        == null) || m_extends       .Equals( other.m_extends        ))
                )
            {
                return true;
            }


            return false;
        }

        protected int InnerGetHashCode()
        {
            int res = m_name.GetHashCode();

            if(m_nameSpace != null)
            {
                res ^= m_nameSpace.GetHashCode();
            }

            res ^= m_owner.GetHashCode();

            return res;
        }

        //
        // Access Methods
        //

        public TypeAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Namespace
        {
            get
            {
                return m_nameSpace;
            }
        }

        public MetaDataTypeDefinitionAbstract[] Interfaces
        {
            get
            {
                return m_interfaces;
            }
        }

        public MetaDataField[] Fields
        {
            get
            {
                return m_fields;
            }
        }

        public MetaDataMethodBase[] Methods
        {
            get
            {
                return m_methods;
            }
        }

        public MetaDataMethodImpl[] MethodImpls
        {
            get
            {
                return m_methodImpls;
            }
        }

        public MetaDataTypeDefinitionAbstract EnclosingClass
        {
            get
            {
                return m_enclosingClass;
            }
        }

        public MetaDataTypeDefinitionAbstract[] NestedClasses
        {
            get
            {
                return m_nestedClasses;
            }
        }

        public MetaDataClassLayout ClassLayout
        {
            get
            {
                return m_classLayout;
            }
        }

        public bool IsNestedType
        {
            get
            {
                return m_enclosingClass != null;
            }
        }

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                if(this.IsNestedType)
                {
                    sb.Append( this.EnclosingClass.FullName );
                    sb.Append( "."                          );
                }

                if(this.Namespace != null && this.Namespace.Length != 0)
                {
                    sb.Append( this.Namespace );
                    sb.Append( "."            );
                }

                sb.Append( this.Name );

                return sb.ToString();
            }
        }

        public bool IsNameMatch( string   nameSpace ,
                                 string   name      ,
                                 string[] nested    )
        {
            MetaDataTypeDefinitionBase td = this;

            if(nested != null)
            {
                int nestedSize = nested.Length;

                while(nestedSize > 0)
                {
                    if(td.IsNestedType == false)
                    {
                        return false;
                    }

                    if(td.m_name != nested[--nestedSize])
                    {
                        return false;
                    }

                    td = (MetaDataTypeDefinitionBase)td.m_enclosingClass;
                }
            }

            if(td.IsNestedType == true)
            {
                return false;
            }

            if(td.m_nameSpace == nameSpace && td.m_name == name)
            {
                return true;
            }

            return false;
        }

        //
        // Debug Methods
        //

        public void InnerDump( IMetaDataDumper            writer        ,
                               MetaDataGenericTypeParam[] genericParams )
        {
            if(m_enclosingClass != null && writer.AlreadyProcessed( m_enclosingClass ) == false)
            {
                return;
            }

            writer = writer.PushContext( this );

            writer.WriteLine( ".class {0} {1}{2}", TokenToString( m_token ), EnumToString( m_flags ), DumpSignature( writer, genericParams ) );

            if(m_extends != null)
            {
                writer.WriteLine( "       extends {0}", m_extends.ToString( writer ) );
            }

            if(m_interfaces != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for(int i = 0; i < m_interfaces.Length; i++)
                {
                    MetaDataTypeDefinitionAbstract itf = m_interfaces[i];

                    sb.Length = 0;

                    if(i == 0)
                    {
                        sb.Append( "       implements " );
                    }
                    else
                    {
                        sb.Append( "                  " );
                    }

                    sb.Append( itf.ToString( writer ) );

                    if(i != m_interfaces.Length - 1)
                    {
                        sb.Append( "," );
                    }

                    writer.WriteLine( sb.ToString() );
                }
            }

            writer.IndentPush( "{" );

            if(m_customAttributes != null)
            {
                foreach(MetaDataCustomAttribute ca in m_customAttributes)
                {
                    writer.Process( ca, false );
                }
                writer.WriteLine();
            }

            if(m_nestedClasses != null)
            {
                foreach(MetaDataTypeDefinitionAbstract nested in m_nestedClasses)
                {
                    writer.Process( nested, true );
                }
            }

            if(m_fields != null)
            {
                foreach(MetaDataField field in m_fields)
                {
                    writer.Process( field, true );
                }
                writer.WriteLine();
            }

            if(m_methods != null)
            {
                foreach(MetaDataMethodBase md in m_methods)
                {
                    writer.Process( md, true );
                }
                writer.WriteLine();
            }

            writer.IndentPop( "} // end of class '" + this.FullName + "'" );
            writer.WriteLine();
        }

        internal string DumpSignature( IMetaDataDumper            writer        ,
                                       MetaDataGenericTypeParam[] genericParams )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( this.FullName );

            if(genericParams != null)
            {
                sb.Append( "<" );

                bool fGot1 = false;

                foreach(MetaDataGenericTypeParam param in genericParams)
                {
                    if(fGot1)
                    {
                        sb.Append( ", " );
                    }
                    else
                    {
                        fGot1 = true;
                    }

                    if(param.m_genericParamConstraints != null)
                    {
                        sb.Append( "(" );

                        bool fGot2 = false;

                        foreach(MetaDataTypeDefinitionAbstract td in param.m_genericParamConstraints)
                        {
                            if(fGot2)
                            {
                                sb.Append( ", " );
                            }
                            else
                            {
                                fGot2 = true;
                            }

                            sb.Append( td.ToStringWithAbbreviations( writer ) );
                        }

                        sb.Append( ")" );
                    }

                    sb.Append( param.m_name );
                }

                sb.Append( ">" );
            }

            return sb.ToString();
        }

        private static string EnumToString( TypeAttributes val )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if((val & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface        ) sb.Append( "interface "          );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.Public           ) sb.Append( "public "             );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NotPublic        ) sb.Append( "private "            );
            if((val & TypeAttributes.Abstract          ) != 0                               ) sb.Append( "abstract "           );
            if((val & TypeAttributes.LayoutMask        ) == TypeAttributes.AutoLayout       ) sb.Append( "auto "               );
            if((val & TypeAttributes.LayoutMask        ) == TypeAttributes.SequentialLayout ) sb.Append( "sequential "         );
            if((val & TypeAttributes.LayoutMask        ) == TypeAttributes.ExplicitLayout   ) sb.Append( "explicit "           );
            if((val & TypeAttributes.StringFormatMask  ) == TypeAttributes.AnsiClass        ) sb.Append( "ansi "               );
            if((val & TypeAttributes.StringFormatMask  ) == TypeAttributes.UnicodeClass     ) sb.Append( "unicode "            );
            if((val & TypeAttributes.StringFormatMask  ) == TypeAttributes.AutoClass        ) sb.Append( "autochar "           );
            if((val & TypeAttributes.Import            ) != 0                               ) sb.Append( "import "             );
            if((val & TypeAttributes.Serializable      ) != 0                               ) sb.Append( "serializable "       );
            if((val & TypeAttributes.Sealed            ) != 0                               ) sb.Append( "sealed "             );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedPublic     ) sb.Append( "nested public "      );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedPrivate    ) sb.Append( "nested private "     );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedFamily     ) sb.Append( "nested family "      );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedAssembly   ) sb.Append( "nested assembly "    );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedFamANDAssem) sb.Append( "nested famandassem " );
            if((val & TypeAttributes.VisibilityMask    ) == TypeAttributes.NestedFamORAssem ) sb.Append( "nested famorassem "  );
            if((val & TypeAttributes.BeforeFieldInit   ) != 0                               ) sb.Append( "beforefieldinit "    );
            if((val & TypeAttributes.SpecialName       ) != 0                               ) sb.Append( "specialname "        );
            if((val & TypeAttributes.RTSpecialName     ) != 0                               ) sb.Append( "rtspecialname "      );

            return sb.ToString();
        }
    }
}
