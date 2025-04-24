using System.Diagnostics;

namespace CatApp.Shared.Helpers
{

    /// <summary>
    /// A helper class to use in docker in case migrations folder of <see cref="Data.DataContext"/> are not 
    /// initally crated in under <see cref="Shared"/> folder location - WHICH are MANDATORY to be present
    /// in order our main app can use it and apply - create the database (this is NOT in the main web api project)
    /// ------ BUT i couldn't find out why was failing in the container during docker compose .... i leave it here for the leagacy
    /// </summary>
    public static class CommandRunner
    {
        public static async Task<string> RunCommandAsync(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,         // e.g., "dotnet", "cmd", "bash"
                Arguments = arguments,       // e.g., "ef migrations add InitialCreate"
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };

            var stdOut = string.Empty;

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    Console.WriteLine(e.Data);      // live stream to VS Output→Tests
                    stdOut += e.Data + Environment.NewLine;
                }
            };

            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                    Console.WriteLine(e.Data);      // stream stderr too
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
                throw new InvalidOperationException(
                    $"{fileName} {arguments} exited with code {proc.ExitCode}");

            return stdOut;
        }
    }
}
