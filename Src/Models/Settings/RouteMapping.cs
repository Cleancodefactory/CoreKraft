﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.Settings
{
    public class RouteMapping
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string SlugExpression { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
