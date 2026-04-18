using SophiaWindowsService.Application.Extensions;
using SophiaWindowsService.Infrastructure.Common;
using SophiaWindowsService.Infrastructure.DataBase;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SophiaWindowsService.Infrastructure.Jobs
{
    public class AuditoriaInsertJob : BaseJob<bool>
    {
        public AuditoriaInsertJob(string connectionName) : base(new AppDbContext(connectionName))
        { }

        protected override string GetSpName() => DataBaseResource.SpInsertarAuditoria;

        public void SetParameters(object[] parameters)
        {
            Parameters = new Dictionary<string, object>
            {
                { "@cod_pre_sgs", parameters[0] },
                { "@cod_sucur", parameters[1] },
                { "@tip_admision", parameters[2] },
                { "@ano_adm", parameters[3] },
                { "@num_adm", parameters[4] },
                { "@reintentos", parameters[5] },
                { "@estado", parameters[6] },
                { "@tipo_rda", parameters[7] },
                { "@orden", parameters[8] },
                { "@metodo_envio", parameters[9] },
                { "@mensaje_envio", parameters[10] },
                { "@mensaje_respuesta", parameters[11] },
                { "@observacion", parameters[12] },
                { "@bundleRda", parameters[13] },
                { "@lError", new SqlParameter("@lError", SqlDbType.Int) { Direction = ParameterDirection.Output } }
            };
        }

        protected override bool MapField(IDataRecord r) => true;

        protected override Task ProcessResult(List<bool> result)
        {
            if (int.TryParse($"{OutPutParameters["@lError"]?.Value}", out var error) && error == 0)
                return Task.FromResult(true);

            var msg = $"Error inserting Auditoria with Tipo Rda {Parameters["@tipo_rda"]}. Error code: {error}";
            msg.WriteLog();
            msg.WriteEventLog(EventLogEntryType.Error);

            return Task.FromResult(error);
        }
    }
}