using Ccf.Ck.SysPlugins.Interfaces.Packet;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.Packet
{
    public class ReturnStatus : IReturnStatus
    {
        // TODO: More properties will be needed
        public bool IsSuccessful { get; set; } = true;
        public List<IStatusResult> StatusResults { get; set; } = new List<IStatusResult>();
        public string ReturnUrl { get; set; }

        public static IReturnStatus Combine(IEnumerable<IReturnStatus> statuses)
        {
            ReturnStatus status = new ReturnStatus();
            return statuses.Aggregate(status, (acc, cur) =>
            {
                if (!cur.IsSuccessful)
                {
                    acc.IsSuccessful = false;
                }

                acc.StatusResults.AddRange(cur.StatusResults);
                return acc;
            });
        }
    }
}
