<<<<<<< HEAD
﻿using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroSection
    {
        public IntroSection()
        {
            IntroItems = new List<IntroItem>();
        }

        public string Caption { get; set; }
        public string ImagePath { get; set; }
        public string Id { get; set; }
        public int OrderIdx { get; set; }
        public IList<IntroItem> IntroItems { get; set; }

    }
}
=======
﻿using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroSection
    {
        public IntroSection()
        {
            IntroItems = new List<IntroItem>();
        }

        public string Caption { get; set; }
        public string ImagePath { get; set; }
        public string Id { get; set; }
        public int OrderIdx { get; set; }
        public IList<IntroItem> IntroItems { get; set; }

    }
}
>>>>>>> develop
