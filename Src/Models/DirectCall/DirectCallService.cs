using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.DirectCall
{
    public class DirectCallService
    {
        private Func<InputModel, ReturnModel> _CallImp;
        private Func<InputModel, CancellationToken, Task<ReturnModel>> _CallImpAsync;
        public static DirectCallService Instance { get; private set; }

        static DirectCallService()
        {
            Instance = new DirectCallService();
        }
        public Func<InputModel, ReturnModel> Call
        {
            get
            {
                return _CallImp;
            }
            set
            {
                _CallImp = value;
            }
        }

        public Func<InputModel, CancellationToken, Task<ReturnModel>> CallAsync
        {
            get
            {
                return _CallImpAsync;
            }
            set
            {
                _CallImpAsync = value;
            }
        }
    }
}
