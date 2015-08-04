using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace LlilumApplication
{
    [Export(typeof(IDeployProvider))]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    internal class LlilumDeployProvider : IDeployProvider
    {
        private static readonly string FlashToolFormat = "{0}tools\\flash_tool.exe";
        /// <summary>
        /// Provides access to the project's properties.
        /// </summary>
        [Import]
        private ProjectProperties Properties { get; set; }

        public async Task DeployAsync(CancellationToken cancellationToken, TextWriter outputPaneWriter)
        {
            var properties = await this.Properties.GetLlilumDebuggerPropertiesAsync();
            var sdkPath = await properties.LlilumSDKPath.GetEvaluatedValueAtEndAsync();
            string flashToolPath = string.Format(FlashToolFormat, sdkPath);
            string binaryPath = await properties.LlilumOutputBin.GetEvaluatedValueAtEndAsync();

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = flashToolPath;
            start.Arguments = string.Format("{0}", binaryPath);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    outputPaneWriter.Write(result);
                }
            }
        }

        public bool IsDeploySupported
        {
            get { return true; }
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }
    }
}