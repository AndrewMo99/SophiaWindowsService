using System;
using System.Data;

namespace SophiaWindowsService.Application.Extensions
{
    public static class DataRecordExtensions
    {
        public static T GetData<T>(this IDataRecord dr, string columnName)
        {
            var i = dr.GetOrdinal(columnName);

            if (dr.IsDBNull(i))
                return default;

            var type = typeof(T);
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            return (T)Convert.ChangeType(dr.GetValue(i), underlying);
        }
    }
}