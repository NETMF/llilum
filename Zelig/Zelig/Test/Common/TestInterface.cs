using System;

namespace Microsoft.Zelig.Test
{
    public enum InitializeResult
    {
        ReadyToGo,
        Skip,
    }

    public interface ITestInterface
    {
        [SetUp]
        InitializeResult Initialize();

        [TearDown]
        void CleanUp();
    }
}
