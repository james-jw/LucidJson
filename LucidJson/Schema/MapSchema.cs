using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public static class SchemaCoreFields {
        public const string Domains = "@domains";
    }

    public static class SchemaFields
    {
        public const string Type = "@type";
        public const string Alias = "@alias";
        public const string Domain = "@domain";
        public const string Container = "@container";
        public const string Default = "@default";
        public const string Contains = "@contains";
        public const string DataType = "@dataType";
        public const string Required = "@required";
    }

    public class MapSchema : MapSchemaItem, INotifyPropertyChanged
    {
        List<IDynamicDomain> _dynamicDomains = new List<IDynamicDomain>();
        public new event PropertyChangedEventHandler PropertyChanged;

        private List<ISchemaAction> _actions = new List<ISchemaAction>();
        private List<Domain> _staticDomains = new List<Domain>();

        [JsonIgnore]
        public Map BaseData { get; set; }

        [JsonIgnore]
        public Func<Map> LambdaProvider { get; set; } 

        [JsonIgnore]
        public Func<IEnumerable<string>> LambdaReferenceProvider { get; set; }

        [JsonProperty("@domains")]
        public List<Domain> StaticDomains {
            get {
                return _staticDomains;
            }
            set {
                _staticDomains = value;
                NotifyPropertyChanged(nameof(Domains));
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [JsonIgnore]
        private IEnumerable<IDomain> DynamicDomains {
            get {
                return _dynamicDomains;
            }
        }

        [JsonIgnore]
        public IEnumerable<IDomain> Domains {
            get {
                return StaticDomains.Concat(DynamicDomains);
            }
        }

        public void AddDynamicDomain(IDynamicDomain domainIn)
        {
            _dynamicDomains.Add(domainIn);
            NotifyPropertyChanged(nameof(Domains));
        }
        public void RemoveDynamicDomain(IDynamicDomain domainIn)
        {
            _dynamicDomains.Remove(domainIn);
            NotifyPropertyChanged(nameof(Domains));
        }

        public void AddStaticDomain(Domain domainIn)
        {
            StaticDomains.Add(domainIn);
            NotifyPropertyChanged(nameof(Domains));
        }

        public void RemoveStaticDomain(Domain domainIn)
        {
            StaticDomains.Remove(domainIn);
            NotifyPropertyChanged(nameof(Domains));
        }

        public static MapSchema ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<MapSchema>(json);
        }

        public void AddSchemaAction(ISchemaAction actionIn)
        {
            _actions.Add(actionIn);
        }

        internal IEnumerable<ISchemaAction> Actions(Map baseData, Map map, string key)
        {
           return _actions.Where(a => a.CanPerform(map, key)).Select(a => a.Prepare(baseData, map, key));
        }
    }
}
