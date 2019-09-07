namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface ITransactionScope
    {
        object StartTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
