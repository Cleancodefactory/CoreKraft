using System.IO;

namespace Ccf.Ck.SysPlugins.Data.Json
{
    public class CopyCommand : IFileSystemCommand
    {
        private readonly string _Source;
        private readonly string _Destiny;

        public CopyCommand(string source, string destiny)
        {
            _Source = source;
            _Destiny = destiny;
        }

        public void Execute()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_Destiny));
            File.Copy(_Source, _Destiny);
        }

        public void Rollback()
        {
            File.Delete(_Destiny);
        }
    }
}
