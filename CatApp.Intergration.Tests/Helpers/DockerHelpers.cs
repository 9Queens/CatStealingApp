using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatApp.Intergration.Tests.Helpers
{

    /// <summary>
    /// Docker helper class for our test solution
    /// </summary>
    public static class DockerHelpers
    {

        public static async Task Run(string file, string args)
        {
            var psi = new ProcessStartInfo(file, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };


            using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
 
            //here wer forword every build command in the console
            p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            await p.WaitForExitAsync();

            if (p.ExitCode != 0)
                throw new InvalidOperationException($"{file} {args} exited {p.ExitCode}");
        }


        /// <summary>
        /// We faced issues when we tried to run docker-compose.yaml file  (hardcoded path from settings)
        /// Thus this function will try to find the correct file for us and return it.
        /// 
        /// -----------------------------> IF NOT a filename path is given we will use the standard default name
        /// -----------------------------> docker-compose.yaml
        /// </summary>
        /// <param name="dockerCompose_filename"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string Find_Docker_Compose_File_Manually(string dockerCompose_filename= "docker-compose.yaml")
        {

            var dir = AppContext.BaseDirectory;       
            while (dir is not null)
            {
                var candidate = Path.Combine(dir, dockerCompose_filename);
                if (File.Exists(candidate))
                    return candidate;         
                dir = Path.GetDirectoryName(dir);  
            }

            throw new FileNotFoundException(
                $"{dockerCompose_filename} not found while walking up from {AppContext.BaseDirectory}");

        }
    }
}
