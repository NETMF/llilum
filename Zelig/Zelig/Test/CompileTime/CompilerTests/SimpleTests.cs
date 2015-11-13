using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zelig.FrontEnd;
using System.IO;

namespace CompilerTests
{
    [TestClass]
    public class SimpleTests
    {
        private const string configPath = @"..\..\..\..\Zelig\CompileTime\CodeGenerator\FrontEnd\mbed_simple_LPC1768.FrontEndConfig";
        private const string irPath = @"..\..\..\..\LLVM2IR_results\mbed\simple\Microsoft.Zelig.Test.mbed.Simple.ZeligIR";

        private static bool compiled = false;

        [TestMethod]
        public void TestCompilation()
        {
            Compile();
        }

        [TestMethod]
        public void TestZeligIR()
        {
            CompileIfNeeded();
            Assert.IsTrue(File.Exists(irPath));
            string str = File.ReadAllText(irPath);

            //Check for Program type
            Assert.IsTrue(str.Contains(@"Type: ConcreteReferenceTypeRepresentation(Microsoft.Zelig.Test.mbed.Simple.Program)"));

            //Check for Main method
            Assert.IsTrue(str.Contains(@"Method StaticMethodRepresentation(void Microsoft.Zelig.Test.mbed.Simple.Program::Main())"));
        }

        private void CompileIfNeeded()
        {
            if (!compiled)
                Compile();
        }

        private void Compile()
        {
            Bench.RunBench(new string[] { "-cfg", configPath });
            compiled = true;
        }
    }
}
