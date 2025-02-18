using System;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class HostingServiceSetting
    {
        public List<string> Signals { get; set; }
        public int IntervalInMinutes { get; set; } = 0; //Default interval
        public List<DayOfWeek> ActiveDays { get; set; } = new List<DayOfWeek>(); // Days to run
        public TimeSpan StartTime { get; set; } = TimeSpan.Zero; // Specific time to start
    }
}
