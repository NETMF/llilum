//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using              Microsoft.Zelig.MetaData;
    using Importer   = Microsoft.Zelig.MetaData.Importer;
    using Normalized = Microsoft.Zelig.MetaData.Normalized;
    using IR         = Microsoft.Zelig.CodeGeneration.IR;
    using TS         = Microsoft.Zelig.Runtime.TypeSystem;

    class GenericInstantiationClosureTester : BaseTester,
        Microsoft.Zelig.MetaData.IMetaDataResolverHelper
    {
        //
        // Constructor Methods
        //

        private GenericInstantiationClosureTester()
        {
            m_metaDataResolverHelper = this;

            Initialize();

            //AddSearchDirectory( @"%WINDIR%\Microsoft.NET\Framework\v1.1.4322" );
            //AddSearchDirectory( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727" );
        }

        public static void Run( string[] args )
        {
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Pass\Pass.exe"                              , true  );
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Fail_Field\Fail_Field.exe"                  , false );
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Fail_GenericMethod1\Fail_GenericMethod1.exe", false );
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Fail_GenericMethod2\Fail_GenericMethod2.exe", false );
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Fail_Inheritance\Fail_Inheritance.exe"      , false );
            Run( @"%DEPOTROOT%\Zelig\Test\GenericInstantiationClosure\Fail_Method\Fail_Method.exe"                , false );
        }

        private static void Run( string file        ,
                                 bool   fShouldPass )
        {
            Exception result = null;

            try
            {
                GenericInstantiationClosureTester pThis;

                pThis = new GenericInstantiationClosureTester();
                pThis.LoadAndResolve( Expand( file ) );
                pThis.ConvertToIR();
            }
            catch(Exception e)
            {
                result = e;
            }

            if((result == null) == fShouldPass)
            {
                Console.WriteLine( "Passed test for '{0}'", file );
            }
            else
            {
                Console.WriteLine( "Failed test for '{0}'", file   );
                Console.WriteLine( "    {0}"              , result );
            }
        }

        //--//

        Importer.MetaData IMetaDataResolverHelper.ResolveAssemblyReference( string          name ,
                                                                            MetaDataVersion ver  )
        {
            return InnerResolveAssemblyReference( name, ver );
        }
    }
}
