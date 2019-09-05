namespace Ccf.Ck.SysPlugins.Data.Json
{
    public interface IFileSystemCommand
    {
        void Execute();

        void Rollback();
    }
}
