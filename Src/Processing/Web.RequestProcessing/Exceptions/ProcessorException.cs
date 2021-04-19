
using System;
using Ccf.Ck.Processing.Web.Request.BaseClasses;

namespace Ccf.Ck.Processing.Web.Request
{
    public class ProcessorException<T>: Exception where T: ProcessorBase {
        public ProcessorException(string description):base(description) {

        }
    }
}