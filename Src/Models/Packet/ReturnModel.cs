using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Packet
{
    public class ReturnModel : IReturnModel
    {
        public ReturnModel()
        {
            Views = new Dictionary<string, IResourceModel>();
            //StatusResults = new List<StatusResult>();
        }
        public IReturnModel GetWrapper()
        {
            return new WrappedReturnModel(this);
        }

        public object Data { get; set; }
        public object BinaryData { get; set; }
        public Dictionary<string, IResourceModel> Views { get; set; }//normal, mini, big
        public object LookupData { get; set; }
        public IReturnStatus Status { get; set; } = new ReturnStatus();
        public IHttpResponseBuilder ResponseBuilder { get; set; }

        public MetaRoot ExecutionMeta { get; set; }

        //public List<StatusResult> StatusResults { get; set; }


        public class WrappedReturnModel : IReturnModel
        {
            private ReturnModel _Model = null;
            public WrappedReturnModel(ReturnModel m)
            {
                _Model = m;
            }

            // TODO: Wrap these things to prevent write access IN DEPTH.
            public object Data { get => ((IReturnModel)_Model).Data; set => ((IReturnModel)_Model).Data = value; }

            public object BinaryData { get => ((IReturnModel)_Model).BinaryData; set => ((IReturnModel)_Model).BinaryData = value; }

            public object LookupData { get => ((IReturnModel)_Model).LookupData; set => ((IReturnModel)_Model).LookupData = value; }

            public IHttpResponseBuilder ResponseBuilder { get => ((IReturnModel)_Model).ResponseBuilder; set => ((IReturnModel)_Model).ResponseBuilder = value; }

            public Dictionary<string, IResourceModel> Views { get => ((IReturnModel)_Model).Views; set => ((IReturnModel)_Model).Views = value; }
            public IReturnStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public MetaRoot ExecutionMeta { get => ((IReturnModel)_Model).ExecutionMeta; set => ((IReturnModel)_Model).ExecutionMeta = value; }
        }
    }
}
