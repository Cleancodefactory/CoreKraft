<<<<<<< HEAD
﻿using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroMetaData
    {
        private List<IntroSection> _Sections;
        public IntroMetaData(List<IntroSection> sections)
        {
            _Sections = sections;
        }

        public IReadOnlyList<IntroSection> Sections
        {
            get
            {
                return _Sections.AsReadOnly();
            }
        }
    }
}
=======
﻿using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroMetaData
    {
        private List<IntroSection> _Sections;
        public IntroMetaData(List<IntroSection> sections)
        {
            _Sections = sections;
        }

        public IReadOnlyList<IntroSection> Sections
        {
            get
            {
                return _Sections.AsReadOnly();
            }
        }
    }
}
>>>>>>> develop
