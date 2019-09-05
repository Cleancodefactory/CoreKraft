namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface INodePlugin : IPlugin
    {
        /// <summary>
        /// ICustomPluginContext will be either ICustomPluginReadContext or ICustomPluginWriteContext depdending on the action in progress
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        void Execute(INodePluginContext p); // StatusResult is part of ProcessingContext.ReturnModel and 'action' can be determine by ProcessingContext.isWriteOperation
    }
}
