//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    [DisableReferenceCounting]
    public abstract class BaseRepresentation
    {
        //
        // State
        //
        
        protected static int                                 s_identity;
        private          int                                 m_identity;

        //--//

        protected CustomAttributeAssociationRepresentation[] m_customAttributes;

        //--//

        //
        // Constructor Methods
        //

        protected BaseRepresentation()
        {
            m_identity         = Interlocked.Increment( ref s_identity );
            m_customAttributes = CustomAttributeAssociationRepresentation.SharedEmptyArray;
        }

        //
        // MetaDataEquality Methods
        //

        public abstract bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set );

        public static bool EqualsThroughEquivalence( BaseRepresentation left  ,
                                                     BaseRepresentation right ,
                                                     EquivalenceSet     set   )
        {
            if(Object.ReferenceEquals( left, right ))
            {
                return true;
            }

            if(left != null && right != null)
            {
                return left.EqualsThroughEquivalence( right, set );
            }

            return false;
        }

        public static bool ArrayEqualsThroughEquivalence<T>( T[]            s   ,
                                                             T[]            d   ,
                                                             EquivalenceSet set ) where T : BaseRepresentation
        {
            return ArrayEqualsThroughEquivalence( s, d, 0, -1, set );
        }

        public static bool ArrayEqualsThroughEquivalence<T>( T[]            s      ,
                                                             T[]            d      ,
                                                             int            offset ,
                                                             int            count  ,
                                                             EquivalenceSet set    ) where T : BaseRepresentation
        {
            int sLen = s != null ? s.Length : 0;
            int dLen = d != null ? d.Length : 0;

            if(count < 0)
            {
                if(sLen != dLen)
                {
                    return false;
                }

                count = sLen - offset;
            }
            else
            {
                if(sLen < count + offset ||
                   dLen < count + offset  )
                {
                    return false;
                }
            }

            while(count > 0)
            {
                if(EqualsThroughEquivalence( s[offset], d[offset], set ) == false)
                {
                    return false;
                }

                offset++;
                count--;
            }

            return true;
        }

        //--//

        //
        // Helper Methods
        //

        public virtual void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_customAttributes );

            context.Pop();
        }
        
        //--//

        public void AddCustomAttribute( CustomAttributeAssociationRepresentation cca )
        {
            m_customAttributes = ArrayUtility.AddUniqueToNotNullArray( m_customAttributes, cca );
        }

        public void CopyCustomAttribute( CustomAttributeAssociationRepresentation caa )
        {
            AddCustomAttribute( new CustomAttributeAssociationRepresentation( caa.CustomAttribute, this, caa.ParameterIndex ) );
        }

        public void RemoveCustomAttribute( CustomAttributeAssociationRepresentation cca )
        {
            m_customAttributes = ArrayUtility.RemoveUniqueFromNotNullArray( m_customAttributes, cca );
        }

        public void RemoveCustomAttribute( CustomAttributeRepresentation ca )
        {
            foreach(CustomAttributeAssociationRepresentation caa in m_customAttributes)
            {
                if(caa.CustomAttribute == ca)
                {
                    RemoveCustomAttribute( caa );
                }
            }
        }

        public bool HasCustomAttribute( TypeRepresentation target )
        {
            return FindCustomAttribute( target, -1 ) != null;
        }

        public CustomAttributeRepresentation FindCustomAttribute( TypeRepresentation target )
        {
            return FindCustomAttribute( target, -1 );
        }

        public List<CustomAttributeRepresentation> FindCustomAttributes( TypeRepresentation target )
        {
            var lst = new List<CustomAttributeRepresentation>();

            FindCustomAttributes( target, lst );

            return lst;
        }

        public CustomAttributeRepresentation FindCustomAttribute( TypeRepresentation target ,
                                                                  int                index  )
        {
            return FindCustomAttribute( target, -1, index );
        }

        public virtual void EnumerateCustomAttributes( CustomAttributeAssociationEnumerationCallback callback )
        {
            foreach(CustomAttributeAssociationRepresentation caa in m_customAttributes)
            {
                callback( caa );
            }
        }

        //--//

        protected CustomAttributeRepresentation FindCustomAttribute( TypeRepresentation target     ,
                                                                     int                paramIndex ,
                                                                     int                index      )
        {
            int pos = index >= 0 ? 0 : index; // So we don't have the double check in the loop.

            foreach(CustomAttributeAssociationRepresentation caa in m_customAttributes)
            {
                if(caa.CustomAttribute.Constructor.OwnerType == target     &&
                   caa.ParameterIndex                        == paramIndex  )
                {
                    if(index == pos)
                    {
                        return caa.CustomAttribute;
                    }

                    pos++;
                }
            }

            return null;
        }

        protected void FindCustomAttributes( TypeRepresentation target, List<CustomAttributeRepresentation> lst )
        {
            foreach(CustomAttributeAssociationRepresentation caa in m_customAttributes)
            {
                if(caa.CustomAttribute.Constructor.OwnerType == target)
                {
                    lst.Add( caa.CustomAttribute );
                }
            }
        }
    
        //--//

        internal virtual void ProhibitUse( TypeSystem.Reachability reachability ,
                                           bool                    fApply       )
        {
            reachability.ExpandProhibition( this );

            foreach(CustomAttributeAssociationRepresentation caa in m_customAttributes)
            {
                reachability.ExpandProhibition( caa );
            }
        }

        internal virtual void Reduce( TypeSystem.Reachability reachability ,
                                      bool                    fApply       )
        {
            for(int i = m_customAttributes.Length; --i >= 0; )
            {
                CustomAttributeAssociationRepresentation caa = m_customAttributes[i];

                CHECKS.ASSERT( reachability.Contains( caa.Target ) == true, "{0} cannot be reachable since its owner is not in the Reachability set", caa );

                if(reachability.Contains( caa.CustomAttribute.Constructor ) == false)
                {
                    CHECKS.ASSERT( reachability.Contains( caa                 ) == false, "{0} cannot belong to both the Reachability and the Prohibition set", caa                 );
                    CHECKS.ASSERT( reachability.Contains( caa.CustomAttribute ) == false, "{0} cannot belong to both the Reachability and the Prohibition set", caa.CustomAttribute );

                    reachability.ExpandProhibition( caa                 );
                    reachability.ExpandProhibition( caa.CustomAttribute );

                    if(fApply)
                    {
                        if(m_customAttributes.Length == 1)
                        {
                            m_customAttributes = CustomAttributeAssociationRepresentation.SharedEmptyArray;
                            return;
                        }

                        m_customAttributes = ArrayUtility.RemoveAtPositionFromNotNullArray( m_customAttributes, i );
                    }
                }
            }
        }

        //--//

        //
        // Access Methods
        //

        public int Identity
        {
            get
            {
                return m_identity;
            }
        }

        public CustomAttributeAssociationRepresentation[] CustomAttributes
        {
            get
            {
                return m_customAttributes;
            }
        }

        //--//

        //
        // Debug Methods
        //
    }
}
