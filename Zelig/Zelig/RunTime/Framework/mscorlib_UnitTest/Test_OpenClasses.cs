
namespace mscorlib_UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime;

    public class BaseClassToExtend1
    {
        public static int s_b = 1;

        int m_a;

        public BaseClassToExtend1( int a )
        {
            m_a = a;
        }

        private int Test1( int a )
        {
            return a + 1;
        }

        public int Test2( int b )
        {
            return b;
        }
    }

    [ExtendClass(typeof(BaseClassToExtend1))]
    class ExtensionTest1
    {
        public static int s_c = 3;

        int m_b;

        [MergeWithTargetImplementation]
        public ExtensionTest1( int a )
        {
            m_b = a;
        }

        [AliasForBaseMethod( "Test1" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private extern int BaseTest1( int a );

        private int Test1( int a )
        {
            return a + 2;
        }

        public int SubTest1( int a )
        {
            return BaseTest1( a + 1 );
        }
    }

    //--//

    class TargetForInjection
    {
        public int EmptyMethod( int a, int b, ref int c )
        {
            return a + b;
        }
    }

    [ExtendClass(typeof(TargetForInjection), NoConstructors=true)]
    class SourceForInjection1
    {
        [InjectAtEntryPoint]
        public void EmptyMethod( int a, int b, ref int c )
        {
            c = a - b;
        }

        [InjectAtExitPoint]
        public int EmptyMethod( int a, int b, ref int c, int res )
        {
            return res * a;
        }
    }

    [ExtendClass(typeof(TargetForInjection), NoConstructors=true, ProcessAfter=typeof(SourceForInjection1))]
    class SourceForInjection2
    {
        [InjectAtEntryPoint]
        public void EmptyMethod( int a, int b, ref int c )
        {
            c = c * 2;
        }

        [InjectAtExitPoint]
        public int EmptyMethod( int a, int b, ref int c, int res )
        {
            return res - 10;
        }
    }

    public class BaseGenericClassToExtend< T >
    {
        [NoInline]
        public T Test1( T a ,
                        T b )
        {
            return a;
        }
    }

    [ExtendClass(typeof(BaseGenericClassToExtend<>), NoConstructors=true)]
    class GenericExtensionTest<T>
    {
        [NoInline]
        public T Test1( T a ,
                        T b )
        {
            return b;
        }
    }

    public class GenericClassTest : BaseGenericClassToExtend<int>
    {
    }
}
