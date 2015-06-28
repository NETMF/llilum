//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class TypeSystemForIR : TypeSystem
    {
        //
        // Constructor Methods
        //

        public TypeSystemForIR( IEnvironmentProvider env ) : base( env )
        {
        }

        //
        // Helper Methods
        //

        public ControlFlowGraphState CreateControlFlowGraphState( MethodRepresentation md )
        {
            VariableExpression[] arguments;
            VariableExpression[] locals;

            return CreateControlFlowGraphState( md, null, null, out arguments, out locals );
        }

        public abstract ControlFlowGraphState CreateControlFlowGraphState(     MethodRepresentation md            ,
                                                                               TypeRepresentation[] localVars     ,
                                                                               string[]             localVarNames ,
                                                                           out VariableExpression[] arguments     ,
                                                                           out VariableExpression[] locals        );

        public abstract ConstantExpression CreateNullPointer( TypeRepresentation td );

        public abstract ConstantExpression CreateConstant( TypeRepresentation td    ,
                                                           object             value );

        public abstract ConstantExpression CreateConstant( int value );

        public abstract ConstantExpression CreateConstant( uint value );

        public abstract ConstantExpression CreateConstantFromObject( object value );

        public abstract Expression[] CreateConstantsFromObjects( params object[] array );

        public abstract ConstantExpression CreateConstantForType( VTable vTable );

        public abstract ConstantExpression CreateConstantForTypeSize( TypeRepresentation td );

        public abstract ConstantExpression CreateConstantForFieldOffset( FieldRepresentation fd );

        public abstract Expression CreateRuntimeHandle( object o );

        public abstract Annotation CreateUniqueAnnotation( Annotation an );

        public abstract Expression[] AddTypePointerToArgumentsOfStaticMethod(        MethodRepresentation md  ,
                                                                              params Expression[]         rhs );
    }
}
