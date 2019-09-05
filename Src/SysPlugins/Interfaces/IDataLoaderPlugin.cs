namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataLoaderPlugin : IPlugin
    {
        /// <summary>
        /// The older method with numerous arguments has been replaced by a single NodeExecutionContext, which
        /// is an object created in he beginning of a node execution and destoyed when it finishes. The same object
        /// is passed to all the plugins that use the node execution as a scope for their operation (custom, security, may be more in future).
        /// </summary>
        /// <param name="execContext"></param>
        /// <returns></returns>
        void Execute(IDataLoaderContext execContext);
    }
}
