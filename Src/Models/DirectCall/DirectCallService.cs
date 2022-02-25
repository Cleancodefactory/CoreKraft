using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.DirectCall
{
    public class DirectCallService
    {
        private Func<InputModel, ReturnModel> _CallImp;
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

        
    }
}
