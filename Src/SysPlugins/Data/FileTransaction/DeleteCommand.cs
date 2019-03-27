using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileTransaction
{
    public class DeleteCommand : IFileSystemCommand
    {
        private readonly string _FilePath;
        private byte[] _FileContent;

        public DeleteCommand(string filePath)
        {
            _FilePath = filePath;
        }

        public void Execute()
        {
            _FileContent = ReadFile(_FilePath);

            File.Delete(_FilePath);
        }

        public void Rollback()
        {
            var fileStream = File.Create(_FilePath);
            fileStream.Write(_FileContent, 0, _FileContent.Length);
            fileStream.Dispose();
        }

        private byte[] ReadFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
