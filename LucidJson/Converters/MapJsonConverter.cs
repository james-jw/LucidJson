using LucidJson.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LucidJson.Converters
{
    public class MapJsonConverter : JsonConverter
    {
        private MapSchema _schema;
        public MapJsonConverter(MapSchema schema)
        {
            _schema = schema;
        }

        public MapJsonConverter() { }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Map) || objectType.BaseType == typeof(Map) || objectType == typeof(Array);
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;

        object readJsonInternal(JsonReader reader, string path, Type baseType = null)
        {
            object currentContainer = null;

            if (reader.TokenType == JsonToken.StartObject)
            {
                currentContainer = baseType == null ? new Map(_schema) : Activator.CreateInstance(baseType);

                if (_schema != null) {
                    ((Map)currentContainer).Path = path;
                }
            }
            else if (reader.TokenType == JsonToken.StartArray)
                currentContainer = new Array(_schema);

            while (reader.Read())
            {
                var token = reader.Value;
                var type = reader.TokenType;
                if (type == JsonToken.PropertyName) {
                    var tokenValue = readJsonInternal(reader, path == "" ? (string)token : $"{path}.{token}");
                    var tokenSchema = MapSchemaHelper.FindSchemaItem(path, (string)token, _schema);

                    if (tokenSchema == null || tokenSchema.Deprecated != true) {
                        ((Map)currentContainer)[(string)token] = tokenValue;
                    }
                }
                else if (type == JsonToken.String || type == JsonToken.Raw || type == JsonToken.Boolean || type == JsonToken.Float || type == JsonToken.Integer
                         || type == JsonToken.Date || type == JsonToken.Null || type == JsonToken.Undefined) {

                    if (currentContainer != null)
                        ((Array)currentContainer).Add(token);
                    else
                        return token;
                }
                else if (type == JsonToken.EndObject || type == JsonToken.EndArray) {
                    return currentContainer;
                }
                else if (type == JsonToken.StartObject) {
                    if (currentContainer != null)
                        ((Array)currentContainer).Add(readJsonInternal(reader, path));
                    else {
                        currentContainer = new Map(_schema);

                        if (_schema != null) {
                            ((Map)currentContainer).Path = path;
                        }
                    }
                }
                else if (type == JsonToken.StartArray) {
                    if (currentContainer != null)
                        ((Array)currentContainer).Add(readJsonInternal(reader, path));
                    else {
                        currentContainer = new Array(_schema);
                    }
                }
                else
                    throw new Exception($"Invalid Json Token '{type}'");
            }

            return currentContainer;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try {
                var objectOut = readJsonInternal(reader, "", objectType == typeof(Map) ? null : objectType);
                return objectOut;
            }
            catch (Exception e) {
                throw new Exception($"Failed to parse json {e.Message}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Map)
                internalWriteJson<Map>((Map)value, writer);
            else
                internalWriteJson<Array>((Array)value, writer);
        }

        private void internalWriteJson<T>(T wrapper, JsonWriter writer)
        {
                JObject jo = JObject.Parse(wrapper.ToString());
                jo.WriteTo(writer);
        }
    }
}
