using System.Collections.Generic;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Data.Internal
{
    public class InternalDataSynchronizeContextScopedImp: IPluginsSynchronizeContextScoped, IContextualBasketConsumer
    {
        public KraftGlobalConfigurationSettings KraftEnvironmentSettings => ProcessingContext.InputModel.KraftGlobalConfigurationSettings;
        public IProcessingContext ProcessingContext { get; set; }

        #region IPluginsSynchronizeContextScoped
        public Dictionary<string, string> CustomSettings
        {
            get; set;
        }
        #endregion

        #region IContextualBasketConsumer
        public void InspectBasket(IContextualBasket basket)
        {
            var pc = basket.PickBasketItem<IProcessingContext>();
            ProcessingContext = pc;
        }
        #endregion IContextualBasketConsumer
    }
}
