using System;
using System.IO;

namespace Ccf.Ck.SysPlugins.Data.Json
{
    public class CreateCommand : IFileSystemCommand
    {
        private readonly string _FilePath;

        public CreateCommand(string filePath)
        {
            _FilePath = filePath;
        }

        public void Execute()
        {
            if (File.Exists(_FilePath))
                throw new Exception("The file already exists");

            Directory.CreateDirectory(Path.GetDirectoryName(_FilePath));
            File.Create(_FilePath).Dispose();
        }

        public void Rollback()
        {
            File.Delete(_FilePath);
        }
    }
}
