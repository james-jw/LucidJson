using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public interface IDynamicDomain : IDomain
    {
        void Context(Map fullContext, Map relativeContext, string relativeKey);
    }
}
