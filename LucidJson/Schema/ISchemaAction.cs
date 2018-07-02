using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public interface ISchemaAction
    {
        string Name { get; }
        string Description { get; }

        bool CanPerform(Map mapIn, string key);

        ISchemaAction Prepare(Map fullContext, Map relativeContext, string key);
        void Perform();
    }
}