namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Data loader plugins can optionally implement this interface to offer the Prepare operation support.
    /// If Prepare operation is configured, but not supported by the loader an exception is thrown.
    /// </summary>
    public interface IDataLoaderPluginPrepare : IDataLoaderPlugin {
        /// <summary>
        /// The IDataLoaderReadContext is used for both read and write cases. Prepare method needs access to
        /// all the rows coming for write and not only to the currently processed one which is the same as what you see through
        /// a typical read context.
        /// 
        /// It is recommended to not create results during a prepare operation unless other solutions are impossible or too complex.
        /// A good examples for intended use of prepare:
        /// Write:
        ///     - delete certain items to enable recreation of all the items every time instead of tracking their states individually
        /// Read:
        ///     - create mockup results while the actual data base is not developed yet.
        /// </summary>
        /// <param name="execContext"></param>
        /// <returns></returns>
        void Prepare(IDataLoaderReadContext execContext);
    }
}
