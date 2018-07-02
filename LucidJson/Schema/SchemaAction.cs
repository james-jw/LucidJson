using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public class SchemaAction : ISchemaAction
    {
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual string Key { get; }

        public virtual bool CanPerform(Map mapIn, string key) {
            return true;
        }

        public virtual ISchemaAction Prepare(Map fullContext, Map relativeContext, string key) {
            return new SchemaAction();
        }

        public virtual void Perform() { }

    }
}
