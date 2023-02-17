using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Utilities.Json
{
    public class DictionaryStringObjectJson
    {
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
        public static Regex _re8601 = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,})?Z", RegexOptions.CultureInvariant);
        public static bool IsFull8601UtcString(string input) {
            return _re8601.IsMatch(input);
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
            string strVal;
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
                                strVal= reader.GetString();
                                if (IsFull8601UtcString(strVal)) {
                                    if (reader.TryGetDateTime(out var date)) {
                                        // Enforce marking the date as UTC
                                        // TODO This requires support for local/unmarked and UTC, but Utc is most important
                                        result.Add(name, DateTime.SpecifyKind(date, DateTimeKind.Utc));
                                        break;
                                    }
                                }
                                result.Add(name, strVal);
                                break;
                            case JsonTokenType.Number:
                                ParseNumber(result, ref reader, name);
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
            string strVal;
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
                        strVal= reader.GetString();
                        if (IsFull8601UtcString(strVal)) {
                            if (reader.TryGetDateTime(out var date)) {
                                // TODO Requires support for all kinds of dates, but Utc is the most important
                                result.Add(DateTime.SpecifyKind(date, DateTimeKind.Utc));
                                break;
                            }
                        }
                        // Treat as string if cannot be used as datetime
                        result.Add(reader.GetString());
                        break;
                    case JsonTokenType.Number:
                        ParseNumber(result, ref reader);
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

        private static void ParseNumber<T>(T result, ref Utf8JsonReader reader, string name = null)
        {
            if (result is Dictionary<string, object> dict)
            {
                if (reader.TryGetInt32(out var numInt))
                {
                    dict.Add(name, numInt);
                }
                else if (reader.TryGetInt64(out var numLong))
                {
                    dict.Add(name, numLong);
                }
                else
                {
                    dict.Add(name, (reader.GetDecimal()));
                }
            }
            else if (result is List<object> list)
            {
                if (reader.TryGetInt32(out var numInt))
                {
                    list.Add(numInt);
                }
                else if (reader.TryGetInt64(out var numLong))
                {
                    list.Add(numLong);
                }
                else
                {
                    list.Add(reader.GetDecimal());
                }
            }
        }
    }
}
