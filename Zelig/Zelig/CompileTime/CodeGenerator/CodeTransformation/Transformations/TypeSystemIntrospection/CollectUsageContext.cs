//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class CollectUsageContext : ScanTypeSystem
    {
        //
        // State
        //

        object           m_target;
        List< object[] > m_occurences;

        //
        // Constructor Methods
        //

        private CollectUsageContext( TypeSystemForCodeTransformation typeSystem ,
                                     object                          target     ) : base( typeSystem, typeof(CollectUsageContext) )
        {
            m_target     = target;
            m_occurences = new List< object[] >();
        }

        //--//

        //
        // Helper Methods
        //

        internal static List< object[] > Execute( TypeSystemForCodeTransformation typeSystem ,
                                                  object                          target     )
        {
            CollectUsageContext pThis = new CollectUsageContext( typeSystem, target );

            pThis.ProcessTypeSystem();

            return pThis.m_occurences;
        }

        //--//

        protected override object ShouldSubstitute(     object             target ,
                                                    out SubstitutionAction result )
        {
            if(target == m_target)
            {
                m_occurences.Add( this.FullContext );
            }

            result = SubstitutionAction.Unknown;
            
            return null;
        }
    }
}
