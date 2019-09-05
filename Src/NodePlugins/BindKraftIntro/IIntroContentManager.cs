<<<<<<< HEAD
﻿using Ccf.Ck.NodePlugins.BindKraftIntro.Models;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro
{
    public interface IIntroContentProvider
    {
        IntroMetaData LoadMenu(INodePluginContext pc);
        IntroItem LoadIntroItem(INodePluginContext pc);
        List<IntroItem> LoadDeletedIntroItem(INodePluginContext pc);
        bool CreateIntroItem(string sectionId, IntroItem introItem);
        bool UpdateIntroItem(string sectionId, IntroItem introItem);
        bool DeleteIntroItem(string sectionId, IntroItem introItem);
        bool HardDeleteAll();

        bool ApproveExample(string sectionId, IntroItem introItem);
    }
}
=======
﻿using Ccf.Ck.NodePlugins.BindKraftIntro.Models;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro
{
    public interface IIntroContentProvider
    {
        IntroMetaData LoadMenu(INodePluginContext pc);
        IntroItem LoadIntroItem(INodePluginContext pc);
        List<IntroItem> LoadDeletedIntroItem(INodePluginContext pc);
        bool CreateIntroItem(string sectionId, IntroItem introItem);
        bool UpdateIntroItem(string sectionId, IntroItem introItem);
        bool DeleteIntroItem(string sectionId, IntroItem introItem);
        bool HardDeleteAll();

        bool ApproveExample(string sectionId, IntroItem introItem);
    }
}
>>>>>>> develop
