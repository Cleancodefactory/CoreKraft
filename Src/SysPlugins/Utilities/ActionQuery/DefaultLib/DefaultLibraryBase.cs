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
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Enumerations;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryBase<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class {

        #region IActionQueryLibrary
        public virtual HostedProc<HostInterface> GetProc(string name) {
            return name switch {
                nameof(Add) => Add,
                nameof(TryAdd) => TryAdd,
                nameof(Sub) => Sub,
                nameof(TrySub) => TrySub,
                nameof(Concat) => Concat,
                nameof(Cast) => Cast,
                nameof(GSetting) => GSetting,
                nameof(Throw) => Throw,
                nameof(Debug) => Debug,
                nameof(NType) => NType,
                nameof(IsEmpty) => IsEmpty,
                nameof(TypeOf) => TypeOf,
                nameof(To8601String) => To8601String,
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
                nameof(RegexMatch) => RegexMatch,
                nameof(Split) => Split,
                nameof(Trim) => Trim,
                nameof(EncodeBase64) => EncodeBase64,
                nameof(DecodeBase64) => DecodeBase64,
                nameof(MD5) => MD5,

                // Lists
                nameof(ConsumeOne) => ConsumeOne,
                nameof(List) => List,
                nameof(ValueList) => ValueList,
                nameof(IsList) => IsList,
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
                nameof(IsDict) => IsDict,
                nameof(DictSet) => DictSet,
                nameof(DictGet) => DictGet,
                nameof(DictClear) => DictClear,
                nameof(DictRemove) => DictRemove,
                nameof(DictRemoveExcept) => DictRemoveExcept,
                nameof(AsDict) => AsDict,
                nameof(IsDictCompatible) => IsDictCompatible,
                nameof(ToNodesetData) => ToNodesetData,
                nameof(ToGeneralData) => ToGeneralData,
                nameof(FromNodesetData) => FromNodesetData,
                nameof(DictToJson) => DictToJson,
                nameof(JsonToDict) => JsonToDict,
                // Meta
                nameof(MetaADOResult) => MetaADOResult,
                nameof(MetaNode) => MetaNode,
                nameof(MetaRoot) => MetaRoot,
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

        [Function(nameof(Or), "Returns true if any of the arguments is a truthy value, otherwise returns false.")]
        [Parameter(0, "values", "array of truthy or false values", TypeFlags.List)]
        [Result("Returns boolean", TypeFlags.Bool)]
        public ParameterResolverValue Or(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.Any(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }

        [Function(nameof(And), "Returns true if all of the arguments are truthy values, otherwise returns false.")]
        [Parameter(0, "values", "array of truthy or false values", TypeFlags.List)]
        [Result("Returns boolean", TypeFlags.Bool)]
        public ParameterResolverValue And(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.All(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }

        [Function(nameof(Not), "Inverts the truthiness of the argument.")]
        [Parameter(0, "value", "Boolean value to invert", TypeFlags.Bool)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue Not(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (ActionQueryHostBase.IsTruthyOrFalsy(args[0])) {
                return new ParameterResolverValue(false);
            } else {
                return new ParameterResolverValue(true);
            }
        }

        [Function(nameof(IsNull), "Returns true if the argument is null, otherwise returns false.")]
        [Parameter(0, "value", "value to check", TypeFlags.Varying)]
        [Result("Returns boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsNull(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (args[0].Value == null) {
                return new ParameterResolverValue(true);
            } else {
                return new ParameterResolverValue(false);
            }
        }

        [Function(nameof(NotNull), "Returns true if the argument is not null, otherwise returns false.")]
        [Parameter(0, "value", "value to check", TypeFlags.Varying)]
        [Result("Returns boolean", TypeFlags.Bool)]
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

        [Function(nameof(Random), "Generates a random integer. If min is specified the integer is at least min or greater. If also max is specified the generated number is equal or greater than min, but lower than max.")]
        [Parameter(0, "min_number", "min number", TypeFlags.Optional | TypeFlags.Int | TypeFlags.Double)]
        [Parameter(1, "max_number", "max number", TypeFlags.Optional | TypeFlags.Int | TypeFlags.Double)]
        [Result("Returns a int32 number", TypeFlags.Int)]
        public ParameterResolverValue Random(HostInterface ctx, ParameterResolverValue[] args) {
            int min = 1;
            int max = 100;

            if (args.Length > 0) {
                if (args[0].Value is int n) {
                    min = n;
                } else if (args[0].Value is long l) {
                    min = (int)l;
                }
                if (args.Length > 1) {
                    if (args[1].Value is int maxi) {
                        max = maxi;
                    } else if (args[1].Value is long maxl) {
                        max = (int)maxl;
                    }
                    if (min >= max) {
                        throw new ArgumentException("Min value is greater or equals than Max value.");
                    }
                }
            }

            return new ParameterResolverValue(RandomNumberGenerator.GetInt32(min, max));
        }

        [Function(nameof(Add), "Returns the sum of all the arguments. If called without arguments will return null.")]
        [Parameter(0, "values", "numeric values to add", TypeFlags.List)]
        [Result("Returns numeric value or null", TypeFlags.Int | TypeFlags.Null)]
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

        [Function(nameof(Neg), "Returns the argument with inverted sign if the value is numeric and null otherwise. It is not recommended to use this with unsigned numbers.")]
        [Parameter(0, "value", "numeric value to invert", TypeFlags.Int | TypeFlags.Double)]
        [Result("Returns numeric value or null", TypeFlags.Int | TypeFlags.Null)]
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
        [Function(nameof(Sub),"Subtracts the argument 2 from a argument 1, requires 2 arguments. At least one of them have to be numeric and the other convertible to the same numeric type")]
        public ParameterResolverValue Sub(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Sub requires two parameters");
            if (args[0].Value == null || args[1].Value == null) return new ParameterResolverValue(null);
            if (args.Any(a => a.Value is double || a.Value is float)) // Double result
            {
                return new ParameterResolverValue(Convert.ToDouble(args[0].Value) - Convert.ToDouble(args[1].Value));
            } else if (args.Any(a => a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is byte || a.Value is char)) {
                return new ParameterResolverValue(Convert.ToInt32(args[0].Value) - Convert.ToInt32(args[1].Value));
            } else {
                return new ParameterResolverValue(null);
            }
        }

        [Function(nameof(TryAdd), "Like Add, but will return null instead of throwing an exception if the conversion fails.")]
        [Parameter(0, "values", "numeric values to add", TypeFlags.List)]
        [Result("Returns numeric value or null if an exception is thrown", TypeFlags.Int | TypeFlags.Null)]
        public ParameterResolverValue TryAdd(HostInterface ctx, ParameterResolverValue[] args) {
            try {
                return Add(ctx, args);
            } catch {
                return new ParameterResolverValue(null);
            }
        }
        [Function(nameof(TrySub),"Subtracts 2 arguments, returns null on exception (usually exception during conversion)")]
        public ParameterResolverValue TrySub(HostInterface ctx, ParameterResolverValue[] args) {
            try {
                return Sub(ctx, args);
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


        // Check Length
        [Function(nameof(ConsumeOne), "When the argument is an AC list removes the last argument of the list and returns it. If the list is empty returns null. When used with queue, dequeues one element and returns it. If the queue is empty returns null.")]
        [Parameter(0, "List", "AC list or queue to consume from", TypeFlags.List)]
        [Result("Returns AC list/Queue consumed element or null", TypeFlags.List | TypeFlags.Null)]
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

        [Function(nameof(List), "Creates and AC list with the arguments added as items in the list. With no arguments it will create an empty list.")]
        [Parameter(0, "Items", "items to be added to the new list", TypeFlags.Optional | TypeFlags.List)]
        [Result("Returns: the created AC list.", TypeFlags.List)]
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

        [Function(nameof(ValueList), "The same as List, but the created AC list is marked as one containing values. See ToNodesetData for details.")]
        [Parameter(0, "Items", "items to be added to the new list", TypeFlags.Optional | TypeFlags.List)]
        [Result("Returns an AC list", TypeFlags.List)]
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

        [Function(nameof(IsList), "Checks if arg is a List created by List or ValueList or returned from another function.")]
        [Parameter(0, "Element", "argument to check if it is a List/ValueList", TypeFlags.List)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsList(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("IsList requires an argument");
            return new ParameterResolverValue(args[0].Value is List<ParameterResolverValue>);
        }

        [Function(nameof(ListAdd), "Adds items at the end of an AC list.")]
        [Parameter(0, "List", "the AC list", TypeFlags.List)]
        [Parameter(1, "Items_To_Add", " 1 - n : items to add to list", TypeFlags.List)]
        [Result("Returns the AC list or null", TypeFlags.List | TypeFlags.Null)]
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

        [Function(nameof(ListRemove), "Removes the element(s) at the specified indexes. The indexes are the positions before doing any removal.")]
        [Parameter(0, "List", "the AC List", TypeFlags.List)]
        [Parameter(1, "Indexes_To_Remove", "1 - n : item indexes to remove", TypeFlags.List)]
        [Result("Returns: the modified AC list or null", TypeFlags.List | TypeFlags.Null)]
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

        // Check Length
        [Function(nameof(ListInsert), "Inserts an element in the AC list at the specified index. Will fail only if the underlying List.insert method fails. Use it with the same assumptions.")]
        [Parameter(0, "List", "the AC list", TypeFlags.List)]
        [Parameter(1, "Insert_Index", "index to insert to", TypeFlags.Int)]
        [Parameter(2, "Item", "element to insert", TypeFlags.Varying)]
        [Result("Returns the AC list", TypeFlags.List | TypeFlags.Null)]
        public ParameterResolverValue ListInsert(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 3) throw new ArgumentException("ListInsert requires 3 arguments");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            if (!IsNumeric(args[1].Value)) throw new ArgumentException("ListInsert argument 2 (index) is not numeric");
            var index = Convert.ToInt32(args[1].Value);
            list.Insert(index, args[2]);
            return new ParameterResolverValue(list);
        }

        [Function(nameof(ListGet), "Gets the element at index and returns it. If the index is out of range returns null.")]
        [Parameter(0, "List", "the AC list", TypeFlags.List)]
        [Parameter(1, "Index", "element index", TypeFlags.Int)]
        [Result("Returns value at index or null", TypeFlags.Varying | TypeFlags.Null)]
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

        [Function(nameof(ListSet), "Sets an element in an AC list. The index must exist in the AC list or an exception will occur.")]
        [Parameter(0, "List", "the AC list", TypeFlags.List)]
        [Parameter(1, "Index", "element index", TypeFlags.Int)]
        [Parameter(2, "Value", "new element value", TypeFlags.Varying)]
        [Result("Returns the AC list or throws an exception!", TypeFlags.Varying | TypeFlags.Error)]
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

        [Function(nameof(ListClear), "Clears the AC list and returns it.")]
        [Parameter(0, "List", "the AC list", TypeFlags.List)]
        [Result("Returns the cleared AC list", TypeFlags.List)]
        public ParameterResolverValue ListClear(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("ListClear requires 1 argument");
            var list = args[0].Value as IList<ParameterResolverValue>;
            if (list == null) return new ParameterResolverValue(null);
            list.Clear();
            return new ParameterResolverValue(list);
        }
        private ParameterResolverValue _AsList<L>(HostInterface ctx, ParameterResolverValue[] args) where L : List<ParameterResolverValue>, new() {
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

        // Check Length
        [Function(nameof(AsList), "Converts an externally obtained list-like object into AC list. This function can be also used to create a copy of an existing AC list. Just call AsList(list) and the result will be a copy of list.")]
        [Parameter(0, "Object", "list like object", TypeFlags.Object)]
        [Result("Returns an AC list", TypeFlags.List)]
        public ParameterResolverValue AsList(HostInterface ctx, ParameterResolverValue[] args) {
            return _AsList<List<ParameterResolverValue>>(ctx, args);
        }

        [Function(nameof(AsValueList), "The same as AsList, but marks the list as list of values. See ToNodesetData for more details.")]
        [Parameter(0, "Object", "list like object", TypeFlags.Object)]
        [Result("Returns an AC list", TypeFlags.List)]
        public ParameterResolverValue AsValueList(HostInterface ctx, ParameterResolverValue[] args) {
            return _AsList<ValueList<ParameterResolverValue>>(ctx, args);
        }
        #endregion

        #region Dictionary (Dict)

        // Check Length
        [Function(nameof(Dict), "Creates an AC dictionary. Can be used without arguments to create an empty one or with pairs of arguments to add elements on creation.")]
        [Parameter(0, "Key", "even index - key (repeating)", TypeFlags.Optional | TypeFlags.Varying)]
        [Parameter(1, "Value", "odd index - value (repeating)", TypeFlags.Optional | TypeFlags.Varying)]
        [Result("Returns a AC dictionary", TypeFlags.Dict)]
        public ParameterResolverValue Dict(HostInterface ctx, ParameterResolverValue[] args) {
            var dict = new Dictionary<string, ParameterResolverValue>(); // The new dictionary
            if (args.Length > 0) {
                if (args.Length % 2 != 0) throw new ArgumentException("Dict accepts only even number of arguments.");
                for (int i = 0; i < args.Length / 2; i++) {
                    var key = Convert.ToString(args[i * 2].Value);
                    var val = args[i * 2 + 1];
                    dict.TryAdd(key, val);
                }
            }
            return new ParameterResolverValue(dict);
        }

        [Function(nameof(IsDict), "Checks if arg is a Dict created by Dict or returned from another function.")]
        [Parameter(0, "Argument", "Argument to check", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsDict(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("IsDict requires an argument.");
            return new ParameterResolverValue(args[0].Value is Dictionary<string, ParameterResolverValue>);
        }

        // Check Length
        [Function(nameof(DictSet), "Sets elements of an AC dictionary. Returns: the same AC dictionary passed with the dict argument with the changes applied to it.")]
        [Parameter(0, "Dictionary", "the AC dictionary", TypeFlags.List)]
        [Parameter(1, "Key", "odd index - key (repeating)", TypeFlags.Varying)]
        [Parameter(2, "Value", "even index - value (repeating)", TypeFlags.Varying)]
        [Result("Returns the modified AC dictionary", TypeFlags.Dict)]
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

        [Function(nameof(DictGet), "Gets a value of an element in an AC dictionary. If the key is missing null wil be returned. Returns: the value of the key.")]
        [Parameter(0, "Dictionary", "the AC dictionary", TypeFlags.List)]
        [Parameter(1, "Index", "index of element to get", TypeFlags.Int)]
        [Result("Returns the value at index or null", TypeFlags.Varying | TypeFlags.Null)]
        public ParameterResolverValue DictGet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("DictGet requires two arguments");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null || args[1].Value == null) return new ParameterResolverValue(null);

            var key = Convert.ToString(args[1].Value);
            if (!dict.ContainsKey(key)) return new ParameterResolverValue(null);
            return dict[key];
        }

        [Function(nameof(DictClear), "Clears an AC dictionary And returns it.")]
        [Parameter(0, "Dictionary", "The AC Dictionary", TypeFlags.Dict)]
        [Result("Returns the cleared AC dictionary", TypeFlags.Dict | TypeFlags.Null)]
        public ParameterResolverValue DictClear(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("DictClear requires one argument");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null) return new ParameterResolverValue(null);
            dict.Clear();
            return new ParameterResolverValue(dict);
        }

        [Function(nameof(DictRemove), "Removes elements from an AC dictionary. All the specified keys are removed from the dictionary.")]
        [Parameter(0, "Dictionary", "the AC dictionary", TypeFlags.Dict)]
        [Parameter(1, "Keys", "1 - n keys to remove", TypeFlags.Varying)]
        [Result("Returns the modified AC dictionary", TypeFlags.Dict | TypeFlags.Null)]
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

        [Function(nameof(DictRemoveExcept), "Clears all the values in the dictionary except those listed after the first argument, which is the dictionary to clear.")]
        [Parameter(0, "Dictionary", "the AC Dictionary", TypeFlags.Dict)]
        [Parameter(1, "Keys", "1 - n keys to keep", TypeFlags.Varying)]
        [Result("Returns the modified AC dictionary", TypeFlags.Dict | TypeFlags.Null)]
        public ParameterResolverValue DictRemoveExcept(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("DictRemoveExcept requires at least one argument");
            var dict = args[0].Value as IDictionary<string, ParameterResolverValue>;
            if (dict == null) return new ParameterResolverValue(null);

            var preservekeys = args.Skip(1).Select(a => Convert.ToString(a.Value)).ToList();
            foreach (var key in dict.Keys) {
                if (!preservekeys.Contains(key)) {
                    dict.Remove(key);
                }
            }
            return new ParameterResolverValue(dict);
        }

        // Check Length
        [Function(nameof(AsDict), "Converts external object to AC dictionary. The function does not throw exceptions for inappropriate arguments and will return an empty dictionary in such a case. ")]
        [Parameter(0, "Object", "object to convert to AC dictionary", TypeFlags.Varying)]
        [Result("Returns a AC dictionary", TypeFlags.Null)]
        public ParameterResolverValue AsDict(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("AsDict requires at least one argument");
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

        [Function(nameof(IsDictCompatible), "Checks if AsDict can succeed with the same arguments. See AsDict for more details about the arguments.")]
        [Parameter(0, "Object", "object to evaluate AC dictionary compatability", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsDictCompatible(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("IsDictCompatible requires at least one argument");
            object arg1 = args[0].Value;
            object arg2 = null;
            if (args.Length > 1) arg2 = args[1].Value;
            if (arg1 is IDictionary || (arg1 is IEnumerable && arg2 is IEnumerable)) return new ParameterResolverValue(true);
            return new ParameterResolverValue(false);
        }
        #endregion

        #region Navigation through Dict/List structures


        //No doc ATM
        [Function(nameof(NavGet), "")]
        public ParameterResolverValue NavGet(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 2) throw new ArgumentException("NavGetGet requires two or more arguments");
            ParameterResolverValue[] chain = null;
            if (args[1].Value is List<ParameterResolverValue> plist) {
                chain = plist.ToArray();
            } else {
                chain = args.Skip(1).ToArray();
            }

            ParameterResolverValue cur = args[0];
            for (int i = 0; i < chain.Length; i++) {
                var idx = chain[i];
                if (cur.Value is IDictionary<string, ParameterResolverValue> pdict) {
                    string skey = Convert.ToString(idx.Value);
                    if (pdict.ContainsKey(skey)) {
                        cur = pdict[skey]; //.Value;
                        continue;
                    } else {
                        cur = new ParameterResolverValue(null);
                        break;
                    }
                } else if (cur.Value is List<ParameterResolverValue> list) {
                    int index = Convert.ToInt32(idx.Value);
                    if (index >= 0 && index < list.Count) {
                        cur = list[index];
                        continue;
                    } else {
                        cur = new ParameterResolverValue(null);
                        break;
                    }
                } else {
                    // Non-collection - cannot navigate more - null
                    cur = new ParameterResolverValue(null);
                }
            }
            return cur;
        }
        #endregion

        #region Comparisons

        // Check Length
        [Function(nameof(Equal), " Compares the two arguments. If any of the two is null false is returned. The arguments are converted to strings and compared - case sensitive.")]
        [Parameter(0, "First", "Argument to compare", TypeFlags.Varying)]
        [Parameter(1, "Second", "Argument to compare", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue Equal(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Equal needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null)) {
                return new ParameterResolverValue(false);
            } else if (args.Any(a => a.Value is double || a.Value is float)) {
                return new ParameterResolverValue(Convert.ToDouble(v1) == Convert.ToDouble(v2));
            } else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool)) {
                return new ParameterResolverValue(Convert.ToInt64(v1) == Convert.ToInt64(v2));
            } else {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) == 0);
            }
        }

        [Function(nameof(Greater), "Compares the arguments and if arg1 is greater than arg2, returns true, otherwise returns false. See Equal for how the arguments are compared.")]
        [Parameter(0, "First", "Argument to compare", TypeFlags.Varying)]
        [Parameter(1, "Second", "Argument to compare", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue Greater(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null)) {
                return new ParameterResolverValue(false);
            } else if (args.Any(a => a.Value is double || a.Value is float)) {
                return new ParameterResolverValue(Convert.ToDouble(v1) > Convert.ToDouble(v2));
            } else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool)) {
                return new ParameterResolverValue(Convert.ToInt64(v1) > Convert.ToInt64(v2));
            } else {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) > 0);
            }
        }

        [Function(nameof(Lower), "Compares the arguments and if arg1 is smaller than arg2, returns true, otherwise returns false. See Equal for how the arguments are compared.")]
        [Parameter(0, "First", "Argument to compare", TypeFlags.Varying)]
        [Parameter(1, "Second", "Argument to compare", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue Lower(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Lower needs two arguments");
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null)) {
                return new ParameterResolverValue(false);
            } else if (args.Any(a => a.Value is double || a.Value is float)) {
                return new ParameterResolverValue(Convert.ToDouble(v1) < Convert.ToDouble(v2));
            } else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool)) {
                return new ParameterResolverValue(Convert.ToInt64(v1) < Convert.ToInt64(v2));
            } else {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) < 0);
            }
        }

        #endregion

        #region Strings

        [Function(nameof(Concat), "Returns the arguments turned to strings and concatenated in the order in which they appear. If any argument is null, it will be treated as an empty string.")]
        [Parameter(0, "Strings", "array of strings to concat", TypeFlags.String)]
        [Result("Returns the concatenated string", TypeFlags.String)]
        public ParameterResolverValue Concat(HostInterface ctx, ParameterResolverValue[] args) {
            return new ParameterResolverValue(String.Concat(args.Select(a => a.Value != null ? a.Value.ToString() : "")));
        }

        [Function(nameof(IsEmpty), "Checks if a string is empty. Does not attempt to convert the argument to string, as result any non-string value will be considered to be empty.")]
        [Parameter(0, "String", "element to check", TypeFlags.String)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsEmpty(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("IsEmpty requires single argument.");
            var val = args[0].Value as string;
            return new ParameterResolverValue(string.IsNullOrWhiteSpace(val));
        }

        [Function(nameof(Slice), "Returns a string containing the characters from the original string starting at start and ending at end (not including the end). If end is omitted, returns to the end of the string.")]
        [Parameter(0, "String", "string to slice", TypeFlags.String)]
        [Parameter(1, "Start_Index","index to start slicing from", TypeFlags.Int)]
        [Result("Returns a string", TypeFlags.String)]
        public ParameterResolverValue Slice(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length >= 2) {
                var str = Convert.ToString(args[0].Value);
                int start = Convert.ToInt32(args[1].Value);
                var end = str.Length;
                if (args.Length > 2) {
                    end = Convert.ToInt32(args[2].Value);
                }
                if (start >= 0 && start <= str.Length && end > start && end <= str.Length) {
                    return new ParameterResolverValue(str.Substring(start, end - start));
                } else {
                    return new ParameterResolverValue(string.Empty);
                }
            } else {
                throw new ArgumentException("Slice - incorrect number of arguments, 2 or more are expected.");
            }
        }

        [Function(nameof(Length), "Depending on the type of the argument, returns the length of the string, list or dictionary.")]
        [Parameter(0, "Argument", "parsed argument", TypeFlags.Varying)]
        [Result("Returns a int32 length of argument", TypeFlags.Int)]
        public ParameterResolverValue Length(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Length accepts exactly one argument.");
            if (args[0].Value is string s) {
                return new ParameterResolverValue(s.Length);
            } else if (args[0].Value is ICollection coll) {
                return new ParameterResolverValue(coll.Count);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(Replace), "Replaces all the occurrences of findwhat in the string with the string passed in replacewidth.")]
        [Parameter(0, "String", "original string", TypeFlags.String)]
        [Parameter(1, "Find", "what to find", TypeFlags.String)]
        [Parameter(2, "Replace", "replace with", TypeFlags.String)]
        [Result("Returns the resulting string", TypeFlags.String)]
        public ParameterResolverValue Replace(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 3) throw new ArgumentException("Replace accepts exactly 3  argument.");
            string str = Convert.ToString(args[0].Value);
            string patt = Convert.ToString(args[1].Value);
            string rep = Convert.ToString(args[2].Value);
            return new ParameterResolverValue(str.Replace(patt, rep));
        }

        // Check Length
        [Function(nameof(RegexReplace), "Replaces substrings matching the pattern in the string with the replacewith. The pattern uses the C# regular expression syntax. All arguments are converted to strings if they are other types.")]
        [Parameter(0, "String", "original string", TypeFlags.String)]
        [Parameter(1, "Pattern", "regex pattern", TypeFlags.String)]
        [Parameter(2, "Replace", "replace with", TypeFlags.String)]
        [Result("Returns the resulting string", TypeFlags.String)]
        public ParameterResolverValue RegexReplace(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 3) throw new ArgumentException("RegexReplace accepts exactly 3  argument.");
            string str = Convert.ToString(args[0].Value);
            string patt = Convert.ToString(args[1].Value);
            string rep = Convert.ToString(args[2].Value);
            if (!string.IsNullOrWhiteSpace(patt)) {
                Regex rex = new Regex(patt);
                return new ParameterResolverValue(rex.Replace(str, rep));
            } else {
                return new ParameterResolverValue(str);
            }

        }
        [Function(nameof(RegexMatch), "Test if the string matches the pattern.Splits the string using the specified separator or using \",\" if it is omitted.")]
        [Parameter(0, "String", "string", TypeFlags.String)]
        [Parameter(1, "Pattern", "regex pattern", TypeFlags.String)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue RegexMatch(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("RegexReplace accepts exactly 3  argument.");
            string str = Convert.ToString(args[0].Value);
            if (str == null) return new ParameterResolverValue(false);
            string patt = Convert.ToString(args[1].Value);
            if (!string.IsNullOrWhiteSpace(patt)) {
                Regex rex = new Regex(patt);
                return new ParameterResolverValue(rex.IsMatch(str));
            } else {
                return new ParameterResolverValue(false);
            }

        }

        [Function(nameof(Split), "Splits the string using the specified separator or using \", \" if it is omitted.")]
        [Parameter(0, "String", "string to split", TypeFlags.String)]
        [Parameter(1, "Separator", "separator", TypeFlags.Varying)]
        [Result("Returns an AC list", TypeFlags.List)]
        public ParameterResolverValue Split(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("Split requires at least 1 argument");
            var str = Convert.ToString(args[0].Value);
            if (args.Length > 1) {
                var sep = Convert.ToString(args[1].Value);
                if (string.IsNullOrEmpty(sep)) {
                    return new ParameterResolverValue(new List<ParameterResolverValue>() { new ParameterResolverValue(str) });
                } else {
                    return new ParameterResolverValue(str.Split(sep).Select(s => new ParameterResolverValue(s)).ToList());
                }

            } else {
                return new ParameterResolverValue(str.Split(',').Select(s => new ParameterResolverValue(s)).ToList());
            }
        }

        [Function(nameof(Trim), "Trims the string and returns the resulting string.")]
        [Parameter(0, "String", "string to trim", TypeFlags.String)]
        [Result("Returns the modified string", TypeFlags.String)]
        public ParameterResolverValue Trim(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Trim requires one parameter.");
            var str = Convert.ToString(args[0].Value);
            return new ParameterResolverValue(str.Trim());
        }

        [Function(nameof(EncodeBase64), "Converts the to string and encodes it to base64 which is returned as string.")]
        [Parameter(0, "Value", "value to encode", TypeFlags.Varying)]
        [Result("Returns the encoded string", TypeFlags.String)]
        public ParameterResolverValue EncodeBase64(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("EncodeBase64 requires one parameter.");
            var text = Convert.ToString(args[0].Value);
            byte[] textAsBytes = System.Text.Encoding.UTF8.GetBytes(text);
            string encodedText = Convert.ToBase64String(textAsBytes);
            return new ParameterResolverValue(encodedText);
        }

        [Function(nameof(DecodeBase64), "Converts the argument to string and decodes it from base64 to UTF8 string.")]
        [Parameter(0, "Argument", "argument to decode", TypeFlags.Varying)]
        [Result("Returns a string", TypeFlags.String)]
        public ParameterResolverValue DecodeBase64(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("DecodeBase64 requires one parameter.");
            var encodedText = Convert.ToString(args[0].Value);
            byte[] textAsBytes = Convert.FromBase64String(encodedText);
            string text = System.Text.ASCIIEncoding.UTF8.GetString(textAsBytes);
            return new ParameterResolverValue(text);
        }

        // Check Length
        [Function(nameof(MD5), "Converts the argument to string, then treating it as UTF8 string converts it to bytes. The hash is calculated from those bytes and returned as string containing the resulting hash in bytes listed in hexdecimal (two digits per byte).")]
        [Parameter(0, "Argument", "argument", TypeFlags.Varying)]
        [Result("Returns a hashed string", TypeFlags.String)]
        public ParameterResolverValue MD5(HostInterface ctx, ParameterResolverValue[] args) {
            string format = "x2";
            if (args.Length < 1 || args.Length > 2) throw new ArgumentException("MD5 accepts 1 or 2 arguments");
            if (args.Length > 1) {
                format = args[1].Value.ToString();
            }
            var text = args[0].Value.ToString();
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] textAsBytes = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] hash = md5.ComputeHash(textAsBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString(format));
            }
            return new ParameterResolverValue(sb.ToString());
        }
        #endregion

        #region Typing

        [Function(nameof(Cast), "Returns the arg casted to the type specified by type")]
        [Parameter(0, "Cast_Type", "type to cast to", TypeFlags.Varying)]
        [Parameter(1, "Value", "value we want to cast", TypeFlags.Varying)]
        [Result("Returns the casted arg", TypeFlags.Varying)]
        public ParameterResolverValue Cast(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Cast requires two arguments.");
            string stype = args[0].Value as string;
            if (stype == null) throw new ArgumentException("Parameter 1 of Case must be string specifying the type to convert to (string,int,double,bool)");
            switch (stype) {
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
                case "datetime":
                    return new ParameterResolverValue(Convert.ToDateTime(args[1].Value));
                case "datetimeutc":
                    if (args[1].Value is DateTime vdt) {
                        return new ParameterResolverValue(vdt.ToUniversalTime());
                    }
                    return new ParameterResolverValue(Convert.ToDateTime(args[1].Value).ToUniversalTime());
                case "datetimeasutc":
                    if (args[1].Value is DateTime cdt) {
                        return new ParameterResolverValue(DateTime.SpecifyKind(cdt, DateTimeKind.Utc));
                    }
                    return new ParameterResolverValue(DateTime.SpecifyKind(Convert.ToDateTime(args[1].Value),DateTimeKind.Utc));
                default:
                    throw new ArgumentException("Parameter 1 contains unrecognized type name valida types are: string,int,long, double,bool");
            }
        }
        [Function(nameof(TypeOf), "Returns the name of the type contained in the argument. Recognizes only these types: null, string, int, long, double, short, byte, bool and everything else is unknown.")]
        [Parameter(0, "Argument", "argument to convert to ISO8601 string. Preferably it should be datetime, if it is not the conversion may be invorrect.", TypeFlags.Varying)]
        [Result("Returns the type as a ISO8601 string", TypeFlags.Varying)]
        public ParameterResolverValue To8601String(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("To8601String requires one argument.");
            var dt = args[0].Value;
            if (dt == null) return new ParameterResolverValue(null);
            if (!(dt is DateTime)) {
                try {
                    dt = Convert.ToDateTime(dt);
                } catch {
                    return new ParameterResolverValue(null);
                }
            }
            if (dt is DateTime vdt) {
                return new ParameterResolverValue(vdt.ToUniversalTime().ToString("u").Replace(" ", "T"));
            }
            return new ParameterResolverValue(null); // This should not happen
        }
        [Function(nameof(TypeOf), "Returns the name of the type contained in the argument. Recognizes only these types: null, string, int, long, double, short, byte, bool and everything else is unknown.")]
        [Parameter(0, "Argument", "argument to check type", TypeFlags.Varying)]
        [Result("Returns the type as a string", TypeFlags.String)]
        public ParameterResolverValue TypeOf(HostInterface ctx, ParameterResolverValue[] args) {
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
            if (tc == typeof(DateTime)) {
                DateTime cdt = (DateTime)args[0].Value;
                if (cdt.Kind == DateTimeKind.Utc) {
                    return new ParameterResolverValue("datetimeutc");
                } else {
                    return new ParameterResolverValue("datetime");
                }
            }

            return new ParameterResolverValue("unknown");
        }

        public static bool IsNumeric(object v) {
            if (v == null) return false;
            Type tc = v.GetType();
            if (tc == typeof(int) || tc == typeof(uint) || tc == typeof(long) || tc == typeof(ulong)
                || tc == typeof(double) || tc == typeof(float) || tc == typeof(short) || tc == typeof(ushort) ||
                tc == typeof(char) || tc == typeof(byte)) return true;
            return false;
        }

        [Function(nameof(IsNumeric), "Returns true if the argument contains a numeric value.")]
        [Parameter(0, "Value", "value to check", TypeFlags.Varying)]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue IsNumeric(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("IsNumeric requires one argument.");
            return new ParameterResolverValue(IsNumeric(args[0].Value));
        }

        #endregion

        #endregion

        #region Settings

        [Function(nameof(GSetting), "Returns the value of a global setting. Only a small number of global settings can be queried. These are typically defined in the appsettings*.json file.")]
        [Parameter(0, "Setting", "global setting to find", TypeFlags.Varying)]
        [Result("Returns a global setting value as string", TypeFlags.Varying)]
        public ParameterResolverValue GSetting(HostInterface _ctx, ParameterResolverValue[] args) {
            KraftGlobalConfigurationSettings kgcf = null;
            var ctx = _ctx as IDataLoaderContext;
            if (ctx != null) {
                kgcf = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            } else {
                var nctx = _ctx as INodePluginContext;
                if (nctx != null) {
                    kgcf = nctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                }
            }
            if (kgcf == null) {
                throw new Exception("Cannot obtain Kraft global settings");
            }
            if (args.Length != 1) {
                throw new ArgumentException($"GSetting accepts one argument, but {args.Length} were given.");
            }
            var name = args[0].Value as string;

            if (name == null) {
                throw new ArgumentException($"GSetting argument must be string - the name of the global kraft setting to obtain.");
            }
            return new ParameterResolverValue(
                name switch {
                    "EnvironmentName" => kgcf?.EnvironmentSettings?.EnvironmentName,
                    "ContentRootPath" => kgcf.EnvironmentSettings?.ContentRootPath,
                    "ApplicationName" => kgcf?.EnvironmentSettings?.ApplicationName,
                    "StartModule" => kgcf?.GeneralSettings?.DefaultStartModule,
                    "ClientId" => kgcf?.GeneralSettings?.ClientId,
                    "HostingUrl" => kgcf?.GeneralSettings?.HostingUrl,
                    "KraftUrlSegment" => kgcf?.GeneralSettings?.KraftUrlSegment,
                    "CssSegment" => kgcf?.GeneralSettings?.KraftUrlCssJsSegment,
                    "RootSegment" => kgcf?.GeneralSettings?.KraftUrlSegment,
                    "ResourceSegment" => kgcf?.GeneralSettings?.KraftUrlResourceSegment,
                    "ModuleImages" => kgcf?.GeneralSettings?.KraftUrlModuleImages,
                    "ModulePublic" => kgcf?.GeneralSettings?.KraftUrlModulePublic,
                    "Theme" => kgcf?.GeneralSettings?.Theme,
                    "HostKey" => kgcf?.GeneralSettings?.ServerHostKey,
                    "Authority" => kgcf?.GeneralSettings?.Authority,
                    "SignalRHub" => kgcf?.GeneralSettings?.SignalRSettings?.HubRoute,
                    _ => null
                }
            );
            
            throw new ArgumentException($"The setting {name} is not supported");
        }

        #endregion

        [Function(nameof(Throw), "Throws an exception with the given description.")]
        [Parameter(0, "Argument", "argument", TypeFlags.Varying)]
        [Result("Throws an exception", TypeFlags.Error)]
        public ParameterResolverValue Throw(HostInterface ctx, ParameterResolverValue[] args) {
            string extext = null;
            if (args.Length > 0) {
                if (args[0].Value is string) {
                    extext = args[0].Value as string;
                }
            } else {
                extext = "Exception raised intentionally from an ActionQuery code";
            }
            throw new Exception(extext);
        }

        //No doc ATM
        [Function(nameof(Debug), "Writes warnings through the KraftLogger. Log level should be higher or equal WARNING.")]
        [Parameter(0, "Text or Array of values", "argument", TypeFlags.Varying)]
        public ParameterResolverValue Debug(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length > 0) {
                string log = string.Join(',', args.Select(a => Convert.ToString(a.Value)));
                KraftLogger.LogWarning($"ACDebug: {log}\n");
                return new ParameterResolverValue(args[0]);
            }
            return new ParameterResolverValue(null);
        }

        public ParameterResolverValue NType(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length > 2) throw new ArgumentException("NType acceptes up to two arguments");
            var bfull = true;
            if (args.Length > 1)
            {
                if (args[1].IsFalsy()) bfull = false;
            }
            var val = args[0].Value;
            if (val == null) return new ParameterResolverValue("null");
            if (bfull) {
                return new ParameterResolverValue(val.GetType().FullName);
            } else {
                return new ParameterResolverValue(val.GetType().Name);
            }
            
        }
        #region MetaInfo
        private MetaNode _NavMetaNodes(MetaNode current, int level) {
            MetaNode result = current;
            for (int i = 0; i < level; i++) {
                if (result != null) {
                    result = result.GetParent();
                } else {
                    break;
                }
            }
            return current;
        }

        [Function(nameof(MetaNode), "Gets specific fields of the general node meta information")]
        [Parameter(0, "field", "field to get: name, step, executions", TypeFlags.Optional | TypeFlags.String)]
        [Parameter(1, "parentLevel", "0 - current, 1 - parent etc.", TypeFlags.Optional | TypeFlags.Int)]
        [Result("Returns the value or null", TypeFlags.Null | TypeFlags.String | TypeFlags.Int)]
        public ParameterResolverValue MetaNode(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is IActionHelpers helper) {
                int level = 0;
                if (args.Length > 1) {
                    level = Convert.ToInt32(args[1].Value);
                }
                if (helper.NodeMeta is MetaNode _node) {
                    MetaNode node = _NavMetaNodes(_node, level);
                    if (node != null && args.Length > 0) {
                        var param = args[0].Value as string;
                        if (param != null) {
                            return new ParameterResolverValue(param switch {
                                "name" => node.Name,
                                "step" => node.Step,
                                "executions" => node.Executions,
                                "datastate" => node.GetVolatileInfo().DataState,
                                "operation" => node.GetVolatileInfo().Operation,
                                _ => null
                            });
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        [Function(nameof(MetaADOResult), "Gets specific fields of the ADOInfo in the node meta information if available")]
        [Parameter(0, "field", "field to get: rowsaffected, rows, fields", TypeFlags.Optional | TypeFlags.String)]
        [Parameter(1, "parentLevel", "0 - current, 1 - parent etc.", TypeFlags.Optional | TypeFlags.Int)]
        [Result("Returns the value or null", TypeFlags.Null | TypeFlags.Int)]
        public ParameterResolverValue MetaADOResult(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is IActionHelpers helper) {
                int level = 0;
                if (args.Length > 1) {
                    level = Convert.ToInt32(args[1].Value);
                }
                if (helper.NodeMeta is MetaNode _node) {
                    MetaNode node = _NavMetaNodes(_node, level);
                    if (node != null && node.GetInfo<ADOInfo>() is ADOInfo ado) {
                        var lastResult = ado.LastResult;
                        if (args.Length > 0) {
                            var param = args[0].Value as string;
                            if (param != null) {
                                return new ParameterResolverValue(param switch {
                                    "rowsaffected" => ado.RowsAffected,
                                    "rows" => lastResult?.Rows,
                                    "fields" => lastResult?.Fields,
                                    _ => null
                                });
                            }
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        [Function(nameof(MetaRoot), "Gets specific fields of root meta information ot if certain flags are set")]
        [Parameter(0, "field", "field to get: rowsaffected, rows, fields", TypeFlags.Optional | TypeFlags.String)]
        [Parameter(1, "parentLevel", "0 - current, 1 - parent etc.", TypeFlags.Optional | TypeFlags.Int)]
        [Result("Returns the value or null", TypeFlags.Null | TypeFlags.Int | TypeFlags.Bool)]
        public ParameterResolverValue MetaRoot(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is IActionHelpers helper) {
                if (helper.NodeMeta is MetaNode node && node.Root is MetaRoot root) {
                    if (args.Length > 1) {
                        var param = args[0].Value as string;
                        if (param != null) {
                            return new ParameterResolverValue(param switch {
                                "steps" => root.Steps,
                                "flags" => (int)root.Flags,
                                "basic" => root.Flags.HasFlag(EMetaInfoFlags.Basic) ? true : false,
                                "trace" => root.Flags.HasFlag(EMetaInfoFlags.Trace) ? true : false,
                                "debug" => root.Flags.HasFlag(EMetaInfoFlags.Debug) ? true : false,
                                "log" => root.Flags.HasFlag(EMetaInfoFlags.Log) ? true : false,
                                _ => null
                            });
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        #endregion

        #region Additional helpers for internal use
        [Function("DictToJson","Converts a Dict to JSON string, the dict can contain lists and other dictionaries")]
        [Parameter(0, "Dictionary", "Dictionary to convert", TypeFlags.Dict)]
        [Result("Returns a json", TypeFlags.Json)]
        public static ParameterResolverValue DictToJson(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("DictToJson requires a single Dict argument");
            Dictionary<string, ParameterResolverValue> dict = args[0].Value as Dictionary<string, ParameterResolverValue>;
            if (dict == null) throw new ArgumentException("DictToJson requires a Dict argument, but received something else");
            var gendata = ConvertToGeneralData(dict);
            return new ParameterResolverValue(JsonConvert.SerializeObject(gendata));
        }
        [Function("JsontoDict", "Converts a JSON string to Dict, if not possible returns null")]
        [Parameter(0, "JSON", "Json string", TypeFlags.String | TypeFlags.Json)]
        [Result("Returns an AC Dictionary or null", TypeFlags.Dict | TypeFlags.Null)]
        public static ParameterResolverValue JsonToDict(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("JsonToDict requires a single string argument");
            var str = args[0].Value as string;
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException("JsonToDict requires a string argument, but received empty string or null or another type");
            var dict  = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
            if (dict is Dictionary<string,object> odict)
            {
                return ConvertFromGenericData(odict);
            } else
            {
                return new ParameterResolverValue(null);
            }
            
            
        }
        /// <summary>
        /// This method converts generic nodeset data to internally usable data (by the AC script). However it is
        /// a bit more tollerant than it should be. For instance it will pack lists not containing dictionaries as ValueList.
        /// This is intentional, because the nodesets can violate the generic data convention and it can be for a reason - in such
        /// a case the script will still be able to work with the data. 
        /// Potential pitfalls: A custom list violating the generic data convention may still contain dictionaries, this will
        /// be mostly Ok, unless the script mistakenly detects this as something corresponsing to a node. While this is unlikely
        /// scenario, it is still possible and one should be aware of the possibility.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ParameterResolverValue ConvertFromGenericData(object data)
        {
            if (data is Dictionary<string, object> dict)
            {
                return new ParameterResolverValue(dict.ToDictionary(kv => kv.Key, kv => new ParameterResolverValue(ConvertFromGenericData(kv.Value))));
            }
            else if (data is string s)
            {
                return new ParameterResolverValue(s);
            }
            else if (data is IEnumerable<Dictionary<string, object>> glist)
            {
                // cosher data for a generic list -> pack it as List
                var list = new List<ParameterResolverValue>();
                foreach (var o in glist)
                {
                    list.Add(new ParameterResolverValue(ConvertFromGenericData(o)));
                }
                return new ParameterResolverValue(list);
            }
            else if (data is IEnumerable enmr)
            {
                var list = new ValueList<ParameterResolverValue>();
                foreach (object o in enmr)
                {
                    list.Add(new ParameterResolverValue(ConvertFromGenericData(o)));
                }
                return new ParameterResolverValue(list);
            }
            else
            {
                return new ParameterResolverValue(data);
            }
        }
        public static object ConvertToGenericData(object data)
        {
            if (data is Dictionary<string, ParameterResolverValue> dict)
            {
                return (dict.ToDictionary(kv => kv.Key, kv => ConvertToGenericData(kv.Value.Value))); // <string, object>
            }
            else if (data is ParameterResolverValue pv)
            {
                return ConvertToGenericData(pv.Value);
            }
            else if (data is ValueList<ParameterResolverValue> vlst)
            {
                var list = new List<object>();
                foreach (var v in vlst)
                {
                    list.Add(v.Value);
                }
                return list;
            }
            else if (data is IEnumerable<ParameterResolverValue> lst)
            {
                var list = new List<Dictionary<string, object>>();
                foreach (var v in lst)
                {
                    if (v.Value is Dictionary<string, ParameterResolverValue> pdict)
                    {
                        list.Add(ConvertToGenericData(pdict) as Dictionary<string, object>);
                    }
                    else
                    {
                        throw new FormatException("A list contains non-dictionary elements. Error occurred while converting to generic data.");
                    }
                }
                return list;
            }
            else
            {
                return data;
            }
        }

        public static object ConvertToGeneralData(object data)
        {
            if (data is Dictionary<string, ParameterResolverValue> dict)
            {
                return (dict.ToDictionary(kv => kv.Key, kv => ConvertToGeneralData(kv.Value.Value))); // <string, object>
            }
            else if (data is ParameterResolverValue pv)
            {
                return ConvertToGenericData(pv.Value);
            }
            else if (data is IEnumerable<ParameterResolverValue> vlst)
            {
                var list = new List<object>();
                foreach (var v in vlst)
                {
                    list.Add(ConvertToGeneralData(v.Value));
                }
                return list;
            }
            else
            {
                return data;
            }
        }

        #endregion

        #region Conversions

        //No doc ATM
        [Function(nameof(ToNodesetData), "")]
        public ParameterResolverValue ToNodesetData(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ToNodesetData takes one argument");
            return new ParameterResolverValue(ConvertToGenericData(args[0].Value));
        }

        //No doc ATM
        [Function(nameof(ToGeneralData), "")]
        public ParameterResolverValue ToGeneralData(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ToGeneralData takes one argument");
            return new ParameterResolverValue(ConvertToGeneralData(args[0].Value));
        }
        public ParameterResolverValue FromNodesetData(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("From Nodeset data requires exactly one argumen");
            var inp = args[0].Value;
            return ConvertFromGenericData(inp);
        }
        #endregion
    }
}
