using Newtonsoft.Json;
using SophiaWindowsService.Application.Abstractions;
using SophiaWindowsService.Application.Extensions;
using SophiaWindowsService.Application.Requests;
using SophiaWindowsService.Application.Responses;
using SophiaWindowsService.Domain.Models;
using SophiaWindowsService.Domain.Values;
using SophiaWindowsService.Infrastructure.Common;
using SophiaWindowsService.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace SophiaWindowsService.Infrastructure.Jobs
{
    public class AuditoriaJob : BaseJob<AuditoriaResult>
    {
        private readonly IAppConfig _appConfig;
        private readonly IHttpRequestService _httpRequestService;

        public AuditoriaJob(
            AppDbContext dbContext,
            IAppConfig appConfig,
            IHttpRequestService httpRequestService) : base(dbContext)
        {
            _appConfig = appConfig;
            _httpRequestService = httpRequestService;
        }

        protected override string GetSpName() => DataBaseResource.SpConsultarAuditoria;

        protected override AuditoriaResult MapField(IDataRecord r)
        {
            return new AuditoriaResult
            {
                Id = r.GetData<int>("id"),
                CodPreSgs = r.GetData<string>("cod_pre_sgs"),
                CodSucur = r.GetData<int>("cod_sucur"),
                TipAdmision = r.GetData<string>("tip_admision"),
                AnoAdm = r.GetData<int>("ano_adm"),
                NumAdm = r.GetData<int>("num_adm"),
                FechaTransaccion = r.GetData<DateTime>("fecha_transaccion"),
                Reintentos = r.GetData<int>("reintentos"),
                Estado = r.GetData<int>("estado"),
                TipoRda = r.GetData<string>("tipo_rda"),
                Orden = r.GetData<int>("orden"),
                MetodoEnvio = r.GetData<string>("metodo_envio"),
                MensajeEnvio = r.GetData<string>("mensaje_envio"),
                MensajeRespuesta = r.GetData<string>("mensaje_respuesta"),
                Observacion = r.GetData<string>("observacion"),
                ClientId = r.GetData<string>("clientId"),
                SecretId = r.GetData<string>("secretId"),
                Localizacion = r.GetData<string>("localizacion"),
                CadenaConexion = r.GetData<string>("cadenaConexion")
            };
        }

        protected override async Task ProcessResult(List<AuditoriaResult> result)
        {
            foreach (var item in result)
            {
                try
                {
                    var token = await _httpRequestService.SendAsync<TokenRequest, TokenResponse>(
                        HttpMethod.Post,
                        _appConfig.ParametricaResult.ObtenerTokenJWTRes1888,
                        new TokenRequest
                        {
                            client_id = item.ClientId,
                            client_secret = item.SecretId
                        }
                    );

                    var url = string.Empty;

                    switch (item.TipoRda)
                    {
                        case RdaType.Paciente:
                            url = _appConfig.ParametricaResult.CrearRDAPacienteRes1888;
                            break;

                        case RdaType.Urgencias:
                            url = _appConfig.ParametricaResult.CrearRDAUrgenciasRes1888;
                            break;

                        case RdaType.Hospitalizacion:
                            url = _appConfig.ParametricaResult.CrearRDAHospitalizacionRes1888;
                            break;

                        case RdaType.Ambulatorio:
                            url = _appConfig.ParametricaResult.CrearRDAConsultaExternaRes1888;
                            break;
                    }

                    var requestResult = await _httpRequestService.SendAsync<object, object>(
                        new HttpMethod(item.MetodoEnvio),
                        url,
                        JsonConvert.DeserializeObject(item.MensajeEnvio),
                        new Dictionary<string, string>{
                        {
                            "Authorization",$"Bearer {token.Data.Access_token}"
                        }}
                    );

                    UpdateAuditoria(item, requestResult);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.IsNullOrWhiteSpace(ex.InnerException?.Message) ? ex.Message : ex.InnerException.Message;
                    UpdateAuditoria(item, null, errorMessage);
                }
            }
        }

        private void UpdateAuditoria(AuditoriaResult item, object requestResult, string errorMessage = null)
        {
            var actulizarJob = new ActualizarAuditoriaJob(item.CadenaConexion);
            var isError = !string.IsNullOrWhiteSpace(errorMessage);

            string response = null;
            string bundleId = null;

            if (!isError)
            {
                response = JsonConvert.SerializeObject(requestResult);
                bundleId = JsonConvert.DeserializeObject<AuditoriaResponse>(response).Data.BundleId;
            }

            actulizarJob.SetParameters(new object[]
            {
                item.Id,
                isError ? AuditoriaStatus.Error : AuditoriaStatus.Success,
                isError ? errorMessage : response,
                ++item.Reintentos,
                isError ? null : bundleId
            });

            actulizarJob.Execute();
        }
    }
}