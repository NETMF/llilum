//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Globalization;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Resources.ResourceManager), NoConstructors=true)]
    public class ResourceManagerImpl
    {
        //
        // State
        //

#pragma warning disable 649
        [TS.WellKnownField( "ResourceManagerImpl_s_resources" )]
        static TS.ResourceRepresentation[] s_resources;

        [AliasForBaseField] internal string                     BaseNameField;
        [AliasForBaseField] internal System.Reflection.Assembly MainAssembly;
#pragma warning restore 649

        //
        // Helper Methods
        //

        [NoInline]
        [TS.WellKnownMethod( "ResourceManagerImpl_GetString1" )]
        public virtual String GetString( String name )
        {
            return (string)GetObject( name, (CultureInfo)null, true );
        }

        [NoInline]
        [TS.WellKnownMethod( "ResourceManagerImpl_GetString2" )]
        public virtual String GetString( String      name    ,
                                         CultureInfo culture )
        {
            return (string)GetObject( name, culture, true );
        }

        [NoInline]
        [TS.WellKnownMethod( "ResourceManagerImpl_GetObject1" )]
        public virtual Object GetObject( String name )
        {
            return GetObject( name, (CultureInfo)null, true );
        }

        [NoInline]
        [TS.WellKnownMethod( "ResourceManagerImpl_GetObject2" )]
        public virtual Object GetObject( String      name    ,
                                         CultureInfo culture )
        {
            return GetObject( name, culture, true );
        }


        internal Object GetObject( String      name                   ,
                                   CultureInfo culture                ,
                                   bool        wrapUnmanagedMemStream )
        {
            foreach(TS.ResourceRepresentation res in s_resources)
            {
                if(res.Name == BaseNameField)
                {
                    if(res.Values != null)
                    {
                        foreach(TS.ResourceRepresentation.Pair pair in res.Values)
                        {
                            if(pair.Key == name)
                            {
                                return pair.Value;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
