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

        public async Task DeployAsync( CancellationToken cancellationToken, TextWriter outputPaneWriter )
        {
            // Kill all instances of PyOcd because they affect the flash-tool and debugger
            await LlilumHelpers.TryKillPyocdAsync();

            var properties = await Properties.GetLlilumDebuggerPropertiesAsync();
            var deployWithFlashTool = await properties.LlilumUseFlashTool.GetEvaluatedValueAtEndAsync();

            if( string.Compare( deployWithFlashTool, "true", StringComparison.OrdinalIgnoreCase ) != 0 )
                return;

            string flashToolPath = await properties.LlilumFlashToolPath.GetEvaluatedValueAtEndAsync( );
            flashToolPath = Path.GetFullPath( flashToolPath.Trim( '"' ) );

            string binaryPath = await properties.LlilumOutputBin.GetEvaluatedValueAtEndAsync( );
            binaryPath = Path.GetFullPath( binaryPath.Trim( '"' ) );

            string flashToolArgs = await properties.LlilumFlashToolArgs.GetEvaluatedValueAtEndAsync( );

            if( !File.Exists( flashToolPath ) )
            {
                var msg = $"Flash programming tool not found: '{flashToolPath}'";
                outputPaneWriter.Write( msg );
                throw new FileNotFoundException( msg );
            }

            if( !File.Exists( binaryPath ) )
            {
                var msg = $"Flash binary file not found: '{binaryPath}'";
                outputPaneWriter.Write( msg );
                throw new FileNotFoundException( msg );
            }

            ProcessStartInfo start = new ProcessStartInfo( );
            start.FileName = flashToolPath;
            start.Arguments = $"{EnsureQuotedPathIfNeeded( binaryPath )} {flashToolArgs}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            using( Process process = Process.Start( start ) )
            using( StreamReader stdOut = process.StandardOutput )
            using( StreamReader stdErr = process.StandardError )
            {
                var stdoutTask = Task.Run( ( ) => SendProcessOutputToPaneAsync( outputPaneWriter, stdOut, cancellationToken ) );
                var stderrTask = Task.Run( ( ) => SendProcessOutputToPaneAsync( outputPaneWriter, stdErr, cancellationToken ) );

                await Task.WhenAll( stdoutTask, stderrTask );
                if( process.ExitCode != 0 )
                    throw new ApplicationException( $"Flash tool failed with exit code {process.ExitCode}" );
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