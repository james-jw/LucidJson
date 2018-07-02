using LucidJson.Schema;
using LucidJson.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LucidJson 
{
    public partial class Map : DynamicObject, IDictionary<string, object>  
    {
        private Func<string, string> mapper; 
        internal Dictionary<string, object> _map { get; private set; }

        public ICollection<string> Keys => _map.Keys;

        public ICollection<object> Values => _map.Values;

        public static bool WarnOnKeyNotFound = false;

        public int Count => _map.Count;

        public bool IsReadOnly => false;

        public bool CreateRequestedMembers { get; set; }

        internal MapSchemaItem _schema;

        /// <summary>
        /// Renames a schema item with the provided fromKey to the provided toKey on the 
        /// </summary>
        /// <param name="fromKey"></param>
        /// <param name="toKey"></param>
        /// <param name="schema"></param>
        internal void MoveSchemaObject(string fromKey, string toKey, MapSchemaItem schema) { 
            if (_schema == null)
                throw new Exception("No parent schema found.");

            MapSchemaItem current = _schema;
            foreach(var part in Path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)) {
                try {
                    if (current.Type == "Array" || current.Container == true) {
                        var contained = current.Contains;
                        if (contained == null)
                            current.Contains = contained = new MapSchemaItem();

                        current = contained;
                    }
                    current = current.Fields[part];
                } catch {
                    throw new Exception($"Failed to find schema at path {Path}. Path not found {part}");
                }
            }

            if (current.Container != true) {
                current.Fields[toKey] = schema;
                current.Fields.Remove(fromKey);
            }
        }

        /// <summary>
        /// Returns the MapSchemaItem in the base schema which is associated to the provided key on this map.
        /// </summary>
        /// <param name="keyIn">The key to find the schema for</param>
        /// <returns></returns>
        public MapSchemaItem Schema(string keyIn = null) {
            if (_schema == null)
                return null;

            MapSchemaItem current = _schema;
            if (Path != String.Empty) {
                foreach (var part in Path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)) {
                    try {
                        if (current.Type == "Array" || current.Container == true) {
                            var contained = current.Contains;
                            if (contained == null)
                                current.Contains = contained = new MapSchemaItem();

                            current = contained;
                        }

                        current = current.GetField(part);
                        if (keyIn == part)
                            return current;
                    }
                    catch {
                        throw new Exception($"Failed to find schema at path {Path}. Path not found {part}");
                    }
                }
            }

            if (current == null)
                throw new Exception($"No schema found for {Path}. Key requested: {keyIn}");

            if (keyIn == null) {
                return current;
            }

            MapSchemaItem schemaOut = null;
            var isContainer = current.Container ?? false;
            if (current.Type == "Array" || isContainer == true) {
                var contained = current.Contains;
                if(contained != null && contained.Type == "Map" && !contained.Fields.ContainsKey(keyIn)) {
                    contained.Fields.Add(keyIn, new MapSchemaItem());
                }

                if (contained == null) {
                    current.Contains = contained = new MapSchemaItem();
                    contained.Type = "Map";
                    contained.Fields.Add(keyIn, new MapSchemaItem());
                }

                current = contained;

                if (isContainer || !current.Fields.ContainsKey(keyIn))
                    schemaOut = current;
                else {
                    if (current.Type == null)
                        current.Type = "Map";

                    schemaOut = current.GetField(keyIn);
                }
            }
            else {
                schemaOut = current.GetField(keyIn);
            }

            return schemaOut; 
        }

        private string _path;
        public string Path {
            get { return _path; }
            set {
                if(_path != value) {
                    _path = value;
                    _key = _path?.Split('.').Last();
                }
            }
        }

        private string _key;
        public string Key => _key;

        public Map()
        {
            _map = new Dictionary<string, object>();
            init();
        }

        public Map(Map mapIn, Func<string, string> mapperIn) : this()
        {
            _map = mapIn;
            mapper = mapperIn;
            Path = String.Empty;
        }

        public Map(Dictionary<string, object> dictionaryIn)
        {
            _map = dictionaryIn;
            init();
        }

        private void init()
        {
            Path = String.Empty;
            mapper = keyIn =>
            {
                foreach (var key in _map.Keys)
                {
                    if (key.Equals(keyIn, StringComparison.CurrentCultureIgnoreCase))
                        return key;
                }

                var regex = new Regex($".*{keyIn}$", RegexOptions.IgnoreCase);

                var matches = new List<string>();
                string match = null;
                foreach (var key in _map.Keys)
                {
                    if (regex.IsMatch(key))
                    {
                        match = key;
                        matches.Add(match);
                    }
                }


                if (matches.Count > 1) {
                    regex = new Regex($".*[.]{keyIn}", RegexOptions.IgnoreCase);
                    foreach (var m in matches) {
                        if (regex.IsMatch(m))
                            match = m;
                    }

                    if(match == null)
                        throw new Exception($"Ambigous key {match}. Found {String.Join(",", matches)} other matches.");
                }

                return match;
            };
        }

        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try {
                return TryGetMemberInternal(binder.Name, out result);
            } catch (Exception e) {
                throw new Exception($"Dynamic failed failed to bind to dynamic member '{binder.Name}'", e);
            }
        }

        private bool TryGetMemberInternal(string property, out object result)
        {
            if(_map.TryGetValue(property, out result))
            {
                result = Map.ProcessValue(result);
                if (result is Map)
                {
                    result = ((Map)result).AsDynamic();
                }
            }
            else if(CreateRequestedMembers)
            {
                object newValue = null;

                if (property.StartsWith("@")) {
                    newValue = null;
                } else { 
                    newValue = new Map() {
                        CreateRequestedMembers = CreateRequestedMembers
                    };
                }

                _map[property] = newValue;
                result = newValue;
            }
            else if(mapper != null)
            {
                var key = mapper(property);

                if (key != null)
                    result = _map[key];
                else {
                    if (WarnOnKeyNotFound) {
                        Console.WriteLine($"The key '{property}' was not found in the dynamic map: {this.ToString()}");
                    }

                    result = null;
                }
            }
            else return false;

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public void Add(string key, object value)
        {
            this[key] = value;
        }

        public bool Remove(string key)
        {
            _map.Remove(key);
            foreach (var pair in _binders)
            {
                var binders = pair.Value;
                var binderToRemove = binders.Where(b => b.Key == key).FirstOrDefault();
                if (binderToRemove != null)
                    binders.Remove(binderToRemove);
            }

            return true;
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _map.Remove(item.Key);
        }

        public bool TryGetValue(string keyIn, out object value)
        {
            var key = mapper(keyIn);
            if (key == null || !_map.ContainsKey(key)) {
                value = null;
                return false;
            }
            else {
                value = _map[mapper(key)];
                return true;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _map.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public dynamic this[string index]
        {
            get {
                if (TryGetMemberInternal(index, out object value))
                    return value;

                return null;
            }
            set {
                if (locked)
                    throw new InvalidOperationException($"Map is locked from editing: {this}");

                var add = false;
                if (!_map.ContainsKey(index))
                    add = true;

                _map[index] = value;
                NotifyPropertyChanged(index);

                if (add && _binders.TryGetValue("all", out ObservableCollection<MapValueViewModel> binders)) {
                    var newBinder = new MapValueViewModel(index, this, null);
                    newBinder.Visible = true;
                    binders.Insert(0, newBinder);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static implicit operator Dictionary<string, object>(Map input)
        {
            return input._map;
        }

        private Dictionary<string, ObservableCollection<MapValueViewModel>> _binders = new Dictionary<string, ObservableCollection<MapValueViewModel>>();
        private static Dictionary<int, Map<MapValueViewModel>> _binderMaps = new Dictionary<int, Map<MapValueViewModel>>();

        private IEnumerable<MapValueViewModel> GetObservableByType(string bindersName, Func<MapSchemaItem, bool> fieldPredicate = null)
        {
            if (!_binders.TryGetValue(bindersName, out ObservableCollection<MapValueViewModel> binders))
            {
                var schema = Schema();

                // Ensure all keys from the json and schema are value bound 
                var schemaFields = schema.Fields.Keys.Count > 0 ?
                    schema.Fields : schema.Contains.Fields;
                var schemaKeys = from pair in schemaFields
                                 let field = pair.Value
                                 where field != null && field.Deprecated != true && field.Hidden != true
                                       && (fieldPredicate == null || fieldPredicate?.Invoke(field) == true)
                                 select pair.Key;

                var undefinedKeys = Keys.Where(k => schemaFields.Keys.Contains(k) == false);
                var keys = from k in schemaKeys.Concat(undefinedKeys).Distinct()
                           where k != Key
                           select k;

                foreach (var key in keys.Where(k => this.ContainsKey(k) == false))
                {
                    this[key] = null;
                }

                var newBinders = keys.Select(k => new MapValueViewModel(k, this, null));
                binders = new ObservableCollection<MapValueViewModel>(newBinders);
                _binders[bindersName] = binders;
            }

            return binders;
        } 

        /// <summary>
        /// Returns an Enumeration of MapValueViewModels for UI interaction with the underlying Map
        /// </summary>
        public IEnumerable<MapValueViewModel> AsObservable {
            get {
                return GetObservableByType("all");
            }
        }

        /// <summary>
        /// Returns an Enumeration of MapValueViewModels for UI interaction with the underlying Map
        /// which are deemed complex, with multiple field types
        /// </summary>
        public IEnumerable<MapValueViewModel> ComplexObservables {
            get {
                return GetObservableByType("complex", f => {
                    if (f.Type != "String")
                        return false;

                    return f.DataType != null && f.DataType != string.Empty;
                });
            }
        }

        /// <summary>
        /// Returns an Enumeration of MapValueViewModels for UI interaction with the underlying Map
        /// which are deemed simple, with few fields and field types 
        /// </summary>
        public IEnumerable<MapValueViewModel> SimpleObservables {
            get {
                return GetObservableByType("simple", f => {
                    if (f.Type == "String") 
                        return f.DataType == null || f.DataType == string.Empty;

                    return f.Type != "Bool";
                });
            }
        }

        /// <summary>
        /// Returns an Enumeration of MapValueViewModels for UI interaction with the underlying Map
        /// which map to a boolean field 
        /// </summary>
        public IEnumerable<MapValueViewModel> BooleanObservables {
            get {
                return GetObservableByType("boolean", f => f.Type == "Bool");
            }
        }

        /// <summary>
        /// Returns a Map of MapValueViewModels for UI interaction with the underlying Map values
        /// </summary>
        public Map<MapValueViewModel> AsValueViewModel {
            get {
                var hashCode = this.GetHashCode();
                if (!_binderMaps.TryGetValue(hashCode, out Map<MapValueViewModel> binderMap)) {
                    var binders = AsObservable;
                    binderMap = new Map<MapValueViewModel>();
                    foreach (var binder in binders)
                        binderMap.Add(binder.Key, binder);

                    _binderMaps[hashCode] = binderMap;
                }

                return binderMap;
            }
        }

    }
}
