using SophiaWindowsService.Domain.Models;

namespace SophiaWindowsService.Application.Abstractions
{
    public interface IAppConfig
    {
        ParametricaResult ParametricaResult { get; }
    }
}