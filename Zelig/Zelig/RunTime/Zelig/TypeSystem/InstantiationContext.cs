//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class InstantiationContext
    {
        //
        // State
        //

        private readonly GrowOnlySet< TypeRepresentation            > m_uniqueTypes;
        private readonly GrowOnlySet< FieldRepresentation           > m_uniqueFields;
        private readonly GrowOnlySet< MethodRepresentation          > m_uniqueMethods;
        private readonly GrowOnlySet< CustomAttributeRepresentation > m_uniqueAttributes;
        private readonly GrowOnlySet< ResourceRepresentation        > m_uniqueResources;

        private readonly TypeRepresentation[]                         m_typeParameters;
        private readonly TypeRepresentation[]                         m_methodParameters;

        //
        // Constructor Methods
        //

        public InstantiationContext()
        {
            m_uniqueTypes      = SetFactory.New< TypeRepresentation            >();
            m_uniqueFields     = SetFactory.New< FieldRepresentation           >();
            m_uniqueMethods    = SetFactory.New< MethodRepresentation          >();
            m_uniqueAttributes = SetFactory.New< CustomAttributeRepresentation >();
            m_uniqueResources  = SetFactory.New< ResourceRepresentation        >();

            m_typeParameters   = TypeRepresentation.SharedEmptyArray;
            m_methodParameters = TypeRepresentation.SharedEmptyArray;
        }

        private InstantiationContext( InstantiationContext template         ,
                                      TypeRepresentation[] typeParameters   ,
                                      TypeRepresentation[] methodParameters )
        {
            m_uniqueTypes      = template.m_uniqueTypes;
            m_uniqueFields     = template.m_uniqueFields;
            m_uniqueMethods    = template.m_uniqueMethods;
            m_uniqueAttributes = template.m_uniqueAttributes;
            m_uniqueResources  = template.m_uniqueResources;

            m_typeParameters   = typeParameters   != null ? typeParameters   : TypeRepresentation.SharedEmptyArray;
            m_methodParameters = methodParameters != null ? methodParameters : TypeRepresentation.SharedEmptyArray;
        }

        //
        // Helper Methods
        //

        public InstantiationContext SetParameters( TypeRepresentation[] typeParameters   ,
                                                   TypeRepresentation[] methodParameters )
        {
            return new InstantiationContext( this, typeParameters, methodParameters );
        }

        //--//

        public TypeRepresentation Lookup( TypeRepresentation td   ,
                                          bool               fAdd )
        {
            TypeRepresentation tdRes;

            if(m_uniqueTypes.Contains( td, out tdRes ))
            {
                return tdRes;
            }

            if(fAdd)
            {
                m_uniqueTypes.Insert( td );
            }

            return null;
        }

        public FieldRepresentation Lookup( FieldRepresentation fd   ,
                                           bool                fAdd )
        {
            FieldRepresentation fdRes;

            if(m_uniqueFields.Contains( fd, out fdRes ))
            {
                return fdRes;
            }

            if(fAdd)
            {
                m_uniqueFields.Insert( fd );
            }

            return null;
        }

        public MethodRepresentation Lookup( MethodRepresentation md   ,
                                            bool                 fAdd )
        {
            MethodRepresentation mdRes;

            if(m_uniqueMethods.Contains( md, out mdRes ))
            {
                return mdRes;
            }

            if(fAdd)
            {
                m_uniqueMethods.Insert( md );
            }

            return null;
        }

        public CustomAttributeRepresentation Lookup( CustomAttributeRepresentation ca   ,
                                                     bool                          fAdd )
        {
            CustomAttributeRepresentation caRes;

            if(m_uniqueAttributes.Contains( ca, out caRes ))
            {
                return caRes;
            }

            if(fAdd)
            {
                m_uniqueAttributes.Insert( ca );
            }

            return null;
        }

        public ResourceRepresentation Lookup( ResourceRepresentation res  ,
                                              bool                   fAdd )
        {
            ResourceRepresentation resRes;

            if(m_uniqueResources.Contains( res, out resRes ))
            {
                return resRes;
            }

            if(fAdd)
            {
                m_uniqueResources.Insert( res );
            }

            return null;
        }

        //--//

        public TypeRepresentation Instantiate( TypeRepresentation td )
        {
            if(td == null || td.IsOpenType == false)
            {
                return td;
            }

            return td.Instantiate( this );
        }

        public TypeRepresentation[] Instantiate( TypeRepresentation[] tdArray )
        {
            if(tdArray == null)
            {
                return tdArray;
            }

            int                  len        = tdArray.Length;
            TypeRepresentation[] tdArrayRes = new TypeRepresentation[len];
            bool                 fChanged   = false;

            for(int i = 0; i < len; i++)
            {
                TypeRepresentation td    = tdArray[i];
                TypeRepresentation tdRes = Instantiate( td );

                tdArrayRes[i] = tdRes;

                if(Object.ReferenceEquals( td, tdRes ) == false)
                {
                    fChanged = true;
                }
            }

            return fChanged ? tdArrayRes : tdArray;
        }

        public MethodRepresentation Instantiate( MethodRepresentation md )
        {
            throw new NotImplementedException();
        }

        public MethodRepresentation[] Instantiate( MethodRepresentation[] mdArray )
        {
            if(mdArray == null)
            {
                return mdArray;
            }

            int                    len        = mdArray.Length;
            MethodRepresentation[] mdArrayRes = new MethodRepresentation[len];
            bool                   fChanged   = false;

            for(int i = 0; i < len; i++)
            {
                MethodRepresentation md    = mdArray[i];
                MethodRepresentation mdRes = Instantiate( md );

                mdArrayRes[i] = mdRes;

                if(Object.ReferenceEquals( md, mdRes ) == false)
                {
                    fChanged = true;
                }
            }

            return fChanged ? mdArrayRes : mdArray;
        }

        public FieldRepresentation Instantiate( FieldRepresentation fd )
        {
            throw new NotImplementedException();
        }

        public FieldRepresentation[] Instantiate( FieldRepresentation[] fdArray )
        {
            if(fdArray == null)
            {
                return fdArray;
            }

            int                   len        = fdArray.Length;
            FieldRepresentation[] fdArrayRes = new FieldRepresentation[len];
            bool                  fChanged   = false;

            for(int i = 0; i < len; i++)
            {
                FieldRepresentation fd    = fdArray[i];
                FieldRepresentation fdRes = Instantiate( fd );

                fdArrayRes[i] = fdRes;

                if(Object.ReferenceEquals( fd, fdRes ) == false)
                {
                    fChanged = true;
                }
            }

            return fChanged ? fdArrayRes : fdArray;
        }

        //--//

        public void RefreshHashCodes()
        {
            m_uniqueTypes     .RefreshHashCodes();
            m_uniqueFields    .RefreshHashCodes();
            m_uniqueMethods   .RefreshHashCodes();
            m_uniqueAttributes.RefreshHashCodes();
            m_uniqueResources .RefreshHashCodes();
        }

        //
        // Access Methods
        //

        public TypeRepresentation[] TypeParameters
        {
            get
            {
                return m_typeParameters;
            }
        }

        public TypeRepresentation[] MethodParameters
        {
            get
            {
                return m_methodParameters;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            System.Text.StringBuilder sb = new  System.Text.StringBuilder();

            if(this.m_typeParameters.Length > 0)
            {
                sb.Append( "Type:[" );

                for(int i = 0; i < this.m_typeParameters.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( this.m_typeParameters[i] );
                }

                sb.Append( "]" );
            }

            if(this.m_methodParameters.Length > 0)
            {
                sb.Append( " => Method:[" );

                for(int i = 0; i < this.m_methodParameters.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( this.m_methodParameters[i] );
                }

                sb.Append( "]" );
            }

            return sb.ToString();
        }
    }
}
