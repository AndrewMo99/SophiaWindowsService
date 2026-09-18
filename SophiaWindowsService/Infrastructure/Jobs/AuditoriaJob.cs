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
using System.Threading;
using System.Threading.Tasks;

namespace SophiaWindowsService.Infrastructure.Jobs
{
    public class AuditoriaJob : BaseJob<AuditoriaResult>
    {
        private readonly ParametricaResult _parametrica;
        private readonly IHttpRequestService _httpRequestService;

        public AuditoriaJob(
            AppDbContext dbContext,
            IAppConfig appConfig,
            IHttpRequestService httpRequestService) : base(dbContext)
        {
            _parametrica = appConfig.ParametricaResult;
            _httpRequestService = httpRequestService;
        }

        protected override string GetSpName() => DataBaseResource.SpConsultarAuditoria;

        protected override AuditoriaResult MapField(IDataRecord r)
        {
            return new AuditoriaResult
            {
                Id = r.GetData<int>("id"),
                CodPreSgs = r.GetData<string>("cod_pre_sgs"),
                CodSucur = r.GetData<string>("cod_sucur"),
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
            var tokenCache = new Dictionary<string, string>();

            foreach (var item in result)
            {
                var isErrorToken = true;
                try
                {
                    var tokenKey = $"{item.ClientId}#{item.SecretId}#{item.Localizacion}";
                    string accessToken;

                    if (tokenCache.TryGetValue(tokenKey, out var value))
                    {
                        accessToken = value;
                    }
                    else
                    {
                        var token = await _httpRequestService.SendAsync<TokenRequest, TokenResponse>(
                            HttpMethod.Post,
                            _parametrica.ObtenerTokenJWTRes1888,
                            new TokenRequest
                            {
                                client_id = item.ClientId,
                                client_secret = item.SecretId
                            }
                        );

                        accessToken = token.Data.Access_token;
                        tokenCache.Add(tokenKey, accessToken);
                    }

                    isErrorToken = string.IsNullOrWhiteSpace(accessToken);

                    if (isErrorToken)
                        throw new InvalidOperationException("The token could not be obtained");

                    var url = string.Empty;

                    switch (item.TipoRda)
                    {
                        case RdaType.Paciente:
                            url = _parametrica.CrearRDAPacienteRes1888;
                            break;

                        case RdaType.Urgencias:
                            url = _parametrica.CrearRDAUrgenciasRes1888;
                            break;

                        case RdaType.Hospitalizacion:
                            url = _parametrica.CrearRDAHospitalizacionRes1888;
                            break;

                        case RdaType.Ambulatorio:
                            url = _parametrica.CrearRDAConsultaExternaRes1888;
                            break;
                    }

                    try
                    {
                        var requestResult = await _httpRequestService.SendAsync<object, object>(
                            new HttpMethod(item.MetodoEnvio),
                            url,
                            JsonConvert.DeserializeObject(item.MensajeEnvio),
                            new Dictionary<string, string>
                            {
                                {
                                    "Authorization", $"Bearer {accessToken}"
                                }
                            }
                        );

                        UpdateAuditoria(item, requestResult);
                    }
                    catch (Exception ex)
                    {
                        UpdateAuditoria(item, null, ex.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = string.IsNullOrWhiteSpace(ex.InnerException?.Message)
                        ? ex.Message
                        : ex.InnerException.Message;

                    if (!isErrorToken)
                        UpdateAuditoria(item, null, errorMessage);
                    else
                        InsertErrorToken(item, errorMessage);
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }

        private static void UpdateAuditoria(AuditoriaResult item, object requestResult, string errorMessage = null)
        {
            var updateJob = new AuditoriaUpdateJob(item.CadenaConexion);
            var isError = !string.IsNullOrWhiteSpace(errorMessage);

            string response = null;
            string bundleId = null;

            if (!isError)
            {
                response = JsonConvert.SerializeObject(requestResult);
                bundleId = JsonConvert.DeserializeObject<AuditoriaResponse>(response).Data.BundleId;
            }

            updateJob.SetParameters(new object[]
            {
                item.Id,
                isError ? AuditoriaStatus.Error : AuditoriaStatus.Success,
                isError ? errorMessage : response,
                item.Reintentos + 1,
                isError ? null : bundleId
            });

            updateJob.Execute();
        }

        private static void InsertErrorToken(AuditoriaResult item, string errorMessage)
        {
            var insertJob = new AuditoriaInsertJob(item.CadenaConexion);

            insertJob.SetParameters(new object[]
            {
                item.CodPreSgs,
                item.CodSucur,
                item.TipAdmision,
                item.AnoAdm,
                item.NumAdm,
                0,
                AuditoriaStatus.ErrorToken,
                RdaType.Token,
                0,
                HttpMethod.Post.ToString(),
                "{}",
                errorMessage,
                "The token could not be obtained",
                string.Empty
            });

            insertJob.Execute();
        }
    }
}