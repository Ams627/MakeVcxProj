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
        public string Optimise { get; set; }
        public bool ComdatFolding { get; set; } = false;
        public bool OptimiseReferences { get; set; } = false;
        public bool WholeProgramOptimise { get; set; }
        public string ConfigurationType { get; set; }
    }
    class VcxProj
    {
        private const string _defaultLibs = "kernel32.lib;" +
            "user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;" +
            "oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib";
        private List<Configuration> _configurations = new List<Configuration> {
            new Configuration {Name = "Debug|Win32", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Release|Win32", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Debug|x64", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"},
            new Configuration {Name = "Release|x64", IsDebug= true, Is32Bit = true, ConfigurationType = "DynamicLibrary"}
        };
        private XDocument _xdocument;
        public VcxProj(
            string projectName,
            bool isDll,
            IEnumerable<string> cfiles, 
            IEnumerable<string> hfiles,
            IEnumerable<string> includePaths,
            IEnumerable<string> preProcessorDefs,
            IEnumerable<string> libs,
            string moduleDefinitionFile,
            string toolsVersion, string platformToolset, string windowsTargetPlatformVersion)
        {
            _configurations.ForEach(x => x.ConfigurationType = isDll ? "DynamicLibrary" : x.ConfigurationType = "Application");

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            _xdocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                    new XElement(ns + "Project", new XAttribute("DefaultTargets", "Build"), new XAttribute("ToolsVersion", toolsVersion),
//                        new XAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003"),
                        new XElement(ns + "ItemGroup", new XAttribute("Label", "ProjectConfigurations"),
                        _configurations.Select(config => new XElement(ns + "ProjectConfiguration", new XAttribute("Include", config.Name),
                            new XElement(ns + "Configuration", config.IsDebug ? "Debug" : "Release"),
                            new XElement(ns + "Platform", config.Is32Bit ? "Win32" : "x64")))),
                    new XElement(ns + "PropertyGroup", new XAttribute("Label", "Globals"),
                        new XElement(ns + "VCProjectVersion", toolsVersion),
                        new XElement(ns + "ProjectGuid", Guid.NewGuid()),
                        new XElement(ns + "Keyword", "Win32Proj"),
                        new XElement(ns + "RootNamespace", projectName),
                        new XElement(ns + "WindowsTargetPlatformVersion", windowsTargetPlatformVersion)),
                    new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.Default.props")),
                    _configurations.Select(config =>
                        new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XAttribute("Label", "Configuration"),
                            new XElement(ns + "ConfigurationType", config.ConfigurationType),
                            new XElement(ns + "UseDebugLibraries", config.IsDebug.ToString().ToLower()),
                            new XElement(ns + "PlatformToolset", platformToolset),
                            new XElement(ns + "CharacterSet", "MultiByte")
                            )),
                        new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.props")),
                        new XElement(ns + "ImportGroup", new XAttribute("Label", "ExtensionSettings")),
                        new XElement(ns + "ImportGroup", new XAttribute("Label", "Shared")),
                        _configurations.Select(config =>
                        new XElement(ns + "ImportGroup", new XAttribute("Label", "PropertySheets"),
                            new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XElement(ns + "Import",
                                new XAttribute("Project", @"$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"),
                                new XAttribute("Condition", @"exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"),
                                new XAttribute("Label", "LocalAppDataPlatform")))),
                        new XElement(ns + "PropertyGroup", new XAttribute("Label", "UserMacros")),
                        _configurations.Select(config =>
                            new XElement(ns + "PropertyGroup", new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config}'"),
                            new XElement(ns + "LinkIncremental", config.LinkIncremental.ToString().ToLower()),
                            includePaths.Count() > 0 ? new XElement(ns + "IncludePath", "" + "$(WindowsSDK_IncludePath);" + string.Join(";", includePaths)) : null)),
                        _configurations.Select(config =>
                        new XElement(ns + "ItemDefinitionGroup", new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|Win32'"),
                            new XElement(ns + "ClCompile",
                                new XElement(ns + "PrecompiledHeader", "NotUsing"),
                                new XElement(ns + "WarningLevel", "Level3"),
                                new XElement(ns + "Optimization", config.Optimise),
                                new XElement(ns + "SDLCheck", "true"),
                                new XElement(ns + "PreprocessorDefinitions", (config.IsDebug ? "_DEBUG" : "") + string.Join(";", preProcessorDefs) + "% (PreprocessorDefinitions)")),
                            new XElement(ns + "Link",
                                new XElement(ns + "SubSystem", "Windows"),
                                new XElement(ns + "ModuleDefinitionFile", moduleDefinitionFile),
                                new XElement(ns + "GenerateDebugInformation", "true"),
                                new XElement(ns + "AdditionalDependencies", $"{string.Join(";", libs)};{_defaultLibs};%(AdditionalDependencies)"),
                                config.ComdatFolding ? new XElement("EnableCOMDATFolding", "true") : null,
                                config.OptimiseReferences ? new XElement("OptimizeReferences", "true") : null))),
                            new XElement(ns + "ItemGroup",
                                hfiles.Select(headerfile => new XElement(ns + "ClInclude", new XAttribute("Include", headerfile)))),
                            new XElement(ns + "ItemGroup",
                                cfiles.Select(sourcefile => new XElement(ns + "ClCompile", new XAttribute("Include", sourcefile))),
                                new XElement(ns + "ClCompile", new XAttribute("Include", "stdafx.cpp"),
                            _configurations.Select(config => new XElement(ns + "PrecompiledHeader",
                                new XAttribute("Condition", $"'$(Configuration)|$(Platform)'=='{config.Name}'"), "Create")))),
                            new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.targets")),
                            new XElement(ns + "ImportGroup", new XAttribute("Label", "ExtensionTargets")))
                            );
            System.Console.WriteLine();
        }

        public void Write(string filename)
        {
            _xdocument.Save(filename);
        }
    }
}
