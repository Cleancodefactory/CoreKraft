using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileTransaction
{
    public class CopyCommand : IFileSystemCommand
    {
        private readonly string _Source;
        private readonly string _Target;

        public CopyCommand(string source, string target)
        {
            _Source = source;
            _Target = target;
        }

        public void Execute()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_Target));
            File.Copy(_Source, _Target);
        }

        public void Rollback()
        {
            File.Delete(_Target);
        }
    }
}
