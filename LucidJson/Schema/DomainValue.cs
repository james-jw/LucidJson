using Newtonsoft.Json;

namespace LucidJson.Schema
{
    public class DomainValue
    {
        public DomainValue(string nameValue)
        {
            Name = nameValue;
            Value = nameValue;
        }

        public DomainValue() { }

        [JsonProperty("name")]
        public string Name { get; set; }
       
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
