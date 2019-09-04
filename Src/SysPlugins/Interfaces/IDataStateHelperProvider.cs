namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// To be implemented by classes that return IDataStateHelper. They usually just return DataStateUtility.Instance, but it is more convenient to obtain
    /// it directly from the context you are using anyway.
    /// </summary>
    public interface IDataStateHelperProvider<DSType>
    {
        IDataStateHelper<DSType> DataState { get; }
    }
}
