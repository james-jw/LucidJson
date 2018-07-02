using Newtonsoft.Json;
using LucidJson.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson 
{
    public class Array : Array<object> {

        public Array() : base() { }

        public Array(MapSchema schema) : base(schema) { }

        public Array(IEnumerable<object> items) : base(items) { }

    }

    public class Array<T> : List<T>, IEnumerable<T>, INotifyCollectionChanged
    {
        public MapSchema Schema { get; set; }
        public Array() { }

        public Array(int capacity) : base(new T[capacity]) { }

        public Array(MapSchema schema)
        {
            Schema = schema;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void NotifyChanged()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public Array(IEnumerable<T> items)
        {
            if(items != null) {
                foreach(var item in items)
                    this.Add(item);
            }
        }

        public static Array ParseJson(string json)
        {
            return Array.ParseJson(json, null);
        }

        public static Array ParseJson(string json, MapSchema schema)
        {
            return Map.ParseJson<Array>(json, schema);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, Map.SerializerSettings(null));
        }
    }
}
