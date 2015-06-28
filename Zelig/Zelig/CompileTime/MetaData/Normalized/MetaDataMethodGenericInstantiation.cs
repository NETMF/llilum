//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethodGenericInstantiation : MetaDataMethodAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataAssembly       m_owner;
        internal          MetaDataMethodAbstract m_baseMethod;
        internal          SignatureType[]        m_parameters;

        //
        // Constructor Methods
        //

        internal MetaDataMethodGenericInstantiation( MetaDataAssembly owner ,
                                                     int              token ) : base( token )
        {
            m_owner = owner;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethodGenericInstantiation) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethodGenericInstantiation other = (MetaDataMethodGenericInstantiation)obj;

                if(m_baseMethod == other.m_baseMethod)
                {
                    if(ArrayUtility.ArrayEquals( m_parameters, other.m_parameters))
                    {
                        return true;
                    }
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
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(m_parameters[i].IsOpenType)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal override MetaDataObject MakeUnique()
        {
            return (MetaDataObject)m_owner.MakeUnique( this );
        }

        //
        // Access Methods
        //

        public MetaDataAssembly Owner
        {
            get
            {
                return m_owner;
            }
        }

        public MetaDataMethodAbstract BaseMethod
        {
            get
            {
                return m_baseMethod;
            }
        }

        public SignatureType[] InstantiationParams
        {
            get
            {
                return m_parameters;
            }
        }

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                return m_baseMethod.FullName;
            }
        }

        public override string ToString()
        {
            return "MetaDataMethodGenericInstantiation(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            string res;

            context = context.PushContext( this );

            res = m_baseMethod.ToString( context );

            return res;
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
