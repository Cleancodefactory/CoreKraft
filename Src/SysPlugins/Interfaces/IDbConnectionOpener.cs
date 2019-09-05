using System;
using System.Data.Common;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDbConnectionOpener
    {
        /// <summary>
        /// Opens a connection, but delegates the creation to the creator callback first.
        /// </summary>
        /// <param name="creator">A callback to create the connection.</param>
        /// <param name="connectionstring">The raw connection string as string (not name in the config)</param>
        /// <returns></returns>
        DbConnection AssistedOpenDbConnectionString(Func<string, DbConnection> creator, string connectionstring);
        /// <summary>
        /// Open connection by a connection string passed directly as parameter.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns>The connection string as string</returns>
        DbConnection OpenDbConnectionString(string connectionstring);
        /// <summary>
        /// Open connection by using a connection string from the standard applicaiton configuration (web config/app config)
        /// </summary>
        /// <param name="connectionstringname">The name of the connection string in the config file.</param>
        /// <returns></returns>
        DbConnection OpenDbConnection(string connectionstringname);
    }
}
