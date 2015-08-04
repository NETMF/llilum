using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;

namespace LlilumApplication
{
    [ExportDebugger(LlilumDebugger.SchemaName)]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    public class LlilumDebuggerLaunchProvider : DebugLaunchProviderBase
    {
        private static readonly string DebuggerOptionsFormat = @"<LocalLaunchOptions xmlns=""http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014""
                                                                   MIDebuggerPath=""{0}\debug.bat""
                                                                   MIDebuggerServerAddress="":3333""
                                                                   TargetArchitecture=""arm""/>";

        // TODO: Specify the assembly full name here
        [ExportPropertyXamlRuleDefinition("LlilumApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9be6e469bc4921f1", "XamlRuleToCode:LlilumDebugger.xaml", "Project")]
        [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
        private object DebuggerXaml { get { throw new NotImplementedException(); } }

        [ImportingConstructor]
        public LlilumDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        /// <summary>
        /// Gets project properties that the debugger needs to launch.
        /// </summary>
        [Import]
        private ProjectProperties DebuggerProperties { get; set; }

        public override async Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            var properties = await this.DebuggerProperties.GetLlilumDebuggerPropertiesAsync();
            string commandValue = await properties.LlilumDebuggerCommand.GetEvaluatedValueAtEndAsync();
            return !string.IsNullOrEmpty(commandValue);
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);

            // The properties that are available via DebuggerProperties are determined by the property XAML files in your project.
            var debuggerProperties = await this.DebuggerProperties.GetLlilumDebuggerPropertiesAsync();
            var dir = await debuggerProperties.LlilumDebuggerWorkingDirectory.GetEvaluatedValueAtEndAsync();
            settings.CurrentDirectory = dir;
            settings.Options = string.Format(DebuggerOptionsFormat, dir);
            settings.Executable = await debuggerProperties.LlilumDebuggerCommand.GetEvaluatedValueAtEndAsync();
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.LaunchDebugEngineGuid = new Guid(Microsoft.MIDebugEngine.EngineConstants.EngineId);

            return new IDebugLaunchSettings[] { settings };
        }
    }
}
