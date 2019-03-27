using System.Collections.Generic;

namespace Ccf.Ck.Utilities.Web.BundleTransformations.Primitives
{
    public class KraftBundleProfiles : KraftBundle
    {
        public List<string> ProfileFiles { get; set; }

        public KraftBundleProfiles()
        {
            ProfileFiles = new List<string>();
        }
    }
}
