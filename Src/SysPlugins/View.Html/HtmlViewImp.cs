using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Ccf.Ck.Models.KraftModule;

namespace Ccf.Ck.SysPlugins.Views.Html
{
    public class HtmlViewImp : ISystemPlugin
    {
        public IProcessingContext Execute(LoadedNodeSet loaderContext, IProcessingContext processingContext, IPluginServiceManager pluginServiceManager, IPluginsSynchronizeContextScoped contextScoped, INode currentNode)
        {
            if (currentNode is View view)
            {
                KraftModuleCollection kraftModuleCollection = pluginServiceManager.GetService<KraftModuleCollection>(typeof(KraftModuleCollection));
                if (kraftModuleCollection != null)
                {
                    KraftModule kraftModule = kraftModuleCollection.GetModule(processingContext.InputModel.Module);
                    if (kraftModule != null)
                    {
                        string cachedView = null;
                        string cacheKey = null;
                        ICachingService cachingService = null;
                        if (processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.EnableOptimization)
                        {
                            cacheKey = kraftModule.Name + processingContext.InputModel.NodeSet + processingContext.InputModel.Nodepath + view.BindingKey + "_View";
                            cachingService = pluginServiceManager.GetService<ICachingService>(typeof(ICachingService));
                            cachedView = cachingService.Get<string>(cacheKey);
                        }
                        if (cachedView == null)
                        {
                            string directoryPath = Path.Combine(
                                processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(kraftModule.Key),
                                kraftModule.Name,
                                "Views");

                            PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
                            cachedView = File.ReadAllText(Path.Combine(directoryPath, view.Settings.Path));
                            if (cachingService != null && processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.EnableOptimization)
                            {
                                cachingService.Insert(cacheKey, cachedView, fileProvider.Watch(view.Settings.Path));
                            }
                        }
                        IResourceModel resourceModel = new ResourceModel();
                        resourceModel.Content = cachedView;
                        resourceModel.SId = $"node/view/{kraftModule.Name}/{processingContext.InputModel.NodeSet}/{processingContext.InputModel.Nodepath}/{view.BindingKey}";
                        processingContext.ReturnModel.Views.Add(view.BindingKey, resourceModel);
                    }                    
                }
                //Error
            }
            else
            {
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = "Current node is null or not OfType(View)" });
                throw new InvalidDataException("HtmlViewSynchronizeContextLocalImp.CurrentNode is null or not OfType(View)");
            }
            return processingContext;
        }

        public Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return Task.FromResult<IPluginsSynchronizeContextScoped>(new HtmlViewSynchronizeContextScopedImp());
        }
    }
}