using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Zelig.Test
{
    public class Test
    {
        private readonly string m_name;
        
        public Test()
        {
            this.m_name = this.GetType().Name;
        }
        public Test(String name)
        {
            this.m_name = name;
        }

        public String Name
        {
            get { return m_name; }
        }

        public virtual Result Run( string[] args )
        {
            TestConsole.WriteLine( String.Format("test '{0}' running...", this.Name ) );

            return Result.Success;
        }
    }
}
