//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


#if DEBUG
#define TRACK_METADATAOBJECT_IDENTITY
#else
//#define TRACK_METADATAOBJECT_IDENTITY
#endif

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public class NotNormalized : Exception
    {
        //
        // Constructor Methods
        //

        public NotNormalized( string reason ) : base( reason )
        {
        }
    }

    //--//

    //
    // This class's primary reason for existence is its place in the class hierarchy.
    //
    // Convenient place to store CustomAttribute information
    //
    public abstract class MetaDataObject : IMetaDataHasCustomAttribute
    {
        //
        // State
        //

#if TRACK_METADATAOBJECT_IDENTITY
        protected static int s_identity;
#endif
        public    int        m_identity;

        //--//

        internal readonly int                       m_token;
        internal          MetaDataCustomAttribute[] m_customAttributes;

        //
        // Constructor Methods
        //

        protected MetaDataObject( int token )
        {
#if TRACK_METADATAOBJECT_IDENTITY
            m_identity = s_identity++;
#endif

            m_token = token;
        }

        //
        // Helper methods
        //

        internal abstract MetaDataObject MakeUnique();

        //
        // Access Methods
        //

        public int Token
        {
            get
            {
                return m_token;
            }
        }

        public MetaDataCustomAttribute[] CustomAttributes
        {
            get
            {
                return m_customAttributes;
            }
        }

        public virtual bool UsesTypeParameters
        {
            get
            {
                return false;
            }
        }

        public virtual bool UsesMethodParameters
        {
            get
            {
                return false;
            }
        }

        //
        // Debug Methods
        //

        public abstract void Dump( IMetaDataDumper writer );

        protected static string TokenToString( int token )
        {
            return String.Format( "/*{0:X8}*/", token );
        }
    }
}
