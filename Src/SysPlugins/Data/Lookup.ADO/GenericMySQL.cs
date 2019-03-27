using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.SysPlugins.Lookups.ADO
{
    public class GenericMySQL : LookupADOBase
    {
        public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return await Task.FromResult(new LookupADOSynchronizeContextScopedDefault<MySqlConnection>());
        }
    }
}