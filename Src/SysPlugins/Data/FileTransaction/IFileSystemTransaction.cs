namespace Ccf.Ck.SysPlugins.Data.FileTransaction
{
    public interface IFileSystemCommand
    {
        void Execute();

        void Rollback();
    }
}