using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LucidJson.Schema;
using LucidJson.Converters;

namespace LucidJson 
{
    [Serializable]
    public class Map<T> : Map { }

    /// <summary>
    /// A meta enhanced dynamic dictionary. 
    /// </summary>
    [Serializable]
    public partial class Map : DynamicObject, IDictionary<string, object>, INotifyPropertyChanged
    {
        public Map(object key, object value) : this()
        {
            this[key.ToString()] = value.ToString();
        }

        public Map(MapSchema schema) : this()
        {
            _schema = schema;
        }

        public dynamic AsDynamic(Func<string, string> keyMapper = null)
        {
            if(keyMapper != null)
                this.mapper = keyMapper;

            return this;
        }

        /// <summary>
        /// Provided a full context map for which this map lives. Returns the parent of this object
        /// </summary>
        /// <param name="context">The parent document context containing this map</param>
        /// <returns>The parent of this map</returns>
        public Map FindParent(Map context)
        {
            foreach (var pair in this) {
                var value = pair.Value;

                if (value is Map map) {
                    if (map == context)
                        return this;

                    var parent = map.FindParent(context);
                    if (parent != null)
                        return parent;
                } else if (value is LucidJson.Array array) {
                    foreach (var item in array) {
                        if (item is Map arrayMap) {
                            if (arrayMap == context)
                                return this;

                            var parent = arrayMap.FindParent(context);
                            if (parent != null)
                                return parent;
                        }
                    }
                }
            }

            return null;
        }

        public static Func<MapSchema, JsonSerializerSettings> SerializerSettings = (schema) => {
            return new JsonSerializerSettings() {
                Converters = new List<JsonConverter>() { new MapJsonConverter(schema) },
                FloatParseHandling = FloatParseHandling.Decimal,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        };

        public static Map ParseJson(string json)
        {
            return ParseJson<Map>(json, null);
        }

        public static Map ParseJson(string json, MapSchema schema)
        {
            var mapOut = ParseJson<Map>(json, schema);
            if (schema is MapSchema mapSchema)
                mapSchema.BaseData = mapOut;

            return mapOut;
        }

        public static T ParseJson<T>(string json, MapSchema schema)
        {
            var mapOut = JsonConvert.DeserializeObject<T>(json, Map.SerializerSettings(schema));
            return mapOut;
        }

        bool locked = false;
        public void lockEdits()
        {
            locked = true;
        }

        public static implicit operator Map(JObject obj)
        {
            return obj.ToObject<Map>();
        }

        public static Map Merge(params Map[] maps)
        {
            Map output = new Map();
            foreach (Map map in maps) {
                foreach (string key in map.Keys.ToArray()) {
                    output[key] = map[key];
                }
            }

            return output;
        }

        /// <summary>
        /// Converts a json value into an appropriate map type for easy traversal and debugging
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>A value</returns>
        internal static object ProcessValue(object value)
        {
            var result = value;
            if (result is JValue) {
                result = ((JValue)result);
            }
            else if (result is JObject)
            {
                result = ((JObject)result).ToObject<Map>();
            }
            else if (result is JArray)
            {
                result = new Array((JArray)result).Select(v => ProcessValue(v));
            }

            //else if(result is IEnumerable<object>)
            //{
            //    result = new Array(ProcessEnumeration((IEnumerable<object>)result));
            //}

            return result;
        }

        private static IEnumerable<object> ProcessEnumeration(IEnumerable<object> result)
        {
            foreach (var value in result)
                yield return ProcessValue(value);
        }

        /// <summary>
        /// Returns an enumeration of descendant Maps as well as starting with this map itself.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string Key, Map Map)> Traverse()
        {
            yield return ("", this);
            foreach (var item in _map) {
                var value = item.Value;
                if (value is Map map) {
                    yield return (item.Key, map);
                    foreach (var innerMap in map.Traverse())
                        yield return innerMap;
                }
                else if (value is IEnumerable<object> array) {
                    foreach (var arrayMap in array.Where(i => i is Map).Cast<Map>()) {
                        yield return (item.Key, arrayMap);
                        foreach (var innerMap in arrayMap.Traverse().Skip(1))
                            yield return innerMap;
                    }
                }
            }
        }

        /// <summary>
        /// Traverses descendant values with the specified keys and type of T 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="names"></param>
        /// <returns></returns>
        public IEnumerable<T> Traverse<T>(params string[] names)
        {
            return TraverseInternal<T>(null, names);
        }

        /// <summary>
        /// Transforms a set of descendant map values with the provided key and contained type, or list of that type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="name"></param>
        public IEnumerable<T> Translate<T>(Func<T, T> transformOperation, params string[] names) {
            return TraverseInternal<T>(transformOperation, names).ToArray();
        }

        public IEnumerable<T> TraverseInternal<T>(Func<T, T> transformOperation, params string[] name)
        {
            string previousKey = "";
            foreach (var mapTuple in Traverse())
            {
                previousKey = mapTuple.Key;
                var map = mapTuple.Map;

                foreach (var key in map.Keys.ToArray())
                {
                    var item = new
                    {
                        Key = key,
                        Value = map[key]
                    };

                    if (name.Contains(item.Key, StringComparer.OrdinalIgnoreCase) ||
                       (name.Contains($"{previousKey}/{key}", StringComparer.OrdinalIgnoreCase)))
                    {
                        var value = item.Value;
                        if (value != null)
                        {
                            if (value.GetType() == typeof(T))
                            {
                                if(transformOperation != null)
                                {
                                    value = transformOperation((T)value);
                                    map[item.Key] = (T)value;
                                }
                                yield return (T)value;
                            }
                            else if (value is IEnumerable<T> list)
                            {
                                if (transformOperation != null)
                                {
                                    var values = list
                                            .Select(v => transformOperation((T)v))
                                            .Where(v => v != null);

                                    if (value is List<T>)
                                        list = new List<T>(values);
                                    else
                                        list = values.ToArray();

                                    map[item.Key] = list;
                                }

                                foreach (var listValue in list)
                                {
                                    yield return listValue;
                                }

                            }
                            else if (value is IEnumerable<object> objectList)
                            {
                                var first = objectList.FirstOrDefault();
                                if (first?.GetType() == typeof(T))
                                {
                                    if (transformOperation != null)
                                    {
                                        var values = objectList
                                                .Select(v => (object)transformOperation((T)v))
                                                .Where(v => v != null);

                                        if (value is List<object>)
                                            objectList = new List<object>(values);
                                        else
                                            objectList = values.ToArray();

                                        map[item.Key] = objectList;
                                    }

                                    foreach (var listValue in objectList)
                                    {
                                        yield return (T)listValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a map to a set of string KeyValuePairs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> ToStringPair()
        {
            return this.Select(kv => new KeyValuePair<string, string>(kv.Key, $"{kv.Value}"));
        }

        /// <summary>
        /// Creates an identical but entirely unique copy of the map
        /// </summary>
        /// <returns></returns>
        public Map Clone()
        {
            var clone = Map.ParseJson(this.ToString());

            foreach(var zip in this.Traverse().Zip(clone.Traverse(), (o, c) => {
                c.Map.Path = o.Map.Path;
                c.Map._schema = o.Map._schema;
                return c;
            })) { }

            return clone;
        }

        public override string ToString()
        {
            if (CreateRequestedMembers && _map.Count == 0) return "";

            return JsonConvert.SerializeObject(this, Formatting.Indented, Map.SerializerSettings(null));
        }

        public string ToString(Action<JsonSerializerSettings> settings)
        {
            if (CreateRequestedMembers && _map.Count == 0) return "";

            var serializerSettings = Map.SerializerSettings(null);
            settings(serializerSettings);

            return JsonConvert.SerializeObject(this, Formatting.Indented, serializerSettings);
        }

        public static Map Diff(Map original, Map newMap, bool stopOnFirstDiff)
        {
            Map diff = null;
            foreach(var key in original.Keys.Concat(newMap.Keys).Distinct()) {
                var v1 = original[key];
                var v2 = newMap[key];
                
                if(v1 != v2) {
                    if ((v1 != null && v2 != null) && (v1 is Map && v2 is Map))
                    {
                        var subDiff = Map.Diff((Map)v1, (Map)v2);
                        if (subDiff != null)
                        {
                            if (diff == null)
                                diff = new Map();
                            diff.Add(key, subDiff);
                        }
                    }
                    else
                    {
                        if (diff == null)
                            diff = new Map();

                        diff.Add(key, v2);
                    }
                }

                if (stopOnFirstDiff && diff != null)  break; 
                else  stopOnFirstDiff = false; 
            }

            return diff;
        }

        public static Map Diff(Map original, Map newMap)
        {
            return Diff(original, newMap, false);
        }

        public static bool DeepEqual(Map map1, Map map2)
        {
            return Diff(map1, map2, true) == null;
        }

        public static Map From<T>(T item)
        {
            var text = JsonConvert.SerializeObject(item, Map.SerializerSettings(null)); 
            return Map.ParseJson(text);
        }

        public T As<T>()
        {
            try {
                return JsonConvert.DeserializeObject<T>(this.ToString());
            } catch (Exception e) {
                throw new Exception($"Failed to convert map as type {typeof(T)}. {e.Message}", e);
            }
        }

        public T GetValueAs<T>(string key)
        {
            var value = _map[key];
            if (value is T tValue)
                return tValue;
            if (value is Map map) {
                try {
                    return JsonConvert.DeserializeObject<T>(map.ToString());
                }
                catch (Exception e) {
                    throw new Exception($"Failed to get value '{key}' as type {typeof(T)}. {e.Message}", e);
                }
            }
            else if (value is null)
                return default(T);

            throw new Exception($"Unable to get {key} value as type {typeof(T).Name}. Value is invalid type {value?.GetType().Name}");

        }

    }
}
