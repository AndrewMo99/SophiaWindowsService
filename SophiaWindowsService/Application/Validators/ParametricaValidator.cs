using System;
using FluentValidation;
using SophiaWindowsService.Domain.Models;

namespace SophiaWindowsService.Application.Validators
{
    public class ParametricaValidator : AbstractValidator<ParametricaResult>
    {
        public ParametricaValidator()
        {
            RuleFor(x => x.SophiaWindowsServiceInterval)
                .GreaterThan(0).WithMessage("El intervalo debe ser mayor a 0.");

            RuleFor(x => x.ObtenerTokenJWTRes1888).Must(BeAValidUrl).WithMessage("URL de Token JWT inválida.");
            RuleFor(x => x.CrearRDAPacienteRes1888).Must(BeAValidUrl).WithMessage("URL de Crear Paciente inválida.");
            RuleFor(x => x.CrearRDAHospitalizacionRes1888).Must(BeAValidUrl).WithMessage("URL de Hospitalización inválida.");
            RuleFor(x => x.CrearRDAUrgenciasRes1888).Must(BeAValidUrl).WithMessage("URL de Urgencias inválida.");
            RuleFor(x => x.CrearRDAConsultaExternaRes1888).Must(BeAValidUrl).WithMessage("URL de Consulta Externa inválida.");
        }

        private static bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}