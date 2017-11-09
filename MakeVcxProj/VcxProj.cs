using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MakeVcxProj
{
    class Configuration
    {
        public string Name { get; set; }
        public bool IsDebug { get; set; }
        public bool Is32Bit { get; set; }
        public bool LinkIncremental { get; set; } = false;
        public bool WholeProgramOptimise { get; set; }
        public string ConfigurationType { get; set; }
    }
    class VcxProj
    {
        private List<Configuration> _configurations = new List<Configuration> {
            new Configuration {Name = "Debug|Win32", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Release|Win32", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Debug|x64", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Release|x64", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"}
        };
        private XDocument _xdocument;
        public VcxProj(string projectName, IEnumerable<string> cfiles,  IEnumerable<string> hfiles, IEnumerable<string> includePaths, string toolsVersion, string platformToolset, string windowsTargetPlatformVersion)
        {
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            _xdocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                    new XElement(ns + "Project", new XAttribute("DefaultTargets", "Build"), new XAttribute("ToolsVersion", toolsVersion),
                        new XAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003"),
                        new XElement("ItemGroup", new XAttribute("Label", "ProjectConfigurations"),
                        _configurations.Select(config => new XElement("ProjectConfiguration", new XAttribute("Include", config.Name),
                            new XElement("Configuration", config.IsDebug ? "Debug" : "Release"),
                            new XElement("Platform", config.Is32Bit ? "Win32" : "x64")))),
                    new XElement("PropertyGroup", new XAttribute("Label", "Globals"),
                        new XElement("VCProjectVersion", toolsVersion),
                        new XElement("ProjectGuid", Guid.NewGuid()),
                        new XElement("Keyword", "Win32Proj"),
                        new XElement("RootNamespace", projectName),
                        new XElement("WindowsTargetPlatformVersion", windowsTargetPlatformVersion)),
                    new XElement("Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.Default.props")),
                    _configurations.Select(config =>
                        new XElement("PropertyGroup",
                            new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XAttribute("Label", "Configuration"),
                            new XElement("ConfigurationType", config.ConfigurationType),
                            new XElement("UseDebugLibraries", config.IsDebug.ToString().ToLower()),
                            new XElement("PlatformToolset", platformToolset),
                            new XElement("CharacterSet", "Unicode")
                            )),
                        new XElement("Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.props")),
                        new XElement("ImportGroup", new XAttribute("Label", "ExtensionSettings")),
                        new XElement("ImportGroup", new XAttribute("Label", "Shared")),
                        _configurations.Select(config =>
                        new XElement("ImportGroup", new XAttribute("Label", "PropertySheets"),
                            new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XElement("Import",
                                new XAttribute("Project", @"$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"),
                                new XAttribute("Condition", @"exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"),
                                new XAttribute("Label", "LocalAppDataPlatform"))))),
                        new XElement("PropertyGroup", new XAttribute("Label", "UserMacros")),
                        _configurations.Select(config =>
                            new XElement("PropertyGroup", new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XElement("LinkIncremental", config.LinkIncremental.ToString().ToLower()),
                            includePaths.Count() > 0 ? new XElement("IncludePath", "" + "$(WindowsSDK_IncludePath);" + string.Join(";", includePaths)) : null)),
                        _configurations.Select(config =>
                        new XElement("ItemDefinitionGroup", new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|Win32'"),
                            new XElement("ClCompile", new XElement("PrecompiledHeader", "Not"






        )
                            );
            System.Console.WriteLine();
        }

        public void Write(string filename)
        {
            _xdocument.Save(filename);
        }
    }
}
