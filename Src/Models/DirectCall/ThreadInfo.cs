using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.DirectCall {
    public class ThreadInfo {
        public int ThreadIndex { get; set; }
        public bool Looping { get; set; }
        public bool LoopingEnded { get; set; }
        public bool DirectCallAvailable { get; set; }
        public bool TaskPicked { get; set; }
        public DateTime LastTaskPickedAt { get; set; }
        public DateTime LastTaskFinishedAt { get; set; }
        public string Executing { get; set; }
        public string LastTaskCompleted { get; set; }
        public bool StartHandler { get; set; }
        public bool FinishtHandler { get; set; }
    }
}
