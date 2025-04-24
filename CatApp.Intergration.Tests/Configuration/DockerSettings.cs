using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatApp.Intergration.Tests.Configuration
{
    public class DockerSettings
    {
        public string ComposeFile { get; set; } 
        public string ApiBaseUrl { get; set; } 
        public int ConnectionAttempts { get; set; } = 60;
        public int DelayInSeconds { get; set; } = 20;

        public int WaitBeforeTearUp { get; set; } = 3;
        public int WaitBeforeTearDownInSeconds { get; set; } = 3;

    }
}
