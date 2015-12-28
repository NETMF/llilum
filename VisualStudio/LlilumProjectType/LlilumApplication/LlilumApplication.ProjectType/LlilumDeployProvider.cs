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

        [Import]
        private IThreadHandling ThreadHandling { get; set; }

        private async Task RunDeployTool(CancellationToken cancellationToken, TextWriter outputPaneWriter, string deployToolPath, string deployToolArgs)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = deployToolPath;
            start.Arguments = deployToolArgs;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            using (Process process = Process.Start(start))
            using (StreamReader stdOut = process.StandardOutput)
            using (StreamReader stdErr = process.StandardError)
            {
                try
                {
                    var stdoutTask = Task.Run(() => SendProcessOutputToPaneAsync(outputPaneWriter, stdOut, cancellationToken));
                    var stderrTask = Task.Run(() => SendProcessOutputToPaneAsync(outputPaneWriter, stdErr, cancellationToken));

                    await Task.WhenAll(stdoutTask, stderrTask);
                    if (process.ExitCode != 0)
                        throw new ApplicationException($"Flash tool failed with exit code {process.ExitCode}");
                }
                catch (OperationCanceledException)
                {
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                    }
                }
            }
        }

        public async Task DeployAsync( CancellationToken cancellationToken, TextWriter outputPaneWriter )
        {
            // Kill all instances of PyOcd and OpenOcd because they affect the flash-tool and debugger
            await LlilumHelpers.TryKillPyocdAsync();
            await LlilumHelpers.TryKillOpenOcdAsync();

            var properties = await Properties.GetLlilumDebuggerPropertiesAsync();
            var deployTool = await properties.LlilumDeployTool.GetEvaluatedValueAtEndAsync();

            string deployToolPath = string.Empty;
            string binaryPath = string.Empty;

            if (string.Compare(deployTool, "pyocdflashtool", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Deploy with pyOCD Flash Tool
                deployToolPath = await properties.LlilumFlashToolPath.GetEvaluatedValueAtEndAsync();
                deployToolPath = Path.GetFullPath(deployToolPath.Trim('"'));
                if (!File.Exists(deployToolPath))
                {
                    var msg = $"Flash programming tool not found: '{deployToolPath}'";
                    outputPaneWriter.Write(msg);
                    throw new FileNotFoundException(msg);
                }

            }
            else if (string.Compare(deployTool, "stlinkutility", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Deploy with ST-Link Utility
                deployToolPath = await properties.LlilumSTLinkUtilityPath.GetEvaluatedValueAtEndAsync();
                deployToolPath = Path.GetFullPath(deployToolPath.Trim('"'));
                if (!File.Exists(deployToolPath))
                {
                    var msg = $"STLink Utility not found: '{deployToolPath}'";
                    outputPaneWriter.Write(msg);
                    throw new FileNotFoundException(msg);
                }

            }
            else if(string.Compare(deployTool, "copytodrive", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Deploy by copying to the drive. Do nothing
            }
            else
            {
                // Deploy with GDB command, or Do Not Deploy
                return;
            }
            
            binaryPath = await properties.LlilumOutputBin.GetEvaluatedValueAtEndAsync( );
            binaryPath = Path.GetFullPath( binaryPath.Trim( '"' ) );
            
            if( !File.Exists( binaryPath ) )
            {
                var msg = $"Flash binary file not found: '{binaryPath}'";
                outputPaneWriter.Write( msg );
                throw new FileNotFoundException( msg );
            }

            if (string.Compare(deployTool, "pyocdflashtool", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Deploy with pyOCD Flash Tool
                string flashToolArgs = await properties.LlilumFlashToolArgs.GetEvaluatedValueAtEndAsync();
                flashToolArgs = $"{EnsureQuotedPathIfNeeded(binaryPath)} {flashToolArgs}";

                await RunDeployTool(cancellationToken, outputPaneWriter, deployToolPath, flashToolArgs);
            }
            else if (string.Compare(deployTool, "stlinkutility", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Deploy with ST-Link Utility
                string stlinkConnectArgs = await properties.LlilumSTLinkUtilityConnectArgs.GetEvaluatedValueAtEndAsync();
                string stlinkEraseArgs = await properties.LlilumSTLinkUtilityEraseArgs.GetEvaluatedValueAtEndAsync();
                string stlinkProgramArgs = await properties.LlilumSTLinkUtilityProgramArgs.GetEvaluatedValueAtEndAsync();
                string stlinkDeployArgs = stlinkConnectArgs + " " + stlinkEraseArgs + " " + stlinkProgramArgs;

                await RunDeployTool(cancellationToken, outputPaneWriter, deployToolPath, stlinkDeployArgs);
            }
            else if (string.Compare(deployTool, "copytodrive", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Copy to the specified drive
                string drive = await properties.LlilumDriveToCopyTo.GetEvaluatedValueAtEndAsync();
                drive = drive.Trim();

                if(string.IsNullOrWhiteSpace(drive))
                {
                    throw new ArgumentNullException("Drive not specified");
                }

                // Get the properly formatted string for the drive letter
                DriveInfo driveInfo = new DriveInfo(drive);
                drive = driveInfo.RootDirectory.FullName;

                if (!Directory.Exists(drive))
                {
                    throw new DriveNotFoundException("The drive specified could not be located");
                }

                string[] binaryPathArr = binaryPath.Split('\\');
                string binaryName = binaryPathArr[binaryPathArr.Length - 1];
                string destFile = System.IO.Path.Combine(drive, binaryName);

                // Allow the copy to fail, and throw an exception on its own
                File.Copy(binaryPath, destFile, true);
            }
        }

        // Crude but effective ensurance of quoted string when a path contains spaces
        // only runs once on deploy so not particularly perf critical
        private static string EnsureQuotedPathIfNeeded( string path )
        {
            if( string.IsNullOrEmpty( path ) )
                return path;

            if( path[ 0 ] == '"' && path[ path.Length - 1 ] == '"' )
                return path;

            if( !path.Contains( " " ) )
                return path;

            return $"\"{path}\"";
        }

        private async Task SendProcessOutputToPaneAsync( TextWriter outputPaneWriter, StreamReader strm, CancellationToken cancellationToken )
        {
            while( !strm.EndOfStream && !cancellationToken.IsCancellationRequested )
            {
                outputPaneWriter.Write( await strm.ReadLineAsync( ) );
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