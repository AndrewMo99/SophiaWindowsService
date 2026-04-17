using SophiaWindowsService.Application.Abstractions;
using SophiaWindowsService.Domain.Models;

namespace SophiaWindowsService.Infrastructure.Configuration
{
    public class AppConfig : IAppConfig
    {
        public ParametricaResult ParametricaResult { get; set; }
    }
}