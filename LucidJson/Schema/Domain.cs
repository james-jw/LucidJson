using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public class Domain : IDomain
    {
        public Domain()
        {
            DefinedValues = new List<DomainValue>();
        }

        [JsonProperty("valueExpression")]
        public string ValueExpression { get; set; }

        [JsonProperty("definedValues")]
        public List<DomainValue> DefinedValues { get; set; }

        public bool? RestrictCustomValues { get; set; }

        [JsonIgnore]
        public IEnumerable<DomainValue> Values {
            get {
                return DefinedValues;
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
