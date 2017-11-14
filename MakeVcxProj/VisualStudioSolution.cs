using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MakeVcxProj
{
    partial class VisualStudioSolution
    {
        private class Project
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public Guid ProjectGuid { get; set; }
            public Guid ProjectTypeGuid { get; set; }
        }
        public static class ProjectTypeGuids
        {
            public static Guid SolutionFolder = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
            public static Guid Test = new Guid("3AC096D0-A1C2-E12C-1390-A8335801FDAB");
            public static Guid WCF = new Guid("3D9AD99F-2412-4246-B90B-4EAA41C64699");
            public static Guid WPF = new Guid("60DC8134-EBA5-43B8-BCC9-BB4BC16C2548");
            public static Guid CPP = new Guid("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
            public static Guid JSharp = new Guid("E6FDF86B-F3D1-11D4-8576-0002A516ECE8");
            public static Guid XamarinAndroid = new Guid("EFBA0AD7-5A72-4C68-AF49-83D382785DCF");
            public static Guid FSharp = new Guid("F2A71F9B-5D33-465A-A702-920D77279786");
            public static Guid CSharp = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
        }

        private readonly string _solutionFilename;
        private readonly string _formatVersion;
        private readonly string _visualStudioVersion;

        private List<Project> _projects = new List<Project>();

        /// <summary>
        /// Initialise an instance of Visual Studio Solution.
        /// </summary>
        /// <param name="name">Solution name</param>
        /// <param name="formatVersion">e.g. 14.0 for Visual Studio 2017</param>
        /// <param name="visualStudioVersion">e.g. 14.0.25429.0 for Visual Studio 2017</param>
        public VisualStudioSolution(string name, string formatVersion = "14.0", string visualStudioVersion="14.0.25429.0")
        {
            _solutionFilename = name;
            _formatVersion = formatVersion;
            _visualStudioVersion = visualStudioVersion;
        }

        /// <summary>
        /// Add a project to the solution.
        /// </summary>
        /// <param name="projectName">Name of project - not a filename or path - just the pure name</param>
        /// <param name="projectPath">relative path to VCXProj - e.g. wonk\wonk.vcxproj</param>
        /// <param name="projectGuid">The Guid returned by the project creator.</param>
        /// <param name="projectTypeGuid">A Guid indicating the project type - e.g. VisualStudioSolution.ProjectTypeGuids.CPP</param>
        public void AddProject(string projectName, string projectPath, Guid projectGuid, Guid projectTypeGuid)
        {
            var project = new Project {Name = projectName, Path = projectPath, ProjectGuid = projectGuid, ProjectTypeGuid = projectTypeGuid };
            _projects.Add(project);
        }

        public void Write()
        {
            // this array needs to be permuted with the project arrays:
            var configs = new []
            {
                "Debug|x64.ActiveCfg = Debug|x64",
                "Debug|x64.Build.0 = Debug|x64",
                "Debug|x86.ActiveCfg = Debug|Win32",
                "Debug|x86.Build.0 = Debug|Win32",
                "Release|x64.ActiveCfg = Release|x64",
                "Release|x64.Build.0 = Release|x64",
                "Release|x86.ActiveCfg = Release|Win32",
                "Release|x86.Build.0 = Release|Win32"
            };

            using (var file = new StreamWriter(_solutionFilename))
            {
                file.WriteLine($"\uFEFFMicrosoft Visual Studio Solution File, Format Version {_formatVersion}");
                file.WriteLine("# Visual Studio 14");
                file.WriteLine($"VisualStudioVersion = {_visualStudioVersion}");
                var projectsSection = string.Join("\n", _projects.Select(
                    p=>$"Project(\"{{{p.ProjectTypeGuid}}}\") = \"{p.Name}\", \"{p.Path}\", \"{{{p.ProjectGuid}}}\"\nEndProject"));
                file.WriteLine(projectsSection);
                file.WriteLine("GlobalSection(SolutionConfigurationPlatforms) = preSolution");
                file.WriteLine("	Debug|x64 = Debug|x64");
                file.WriteLine("	Debug|x86 = Debug|x86");
                file.WriteLine("	Release|x64 = Release|x64");
                file.WriteLine("	Release|x86 = Release|x86");
                file.WriteLine("EndGlobalSection");
                file.WriteLine("	GlobalSection(ProjectConfigurationPlatforms) = postSolution");

                var globalSection = _projects.Select(p => string.Join("\n", configs.Select(c => $"                {{{p.ProjectGuid}}}.{c}")));
                var g = string.Join("\n", globalSection);
                file.WriteLine(g);
                file.WriteLine("	EndGlobalSection");

                file.WriteLine("        GlobalSection(SolutionProperties) = preSolution");
                file.WriteLine("                HideSolutionNode = FALSE");
                file.WriteLine("        EndGlobalSection");
                file.WriteLine("        GlobalSection(ExtensibilityGlobals) = postSolution");
                file.WriteLine($"                SolutionGuid = {{{Guid.NewGuid()}}}");
                file.WriteLine("        EndGlobalSection");
                file.WriteLine("EndGlobal");
            }
        }
    }
}
