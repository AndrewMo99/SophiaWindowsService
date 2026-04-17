using SophiaWindowsService.Application.Extensions;
using SophiaWindowsService.Application.Validators;
using SophiaWindowsService.Domain.Models;
using SophiaWindowsService.Infrastructure.Common;
using SophiaWindowsService.Infrastructure.Configuration;
using SophiaWindowsService.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SophiaWindowsService.Infrastructure.Jobs
{
    public class ParametricaJob : BaseJob<ParametricaResult>
    {
        private readonly AppConfig _appConfig;
        private readonly ParametricaValidator _parametricaValidator;

        public ParametricaJob(
            AppDbContext dbContext,
            AppConfig appConfig,
            ParametricaValidator parametricaValidator) : base(dbContext)
        {
            _appConfig = appConfig;
            _parametricaValidator = parametricaValidator;
            SetParameters();
        }

        protected override string GetSpName() => DataBaseResource.SpConsultarParametros;

        private void SetParameters()
        {
            Parameters = new Dictionary<string, object>
            {
                {"@cod_pre_sgs", ""},{"@cod_sucur",""}
            };
        }

        protected override ParametricaResult MapField(IDataRecord r)
        {
            return new ParametricaResult
            {
                ObtenerTokenJWTRes1888 = r.GetData<string>("ObtenerTokenJWTRes1888"),
                CrearRDAPacienteRes1888 = r.GetData<string>("CrearRDAPacienteRes1888"),
                CrearRDAHospitalizacionRes1888 = r.GetData<string>("CrearRDAHospitalizacionRes1888"),
                CrearRDAUrgenciasRes1888 = r.GetData<string>("CrearRDAUrgenciasRes1888"),
                CrearRDAConsultaExternaRes1888 = r.GetData<string>("CrearRDAConsultaExternaRes1888"),
                SophiaWindowsServiceInterval = r.GetData<int>("SophiaWindowsServiceInterval"),
            };
        }

        protected override Task ProcessResult(List<ParametricaResult> result)
        {
            var item = result.FirstOrDefault();
            if (item is null)
            {
                var ex = new Exception($"App configuration could not be obtained from: {GetSpName()}");
                ex.GetErrorMessage().WriteEventLog(EventLogEntryType.Error);
                throw ex;
            }

            var validationResult = _parametricaValidator.Validate(item);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("\n- ", validationResult.Errors.Select(e => e.ErrorMessage));

                var ex = new Exception($"Failed validation:\n- {errors}");
                ex.GetErrorMessage().WriteEventLog(EventLogEntryType.Error);
                throw ex;
            }

            _appConfig.ParametricaResult = item;
            return Task.FromResult(_appConfig.ParametricaResult);
        }
    }
}