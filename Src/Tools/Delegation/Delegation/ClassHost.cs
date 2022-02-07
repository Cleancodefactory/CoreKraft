using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegation
{
    public delegate void DelegateStringSimple(string arg);
    //public delegate void DelegateString(ClassBase _this, string arg);

    public class ClassHost
    {
        //public static Dictionary<string, DelegateString> _cache = new Dictionary<string, DelegateString>();

        //public string _instanceName = null;
        //public ClassHost(string iname)
        //{
        //    _instanceName = iname;
        //}
        //static ClassHost()
        //{
        //    var t = typeof(ClassHost);
        //    var mi1 = t.GetMethod("M1");
        //    _cache.Add("M1", mi1.CreateDelegate<DelegateString>(null));
        //    mi1 = t.GetMethod("M2");
        //    _cache.Add("M2", mi1.CreateDelegate<DelegateString>(null));
        //}

        //public void Call(string method)
        //{
        //    if (_cache.ContainsKey(method))
        //    {
        //        var d = _cache[method];
        //        d(this, "sdfsdF");
        //        return;
        //    }
        //    Console.WriteLine("Method not found");
        //}





        public void M1(string arg)
        {
            //Console.WriteLine("M1:{0}", _instanceName);
        }
        public void M2(string arg)
        {
            //Console.WriteLine("M2:{0}", _instanceName);
        }
    }
}
