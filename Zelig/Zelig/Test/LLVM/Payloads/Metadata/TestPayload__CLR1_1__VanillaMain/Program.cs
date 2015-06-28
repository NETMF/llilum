using System;

namespace Microsoft.Zelig.Test
{
    namespace Payload
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var p = new TestPayload__CLR1_1__VanillaSingleClass();

                int val1 = p.IntegerGetter;

                p.SwapInitializer();

                int val2 = p.IntegerGetter;

                if (val1 == val2) 
                {
                    // no code
                }
                else 
                {
                    p.SwapInitializer();
                }
            }
        }
    }
}
