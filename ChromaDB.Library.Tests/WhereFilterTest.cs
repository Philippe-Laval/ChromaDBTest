using Chroma;
using System.Text.Json;

namespace ChromaDB.Library.Tests
{
    [TestClass]
    public sealed class WhereFilterTest
    {
        [TestMethod]
        public void TestEquals()
        {
            var whereFilter = new WhereFilter()
                        .Equals("category", "Botanic books");
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"category":"Botanic books"}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestGreaterThan()
        {
            var whereFilter = new WhereFilter()
                        .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"page":{"$gt":10}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestAnd()
        {
            var whereFilter = new WhereFilter()
                        .Equals("category", "Botanic books")
                        .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"category":"Botanic books"},{"page":{"$gt":10}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestOr()
        {
            var whereFilter = new WhereFilter()
                        .Equals("category", "Botanic books")
                        .Or()
                        .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$or":[{"category":"Botanic books"},{"page":{"$gt":10}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestIn()
        {
            var whereFilter2 = new WhereFilter()
                       .Equals("category", "Botanic books")
                       .GreaterThan("page", 10)
                       .In("language", new List<string> { "en", "fr" });
            JsonElement whereAsJsonElement = whereFilter2.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"category":"Botanic books"},{"page":{"$gt":10}},{"language":{"$in":["en","fr"]}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestAny()
        {
            var whereFilter3 = new WhereFilter()
                       .Equals("published", true)
                       .Any(
                           new WhereFilter().Equals("language", "en"),
                           new WhereFilter().Equals("language", "fr"));
            JsonElement whereAsJsonElement = whereFilter3.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"published":true},{"$or":[{"$and":[{"language":"en"}]},{"$and":[{"language":"fr"}]}]}]}""", whereAsJson);
        }
    }
}
