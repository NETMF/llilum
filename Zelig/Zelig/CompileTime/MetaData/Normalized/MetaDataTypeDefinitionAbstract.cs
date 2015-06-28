//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataTypeDefinitionAbstract : MetaDataObject,
        IMetaDataHasDeclSecurity
    {
        //
        // State
        //

        internal readonly MetaDataAssembly               m_owner;
        internal          ElementTypes                   m_elementType;
        internal          MetaDataTypeDefinitionAbstract m_extends;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionAbstract( MetaDataAssembly owner ,
                                                 int              token ) : base( token )
        {
            m_owner = owner;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return MakeUnique( (IMetaDataUnique)this );
        }

        internal MetaDataObject MakeUnique( IMetaDataUnique obj )
        {
            return (MetaDataObject)m_owner.MakeUnique( obj );
        }

        //
        // Access Methods
        //

        public ElementTypes ElementType
        {
            get
            {
                return m_elementType;
            }
        }

        public MetaDataTypeDefinitionAbstract Extends
        {
            get
            {
                return m_extends;
            }
        }

        public MetaDataAssembly Owner
        {
            get
            {
                return m_owner;
            }
        }

        //
        // Helper Methods
        //

        public abstract bool IsOpenType
        {
            get;
        }

        public abstract bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          );

        public bool IsScalar
        {
            get
            {
                switch(m_elementType)
                {
                    case ElementTypes.VOID   :
                    case ElementTypes.BOOLEAN:
                    case ElementTypes.CHAR   :
                    case ElementTypes.I1     :
                    case ElementTypes.U1     :
                    case ElementTypes.I2     :
                    case ElementTypes.U2     :
                    case ElementTypes.I4     :
                    case ElementTypes.U4     :
                    case ElementTypes.I8     :
                    case ElementTypes.U8     :
                    case ElementTypes.R4     :
                    case ElementTypes.R8     :
                    case ElementTypes.I      :
                    case ElementTypes.U      :
                        return true;
                }

                return false;
            }
        }

        public bool IsInteger // BEWARE: this only applies to Scalars.
        {
            get
            {
                switch(m_elementType)
                {
                    case ElementTypes.BOOLEAN:
                    case ElementTypes.CHAR   :
                    case ElementTypes.I1     :
                    case ElementTypes.U1     :
                    case ElementTypes.I2     :
                    case ElementTypes.U2     :
                    case ElementTypes.I4     :
                    case ElementTypes.U4     :
                    case ElementTypes.I8     :
                    case ElementTypes.U8     :
                    case ElementTypes.I      :
                    case ElementTypes.U      :
                        return true;
                }

                return false;
            }
        }

        public uint ScalarSize
        {
            get
            {
                switch(m_elementType)
                {
                    case ElementTypes.VOID   : return 0;
                    case ElementTypes.BOOLEAN: return 1;
                    case ElementTypes.CHAR   : return 2;
                    case ElementTypes.I1     : return 1;
                    case ElementTypes.U1     : return 1;
                    case ElementTypes.I2     : return 2;
                    case ElementTypes.U2     : return 2;
                    case ElementTypes.I4     : return 4;
                    case ElementTypes.U4     : return 4;
                    case ElementTypes.I8     : return 8;
                    case ElementTypes.U8     : return 8;
                    case ElementTypes.R4     : return 4;
                    case ElementTypes.R8     : return 8;
                    case ElementTypes.I      : return 4;
                    case ElementTypes.U      : return 4;
                }

                return uint.MaxValue;
            }
        }

        public bool IsSigned
        {
            get
            {
                switch(m_elementType)
                {
                    case ElementTypes.I1:
                    case ElementTypes.I2:
                    case ElementTypes.I4:
                    case ElementTypes.I8:
                    case ElementTypes.R4:
                    case ElementTypes.R8:
                    case ElementTypes.I :
                        return true;
                }

                return false;
            }
        }

        //
        // Debug Methods
        //

        public abstract String FullName
        {
            get;
        }

        public virtual String FullNameWithAbbreviation
        {
            get
            {
                string result = GetAbbreviation();

                if(result == null)
                {
                    return this.FullName;
                }
                else
                {
                    return result;
                }
            }
        }

        public abstract String ToString( IMetaDataDumper context );

        public string GetAbbreviation()
        {
            switch(this.ElementType)
            {
                case ElementTypes.VOID   : return "void"   ;
                case ElementTypes.BOOLEAN: return "bool"   ;
                case ElementTypes.CHAR   : return "char"   ;
                case ElementTypes.I1     : return "sbyte"  ;
                case ElementTypes.U1     : return "byte"   ;
                case ElementTypes.I2     : return "short"  ;
                case ElementTypes.U2     : return "ushort" ;
                case ElementTypes.I4     : return "int"    ;
                case ElementTypes.U4     : return "uint"   ;
                case ElementTypes.I8     : return "long"   ;
                case ElementTypes.U8     : return "ulong"  ;
                case ElementTypes.R4     : return "float"  ;
                case ElementTypes.R8     : return "double" ;
                case ElementTypes.STRING : return "string" ;
                case ElementTypes.OBJECT : return "object" ;
                default                  : return null;
            }
        }

        public string ToStringWithAbbreviations( IMetaDataDumper context )
        {
            string result = GetAbbreviation();

            if(result == null)
            {
                return this.ToString( context );
            }
            else
            {
                return result;
            }
        }

        internal static string ParameterToString( IMetaDataDumper context        ,
                                                  int             number         ,
                                                  bool            fIsMethodParam ,
                                                  string          fallback       )
        {
            string res;

            if(fIsMethodParam)
            {
                MetaDataMethodAbstract md = context.GetContextMethodAndPop( out context );

                if(md is MetaDataMethodGeneric)
                {
                    MetaDataMethodGeneric md2 = (MetaDataMethodGeneric)md;

                    res = "!!" + md2.GenericParams[number].m_name;
                }
                else if(md is MetaDataMethodGenericInstantiation)
                {
                    MetaDataMethodGenericInstantiation md2 = (MetaDataMethodGenericInstantiation)md;

                    res = md2.m_parameters[number].ToStringWithAbbreviations( context );
                }
                else
                {
                    if(fallback == null)
                    {
                        throw new NotNormalized( "ParametersToString" );
                    }

                    res = fallback;
                }
            }
            else
            {
                MetaDataTypeDefinitionAbstract td = context.GetContextTypeAndPop( out context );

                if(td is MetaDataTypeDefinitionGeneric)
                {
                    MetaDataTypeDefinitionGeneric td2 = (MetaDataTypeDefinitionGeneric)td;

                    res = "!" + td2.GenericParams[number].m_name;
                }
                else if(td is MetaDataTypeDefinitionGenericInstantiation)
                {
                    MetaDataTypeDefinitionGenericInstantiation td2 = (MetaDataTypeDefinitionGenericInstantiation)td;

                    res = td2.m_parameters[number].ToStringWithAbbreviations( context );
                }
                else
                {
                    if(fallback == null)
                    {
                        throw new NotNormalized( "ParametersToString" );
                    }

                    res = fallback;
                }
            }

            return res;
        }

        protected string QualifiedToString( string prefix )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( prefix );

            sb.Append( "(" );

            sb.Append( "[" );
            sb.Append( this.Owner.Name );
            sb.Append( "]" );
            sb.Append( this.FullName );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
