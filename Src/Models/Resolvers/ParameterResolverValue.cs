﻿using Ccf.Ck.Models.Enumerations;
using System.Collections;
using System.Text.RegularExpressions;


// Moved here from Ccf.Ck.SysPlugins.Support.ParameterExpression
namespace Ccf.Ck.Models.Resolvers
{

    /// <summary>
    /// The type in which parameters are packed and given (after processing) to data loaders and custom plugins.
    /// This type should stay here to avoid circular references
    /// </summary>
    public struct ParameterResolverValue {
        /// <summary>
        /// Constructs parameter value
        /// </summary>
        /// <param name="value">The value to pack</param>
        /// <param name="vtype">By default valueType, if you want literal set this to ContentType.</param>
        /// <param name="dataType">Specify specific type <see cref="Ccf.Ck.Models.Enumerations.EValueDataType" /> and the optional <see cref="Ccf.Ck.Models.Enumerations.EValueDataSize" /></param>
        /// <param name="size">Explicit size of the field in wich this goes - almost excliusively used for VARCHAR and similar database types. Other wise keep this 0 - which should be treated as undefined by all components.</param>
        public ParameterResolverValue(
                object value,
                EResolverValueType vtype = EResolverValueType.ValueType,
                uint dataType = (uint)EValueDataType.any,
                uint size = 0)
        {
            if (value is ParameterResolverValue prv)
            {
                Value = prv.Value;
                DataType = prv.DataType;
                DataSize = prv.DataSize;
                ValueType = prv.ValueType;
            }
            else
            {
                Value = value;
                DataType = dataType;
                DataSize = size;
                ValueType = vtype;
                if (value is ICollection)
                {
                    DataType |= (uint)EValueDataType.Collection;
                }
            }
        }
        /// <summary>
        /// Constructs parameter value of value type (non-literal)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="datatype"></param>
        /// <param name="datasize"></param>
        /// <param name="size"></param>
        public ParameterResolverValue(
                object value,
                EValueDataType datatype,
                EValueDataSize datasize = EValueDataSize.Normal,
                uint reserved = 0,
                uint size = 0)
        {
            if (value is ParameterResolverValue prv)
            {
                Value = prv.Value;
                DataType = prv.DataType;
                DataSize = prv.DataSize;
                ValueType = prv.ValueType;
            }
            else
            {
                Value = value;
                DataType = (uint)datatype | (uint)datasize | reserved;
                if (value is ICollection && reserved == 0 && (DataType & (uint)EValueDataType.AdvancedMask) == 0)
                {
                    DataType |= (uint)EValueDataType.Collection;
                }
                DataSize = size;
                ValueType = EResolverValueType.ValueType;
            }
        }
        /// <summary>
        /// The value itself
        /// </summary>
        public object Value;
        /// <summary>
        /// Hints what is the intended type of the value. The consumer may decide differently, but in most cases this will help the consumer
        /// to perform the best conversion when it sends the parameter to a database ...
        /// </summary>
        public uint DataType; // should be constructed by combining ValueDataType and ValueDataSize
        /// <summary>
        /// Explicit size - mostly for textual values (VARCHAR(X) for example). If 0 (unspecified) the DataLoader should use defaults,
        /// Intelligent casting determined by the detected type of the Value or/and the hint in the high part of the DataType.
        /// </summary>
        public uint DataSize; // 0 - should be treated as unspecified.
        /// <summary>
        /// The role of the value - just a value, piece of SQL code etc.
        /// </summary>
        public EResolverValueType ValueType;

        private static Regex _reFalsy = new Regex(@"0|false|no|\-0", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public bool IsTruthy() {
            var v = Value;
            if (v == null) return false;
            if (v is bool b) return b;
            if (v is int i) return i != 0;
            if (v is uint ui) { return ui != 0; }
            if (v is long l) return l != 0;
            if (v is ulong ul) return ul != 0;
            if (v is float f) return f != 0;
            if (v is double d) return d != 0;
            if (v is short s) return s != 0;
            if (v is ushort us) return us != 0;
            if (v is byte bt) return bt != 0;
            if (v is char c) return c != 0;
            if (v is string str) {
                if (string.IsNullOrWhiteSpace(str)) return false;
                if (_reFalsy.IsMatch(str)) return false;
                return true;
            }
            return false;
        }
        public bool IsFalsy() { return !IsTruthy(); }
        private string _ValTypeString()
        {
            if (Value == null) return "null";
            if (Value is int || Value is uint) return $"{Value} int";
            if (Value is short || Value is ushort) return $"{Value} short";
            if (Value is char || Value is byte) return $"{Value} byte";
            if (Value is bool) return $"{Value} bool";
            if (Value is long || Value is ulong) return $"{Value} long";
            if (Value is string) return $"'{Value}' string";
            if (Value is double || Value is float) return $"{Value} double";
            if (Value is decimal) return $"{Value} decimal";
            return $"other({Value.GetType().Name})";
        }

        public override string ToString()
        {
            EValueDataType datatype = (EValueDataType)(DataType & 0x00FFFF);
            EValueDataSize datasize = (EValueDataSize)(DataType & 0xFF0000);
            return $"<{_ValTypeString()} [{ValueType.ToString()}] ({datatype.ToString()},{datasize.ToString()})>";
        }

        #region basic helpers
        public EValueDataType ValueDataType {
            get => (EValueDataType)(0xFFFF & (uint)DataType);
            set => DataType = ((0xFF0000 & (uint)DataType) | (uint)value);
        }
        public EValueDataSize ValueDataSize {
            get => (EValueDataSize)(0xFF0000 & (uint)DataType);
            set => DataType = ((0xFFFF & (uint)DataType) | (uint)value);
        }
        #endregion

        #region Static tools
        public static object Strip(object val) {
            // TODO: May be recursive?
            if (val is ParameterResolverValue prv) {
                return prv.Value;
            }
            return val;
        }
        #endregion
    }

    #region Chaining helpers
    public static class ParameterValueExtensions {
        public static ParameterResolverValue SetValue(this ParameterResolverValue self, object v) {
            self.Value = v;
            self.ValueType = EResolverValueType.ValueType;
            return self;
        }
        public static ParameterResolverValue SetContent(this ParameterResolverValue self, string v) {
            self.Value = v;
            self.ValueType = EResolverValueType.ContentType;
            return self;
        }
        public static ParameterResolverValue AsValue(this ParameterResolverValue self) {
            self.ValueType = EResolverValueType.ValueType;
            return self;
        }
        public static ParameterResolverValue AsContent(this ParameterResolverValue self) {
            self.ValueType = EResolverValueType.ContentType;
            return self;
        }
        public static ParameterResolverValue AsDataType(this ParameterResolverValue self,EValueDataType dt) {
            self.DataType = (self.DataType & 0xFF0000) | ((uint)dt & 0xFFFF);
            return self;
        }
        public static ParameterResolverValue AsDataSize(this ParameterResolverValue self, EValueDataSize dt) {
            self.DataType = (self.DataType & 0x00FFFF) | ((uint)dt & 0xFF0000);
            return self;
        }
        public static ParameterResolverValue DefaultSize(this ParameterResolverValue self) {
            return self.AsDataSize(EValueDataSize.Normal);
        }
        public static ParameterResolverValue AnyType(this ParameterResolverValue self) {
            return self.AsDataType(EValueDataType.any);
        }

    }
    #endregion
}
