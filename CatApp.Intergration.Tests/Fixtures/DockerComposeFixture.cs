using System.Diagnostics;
using CatApp.Intergration.Tests.Configuration;
using CatApp.Intergration.Tests.Helpers;
using CatApp.Shared.Helpers;
using Microsoft.Extensions.Configuration;

public sealed class ComposeCliFixture : IAsyncLifetime
{
    private readonly IConfiguration _configuration;

    //this is our settings ( we take them from the appsettings.test.json -- NOT from the environmental of launchSettings.json
    public DockerSettings DockerSettings;

    public ComposeCliFixture()
    {
        DockerSettings = InitializeSettings();
    }


    public async Task InitializeAsync()
    {
        // a typcal tear up message .....
        Console.WriteLine($"Docker file that will be used at location  ----->  {DockerSettings.ComposeFile} ");
        Console.WriteLine($"Tearing Up  of the dockerized solution will start in  {DockerSettings.WaitBeforeTearUp} ......  ");
        Thread.Sleep(DockerSettings.WaitBeforeTearDownInSeconds);

        //await Run("docker", $"compose -f {DockerSettings.ComposeFile} up --build -d");

        await CommandRunner.RunCommandAsync("docker", $"compose -f \"{DockerSettings.ComposeFile}\" up --build -d");

        /// we give some time for the server to be availiable before start testing .....
        using var http = new HttpClient();
        for (int i = 0; i < DockerSettings.ConnectionAttempts; i++)
        {
            try
            {
                if ((await http.GetAsync($"{DockerSettings.ApiBaseUrl}/swagger")).IsSuccessStatusCode)
                    return;
            }
            catch
            {
                Console.WriteLine($"--- Intergration tests - from  {nameof(ComposeCliFixture)} :");
                Console.WriteLine($"--- Waaitting for server availiability before start testing ..... :");
            }

            await Task.Delay(DockerSettings.DelayInSeconds * 1000);
        }

        throw new InvalidOperationException("catapp‑api did not become healthy in time.");
    }





    public async Task DisposeAsync()
    {
        // another typical message for the tear down....
        Console.WriteLine($"Tearing down our test container in  {DockerSettings.WaitBeforeTearDownInSeconds} ");
        Thread.Sleep(DockerSettings.WaitBeforeTearDownInSeconds);



        await Run("docker", $"compose -f {DockerSettings.ComposeFile} down -v");

    }


    private static async Task Run(string file, string args)
    {
        Thread.Sleep(3000);
        Console.WriteLine($"Dockerazation starts in {3} seconds ....... Mew ! ");

        await DockerHelpers.Run(file, args);




        // var p = Process.Start(new ProcessStartInfo(file, args) { RedirectStandardOutput = true });
        // if (p == null) throw new InvalidOperationException($"failed to start {file}");
        // 
        // await p.WaitForExitAsync();
        // 
        // if (p.ExitCode != 0)
        //     throw new InvalidOperationException($"{file} {args} exited {p.ExitCode}");
    }

    //as is without try or any catch.. i want it to explode if something is missing...
    private DockerSettings InitializeSettings()
    {
        // --- here we load environment variables from launchsettigs.json file
        var cfg = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.test.json", optional: false)
             .Build();

        var docker = cfg.GetSection("Docker").Get<DockerSettings>();

        // i give some defaults here just to exist...
        if (docker.DelayInSeconds <= 0)
            docker.DelayInSeconds = 1;

        if (docker.ConnectionAttempts <= 0)
            docker.ConnectionAttempts = 14;


        // cuurent work dir --->  CatStealingApp\CatApp.Intergration.Tests\bin\Debug\net9.0
        //var currDir = Directory.GetCurrentDirectory();

        // just in case as well ....
        if (!File.Exists(docker.ComposeFile))
        {

            Console.WriteLine($"Docker  - Compose file not exists ... in location supplied at  :  {docker.ComposeFile}");

            //i faced an issue here trying to locate file so if fails we will find it manually....

            Console.WriteLine($"Trying to locate compose file manually starting from  {Directory.GetCurrentDirectory()}");


            var dockerFileName = Path.GetFileName(docker.ComposeFile);
            var locatedFile = DockerHelpers.Find_Docker_Compose_File_Manually(dockerFileName);

            if (!string.IsNullOrEmpty(locatedFile))
                docker.ComposeFile = locatedFile;
            else
                throw new ArgumentException("Docker compose file not supplied or path not exists.... exitig..... ");

        }

        if (string.IsNullOrEmpty(docker.ApiBaseUrl))
        {
            Console.WriteLine($"test base url is not supplied .. exiting .....");
            throw new ArgumentException("test base url is not supplied .. exiting .....");
        }

        return docker;
    }
}
