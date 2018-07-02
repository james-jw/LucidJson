using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public class MapSchemaItem : INotifyPropertyChanged 
    {
        public MapSchemaItem() {
            Fields = new Dictionary<string, MapSchemaItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(params string[] parameters)
        {
            if (PropertyChanged != null) {
                foreach (var param in parameters) {
                    PropertyChanged(this, new PropertyChangedEventArgs(param));
                }
            }
        }

        public MapSchemaItem GetField(string name)
        {
            if (!Fields.TryGetValue(name, out MapSchemaItem fieldOut)) {
                fieldOut = new MapSchemaItem();
                Fields[name] = fieldOut;
            }

            return fieldOut;
        }

        string _type;
        [JsonProperty("@type")]
        public string Type {
            get {
                return _type;
            }
            set {
                if (_type != value) {
                    _type = value;
                    NotifyPropertyChanged(nameof(Type));
                }
            }
        }

        string _dataType;
        [JsonProperty("@dataType")]
        public string DataType {
            get {
                return _dataType;
            }
            set {
                if (_dataType != value) {
                    _dataType = value;
                    NotifyPropertyChanged(nameof(DataType));
                }
            }
        }

        string _domain;
        [JsonProperty("@domain")]
        public string Domain {
            get {
                return _domain;
            }
            set {
                if (_domain != value) {
                    _domain = value;
                    NotifyPropertyChanged(nameof(Domain));
                }
            }
        }

        string _alias;
        [JsonProperty("@alias")]
        public string Alias {
            get {
                return _alias;
            }
            set {
                if (_alias != value) {
                    _alias = value;
                    NotifyPropertyChanged(nameof(Alias));
                }
            }
        }

        string _name;
        [JsonProperty("@name")]
        public string Name {
            get {
                return _name;
            }
            set {
                if (_name != value) {
                    _name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        string _defaultValue;
        [JsonProperty("@defaultValue")]
        public string DefaultValue {
            get {
                return _defaultValue;
            }
            set {
                if (_defaultValue != value) {
                    _defaultValue = value;
                    NotifyPropertyChanged(nameof(DefaultValue));
                }
            }
        }

        MapSchemaItem _contains;
        [JsonProperty("@contains")]
        public MapSchemaItem Contains {
            get {
                return _contains;
            }
            set {
                if (_contains != value) {
                    _contains = value;
                    NotifyPropertyChanged(nameof(Contains));
                }
            }
        }

        bool? _required;
        [JsonProperty("@required")]
        public bool? Required {
            get {
                return _required;
            }
            set {
                if (_required != value) {
                    _required = value;
                    NotifyPropertyChanged(nameof(Required));
                }
            }
        }

        bool? _container;
        [JsonProperty("@container")]
        public bool? Container {
            get {
                return _container;
            }
            set {
                if (_container != value) {
                    _container = value;
                    NotifyPropertyChanged(nameof(Container));
                }
            }
        }

        string _title;
        [JsonProperty("@title")]
        public string Title {
            get {
                return _title;
            }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyPropertyChanged(nameof(Title));
                }
            }
        }

        string _description;
        [JsonProperty("@description")]
        public string Description {
            get {
                return _description;
            }
            set {
                if (_description != value) {
                    _description = value;
                    NotifyPropertyChanged(nameof(Description));
                }
            }
        }

        bool? _editable;
        [JsonProperty("@editable")]
        public bool? Editable {
            get {
                return _editable;
            }
            set {
                if (_editable != value) {
                    _editable = value;
                    NotifyPropertyChanged(nameof(Editable));
                }
            }
        }

        bool? _deprecated;
        [JsonProperty("@deprecated")]
        public bool? Deprecated {
            get {
                return _deprecated;
            }
            set {
                if(_deprecated != value) {
                    _deprecated = value;
                    NotifyPropertyChanged(nameof(Deprecated));
                }
            }
        }

        bool? _hidden;
        [JsonProperty("@hidden")]
        public bool? Hidden {
            get {
                return _hidden;
            }
            set {
                if(_hidden != value) {
                    _hidden = value;
                    NotifyPropertyChanged(nameof(Hidden));
                }
            }
        }

        string _validation;
        [JsonProperty("@validation")]
        public string Validation {
            get {
                return _validation;
            }
            set {
                if (_validation != value) {
                    _validation = value;
                    NotifyPropertyChanged(nameof(Validation));
                }
            }
        }

        Dictionary<String, MapSchemaItem> _fields;
        [JsonProperty("@fields")]
        public Dictionary<String, MapSchemaItem> Fields {
            get {
                return _fields;
            }
            set {
                if (_fields != value) {
                    _fields = value;
                    NotifyPropertyChanged(nameof(Fields));
                }
            }
        }
    }
}
