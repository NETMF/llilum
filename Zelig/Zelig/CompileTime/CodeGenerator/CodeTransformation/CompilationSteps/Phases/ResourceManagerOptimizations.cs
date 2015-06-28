//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(ApplyConfigurationSettings), ExecuteBefore=typeof(HighLevelTransformations) )]
    public sealed class ResourceManagerOptimizations : PhaseDriver
    {
        //
        // State
        //

        GrowOnlyHashTable< StaticMethodRepresentation, string > m_lookup;

        //
        // Constructor Methods
        //

        public ResourceManagerOptimizations( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            this.CallsDataBase.Analyze( this.TypeSystem );

            m_lookup = HashTableFactory.New< StaticMethodRepresentation, string >();

            Handle_ResourceManagerImpl_GetObject1( "ResourceManagerImpl_GetObject1" );
            Handle_ResourceManagerImpl_GetObject1( "ResourceManagerImpl_GetString1" );

            Handle_ResourceManagerImpl_GetObject2( "ResourceManagerImpl_GetObject2" );
            Handle_ResourceManagerImpl_GetObject2( "ResourceManagerImpl_GetString2" );

            return this.NextPhase;
        }

        //--//

        //
        // The VS Designer generates this code pattern for resources:
        //
        //    /// <summary>
        //    ///   Returns the cached ResourceManager instance used by this class.
        //    /// </summary>
        //    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        //    internal static global::System.Resources.ResourceManager ResourceManager {
        //        get {
        //            if (object.ReferenceEquals(resourceMan, null)) {
        //                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager( <resource group>, typeof(Files).Assembly);
        //                resourceMan = temp;
        //            }
        //            return resourceMan;
        //        }
        //    }
        //    
        //    /// <summary>
        //    ///   Overrides the current thread's CurrentUICulture property for all
        //    ///   resource lookups using this strongly typed resource class.
        //    /// </summary>
        //    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        //    internal static global::System.Globalization.CultureInfo Culture {
        //        get {
        //            return resourceCulture;
        //        }
        //        set {
        //            resourceCulture = value;
        //        }
        //    }
        //    
        //    internal static <SomeType> <SomeName> {
        //        get {
        //            object obj = ResourceManager.GetObject( <resource key>, resourceCulture);
        //            return (<SomeType>)(obj));
        //        }
        //    }
        //
        // For an application that doesn't need to support runtime localization (aka: if resourceCulture is never assigned),
        // we could detect this code pattern and substitute the runtime calls with a compile-time assignment.
        //

        private void Handle_ResourceManagerImpl_GetObject1( string name )
        {
            var md = this.TypeSystem.GetWellKnownMethodNoThrow( name );
            if(md != null)
            {
                foreach(var op in this.CallsDataBase.CallsToMethod( md ))
                {
                    if(op.BasicBlock == null)
                    {
                        //
                        // Deleted call.
                        //
                        continue;
                    }

                    ConstantExpression exRes = GetActualResource( op, 1, 1 );
                    if(exRes != null)
                    {
                        Operator opResourceManager = op.GetPreviousOperator();

                        opResourceManager.Delete();

                        SubstituteGetObjectWithAssignment( op, exRes );
                    }
                }
            }
        }

        private void Handle_ResourceManagerImpl_GetObject2( string name )
        {
            var md = this.TypeSystem.GetWellKnownMethodNoThrow( name );
            if(md != null)
            {
                foreach(var op in this.CallsDataBase.CallsToMethod( md ))
                {
                    ConstantExpression exRes = GetActualResource( op, 1, 2 );
                    if(exRes != null)
                    {
                        Operator opCultureInfo = op.GetPreviousOperator();
                        Operator opResourceManager = opCultureInfo.GetPreviousOperator();

                        opCultureInfo.Delete();
                        opResourceManager.Delete();

                        SubstituteGetObjectWithAssignment( op, exRes );
                    }
                }
            }
        }

        private static void SubstituteGetObjectWithAssignment( CallOperator       op    ,
                                                               ConstantExpression exRes )
        {
            var ex = op.FirstResult;

            if(ex.Type.CanBeAssignedFrom( exRes.Type, null ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Incompatible resource type found at {0}: {1} != {2}", op.BasicBlock.ToShortString(), ex.Type, exRes.Type );
            }

            MethodRepresentation md = op.BasicBlock.Owner.Method;

            op.SubstituteWithOperator( SingleAssignmentOperator.New( op.DebugInfo, ex, exRes ), Operator.SubstitutionFlags.Default );

            md.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.Inline;
            md.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.NoInline;
        }

        private ConstantExpression GetActualResource( CallOperator op                 ,
                                                      int          resourceNameOffset ,
                                                      int          resourceIdOffset   )
        {
            string resourceName = GetResourceName( op, resourceNameOffset );
            if(resourceName != null)
            {
                StaticMethodRepresentation md = FindCallToResourceManager( op, resourceIdOffset, op.BasicBlock.Owner.Method.OwnerType );
                if(md != null)
                {
                    string resourceId = ExtractResourceIdentifier( md );

                    object res = FindResource( resourceId, resourceName );
                    if(res != null)
                    {
                        return this.TypeSystem.CreateConstantFromObject( res );
                    }
                }
            }

            return null;
        }

        private string GetResourceName( CallOperator op    ,
                                        int          index )
        {
            if(op.Arguments.Length > index && op.Arguments[index].Type == this.TypeSystem.WellKnownTypes.System_String)
            {
                var ex = op.Arguments[index] as ConstantExpression;
                if(ex != null)
                {
                    var od = (DataManager.ObjectDescriptor)ex.Value;

                    return (string)od.Source;
                }
            }

            return null;
        }

        private object FindResource( string resourceId   ,
                                     string resourceName )
        {
            foreach(var res in this.TypeSystem.Resources)
            {
                if(res.Name == resourceId)
                {
                    foreach(var pair in res.Values)
                    {
                        if(pair.Key == resourceName)
                        {
                            return pair.Value;
                        }
                    }
                }
            }

            return null;
        }

        private static StaticMethodRepresentation FindCallToResourceManager( Operator           op      ,
                                                                             int                offset  ,
                                                                             TypeRepresentation context )
        {
            while(op != null && offset-- > 0)
            {
                op = op.GetPreviousOperator();
            }

            var call = op as CallOperator;
            if(call != null && call.Results.Length == 1 && call.FirstResult is TemporaryVariableExpression)
            {
                var md = call.TargetMethod as StaticMethodRepresentation;
                if(md != null && md.Name == "get_ResourceManager" && md.OwnerType == context)
                {
                    return md;
                }
            }

            return null;
        }

        private string ExtractResourceIdentifier( StaticMethodRepresentation md )
        {
            string res;

            if(m_lookup.TryGetValue( md, out res ) == false)
            {
                var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                if(cfg != null)
                {
                    var wkt = this.TypeSystem.WellKnownTypes;
                    var td  = wkt.System_Resources_ResourceManager;

                    foreach(var call in cfg.FilterOperators< CallOperator >())
                    {
                        var ctor = call.TargetMethod as ConstructorMethodRepresentation;
                        if(ctor != null && ctor.OwnerType == td)
                        {
                            if(call.Arguments.Length == 3 && call.SecondArgument.Type == wkt.System_String)
                            {
                                var ex = call.SecondArgument as ConstantExpression;
                                if(ex != null)
                                {
                                    var od = (DataManager.ObjectDescriptor)ex.Value;

                                    res = (string)od.Source;

                                    m_lookup[md] = res;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }
    }
}
