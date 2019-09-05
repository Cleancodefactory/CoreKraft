using System.Data.Common;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IADOTransactionScope: ITransactionScope
    {
        /// <summary>
        /// Gets/creates and opens the current connection.
        /// </summary>
        DbConnection Connection { get; }
        /// <summary>
        /// Gets the current transaction if one is started, but will not start a new one if there is none yet.
        /// This property is mostly for convenience.
        /// </summary>
        DbTransaction CurrentTransaction { get; }

        /// <summary>
        /// Specialized variant for DbTransaction - just casts the transction to DbTransaction.
        /// </summary>
        /// <returns></returns>
        DbTransaction StartADOTransaction();
    }
}
