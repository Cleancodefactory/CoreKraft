namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IRequestRecordersStore
    {
        IRequestRecorder Get(string key);
        void Set(IRequestRecorder requestRecorder, string key);
        void Remove(string key);
    }
}
