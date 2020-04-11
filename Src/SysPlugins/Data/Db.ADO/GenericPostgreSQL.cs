using Microsoft.Data.Sqlite;
using Npgsql;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class GenericPostgreSQL : ADOBase<NpgsqlConnection> {
        //public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync() {
        //    return await Task.FromResult(new ADOSynchronizeContextScopedDefault<SqliteConnection>());
        //}
    }
}
