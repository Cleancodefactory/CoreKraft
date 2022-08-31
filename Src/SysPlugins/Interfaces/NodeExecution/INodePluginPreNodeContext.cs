﻿using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface INodePluginPreNodeContext: INodePluginContextWithResults {
        /// <summary>
        /// During read operations plugins can inspect, change and add/insert results as Dictionary(string, object)
        /// </summary>
        List<Dictionary<string, object>> Results { get; }
    }
}
