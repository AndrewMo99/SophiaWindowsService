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
    public class ActualizarAuditoriaJob : BaseJob<bool>
    {
        public ActualizarAuditoriaJob(string connectionName) : base(new AppDbContext(connectionName))
        { }

        protected override string GetSpName() => DataBaseResource.SpActualizaEstadoAuditoria;

        public void SetParameters(object[] parameters)
        {
            Parameters = new Dictionary<string, object>
            {
                { "@id", parameters[0] },
                { "@estado", parameters[1] },
                { "@mensaje_respuesta", parameters[2] },
                { "@reintentos", parameters[3] },
                { "@bundleRda", parameters[4] },
                { "", new SqlParameter("@lError", SqlDbType.Int) {Direction = ParameterDirection.Output} }
            };
        }

        protected override bool MapField(IDataRecord r) => true;

        protected override Task ProcessResult(List<bool> result)
        {
            if (int.TryParse($"{OutPutParameters["@lError"]?.Value}", out var error) && error == 0)
                return Task.FromResult(true);

            var msg = $"Error updating Auditoria with Id {Parameters["@id"]}. Error code: {error}";
            msg.WriteLog();
            msg.WriteEventLog(EventLogEntryType.Error);

            return Task.FromResult(error);
        }
    }
}