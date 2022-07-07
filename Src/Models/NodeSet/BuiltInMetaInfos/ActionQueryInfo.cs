using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class ActionQueryInfo: MetaInfoBase {
        public ActionQueryInfo() {
        }

        private List<Script> _Scripts;

        public ReadOnlyCollection<Script> Scripts {  get {
                if (Flags.HasFlag(Enumerations.EMetaInfoFlags.Trace)) {
                    return new ReadOnlyCollection<Script>(_Scripts);
                } else {
                    return null;
                }
            } 
        }

        #region Plugin reporting

        public void AddScript(ScriptType scriptType, string program) {
            if (Flags.HasFlag(Enumerations.EMetaInfoFlags.Trace)) {
                if (_Scripts == null) _Scripts = new List<Script>();
                _Scripts.Add(new Script(scriptType, program));
            }
        }

        #endregion


        public enum ScriptType {
            Before = 1,
            Main = 0,
            After = 2,
            AfterChildren = 3
        }

        public record Script (ScriptType ScriptType, string Program ); // To be extended
    }
}
