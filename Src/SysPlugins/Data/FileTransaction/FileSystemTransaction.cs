using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.FileTransaction
{
    public class FileSystemTransaction
    {
        private readonly IList<IFileSystemCommand> _Commands;
        private int _TransactionStart;
        private int _TransactionEnd;

        public FileSystemTransaction()
        {
            _Commands = new List<IFileSystemCommand>();
            _TransactionStart = 0;
            _TransactionEnd = 0;
        }

        public void Create(string filePath)
        {
            var command = new CreateCommand(filePath);

            Add(command);
        }

        //public void Move(string source, string target)
        //{
        //    var command = new MoveCommand(source, target);

        //    Add(command);
        //}

        public void Copy(string source, string destiny)
        {
            var command = new CopyCommand(source, destiny);

            Add(command);
        }

        public void Delete(string filePath)
        {
            var command = new DeleteCommand(filePath);

            Add(command);
        }

        public void Write(string fileToWrite, string content)
        {
            var command = new WriteCommand(fileToWrite, content);

            Add(command);
        }

        public void Commit()
        {
            for (var i = _TransactionStart; i < _TransactionEnd; i++)
            {
                _Commands[i].Execute();
            }

            _TransactionStart = _TransactionEnd;
        }

        public void Rollback()
        {
            for (var i = _TransactionEnd - 2; i >= _TransactionStart; i--)
            {
                _Commands[i].Rollback();
            }
        }

        public bool HasCommands()
        {
            return _TransactionStart != _TransactionEnd;
        }

        public void Add(IFileSystemCommand command)
        {
            _Commands.Add(command);

            _TransactionEnd++;
        }
    }
}
