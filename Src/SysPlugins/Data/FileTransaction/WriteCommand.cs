using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileTransaction
{
    public class WriteCommand : IFileSystemCommand
    {
        private readonly string _FileToWrite;
        private readonly string _Content;

        public WriteCommand(string fileToWrite, string content)
        {
            _FileToWrite = fileToWrite;
            _Content = content;
        }

        public void Execute()
        {
            if (!File.Exists(_FileToWrite))
                throw new FileNotFoundException();

            File.WriteAllText(_FileToWrite, _Content);
        }

        public void Rollback()
        {
            FileStream fileStream = File.OpenWrite(_FileToWrite);

            long length = fileStream.Length;
            fileStream.SetLength(length - _Content.Length);

            fileStream.Dispose();
        }
    }
}
