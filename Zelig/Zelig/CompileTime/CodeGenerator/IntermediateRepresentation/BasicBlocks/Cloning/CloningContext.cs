//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class CloningContext
    {
        //
        // State
        //

        protected readonly ControlFlowGraphState               m_cfgDestination;
        protected readonly ControlFlowGraphState               m_cfgSource;
        protected readonly InstantiationContext                m_ic;
        protected readonly GrowOnlyHashTable< object, object > m_cloned = HashTableFactory.New< object, object >();

        //
        // Constructor Methods
        //

        protected CloningContext( ControlFlowGraphState cfgSource      ,
                                  ControlFlowGraphState cfgDestination ,
                                  InstantiationContext  ic             )
        {
            m_cfgSource      = cfgSource;
            m_cfgDestination = cfgDestination;
            m_ic             = ic;
        }

        //--//

        public T ConvertType<T>( T td ) where T : TypeRepresentation
        {
            if(m_ic != null)
            {
                td = (T)m_ic.Instantiate( td );
            }

            return td;
        }

        public T ConvertField<T>( T fd ) where T : FieldRepresentation
        {
            if(m_ic != null)
            {
                fd = (T)m_ic.Instantiate( fd );
            }

            return fd;
        }

        public T[] ConvertFields<T>( T[] fdArray ) where T : FieldRepresentation
        {
            if(m_ic != null && fdArray != null)
            {
                fdArray = (T[])m_ic.Instantiate( fdArray );
            }

            return fdArray;
        }

        public T ConvertMethod<T>( T md ) where T : MethodRepresentation
        {
            if(m_ic != null)
            {
                md = (T)m_ic.Instantiate( md );
            }

            return md;
        }

        public T[] ConvertMethods<T>( T[] mdArray ) where T : MethodRepresentation
        {
            if(m_ic != null && mdArray != null)
            {
                mdArray = (T[])m_ic.Instantiate( mdArray );
            }

            return mdArray;
        }

        //--//

        protected virtual void RegisterInner( object from ,
                                              object to   )
        {
            m_cloned[from] = to;
        }

        public void Register( BasicBlock from ,
                              BasicBlock to   )
        {
            RegisterInner( from, to );
        }

        public void Register( Expression from ,
                              Expression to   )
        {
            RegisterInner( from, to );
        }

        public void Register( Operator from ,
                              Operator to   )
        {
            RegisterInner( from, to );
        }

        public void Register( Annotation from ,
                              Annotation to   )
        {
            RegisterInner( from, to );
        }

        public void Register( ExceptionClause from ,
                              ExceptionClause to   )
        {
            RegisterInner( from, to );
        }

        public T LookupRegistered<T>( T key )
        {
            object val;

            m_cloned.TryGetValue( key, out val );
            
            return (T)val;
        }

        //--//

        protected virtual bool FoundInCache(     object from ,
                                             out object to   )
        {
            return m_cloned.TryGetValue( from, out to );
        }

        private bool ShouldClone<T>( ref T from )
        {
            object o;

            if(from == null) return false;

            if(FoundInCache( from, out o ))
            {
                from = (T)o;
                return false;
            }

            return true;
        }

        private bool ShouldClone<T>( ref T[] from )
        {
            object o;

            if(from == null) return false;

            if(from.Length == 0) return false;

            if(FoundInCache( from, out o ))
            {
                from = (T[])o;
                return false;
            }

            return true;
        }

        //--//

        public BasicBlock Clone( BasicBlock from )
        {
            if(!ShouldClone( ref from )) return from;

            BasicBlock clone = from.Clone( this );

            return clone;
        }

        public BasicBlock[] Clone( BasicBlock[] from )
        {
            if(!ShouldClone( ref from )) return from;

            BasicBlock[] clone = new BasicBlock[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = Clone( from[i] );
            }

            return clone;
        }

        public ExceptionHandlerBasicBlock[] Clone( ExceptionHandlerBasicBlock[] from )
        {
            if(!ShouldClone( ref from )) return from;

            ExceptionHandlerBasicBlock[] clone = new ExceptionHandlerBasicBlock[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = (ExceptionHandlerBasicBlock)Clone( from[i] );
            }

            return clone;
        }

        //--//

        public Expression Clone( Expression from )
        {
            if(!ShouldClone( ref from )) return from;

            if(m_cfgSource == m_cfgDestination)
            {
                //
                // Same flow graph, we don't create new local and argument variables.
                //
                if(from is ArgumentVariableExpression)
                {
                    return from;
                }

                if(from is LocalVariableExpression)
                {
                    return from;
                }
            }

            Expression clone = from.Clone( this );

            return clone;
        }

        public Expression[] Clone( Expression[] from )
        {
            if(!ShouldClone( ref from )) return from;

            Expression[] clone = new Expression[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = Clone( from[i] );
            }

            return clone;
        }

        public VariableExpression[] Clone( VariableExpression[] from )
        {
            if(!ShouldClone( ref from )) return from;

            VariableExpression[] clone = new VariableExpression[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = (VariableExpression)Clone( from[i] );
            }

            return clone;
        }

        //--//

        public Operator Clone( Operator from )
        {
            if(!ShouldClone( ref from )) return from;

            Operator clone = from.Clone( this );

            return clone;
        }

        public Operator[] Clone( Operator[] from )
        {
            if(!ShouldClone( ref from )) return from;

            Operator[] clone = new Operator[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = Clone( from[i] );
            }

            return clone;
        }

        //--//

        public Annotation Clone( Annotation from )
        {
            if(!ShouldClone( ref from )) return from;

            Annotation clone = from.Clone( this );

            return clone;
        }

        public Annotation[] Clone( Annotation[] from )
        {
            if(!ShouldClone( ref from )) return from;

            Annotation[] clone = new Annotation[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = Clone( from[i] );
            }

            return clone;
        }

        //--//

        public ExceptionClause Clone( ExceptionClause from )
        {
            if(!ShouldClone( ref from )) return from;

            ExceptionClause clone = from.Clone( this );

            return clone;
        }

        public ExceptionClause[] Clone( ExceptionClause[] from )
        {
            if(!ShouldClone( ref from )) return from;

            ExceptionClause[] clone = new ExceptionClause[from.Length];

            m_cloned[from] = clone;

            for(int i = 0; i < from.Length; i++)
            {
                clone[i] = Clone( from[i] );
            }

            return clone;
        }

        //--//

        //
        // Access Methods
        //

        public ControlFlowGraphState ControlFlowGraphSource
        {
            get
            {
                return m_cfgSource;
            }
        }

        public ControlFlowGraphState ControlFlowGraphDestination
        {
            get
            {
                return m_cfgDestination;
            }
        }

        public TypeSystemForIR TypeSystem
        {
            get
            {
                return m_cfgDestination.TypeSystemForIR;
            }
        }
    }
}
