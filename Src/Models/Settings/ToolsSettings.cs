using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.Settings
{
    public class ToolsSettings
    {
        public ToolsSettings()
        {
            RequestRecorder = new RequestRecorderSetting();
            Tools = new List<ToolSettings>();
        }
        public RequestRecorderSetting RequestRecorder { get; set; }
        public List<ToolSettings> Tools { get; set; }
    }
}
