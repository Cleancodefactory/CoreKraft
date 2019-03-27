using Ccf.Ck.Models.Resolvers;
using MySql.Data.MySqlClient;
using System.Data;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class GenericMySQL : ADOBase<MySqlConnection>
    {
        //public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        //{
        //    return await Task.FromResult(new ADOSynchronizeContextScopedDefault<MySqlConnection>());
        //}

        protected override int? GetParameterType(ParameterResolverValue value)
        {
            if (int.TryParse(value.Value.ToString(), out int num))
            {
                return (int)DbType.Int32;
            }


            return null;
        }
    }
}