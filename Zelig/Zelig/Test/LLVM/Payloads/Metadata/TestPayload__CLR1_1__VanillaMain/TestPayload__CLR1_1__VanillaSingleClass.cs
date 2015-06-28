using System;

namespace Microsoft.Zelig.Test
{
    namespace Payload
    {
        public class TestPayload__CLR1_1__VanillaSingleClass
        {
            public static readonly int sIntegerReadonlyStaticValue = 43;

            //--//

            private const int cIntegerConstValue = 42;

            //--//

            private int m_IntegerPrivateMember;

            //--//

            public TestPayload__CLR1_1__VanillaSingleClass(int val)
            {
                m_IntegerPrivateMember = val;
            }

            public TestPayload__CLR1_1__VanillaSingleClass()
                : this(cIntegerConstValue)
            {
            }

            public int GetValue()
            {
                return m_IntegerPrivateMember;
            }

            public int GetDefaultConstInitializer()
            {
                return cIntegerConstValue;
            }

            public int GetDefaultStaticReadonlyInitializer()
            {
                return sIntegerReadonlyStaticValue;
            }

            public int IntegerGetter
            {
                get
                {
                    return m_IntegerPrivateMember;
                }
            }

            public int SwapInitializer()
            {
                int current = m_IntegerPrivateMember;

                if(m_IntegerPrivateMember == cIntegerConstValue)
                {
                    m_IntegerPrivateMember = sIntegerReadonlyStaticValue;
                }
                else
                {
                    m_IntegerPrivateMember = cIntegerConstValue;
                }

                return current;
            }
        }
    }
}
