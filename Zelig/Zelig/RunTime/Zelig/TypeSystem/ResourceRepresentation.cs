//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class ResourceRepresentation
    {
        //
        // This is just a copy of Microsoft.Zelig.MetaData.ManifestResourceAttributes, needed to break the dependency of TypeSystem from MetaData.
        //
        [Flags]
        public enum Attributes
        {
            VisibilityMask = 0x0007,
            Public         = 0x0001, // The Resource is exported from the Assembly.
            Private        = 0x0002  // The Resource is private to the Assembly.
        }

        [AllowCompileTimeIntrospection]
        public class Pair
        {
            //
            // State
            //

            public readonly string Key;
            public readonly object Value;

            //
            // Constructor Methods
            //

            public Pair( string key   ,
                         object value )
            {
                this.Key   = key;
                this.Value = value;
            }
        }

        //
        // State
        //

        private AssemblyRepresentation m_owner;
        private Attributes             m_flags;
        private string                 m_name;
        private Pair[]                 m_values;

        //
        // Constructor Methods
        //

        public ResourceRepresentation( AssemblyRepresentation owner )
        {
            m_owner = owner;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is ResourceRepresentation)
            {
                ResourceRepresentation other = (ResourceRepresentation)obj;

                if(m_owner == other.m_owner &&
                   m_name  == other.m_name   )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_name.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_owner  );
            context.Transform( ref m_flags  );
            context.Transform( ref m_name   );
            context.Transform( ref m_values );

            context.Pop();
        }

        //--//

        internal void CompleteIdentity(     TypeSystem                                   typeSystem ,
                                        ref ConversionContext                            context    ,
                                            MetaData.Normalized.MetaDataManifestResource res        )
        {
            m_flags = (Attributes)res.Flags;
            m_name  =             res.Name;

            string suffix = ".resources";
            if(m_name.EndsWith( suffix ))
            {
                m_name = m_name.Substring( 0, m_name.Length - suffix.Length );
            }

            var table = res.Values;
            if(table != null)
            {
                int    pos   = 0; 
                int    num   = table.Count;
                Pair[] pairs = new Pair[num];

                foreach(string key in table.Keys)
                {
                    pairs[pos++] = new Pair( key, table[key] );
                }

                m_values = pairs;
            }
        }

        //--//

        //
        // Access Methods
        //

        public AssemblyRepresentation Owner
        {
            get
            {
                return m_owner;
            }
        }

        public Attributes Flags
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

        public Pair[] Values
        {
            get
            {
                return m_values;
            }
        }
    }
}
