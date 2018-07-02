using LucidJson.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LucidJson.View
{
    public class MapValueViewModel : INotifyPropertyChanged 
    {
        Map _map;

        public Map Map {
            get { return _map; }
        }

        public MapValueViewModel(string key, Map Map, MapValueViewModel parent)
        {
            _key = key;
            _map = Map;

            // Run validation
            var value = _map[_key];
            ValidateValue(value);

            if(value is Array array)
                array.CollectionChanged += Array_CollectionChanged;

            _map.PropertyChanged += _Map_PropertyChanged;
            foreach(var innerValue in _map.Values)
            {
                if (innerValue is Map map)
                    map.PropertyChanged += _Map_PropertyChanged;
            }
        }

        private void Array_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Value), nameof(ValueBinders));
        }

        string _key;
        public string Key {
            get { return _key; }
            set {
                if (_key != value) {
                    _map._map[value] = _map[Key];
                    _map._map.Remove(_key);

                    _map.MoveSchemaObject(_key, value, _schema);

                    _key = value;
                    _schemaKey = value;

                    NotifyPropertyChanged(nameof(Key), nameof(Alias));
                }
            }
        }

        public string Alias {
            get {
                return Schema?.Alias ?? _key;
            }
        }

        public string Title {
            get {
                var firstValue = _map.Values.Where(v => v is string).FirstOrDefault();
                var titleExp = Schema.Contains?.Title;
                //if(CSharpUtility.IsTemplate(titleExp)) {
                //    var exp = CSharpUtility.CompileTemplate(titleExp);
                //    return exp(_map);
                //}
                //else if (CSharpUtility.IsLambda(titleExp)) {
                //    var exp = CSharpUtility.CompileLambda<dynamic, dynamic>(titleExp);
                //    return exp(_map);
                //}

                return (string)firstValue;
            }
        }

        public bool Visible {
            get {
                return _isVisible ?? false;
            }
            set {
                _isVisible = value;
                NotifyPropertyChanged(nameof(Visible));
            }
        }

        private string _validationError;
        public bool IsValid {
            get {
                return _validationError == null || _validationError == string.Empty;
            }
        }

        public bool IsInvalid {
            get { return !IsValid; }
        }

        public string ValidationError {
            get {
                return _validationError;
            }
            private set {
                _validationError = value;
                NotifyPropertyChanged(nameof(ValidationError), nameof(IsValid), nameof(IsInvalid));
            }
        }

        private void ValidateValue(dynamic value)
        {
            // TODO Add validation logic
            if(this.Required) {
                if(value == null || (value is string strValue && strValue == string.Empty)) {
                    ValidationError = "A value is required.";
                    return;
                }
            }  

            if(this.Type == "String") {
                var stringValue = (string)value;
                if(false) { // CSharpUtility.IsLambda(stringValue)) {
                    //Task.Run(() => {
                    //    var baseSchema = BaseSchema;
                    //    var lambdaProvider = baseSchema?.LambdaProvider;
                    //    var referenceProvider = baseSchema?.LambdaReferenceProvider;

                    //    var supportLambdas = lambdaProvider != null ? lambdaProvider() : new Map();
                    //    var references = referenceProvider != null ? referenceProvider().ToArray() : new string[0]; 
                    //    switch (CSharpUtility.LambdaParameterCount(stringValue)) {
                    //        case 4:
                    //            CSharpUtility.CompileLambda<dynamic, dynamic, dynamic, dynamic, dynamic>(stringValue, supportLambdas.ToStringPair(), null, references);
                    //            break;
                    //        case 3:
                    //            CSharpUtility.CompileLambda<dynamic, dynamic, dynamic, dynamic>(stringValue, supportLambdas.ToStringPair(), null, references);
                    //            break;
                    //        case 2:
                    //            CSharpUtility.CompileLambda<dynamic, dynamic, dynamic>(stringValue, supportLambdas.ToStringPair(), null, references);
                    //            break;
                    //        case 1:
                    //            CSharpUtility.CompileLambda<dynamic, dynamic>(stringValue, supportLambdas.ToStringPair(), null, references);
                    //            break;
                    //        default:
                    //            CSharpUtility.CompileLambda<dynamic>(stringValue, supportLambdas.ToStringPair(), null, references);
                    //            break;
                    //    }
                    //}).ContinueOn(t => {
                    //    if (t.IsFaulted) {
                    //        var appException = (ApplicationException)t.Exception?.InnerException?.InnerException;
                    //        if(appException != null)
                    //            ValidationError = $"Invalid lambda expression: {appException.Message}";
                    //        else
                    //            ValidationError = $"Invalid lambda expression: {t.Exception.Message}";
                    //    }
                    //});
                }
                else if(stringValue?.ToLower().StartsWith("http") == true) { 
                    try {
                        var testUri = new Uri(stringValue);
                        Task.Run(async () => {
                            var client = new HttpClient();
                            var result = await client.GetStringAsync(testUri);

                        }).ContinueOn(t => {
                            if (t.IsFaulted) {
                                ValidationError = "URI provided is unreachable.";
                            }
                        });
                    }
                    catch {
                        ValidationError = "Invalid URI provided.";
                        return;
                    }
                }
            }

            ValidationError = null;
        }

        public dynamic Value {
            get { return _map[Key]; }
            set {
                var currentValue = _map[Key];
                var currentType = currentValue?.GetType();
                if (!_map.ContainsKey(Key) || value is IEnumerable<object> || currentType != value?.GetType() || _map[Key] != value)
                {
                    _map[Key] = value;

                    NotifyPropertyChanged(nameof(Value), nameof(ValueBinders));
                    ValidateValue(value);
                }
            }
        }

        public string DataType {
            get {
                return Schema.DataType ?? "";
            }
        }
        public bool Required {
            get {
                return Schema?.Required == true ? true : false;
            }
        }

        public string Type {
            get {
                return Schema?.Type ?? _map[Key]?.GetType().Name;
            }
        }

        public void RemoveChild(MapValueViewModel child)
        {
            var value = Value;
            if (!(value is Array))
                throw new Exception($"Cannot remove child from none array item {Key}");

            var array = (Array)value;
            array.Remove(child.Map);
            Value = array;
        }

        private List<dynamic> _valueBinders = null;
        public IEnumerable<dynamic> ValueBinders {
            get {
                var value = Value;
                if (!(value is Array))
                    throw new Exception($"Cannot retrieve value binders from none array item {Key}");

                var array = (Array)_map[Key];
                if (_valueBinders == null || array.Count < _valueBinders.Count) {
                    _valueBinders = new List<dynamic>();
                    foreach (var item in array) {
                        if (item is Map)
                            _valueBinders.Add(new MapValueViewModel(_key, (Map)item, this));
                        else
                            _valueBinders.Add(item);
                    }
                } else {
                    foreach (var item in array) {
                        if (item is Map map) {
                            var existing = _valueBinders.Where(v => v is MapValueViewModel)
                                .Select(v => (MapValueViewModel)v).Where(b => b._map == map).FirstOrDefault();

                            if(existing == null) {
                                _valueBinders.Insert(0, new MapValueViewModel(_key, map, this) {
                                    Visible = true
                                });
                            }
                        }
                        else {
                            var existing = _valueBinders.Where(v => v == item).FirstOrDefault();

                            if(existing == null)
                                _valueBinders.Insert(0, item);
                        }
                    }
                }

                return _valueBinders.ToArray();
            }
        }

        MapSchemaItem _schema;
        string _schemaKey;
        private bool? _isVisible = false;

        public MapSchema BaseSchema {
            get { return (MapSchema)_map._schema; }
        }

        public IEnumerable<ISchemaAction> Actions {
            get {
                var baseData = BaseSchema.BaseData;
                return BaseSchema.Actions(baseData, _map, _key);
            }
        }

        public MapSchemaItem Schema {
            get {

                if (_schema == null || _schemaKey != _key) {
                    _schemaKey = _key;
                    
                    var schema = _map.Schema(_key);

                    if (schema != _schema) {
                        _schema = schema;
                        _schema.PropertyChanged += _schema_PropertyChanged; ;
                    }
                }

                return _schema;
            }
        }

        public IDomain Domain {
            get {
                var schema = Schema;

                if (schema == null)
                    return null;

                var domainName = schema.Domain;
                var domain = domainName == null ? null : 
                    ((MapSchema)_map._schema).Domains?.Where(d => d.Name == domainName)?.FirstOrDefault();

                if(domain != null && domain is IDynamicDomain) {
                    var dynamicDomain = (IDynamicDomain)domain;
                    var type = dynamicDomain.GetType();

                    domain = (IDomain)Activator.CreateInstance(type);
                    ((IDynamicDomain)domain).Context(BaseSchema.BaseData , _map, Key);
                }

                return domain;
            }
        }

        private void _schema_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;
            if(propertyName == "Key" || propertyName == SchemaFields.Alias) {
                NotifyPropertyChanged(nameof(Alias));
            }

            if(propertyName == SchemaFields.Domain) {
                NotifyPropertyChanged(nameof(Domain));
            }

            NotifyPropertyChanged(nameof(Schema));
        }

        private void _Map_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName, nameof(Title), nameof(Actions));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(params String[] propertyNames)
        {
            foreach(var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
