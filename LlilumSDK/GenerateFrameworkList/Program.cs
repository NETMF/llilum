using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace GenerateFrameworkList
{
    /// <summary>Build utility to generate the .NET Multitargeting Frameworklist.xml file from the WIX source</summary>
    /// <remarks>
    /// <para>The FrameworkList.xml file contains information about a framework and it's assemblies for the 
    /// multi-targeting support in Visuals Studio and MSBuild. In particular it contains details of the
    /// assemblies as 'File' elements. Given that the WIX source already lists all the files to go into
    /// the MSI this tool saves on duplication of data (and the potential for errors) by generating the
    /// FrameworkList.xml from the wix source itself, thus acurately reflecting what is actually installed.</para>
    /// <para>This is not doen as an MSBuild task due to the fact that it needs to load the assemblies via
    /// reflection to determine the values of various attributes for the 'generated 'File' elements. Assemblies
    /// loaded via reflection are never unloaded unless the AppDomain they are loaded into is unloaded. Also,
    /// MSBuild uses an optimization that leaves an instance of istelf running under the expectation that,
    /// during a build, it will be needed again, thus eliminating the need to reload and re-jit everything.
    /// This means that the MSBuild task would load the assemblies and then linger around after the build is 
    /// finished holding on to an open file for the loaded assemblies, causing weird issues with "File in use"
    /// types of errors. While it is plausible to write an MSBuild task to run it's core functionality in an
    /// isolated AppDomain, doing this as a seperate exe is simpler and more obvious.
    /// </para>
    /// </remarks>
    class Program
    {
        const string LlilumFrameworkDisplayName = "Llilum 1.0";
        const string LlilumRedistComponentId = "Microsoft-Llilum-CLRCoreComp-1.0";
        const string WixNamespace = "http://schemas.microsoft.com/wix/2006/wi";

        private readonly static XName WixComponentGroupElementName = XName.Get("ComponentGroup", WixNamespace);
        private readonly static XName WixFileElementName = XName.Get("File", WixNamespace);

        /// <summary>Entry point for the application</summary>
        /// <param name="args">Input command line args</param>
        /// <returns>0 on success non zero on error</returns>
        /// <remarks>
        /// Commandline args:
        /// <list type="number">
        /// <item><term>Wix Source</term><description>Wix fragment source file describing the framework assemblies component</description></item>
        /// <item><term>InputPath</term><description>Path to use when finding the assemblies referenced in the Wix Source</description></item>
        /// <item><term>OutputPath</term><description>Path write the generated FrameworkList.xml file into</description></item>
        /// </list>
        /// </remarks>
        static int Main(string[] args)
        {
            string inputXml;
            string outputPath;
            string inputPath;

            if (!ValidateArgs( args, out inputXml, out inputPath, out outputPath ))
            {
                return -1;
            }

            using (var strm = File.OpenText(inputXml))
            {
                var fileListElement = new XElement("FileList"
                                                  , new XAttribute("Name", LlilumFrameworkDisplayName)
                                                  , new XAttribute("Redist", LlilumRedistComponentId)
                                                  , new XAttribute("ToolsVersion", "4.0")
                                                  );

                var outputDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), fileListElement);
                var inputDoc = XDocument.Load(strm);

                // find the files elements in the component group containing all the reference assemblies
                // only use files with the KeyPath == 'yes" as the others are the PDBs.
                var assemblyFiles = from componentGroup in inputDoc.Descendants(WixComponentGroupElementName)
                                    where componentGroup.Attribute("Id")?.Value == "ReferenceAssembliesComponentGroup"
                                    from file in componentGroup.Descendants(WixFileElementName)
                                    where file.Attribute("KeyPath")?.Value == "yes"
                                    select Path.Combine(inputPath, file.Attribute("Name").Value);

                foreach (var assemblyFile in assemblyFiles)
                {
                    try
                    {
                        var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                        AssemblyName name = assembly.GetName();
                        fileListElement.Add(new XElement("File"
                                                        , new XAttribute("AssemblyName", name.Name)
                                                        , new XAttribute("Version", name.Version.ToString())
                                                        , new XAttribute("PublicKeyToken", FormatPublicKeyToken(name))
                                                        , new XAttribute("Culture", name.CultureName)
                                                        , new XAttribute("ProcessorArchitecture", name.ProcessorArchitecture.ToString())
                                                        , new XAttribute("InGac", "false")
                                                        )
                                           );
                    }
                    catch( BadImageFormatException )
                    { /*ignore unmanaged binaries */ }
                    catch( FileLoadException )
                    { /*ignore unloadeable binaries */ }
                }

                outputDoc.Save(Path.Combine(outputPath, "FrameworkList.xml"));
            }

            return 0;
        }

        private static string FormatPublicKeyToken(AssemblyName name)
        {
            var token = name.GetPublicKeyToken();
            if (token == null || token.Length == 0)
                return string.Empty;

            var bldr = new StringBuilder(token.Length * 2);
            for (int i = 0; i < token.Length; i++)
                bldr.AppendFormat("{0:x2}", token[i]);

            return bldr.ToString();
        }

        private static bool ValidateArgs(string[] args, out string inputWixFragment, out string inputPath, out string outputPath )
        {
            if ( args.Length != 3)
            {
                Console.WriteLine("Incorrect number of arguments provided");
                inputWixFragment = null;
                inputPath = null;
                outputPath = null;
                return false;
            }

            inputWixFragment = FixQuotedArg(args[0]);
            inputPath = FixQuotedArg(args[1]);
            outputPath = FixQuotedArg(args[2]);

            if (string.IsNullOrWhiteSpace(inputWixFragment))
            {
                Console.Error.WriteLine("Empty strings for InputWixFragment are not allowed");
                return false;
            }

            if (!File.Exists(inputWixFragment))
            {
                Console.Error.WriteLine("File '{0}' specified in InputWixFragment does not exist", inputWixFragment);
                return false;
            }

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.Error.WriteLine("Empty strings for InputPath are not allowed");
                return false;
            }

            if (!Directory.Exists(inputPath))
            {
                Console.Error.WriteLine("Directory '{0}' specified for InputPath does not exist", inputPath);
                return false;
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Console.Error.WriteLine("Empty strings for OutputPath are not allowed");
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.Error.WriteLine("Directory '{0}' specified for OutputPath does not exist", outputPath);
                return false;
            }

            return true;
        }

        private static string FixQuotedArg(string arg)
        {
            // .NET arg parsing has an unfortunate side effect for paths containing a trailing seperator.
            // On Windows when the arg is quoted (e.g. "c:\path\sub path\" ) the trailing \" is treated 
            // as an escaped " character and the \ is lost. This fixes that case
            if (arg[arg.Length - 1] == '"')
                return arg.Substring(0, arg.Length - 1);
            else
                return arg;
        }
    }
}
