using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Zelig.Test
{
    public class TestBase
    {
        private readonly string m_name;
        
        public TestBase()
        {
            this.m_name = this.GetType().Name;
        }
        public TestBase(String name)
        {
            this.m_name = name;
        }

        public String Name
        {
            get { return m_name; }
        }

        public virtual TestResult Run( string[] args )
        {
            TestConsole.WriteLine( String.Format("test '{0}' running...", this.Name ) );

            return TestResult.Pass;
        }
    }
}
