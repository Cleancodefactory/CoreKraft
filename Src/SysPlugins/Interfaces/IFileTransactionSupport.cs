using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface IFileTransactionSupport {
        public void DeleteOnRollback(string filepath);
        public void DeleteOnCommit(string filepath);

    }
}
