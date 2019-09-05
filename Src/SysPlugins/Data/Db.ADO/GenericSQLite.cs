using Microsoft.Data.Sqlite;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class GenericSQLite : ADOBase<SqliteConnection> {
        //public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync() {
        //    return await Task.FromResult(new ADOSynchronizeContextScopedDefault<SqliteConnection>());
        //}
    }
}
