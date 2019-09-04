using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroMetaDataDto
    {
        public IntroMetaDataDto()
        {
            Sections = new List<IntroSection>();
        }

        public List<IntroSection> Sections { get; set; }

        public IntroSection AddSection(IntroSection introSection)
        {
            IntroSection section = Sections.FirstOrDefault(s => s.Id.Equals(introSection.Id, StringComparison.OrdinalIgnoreCase));
            if (section == null)
            {
                Sections.Add(introSection);
                section = introSection;
            }
            return section;
        }
    }
}
