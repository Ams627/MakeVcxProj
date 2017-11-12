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
        public bool WholeProgramOptimise { get; set; } = false;
        public string ConfigurationType { get; set; }
    }
    class VcxProj
    {
        private const string _defaultLibs = "kernel32.lib;" +
            "user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;" +
            "oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib";
        private List<Configuration> _configurations = new List<Configuration> {
            new Configuration {Name = "Debug|Win32", IsDebug = true, Is32Bit = true },
            new Configuration {Name = "Release|Win32", IsDebug = false, Is32Bit = true },
            new Configuration {Name = "Debug|x64", IsDebug = true, Is32Bit = false },
            new Configuration {Name = "Release|x64", IsDebug = false, Is32Bit = false }
        };
        private XDocument _xdocProject;
        private XDocument _xdocProjectFilters;

        /// <summary>
        /// Generate a .vcxproj file for Visual Studio 2017 (might work for 2015 - I don't know!)
        /// </summary>
        /// <param name="projectName">Name of the project - THIS IS NOT THE FILENAME OF THE VCXPROJ. Normally it is the project filename without the vcxproj extension</param>
        /// <param name="isDll">true for a DLL, false for an EXE</param>
        /// <param name="isConsole">true for a console program, false for a windows program</param>
        /// <param name="cfiles">a list of C or CPP files to include in the project</param>
        /// <param name="hfiles">a list of header files to include in the project</param>
        /// <param name="includePaths">a list of extra include paths to include in the project</param>
        /// <param name="preProcessorDefs">a list of extra preprocessor definitions</param>
        /// <param name="libs">a list of extra libraries to link (normally .libs)</param>
        /// <param name="moduleDefinitionFile">a single module definition file - normally has an extension .def</param>
        /// <param name="toolsVersion">A string indicating the tools version - e.g 15.0 for Visual Studio 2017</param>
        /// <param name="platformToolset">e.g. v141 for Windows 2017</param>
        /// <param name="windowsTargetPlatformVersion">e.g. 10.0.16299.0 for windows 10</param>
        public VcxProj(
            string projectName,
            bool isDll,
            bool isConsole,
            IEnumerable<string> cfiles, 
            IEnumerable<string> hfiles,
            IEnumerable<string> includePaths,
            IEnumerable<string> preProcessorDefs,
            IEnumerable<string> libs,
            string moduleDefinitionFile,
            string toolsVersion, string platformToolset, string windowsTargetPlatformVersion)
        {
            for (var i = 0; i < _configurations.Count(); ++i)
            {
                _configurations[i].ConfigurationType = isDll ? "DynamicLibrary" : "Application";
                _configurations[i].ComdatFolding = !_configurations[i].IsDebug;
                _configurations[i].WholeProgramOptimise = !_configurations[i].IsDebug;
                _configurations[i].OptimiseReferences = !_configurations[i].IsDebug;
                _configurations[i].Optimise = _configurations[i].IsDebug ? "Disabled" : "MaxSpeed";
            }

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            _xdocProject = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                    new XElement(ns + "Project", new XAttribute("DefaultTargets", "Build"), new XAttribute("ToolsVersion", toolsVersion),
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
                                new XElement(ns + "PreprocessorDefinitions",
                                                (config.IsDebug ? "_DEBUG;" : "NDEBUG;") +
                                                (isConsole ? "_CONSOLE;" : "") +
                                                string.Join(";", preProcessorDefs) + ";%(PreprocessorDefinitions)")),
                            new XElement(ns + "Link",
                                new XElement(ns + "SubSystem", isConsole ? "Console" : "Windows"),
                                new XElement(ns + "ModuleDefinitionFile", moduleDefinitionFile),
                                new XElement(ns + "GenerateDebugInformation", "true"),
                                new XElement(ns + "AdditionalDependencies", $"{string.Join(";", libs)};{_defaultLibs};%(AdditionalDependencies)"),
                                config.ComdatFolding ? new XElement(ns + "EnableCOMDATFolding", "true") : null,
                                config.OptimiseReferences ? new XElement(ns + "OptimizeReferences", "true") : null))),
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

            _xdocProjectFilters = new XDocument(
                    new XDeclaration("1.0", "utf-8", "no"),
                    new XElement(ns + "Project", new XAttribute("DefaultTargets", "Build"), new XAttribute("ToolsVersion", "4.0"),
                        new XElement(ns + "ItemGroup",
                            new XElement(ns + "Filter", new XAttribute("Include", "Source Files"),
                                new XElement(ns + "UniqueIdentifier", "{4FC737F1-C7A5-4376-A066-2A32D752A2FF}"),
                                new XElement(ns + "Extensions", "cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx")),
                            new XElement(ns + "Filter", new XAttribute("Include", "Header Files"),
                                new XElement(ns + "UniqueIdentifier", "{93995380-89BD-4b04-88EB-625FBE52EBFB}"),
                                new XElement(ns + "Extensions", "cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx")),
                            new XElement(ns + "Filter", new XAttribute("Include", "Resource Files"),
                                new XElement(ns + "UniqueIdentifier", "{67DA6AB6-F800-4c08-8B7A-83BB121AAD01}"),
                                new XElement(ns + "Extensions", "cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx")),
                        new XElement(ns + "ItemGroup",
                            new XElement(ns + "ClInclude", new XAttribute("Include", "stdafx.h"),
                                new XElement(ns + "Filter", "Header Files"))),
                        new XElement(ns + "ItemGroup",
                            new XElement(ns + "ClCompile", new XAttribute("Include", "stdafx.cpp"),
                                new XElement(ns + "Filter", "Source Files")))
                                )));

        }

        public void Write(string projectFilename)
        {
            _xdocProject.Save(projectFilename);
            var filterFilename = projectFilename + ".filters";
            _xdocProjectFilters.Save(filterFilename);
        }
    }
}
