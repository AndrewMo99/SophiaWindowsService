using Autofac;
using SophiaWindowsService.Application.Abstractions;
using SophiaWindowsService.Application.Validators;
using SophiaWindowsService.Infrastructure.Configuration;
using SophiaWindowsService.Infrastructure.DataBase;
using SophiaWindowsService.Infrastructure.Http;
using SophiaWindowsService.Infrastructure.Jobs;
using SophiaWindowsService.Infrastructure.Services;
using System.Net;
using System.Net.Http;

namespace SophiaWindowsService.Infrastructure
{
    internal static class DependencyInjection
    {
        public static void AddInfrastructure(this ContainerBuilder builder)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            builder
                .AddConfig()
                .AddHttClient()
                .AddJobs()
                .AddService();
        }

        private static ContainerBuilder AddConfig(this ContainerBuilder builder)
        {
            builder.RegisterType<AppDbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<AppConfig>().AsSelf().As<IAppConfig>().SingleInstance();

            builder.RegisterType<ParametricaValidator>().AsSelf().SingleInstance();

            builder.RegisterType<ParametricaJob>().AsSelf().InstancePerDependency();

            return builder;
        }

        private static ContainerBuilder AddHttClient(this ContainerBuilder builder)
        {
            builder.RegisterType<HttpClient>().AsSelf().SingleInstance();

            builder.RegisterType<HttpRequestService>().As<IHttpRequestService>().SingleInstance();

            return builder;
        }

        private static ContainerBuilder AddJobs(this ContainerBuilder builder)
        {
            builder.RegisterType<AuditoriaJob>().As<IJob>().InstancePerDependency();

            return builder;
        }

        private static void AddService(this ContainerBuilder builder)
        {
            builder.RegisterType<AuditoriaWindowsService>().AsSelf().InstancePerDependency();
        }
    }
}