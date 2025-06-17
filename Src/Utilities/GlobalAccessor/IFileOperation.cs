using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.GlobalAccessor
{
    public interface IFileOperation : IOperation
    {
        string TargetPath { get; set; }
        string SourcePath { get; set; }
    }
}
