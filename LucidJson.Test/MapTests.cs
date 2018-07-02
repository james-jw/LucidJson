using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using LucidJson.Schema;

namespace LucidJson.Tests
{
    [TestClass()]
    public class MapTests
    {
        MapSchema schema;
        Map map;

        private void PrepareSchemaTest()
        {
            var schemaJson = File.ReadAllText(@".\Data\schneiderville-to-un.schema.json");
            schema = MapSchema.ParseJson(schemaJson);

            var mapJson = File.ReadAllText(@".\Data\schneiderville-to-un.json");
            map = Map.ParseJson(mapJson, schema);
        }

        [TestMethod()]
        public void SchemaTest()
        {
            PrepareSchemaTest();

            var settings = (Map)map["settings"];
            var source = settings["sourceFeatureService"];

        }

        [TestMethod()]
        public void MapDiffTest()
        {
            var map1 = new Map() {
                { "Name", "Test" },
                { "Age", 23 }
            };

            var map2 = new Map() {
                { "Name", "Test" },
                { "Age", 25 },
                { "Title", "Cook" },
            };

            var diff = Map.Diff(map1, map2);
            Assert.AreEqual(25, diff["Age"]);
            Assert.AreEqual("Cook", diff["Title"]);
            Assert.AreEqual(2, diff.Count());
        }

        Map data = new Map()
            {
                { "id", 23 },
                { "items", new []
                   { new Map() { { "ids", new[] { 24, 25 } } },
                     new Map() { { "ids", new[] { 26, 27 } } } }
                }
            };

        Map dataString = new Map()
            {
                { "id", "23" },
                { "items", new []
                   { new Map() { { "ids", new[] { "24", "25" } } },
                     new Map() { { "ids", new[] { 26, 27 } } } }
                }
            };

        [TestMethod()]
        public void TraverseTest()
        {

            var valuesToCheck = new[] { 23, 24, 25, 26, 27 };
            var traversedIds = data.Traverse<int>("id", "ids");
            foreach (var value in valuesToCheck)
            {
                var ids = traversedIds;
                if (ids.Contains(value) == false)
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod()]
        public void TraverseTestSubKey()
        {
            var valuesToCheck = new[] { 24, 25, 26, 27 };
            var traversedIds = data.Traverse<int>("items/ids").ToArray();
            foreach (var value in valuesToCheck)
            {
                var ids = traversedIds;
                if (ids.Contains(value) == false)
                {
                    Assert.Fail();
                }
            }

            Assert.AreEqual(4, traversedIds.Count());
            Assert.AreEqual(5, data.Traverse<int>("id", "ids").ToArray().Count());
        }

        [TestMethod()]
        public void TransformTest()
        {
            var valuesToCheck = new[] { 23, 24, 25, 26, 27 };
            var traversedIds = data.Translate<int>(i => i + 5, "id", "ids").ToArray();
            traversedIds = data.Traverse<int>("id", "ids").ToArray();

            foreach (var value in valuesToCheck)
            {
                var ids = traversedIds;
                if (ids.Contains(value + 5) == false)
                {
                    Assert.Fail();
                }
            }

        }

        [TestMethod()]
        public void TransformTestString()
        {
            var valuesToCheck = new[] { "23", "24", "25" };
            var traversedIds = dataString.Translate<string>(i => $"{int.Parse(i) + 5}", "id", "ids").ToArray();
            traversedIds = dataString.Traverse<string>("id", "ids").ToArray();

            foreach (var value in valuesToCheck)
            {
                var ids = traversedIds;
                if (ids.Contains($"{int.Parse(value) + 5}") == false)
                {
                    Assert.Fail();
                }
            }

        }

    }
}