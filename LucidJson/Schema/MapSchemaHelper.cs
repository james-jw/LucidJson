using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidJson.Schema
{
    public static class MapSchemaHelper
    {
        public static object GenerateItem(MapSchemaItem localSchema, String path, MapSchema baseSchema)
        {
            var type = localSchema.Type;
            var isContainer = localSchema.Container ?? false;
            object defaultValue = localSchema.DefaultValue ?? null;
            
            switch(type) {
                case "Map":
                    var item = new Map(baseSchema);
                    item.Path = path;
                    if (!isContainer) {
                        GenerateProperties(item, localSchema, path, baseSchema);
                    }

                    return item;
                case "Array":
                    return new LucidJson.Array();
                case "String":
                    return defaultValue ?? String.Empty;
                case "Boolean":
                    return defaultValue ?? false;
                default:
                    return null;
            }
        }

        public static MapSchemaItem FindSchemaItem(string path, string keyIn, MapSchema baseSchema)
        {
            if (baseSchema == null)
                return null;

            MapSchemaItem current = baseSchema;
            if (path != String.Empty) {
                foreach (var part in path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)) {
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
                        throw new Exception($"Failed to find schema at path {path}. Path not found {part}");
                    }
                }
            }

            if (current == null)
                throw new Exception($"No schema found for {path}. Key requested: {keyIn}");

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

        private static void GenerateProperties(Map map, MapSchemaItem localSchema, String path, MapSchema baseSchema)
        {
            foreach(var prop in localSchema.Fields.Where(f => f.Value.Deprecated != true)) {
                map[prop.Key] = GenerateItem(prop.Value, $"{path}.{prop.Key}".Trim('.'), baseSchema);
            }
        }
    }
}
