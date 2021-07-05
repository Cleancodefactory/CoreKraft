using Ccf.Ck.Libs.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.Collections;
using System.Text.RegularExpressions;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryBase<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class {

        #region IActionQueryLibrary
        public virtual HostedProc<HostInterface> GetProc(string name) {
            return name switch
            {
                nameof(Add) => Add,
                nameof(TryAdd) => TryAdd,
                nameof(Concat) => Concat,
                nameof(Cast) => Cast,
                nameof(GSetting) => GSetting,
                nameof(Throw) => Throw,
                nameof(Debug) => Debug,
                nameof(IsEmpty) => IsEmpty,
                nameof(TypeOf) => TypeOf,
                nameof(IsNumeric) => IsNumeric,
                nameof(Random) => Random,
                nameof(Neg) => Neg,
                nameof(Equal) => Equal,
                nameof(Greater) => Greater,
                nameof(Lower) => Lower,
                nameof(Or) => Or,
                nameof(And) => And,
                nameof(Not) => Not,
                nameof(IsNull) => IsNull,
                nameof(NotNull) => NotNull,
                nameof(Slice) => Slice,
                nameof(Length) => Length,
                nameof(Replace) => Replace,
                nameof(RegexReplace) => RegexReplace,
                nameof(Split) => Split,
                nameof(Trim) => Trim,
                // Lists
                nameof(ConsumeOne) => ConsumeOne,
                nameof(List) => List,
                nameof(ValueList) => ValueList,
                nameof(ListAdd) => ListAdd,
                nameof(ListGet) => ListGet,
                nameof(ListInsert) => ListInsert,
                nameof(ListRemove) => ListRemove,
                nameof(ListSet) => ListSet,
                nameof(ListClear) => ListClear,
                nameof(AsList) => AsList,
                nameof(AsValueList) => AsValueList,
                // Dict
                nameof(Dict) => Dict,
                nameof(DictSet) => DictSet,
                nameof(DictGet) => DictGet,
                nameof(DictClear) => DictClear,
                nameof(DictRemove) => DictRemove,
                nameof(AsDict) => AsDict,
                nameof(IsDictCompatible) => IsDictCompatible,
                nameof(ToNodesetData) => ToNodesetData,
                nameof(ToGeneralData) => ToGeneralData,
                // Nav
                nameof(NavGet) => NavGet,
                // Errors
                "Error" => Error.GenError,
                nameof(Error.IsError) => Error.IsError,
                nameof(Error.ErrorText) => Error.ErrorText,
                nameof(Error.ErrorCode) => Error.ErrorCode,
                _ => null,
            };
        }
        public virtual SymbolSet GetSymbols() {
            return new SymbolSet("Default library (no symbols)", null);
        }
        public void ClearDisposables() {
            // Nothing by default
        }
        #endregion

        #region Basic procedures

        #region Logical

        public ParameterResolverValue Or(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.Any(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }
        public ParameterResolverValue And(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.All(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }
        public ParameterResolverValue Not(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (ActionQueryHostBase.IsTruthyOrFalsy(args[0])) {
                return new ParameterResolverValue(false);
            } else {
                return new ParameterResolverValue(true);
            }
        }
        public ParameterResolverValue IsNull(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (args[0].Value == null) {
                return new ParameterResolverValue(true);
            } else {
                return new ParameterResolverValue(false);
            }
        }
        public ParameterResolverValue NotNull(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (args[0].Value == null) {
                return new ParameterResolverValue(false);
            } else {
                return new ParameterResolverValue(true);
            }
        }
        #endregion

        #region Arithmetic

        public ParameterResolverValue Random(HostInterface ctx, ParameterResolverValue[] args) {
            int min = 0;
            var random = new Random();
            if (args.Length > 0) {
                if (args[0].Value is int n) {
                    min = n;
                } else if (args[0].Value is long l) {
                    min = (int)l;
                }
                if (args.Length > 1) {
                    if (args[1].Value is int maxi) {
                        return new ParameterResolverValue(random.Next(min, maxi));
                    } else if (args[1].Value is long maxl) {
                        return new ParameterResolverValue(random.Next(min, (int)maxl));
                    } else {
                        return new ParameterResolverValue(random.Next(min)); // min is max
                    }
                } else {
                    return new ParameterResolverValue(random.Next(min)); // min is max
                }
            }
            return new ParameterResolverValue(random.Next());
        }

        public ParameterResolverValue Add(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Any(a => a.Value is double || a.Value is float)) // Double result
            {
                return new ParameterResolverValue(args.Sum(a => Convert.ToDouble(a.Value)));
            } else if (args.Any(a => a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is byte || a.Value is char)) {
                return new ParameterResolverValue(args.Sum(a => Convert.ToInt32(a.Value)));
            } else {
                return new ParameterResolverValue(null);
            }
        }
        public ParameterResolverValue Neg(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Neg needs single numeric argument");
            var v = args[0].Value;
            if (v is double || v is float) {
                return new ParameterResolverValue(-Convert.ToDouble(v));
            } else if (v is int || v is uint || v is short || v is ushort || v is char || v is byte) {
                return new ParameterResolverValue(-Convert.ToInt32(v));
            } else if (v is long || v is ulong) {
                return new ParameterResolverValue(-Convert.ToInt64(v));
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue TryAdd(HostInterface ctx, ParameterResolverValue[] args) {
            try {
                return Add(ctx, args);
            } catch {
                return new ParameterResolverValue(null);
            }
        }

        #endregion

        #region Enumeration and collections
        /// <summary>
        /// Consumes one item from a List or queue
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue ConsumeOne(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("ConsumeOne takes one argument - a collection ");
            var _coll = args[0].Value;
            if (_coll is Queue<ParameterResolverValue> queue) {
                if (queue.Count > 0) {
                    return queue.Dequeue();
                }
            } else if (_coll is IList<ParameterResolverValue> list) {
                if (list.Count > 0) {
                    var r = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    return r;
                }
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue List(HostInterface ctx, ParameterResolverValue[] args) {
            var list = new List<ParameterResolverValue>(); // The new list
            if (args.Length > 0) {
                for (int i = 0; i < args.Length; i++) {
                    var arg = args[i];
                    if (arg.Value is IEnumerable<ParameterResolverValue> lp) {
                        list.AddRange(lp);
                    } else {
                        list.Add(arg);
                    }
                }
            }
            return new ParameterResolverValue(list);
        }
        public ParameterResolverValue ValueList(HostInterface ctx, ParameterResolverValue[] args) {
            var list = new ValueList<ParameterResolverValue>(); // The new list
            if (args.Length > 0) {
                for (int i = 0; i < args.Length; i++) {
                    var arg = args[i];
                    if (arg.Value is IEnumerable<ParameterResolverValue> lp) {
                        list.AddRange(lp);
                    } else {
                        list.Add(arg);
                    }
                }
            }
            return new ParameterResolverValue(list);
        }
        public ParameterResolverValue ListAdd(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("ListAdd requires some arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list != null) {
                for (int i = 1; i < args.Length; i++) {
                    list.Add(args[i]); // TODO: Should we break lists?
                }
            } else {
                return new ParameterResolverValue(null);
            }
            return new ParameterResolverValue(list);
        }
        public ParameterResolverValue ListRemove(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("ListRemove requires some arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list != null) {
                var indices = args.Skip(1).OrderByDescending(a => Convert.ToInt32(a.Value));
                if (indices.Count() > 0) {
                    foreach (var index in indices) {
                        if (IsNumeric(index.Value)) {
                            var i = Convert.ToInt32(index.Value);
                            list.RemoveAt(i);
                        }
                    }
                }
            } else {
                return new ParameterResolverValue(null);
            }
            return new ParameterResolverValue(list);
        }
        public ParameterResolverValue ListInsert(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 3) throw new ArgumentException("ListInsert requires 3 arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            if (!IsNumeric(args[1].Value)) throw new ArgumentException("ListInsert argument 2 (index) is not numeric");
            var index = Convert.ToInt32(args[1].Value);
            list.Insert(index, args[2]);
            return new ParameterResolverValue(list);
        }
        public ParameterResolverValue ListGet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("ListGet requires 2 arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            if (!IsNumeric(args[1].Value)) throw new ArgumentException("ListGet index (arg 2) is not numeric");
            var index = Convert.ToInt32(args[1].Value);
            if (index >= 0 && index < list.Count) {
                return list[index];
            } else {
                return new ParameterResolverValue(null);
            }
        }
        public ParameterResolverValue ListSet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 3) throw new ArgumentException("ListSet requires 3 arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            if (!IsNumeric(args[1].Value)) throw new ArgumentException("ListSet index (arg 2) is not numeric");
            var index = Convert.ToInt32(args[1].Value);
            if (index >= 0 && index < list.Count) {
                list[index] = args[2];
                return new ParameterResolverValue(list);
            } else {
                throw new IndexOutOfRangeException("ListSet index is out of range");
            }
        }
        public ParameterResolverValue ListClear(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("ListClear requires 1 argument");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            list.Clear();
            return new ParameterResolverValue(list);
        }
        private ParameterResolverValue _AsList<L>(HostInterface ctx, ParameterResolverValue[] args) where L: List<ParameterResolverValue>, new() {
            if (args.Length != 1) throw new ArgumentException("AsList requires 1 argument");
            var arg = args[0].Value;
            if (arg is string) {
                return new ParameterResolverValue(new List<ParameterResolverValue>() { new ParameterResolverValue(arg) });
            }
            var list = new L();
            if (arg is IDictionary argd) {
                foreach (object o in argd.Values) {
                    list.Add(new ParameterResolverValue(o));
                }
                return new ParameterResolverValue(list);
            }
            if (arg is IEnumerable arge) {
                foreach (object o in arge) {
                    list.Add(new ParameterResolverValue(o));
                }
                return new ParameterResolverValue(list);
            }
            list.Add(new ParameterResolverValue(arg));
            return new ParameterResolverValue(arg);
        }
        public ParameterResolverValue AsList(HostInterface ctx, ParameterResolverValue[] args) {
            return _AsList<List<ParameterResolverValue>>(ctx, args);
        }
        public ParameterResolverValue AsValueList(HostInterface ctx, ParameterResolverValue[] args) {
            return _AsList<ValueList<ParameterResolverValue>>(ctx, args);
        }
        #endregion

        #region Dictionary (Dict)
        public ParameterResolverValue Dict(HostInterface ctx, ParameterResolverValue[] args) {
            var dict = new Dictionary<string, ParameterResolverValue>(); // The new dictionary
            if (args.Length > 0) {
                if (args.Length % 2 != 0) throw new ArgumentException("Dict accpets only even number of arguments.");
                for (int i = 0; i < args.Length / 2; i++) {
                    var key = Convert.ToString(args[i * 2].Value);
                    var val = args[i * 2 + 1];
                    dict.TryAdd(key, val);
                }
            }
            return new ParameterResolverValue(dict);
        }
        public ParameterResolverValue DictSet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length % 2 == 0) throw new ArgumentException("DictSet requires odd number of  arguments");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null) return new ParameterResolverValue(null);

            for (int i = 1; i < args.Length; i += 2) {
                if (args[i].Value == null) continue;
                var key = Convert.ToString(args[i].Value);
                var val = args[i + 1];
                dict[key] = val;
            }
            return new ParameterResolverValue(dict);
        }
        public ParameterResolverValue DictGet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("DictGet requires two arguments");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null || args[1].Value == null) return new ParameterResolverValue(null);

            var key = Convert.ToString(args[1].Value);
            if (!dict.ContainsKey(key)) return new ParameterResolverValue(null);
            return dict[key];
        }
        public ParameterResolverValue DictClear(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("DictClear requires one argument");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null) return new ParameterResolverValue(null);
            dict.Clear();
            return new ParameterResolverValue(dict);
        }
        public ParameterResolverValue DictRemove(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("DictRemove requires at least one argument");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null) return new ParameterResolverValue(null);

            var keys = args.Skip(1).Select(a => Convert.ToString(a.Value));
            foreach (var key in keys) {
                dict.Remove(key);
            }
            return new ParameterResolverValue(dict);
        }
        public ParameterResolverValue AsDict(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("DictClear requires at least one argument");
            object arg1 = args[0].Value;
            object arg2 = null;
            if (args.Length > 1) arg2 = args[1].Value;
            var dict = new Dictionary<string, ParameterResolverValue>();

            if (arg1 is IDictionary _dict) {// Ignore the second argument entirely
                foreach (DictionaryEntry e in _dict) {
                    dict.TryAdd(Convert.ToString(e.Key), new ParameterResolverValue(e.Value));
                }
                return new ParameterResolverValue(dict);
            } else if (arg1 is IEnumerable enm1 && arg2 is IEnumerable enm2) {
                var keys = new List<string>();
                foreach (object el in enm1) {
                    if (el == null) {
                        keys.Add(null);
                    } else {
                        keys.Add(Convert.ToString(el));
                    }
                }
                var vals = new List<object>();
                foreach (object el in enm2) {
                    vals.Add(el);
                }
                var count = Math.Min(keys.Count, vals.Count);
                for (int i = 0; i < count; i++) {
                    if (keys[i] == null) continue;
                    dict.TryAdd(keys[i], new ParameterResolverValue(vals[i]));
                }
                return new ParameterResolverValue(dict);
            }
            return new ParameterResolverValue(dict);
        }
        public ParameterResolverValue IsDictCompatible(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("DictClear requires at least one argument");
            object arg1 = args[0].Value;
            object arg2 = null;
            if (args.Length > 1) arg2 = args[1].Value;
            if (arg1 is IDictionary || (arg1 is IEnumerable && arg2 is IEnumerable)) return new ParameterResolverValue(true);
            return new ParameterResolverValue(false);
        }
        #endregion

        #region Navigation through Dict/List structures
        public ParameterResolverValue NavGet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 2) throw new ArgumentException("NavGetGet requires two or more arguments");
            ParameterResolverValue[] chain = null;
            if (args[1].Value is List<ParameterResolverValue> plist) {
                chain = plist.ToArray();
            } else {
                chain = args.Skip(1).ToArray();
            }

            object cur = args[0].Value;
            for (int i = 0; i < chain.Length; i++) {
                var idx = chain[i];
                if (cur is IDictionary<string, ParameterResolverValue> pdict) {
                    string skey = Convert.ToString(idx.Value);
                    if (pdict.ContainsKey(skey)) {
                        cur = pdict[skey].Value;
                        continue;
                    } else {
                        cur = null;
                        break;
                    }
                } else if (cur is List<ParameterResolverValue> list) {
                    int index = Convert.ToInt32(idx.Value);
                    if (index >= 0 && index < list.Count) {
                        cur = list[index].Value;
                        continue;
                    } else {
                        cur = null;
                        break;
                    }
                }
            }
            return new ParameterResolverValue(cur);
        }
        #endregion

        #region Comparisons
        public ParameterResolverValue Equal(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Equal needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) == Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) == Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) == 0);
            }
        }
        public ParameterResolverValue Greater(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) > Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) > Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) > 0);
            }
        }
        public ParameterResolverValue Lower(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Lower needs two arguments");
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) < Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) < Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) < 0);
            }
        }

        #endregion

        #region Strings

        public ParameterResolverValue Concat(HostInterface ctx, ParameterResolverValue[] args)
        {
            return new ParameterResolverValue(String.Concat(args.Select(a => a.Value != null ? a.Value.ToString() : "")));
        }
        public ParameterResolverValue IsEmpty(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsEmpty requires single argument.");
            var val = args[0].Value as string;
            return new ParameterResolverValue(string.IsNullOrWhiteSpace(val));
        }
        public ParameterResolverValue Slice(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length >= 2)
            {
                var str = Convert.ToString(args[0].Value);
                int start = Convert.ToInt32(args[1].Value);
                var end = str.Length;
                if (args.Length > 2)
                {
                    end = Convert.ToInt32(args[2].Value);
                }
                if (start >= 0 && start <= str.Length && end > start && end <= str.Length)
                {
                    return new ParameterResolverValue(str.Substring(start, end - start));
                } 
                else
                {
                    return new ParameterResolverValue(string.Empty);
                }
            } 
            else
            {
                throw new ArgumentException("Slice - incorrect number of arguments, 2 o3 are expected.");
            }
        }
        public ParameterResolverValue Length(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Length accepts exactly one argument.");
            if (args[0].Value is string s)
            {
                return new ParameterResolverValue(s.Length);
            }
            else if (args[0].Value is ICollection coll)
            {
                return new ParameterResolverValue(coll.Count);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue Replace(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 3) throw new ArgumentException("Replace accepts exactly 3  argument.");
            string str = Convert.ToString(args[0].Value);
            string patt = Convert.ToString(args[1].Value);
            string rep = Convert.ToString(args[2].Value);
            return new ParameterResolverValue(str.Replace(patt, rep));
        }
        public ParameterResolverValue RegexReplace(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 3) throw new ArgumentException("RegexReplace accepts exactly 3  argument.");
            string str = Convert.ToString(args[0].Value);
            string patt = Convert.ToString(args[1].Value);
            string rep = Convert.ToString(args[2].Value);
            if (!string.IsNullOrWhiteSpace(patt))
            {
                Regex rex = new Regex(patt);
                return new ParameterResolverValue(rex.Replace(str, rep));
            } 
            else
            {
                return new ParameterResolverValue(str);
            }
            
        }
        public ParameterResolverValue Split(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("Split requires at least 1 argument");
            var str = Convert.ToString(args[0].Value);
            if (args.Length > 1)
            {
                var sep = Convert.ToString(args[1].Value);
                if (string.IsNullOrEmpty(sep))
                {
                    return new ParameterResolverValue(new List<ParameterResolverValue>() { new ParameterResolverValue(str) });
                } 
                else
                {
                    return new ParameterResolverValue(str.Split(sep).Select(s => new ParameterResolverValue(s)).ToList());
                }
            
            }
            else
            {
                return new ParameterResolverValue(str.Split(',').Select(s => new ParameterResolverValue(s)).ToList());
            }
        }
        public ParameterResolverValue Trim(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Trim requires oneparameter.");
            var str = Convert.ToString(args[0].Value);
            return new ParameterResolverValue(str.Trim());
        }
        #endregion

        #region Typing

        public ParameterResolverValue Cast(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Cast requires two arguments.");
            string stype = args[0].Value as string;
            if (stype == null) throw new ArgumentException("Parameter 1 of Case must be string specifying the type to convert to (string,int,double,bool)");
            switch (stype)
            {
                case "string":
                    return new ParameterResolverValue(Convert.ToString(args[1].Value));
                case "bool":
                    return new ParameterResolverValue(Convert.ToBoolean(args[1].Value));
                case "int":
                    return new ParameterResolverValue(Convert.ToInt32(args[1].Value));
                case "long":
                    return new ParameterResolverValue(Convert.ToInt64(args[1].Value));
                case "double":
                    return new ParameterResolverValue(Convert.ToDouble(args[1].Value));
                default:
                    throw new ArgumentException("Parameter 1 contains unrecognized type name valida types are: string,int,long, double,bool");
            }
        }

        public ParameterResolverValue TypeOf(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("TypeOf requires one argument.");
            if (args[0].Value == null) return new ParameterResolverValue("null");
            if (args[0].Value is string) return new ParameterResolverValue("string");
            Type tc = args[0].Value.GetType();
            if (tc == typeof(int) || tc == typeof(uint)) return new ParameterResolverValue("int");
            if (tc == typeof(long) || tc == typeof(ulong)) return new ParameterResolverValue("long");
            if (tc == typeof(double) || tc == typeof(float)) return new ParameterResolverValue("double");
            if (tc == typeof(short) || tc == typeof(ushort)) return new ParameterResolverValue("short");
            if (tc == typeof(char) || tc == typeof(byte)) return new ParameterResolverValue("byte");
            if (tc == typeof(bool)) return new ParameterResolverValue("bool");

            return new ParameterResolverValue("unknown");
        }

        public static bool IsNumeric(object v)  {
            if (v == null) return false;
            Type tc = v.GetType();
            if (tc == typeof(int) || tc == typeof(uint) || tc == typeof(long) || tc == typeof(ulong)
                || tc == typeof(double) || tc == typeof(float) || tc == typeof(short) || tc == typeof(ushort) ||
                tc == typeof(char) || tc == typeof(byte)) return true;
            return false;
        }
        public ParameterResolverValue IsNumeric(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsNumeric requires one argument.");
            return new ParameterResolverValue(IsNumeric(args[0].Value));
        }

        #endregion

        #endregion

        #region Settings
        public ParameterResolverValue GSetting(HostInterface _ctx, ParameterResolverValue[] args)
        {
            KraftGlobalConfigurationSettings kgcf = null;
            var ctx = _ctx as IDataLoaderContext;
            if (ctx != null)
            {
                kgcf = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            }
            else
            {
                var nctx = _ctx as INodePluginContext;
                if (nctx != null)
                {
                    kgcf = nctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                }
            }
            if (kgcf == null)
            {
                throw new Exception("Cannot obtain Kraft global settings");
            }
            if (args.Length != 1)
            {
                throw new ArgumentException($"GSetting accepts one argument, but {args.Length} were given.");
            }
            var name = args[0].Value as string;

            if (name == null)
            {
                throw new ArgumentException($"GSetting argument must be string - the name of the global kraft setting to obtain.");
            }
            switch (name)
            {
                case "EnvironmentName":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.EnvironmentName);
                case "ContentRootPath":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.ContentRootPath);
                case "ApplicationName":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.ApplicationName);
                case "StartModule":
                    return new ParameterResolverValue(kgcf.GeneralSettings.DefaultStartModule);
                case "ClientId":
                    return new ParameterResolverValue(kgcf.GeneralSettings.ClientId);

            }
            throw new ArgumentException($"The setting {name} is not supported");
        }

        #endregion

        public ParameterResolverValue Throw(HostInterface ctx, ParameterResolverValue[] args)
        {
            string extext = null;
            if (args.Length > 0)
            {
                if (args[0].Value is string)
                {
                    extext = args[0].Value as string;
                }
            } 
            else
            {
                extext = "Exception raised intentionally from an ActionQuery code";
            }
            throw new Exception(extext);
        }
        public ParameterResolverValue Debug(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length > 0) {
                string log = string.Join(',', args.Select(a => Convert.ToString(a.Value)));
                KraftLogger.LogError($"ACDebug: {log}\n");
                return new ParameterResolverValue(args[0]);
            }
            return new ParameterResolverValue(null);
        }

        #region Additional helpers for internal use
        public static ParameterResolverValue ConvertFromGenericData(object data) {
            if (data is Dictionary<string, object> dict) {
                return new ParameterResolverValue(dict.ToDictionary(kv => kv.Key, kv => new ParameterResolverValue(ConvertFromGenericData(kv.Value))));
            } else if (data is string s) {
                return new ParameterResolverValue(s);
            } else if (data is IEnumerable enmr) {
                var list = new List<ParameterResolverValue>();
                foreach (object o in enmr) {
                    list.Add(new ParameterResolverValue(ConvertFromGenericData(o)));
                }
                return new ParameterResolverValue(list);
            } else {
                return new ParameterResolverValue(data);
            }
        }
        public static object ConvertToGenericData(object data) {
            if (data is Dictionary<string, ParameterResolverValue> dict) {
                return (dict.ToDictionary(kv => kv.Key, kv => ConvertToGenericData(kv.Value.Value))); // <string, object>
            } else if (data is ParameterResolverValue pv) {
                return ConvertToGenericData(pv.Value);
            } else if (data is ValueList<ParameterResolverValue> vlst) {
                var list = new List<object>();
                foreach (var v in vlst) {
                    list.Add(v.Value);
                }
                return list;
            } else if (data is IEnumerable<ParameterResolverValue> lst) {
                var list = new List<Dictionary<string, object>>();
                foreach (var v in lst) {
                    if (v.Value is Dictionary<string, ParameterResolverValue> pdict) {
                        list.Add(ConvertToGenericData(pdict) as Dictionary<string, object>);
                    } else {
                        throw new FormatException("A list contains non-dictionary elements. Error occurred while converting to generic data.");
                    }
                }
                return list;
            } else {
                return data;
            }
        }

        public static object ConvertToGeneralData(object data) {
            if (data is Dictionary<string, ParameterResolverValue> dict) {
                return (dict.ToDictionary(kv => kv.Key, kv => ConvertToGeneralData(kv.Value.Value))); // <string, object>
            } else if (data is ParameterResolverValue pv) {
                return ConvertToGenericData(pv.Value);
            } else if (data is IEnumerable<ParameterResolverValue> vlst) {
                var list = new List<object>();
                foreach (var v in vlst) {
                    list.Add(ConvertToGeneralData(v.Value));
                }
                return list;
            } else {
                return data;
            }
        }

        #endregion

        #region Conversions
        public ParameterResolverValue ToNodesetData(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("ToData takes one argument");
            return new ParameterResolverValue(ConvertToGenericData(args[0].Value));
        }
        public ParameterResolverValue ToGeneralData(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("ToGeneralData takes one argument");
            return new ParameterResolverValue(ConvertToGeneralData(args[0].Value));
        }
        #endregion
    }
}
