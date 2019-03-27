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

namespace Ccf.Ck.SysPlugins.Views.Html
{
    public class HtmlViewImp : ISystemPlugin
    {
        public async Task<IProcessingContext> ExecuteAsync(LoadedNodeSet loaderContext, IProcessingContext processingContext, IPluginServiceManager pluginServiceManager, IPluginsSynchronizeContextScoped contextScoped, INode currentNode)
        {
            if (currentNode is View view)
            {
                string cacheKey = processingContext.InputModel.Module + processingContext.InputModel.NodeSet + processingContext.InputModel.Nodepath + view.BindingKey + "_View";
                ICachingService cachingService = pluginServiceManager.GetService<ICachingService>(typeof(ICachingService));
                string cachedView = cachingService.Get<string>(cacheKey);
                if (cachedView == null)
                {
                    string directoryPath = Path.Combine(
                        processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder,
                        processingContext.InputModel.Module,
                        "Views");

                    PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
                    cachedView = File.ReadAllText(Path.Combine(directoryPath, view.Settings.Path));
                    cachingService.Insert(cacheKey, cachedView, fileProvider.Watch(view.Settings.Path));
                }
                IResourceModel resourceModel = new ResourceModel();
                resourceModel.Content = cachedView;
                resourceModel.SId = $"node/view/{processingContext.InputModel.Module}/{processingContext.InputModel.NodeSet}/{processingContext.InputModel.Nodepath}/{view.BindingKey}";
                processingContext.ReturnModel.Views.Add(view.BindingKey, resourceModel);
            }
            else
            {
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = "Current node is null or not OfType(View)" });
                throw new InvalidDataException("HtmlViewSynchronizeContextLocalImp.CurrentNode is null or not OfType(View)");
            }
            return await Task.FromResult(processingContext);
        }

        public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return await Task.FromResult(new HtmlViewSynchronizeContextScopedImp());
        }
    }
}