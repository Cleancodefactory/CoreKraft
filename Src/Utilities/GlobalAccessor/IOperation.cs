namespace Ccf.Ck.Utilities.GlobalAccessor
{
    public interface IOperation
    {
        void Execute();
        string GetKey(); // Unique key for storage/removal
    }
}
