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
        /// <summary>
        /// Provides access to the project's properties.
        /// </summary>
        [Import]
        private ProjectProperties Properties { get; set; }

        public async Task DeployAsync(CancellationToken cancellationToken, TextWriter outputPaneWriter)
        {
            var properties = await this.Properties.GetLlilumDebuggerPropertiesAsync();
            var deployWithFlashTool = await properties.LlilumUseFlashTool.GetEvaluatedValueAtEndAsync();

            if (string.Compare(deployWithFlashTool, "true", true) == 0)
            {
                string flashToolPath = await properties.LlilumFlashToolPath.GetEvaluatedValueAtEndAsync();
                string flashToolArgs = await properties.LlilumFlashToolArgs.GetEvaluatedValueAtEndAsync();
                string binaryPath = await properties.LlilumOutputBin.GetEvaluatedValueAtEndAsync();

                if(!string.IsNullOrEmpty(flashToolPath))
                {
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = flashToolPath;
                    start.Arguments = string.Format("{0} {1}", binaryPath, flashToolArgs);
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