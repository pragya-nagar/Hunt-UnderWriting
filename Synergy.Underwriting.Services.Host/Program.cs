using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Synergy.Common.Aws.Extensions;

namespace Synergy.Underwriting.Services.Host
{
    internal class Program
    {
        public static void Main() => WebHost.CreateDefaultBuilder().UseDefaultSettings<Startup>().Build().Run();
    }
}
