using System.Collections.Generic;

namespace LucidJson.Schema
{
    public interface IDomain
    {
        IEnumerable<DomainValue> Values { get; }
        string Name { get; }

        string Type { get; }
        bool? RestrictCustomValues { get; }
    }
}