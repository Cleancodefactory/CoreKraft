﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegation
{
    public delegate void DelegateStringA(ClassA _this, string arg);
    public class ClassA: ClassBase
    {
        private static Dictionary<string, DelegateStringA> _cache = new Dictionary<string, DelegateStringA>();
        static ClassA()
        {
            var t = typeof(ClassA);
            var mi1 = t.GetMethod("M1");
            _cache.Add("M1", mi1.CreateDelegate<DelegateStringA>(null));
            mi1 = t.GetMethod("M2");
            _cache.Add("M2", mi1.CreateDelegate<DelegateStringA>(null));
            mi1 = t.GetMethod("M3");
            _cache.Add("M3", mi1.CreateDelegate<DelegateStringA>(null));
            mi1 = t.GetMethod("M4");
            _cache.Add("M4", mi1.CreateDelegate<DelegateStringA>(null));
        }
        public DelegateStringSimple GetProc(string name) => name switch
        {
            nameof(M1) => M1,
            nameof(M2) => M2,
            nameof(M3) => M3,
            nameof(M4) => M4,
            _ => null,
        };
        public DelegateStringA GetProc2(string methodName) {
            if (_cache.ContainsKey(methodName))
            {
                return _cache[methodName];
            }
            return null;
        }
        public void M1(string argument)
        {

        }

        public void M2(string argument)
        {

        }

        public void M3(string argument)
        {

        }

        public void M4(string argument)
        {

        }
    }
}
