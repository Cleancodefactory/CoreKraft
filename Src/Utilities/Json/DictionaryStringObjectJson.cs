using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.Json
{

    /// <summary>
    /// JSON deserializer/serializer woring with generic data as back end store
    /// </summary>
    public class DictionaryStringObjectJson
    {
        #region Deserialization
        public static object Deserialize(string input, JsonReaderOptions jsonReaderOptions = default)
        {
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(input), jsonReaderOptions);
            object result = Traverse(ref reader);
            if (reader.Read())
            {
                throw new Exception($"Deserialize: Token: {reader.TokenType} at position: {reader.Position} after complete deserialization.");
            }
            return result;
        }

        private static object Traverse(ref Utf8JsonReader reader)
        {
            reader.Read();
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    return null;
                case JsonTokenType.StartObject:
                    return TraverseObject(ref reader);
                case JsonTokenType.StartArray:
                    return TraverseArray(ref reader);
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                case JsonTokenType.PropertyName:
                case JsonTokenType.Comment:
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Null:
                default:
                    throw new Exception($"Traverse: Token: {reader.TokenType} not allowed at position: {reader.Position}");
            }
        }

        private static object TraverseObject(ref Utf8JsonReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None:
                        return null;
                    case JsonTokenType.EndObject:
                        return result;
                    case JsonTokenType.StartArray:
                        return TraverseArray(ref reader);
                    case JsonTokenType.PropertyName:
                        string name = reader.GetString();
                        reader.Read();
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.None:
                                break;
                            case JsonTokenType.StartObject:
                                result.Add(name, TraverseObject(ref reader));
                                break;
                            case JsonTokenType.StartArray:
                                result.Add(name, TraverseArray(ref reader));
                                break;
                            case JsonTokenType.Comment:
                                continue;
                            case JsonTokenType.String:

                                if (reader.TryGetDateTime(out var date))
                                {
                                    result.Add(name, date);
                                    break;
                                }
                                result.Add(name, reader.GetString());
                                break;
                            case JsonTokenType.Number:
                                var strVal = reader.GetString();
                                if (!string.IsNullOrEmpty(strVal)) {
                                    if (strVal.IndexOf('.') >= 0) { // Looks like floating point
                                        if (reader.TryGetDouble(out double dval)) {
                                            result.Add(name, dval);
                                            break;
                                        } else if (reader.TryGetDecimal(out decimal decval)) {
                                            result.Add(name, dval);
                                            break;
                                        }
                                    } 
                                    if (reader.TryGetInt64(out long lval)) {
                                        if (lval >= int.MinValue && lval <= int.MaxValue) {
                                            result.Add(name, (int)lval);
                                        } else {
                                            result.Add(name, lval);
                                        }
                                        break;
                                    }
                                    throw new Exception($"TraverseObject: Number value cannot be parsed at position: {reader.Position}");
                                } else {
                                    result.Add(name, null); // Should be impossible because of the reader
                                }
                                break;
                            case JsonTokenType.True:
                                result.Add(name, true);
                                break;
                            case JsonTokenType.False:
                                result.Add(name, false);
                                break;
                            case JsonTokenType.Null:
                                result.Add(name, null);
                                break;
                            case JsonTokenType.EndArray:
                            case JsonTokenType.PropertyName:
                            case JsonTokenType.EndObject:
                                throw new Exception($"TraverseObject: Token: {reader.TokenType} not allowed at position: {reader.Position}");
                        }
                        break;
                    case JsonTokenType.Comment:
                        break;
                    case JsonTokenType.EndArray:
                    case JsonTokenType.StartObject:
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                    default:
                        throw new Exception($"TraverseObject: Token: {reader.TokenType} not allowed at position: {reader.Position}");
                }
            }
            throw new Exception($"TraverseObject: No object created and returned at position: {reader.Position}");
        }

        private static object TraverseArray(ref Utf8JsonReader reader)
        {
            List<object> result = new List<object>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None:
                        return null;
                    case JsonTokenType.StartObject:
                        result.Add(TraverseObject(ref reader));
                        break;
                    case JsonTokenType.StartArray:
                        result.Add(TraverseArray(ref reader));
                        break;
                    case JsonTokenType.EndArray:
                        return result;
                    case JsonTokenType.Comment:
                        continue;
                    case JsonTokenType.String:
                        if (reader.TryGetDateTime(out var date))
                        {
                            result.Add(date);
                            break;
                        }
                        result.Add(reader.GetString());
                        break;
                    case JsonTokenType.Number:
                        var strVal = reader.GetString();
                        if (!string.IsNullOrEmpty(strVal)) {
                            if (strVal.IndexOf('.') >= 0) { // Looks like floating point
                                if (reader.TryGetDouble(out double dval)) {
                                    result.Add(dval);
                                    break;
                                } else if (reader.TryGetDecimal(out decimal decval)) {
                                    result.Add(dval);
                                    break;
                                }
                            }
                            if (reader.TryGetInt64(out long lval)) {
                                if (lval >= int.MinValue && lval <= int.MaxValue) {
                                    result.Add((int)lval);
                                } else {
                                    result.Add(lval);
                                }
                                break;
                            }
                            throw new Exception($"TraverseObject: Number value cannot be parsed at position: {reader.Position}");
                        } else {
                            result.Add(null); // Should be impossible because of the reader
                        }
                        break;
                    case JsonTokenType.True:
                        result.Add(true);
                        break;
                    case JsonTokenType.False:
                        result.Add(false);
                        break;
                    case JsonTokenType.Null:
                        result.Add(null);
                        break;
                    case JsonTokenType.PropertyName:
                    case JsonTokenType.EndObject:
                    default:
                        throw new Exception($"TraverseArray: Token: {reader.TokenType} not allowed at position: {reader.Position}");
                }
            }
            throw new Exception($"TraverseArray: No array created and returned at position: {reader.Position}");
        }
        #endregion Deserialization

        #region Serialization
        public static object Serialize(string input, JsonReaderOptions jsonReaderOptions = default) {
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(input), jsonReaderOptions);
            object result = Traverse(ref reader);
            if (reader.Read()) {
                throw new Exception($"Deserialize: Token: {reader.TokenType} at position: {reader.Position} after complete deserialization.");
            }
            return result;
        }
        #endregion
    }
}
