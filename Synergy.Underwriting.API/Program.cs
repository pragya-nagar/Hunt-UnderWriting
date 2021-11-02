using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Synergy.Common.Aws.Extensions;

namespace Synergy.Underwriting.API
{
    public static class Program
    {
      public static void Main(string[] args) => WebHost.CreateDefaultBuilder(args).UseDefaultSettings<Startup>().Build().Run();
    }
}
