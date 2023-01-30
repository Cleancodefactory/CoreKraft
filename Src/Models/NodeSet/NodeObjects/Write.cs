namespace Ccf.Ck.Models.NodeSet
{
    public class Write : OperationBase
    {
        
        public Insert Insert
        {
            get;
            set;
        }

        public Update Update
        {
            get;
            set;
        }

        public Delete Delete
        {
            get;
            set;
        }
    }
}