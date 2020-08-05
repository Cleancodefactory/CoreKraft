using System;
using System.Collections.Generic;
using System.Text;

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
                if (_CallImp == null)
                {
                    _CallImp = value;
                }
            }
        }
    }
}
