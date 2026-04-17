using Autofac;
using SophiaWindowsService.Infrastructure;
using SophiaWindowsService.Infrastructure.Services;
using System.ServiceProcess;

namespace SophiaWindowsService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var builder = new ContainerBuilder();

            builder.AddInfrastructure();

            using (var container = builder.Build())
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    ServiceBase[] servicesToRun =
                    {
                        scope.Resolve<AuditoriaWindowsService>()
                    };
                    ServiceBase.Run(servicesToRun);
                }
            }
        }
    }
}