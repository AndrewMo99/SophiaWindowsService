using SophiaWindowsService.Application.Abstractions;
using SophiaWindowsService.Application.Extensions;
using SophiaWindowsService.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SophiaWindowsService.Infrastructure.Common
{
    public abstract class BaseJob<T> : IJob
    {
        private readonly AppDbContext _dbContext;

        protected BaseJob(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async void Execute()
        {
            try
            {
                var result = PrepareQuery(GetSpName());
                await ProcessResult(result);
            }
            catch (Exception ex)
            {
                ex.GetErrorMessage().WriteLog();
            }
        }

        private List<T> PrepareQuery(string spName)
        {
            var connection = (SqlConnection)_dbContext.Database.Connection;
            var wasClosed = connection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) connection.Open();
                return ExecuteSp(spName, connection);
            }
            finally
            {
                if (wasClosed) connection.Close();
            }
        }

        private List<T> ExecuteSp(string spName, SqlConnection connection)
        {
            LogExtensions.WriteEventLog($"Executing SP: {spName}", EventLogEntryType.Information);
            var result = new List<T>();

            using (var cmd = new SqlCommand(spName, connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var keyValuePair in GetParameters())
                {
                    var value = keyValuePair.Value ?? DBNull.Value;

                    if (value is SqlParameter parameter)
                    {
                        cmd.Parameters.Add(parameter);
                        continue;
                    }
                    cmd.Parameters.AddWithValue(keyValuePair.Key, value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    LogExtensions.WriteEventLog($"Executed Successfully SP: {spName}", EventLogEntryType.Information);
                    OutPutParameters = cmd.Parameters;

                    while (reader.Read())
                    {
                        result.Add(MapField(reader));
                    }
                }
            }

            return result;
        }

        protected abstract string GetSpName();

        protected Dictionary<string, object> Parameters = new Dictionary<string, object>();

        protected SqlParameterCollection OutPutParameters;

        private Dictionary<string, object> GetParameters() => Parameters;

        protected abstract T MapField(IDataRecord r);

        protected abstract Task ProcessResult(List<T> result);
    }
}