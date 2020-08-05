using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.DirectCall
{
    public class ReturnModel
    {
        public object Data { get; set; }
        public object BinaryData { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}
