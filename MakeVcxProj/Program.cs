using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeVcxProj
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var cfiles = new List<string> { "a.c", "b.c" };
                var hfiles = new List<string> { "x.h", "y.h" };
                var predefs = new[] { "_WIN32", "_NODEFAULT" };
                var linklibs = new[] { "ws2_32.lib" };
                var includePaths = new[] { "K:\\include", "Q:\\include" };
                var moddef = "ll32.def";

                // create the Vcxproj file: (it's an XML document):
                var project = new VcxProj("wonk", true, cfiles, hfiles, includePaths, predefs, linklibs, moddef, "15.0", "v141", "10.0.16299.0");

                // write it out:
                project.Write("wonk.vcxproj");
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
