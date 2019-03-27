using System.Data.SqlClient;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class GenericSQLServer : ADOBase<SqlConnection>
    {
        public GenericSQLServer() : base()
        {
            SupportTableParameters = true;
        }
        //public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync() {
        //    return await Task.FromResult(new ADOSynchronizeContextScopedDefault<SqlConnection>());
        //}
    }
}