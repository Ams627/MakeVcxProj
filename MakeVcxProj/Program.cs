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
                var project = new VcxProj("wonk", cfiles, hfiles, "15.0", "v141", "10.0.16299.0");
                project.Write("temp.v1");
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
