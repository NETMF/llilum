//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethodWithContext : MetaDataMethodAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract m_contextType;
        internal readonly MetaDataMethodAbstract         m_contextMethod;
        internal readonly MetaDataMethodBase             m_baseMethod;

        //
        // Constructor Methods
        //

        internal MetaDataMethodWithContext( MetaDataTypeDefinitionAbstract contextType   ,
                                            MetaDataMethodAbstract         contextMethod ,
                                            MetaDataMethodBase             baseMethod    ) : base( 0 )
        {
            m_contextType   = contextType;
            m_contextMethod = contextMethod;
            m_baseMethod    = baseMethod;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethodWithContext) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethodWithContext other = (MetaDataMethodWithContext)obj;

                if(m_contextType   == other.m_contextType   &&
                   m_contextMethod == other.m_contextMethod &&
                   m_baseMethod    == other.m_baseMethod     )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_baseMethod.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenMethod
        {
            get
            {
                if(m_baseMethod.IsOpenMethod)
                {
                    if(m_contextMethod != null)
                    {
                        if(m_contextMethod.IsOpenMethod == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_baseMethod.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_baseMethod.UsesMethodParameters;
            }
        }

        internal override MetaDataObject MakeUnique()
        {
            return m_contextType.MakeUnique( (IMetaDataUnique)this );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract ContextType
        {
            get
            {
                return m_contextType;
            }
        }

        public MetaDataMethodAbstract ContextMethod
        {
            get
            {
                return m_contextMethod;
            }
        }

        public MetaDataMethodBase BaseMethod
        {
            get
            {
                return m_baseMethod;
            }
        }

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( "{" );
                sb.Append( m_contextType.FullName );

                if(m_contextMethod != null)
                {
                    sb.Append( "," );
                    sb.Append( m_contextMethod.FullName );
                }

                sb.Append( "}" );

                sb.Append( m_baseMethod.FullName );

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return "MetaDataMethodWithContext(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            string res;

            context = context.PushContext( m_contextType   );
            context = context.PushContext( m_contextMethod );

            res = m_baseMethod.ToString( context );

            return res;
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
