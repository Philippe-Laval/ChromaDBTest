using Chroma;
using System.Text.Json;

namespace ChromaDB.Library.Tests
{
    [TestClass]
    public sealed class WhereFilterTest
    {
        [TestMethod]
        public void TestWhereFilter_Equals()
        {
            var whereFilter = new WhereFilter()
                        .Equals("category", "Botanic books");
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"category":"Botanic books"}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_GreaterThan()
        {
            var whereFilter = new WhereFilter()
                        .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"page":{"$gt":10}}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_NotIn()
        {
            // Example for documentation : "metadata_field": {"$nin": ["value1", "value2", "value3"]}
            var whereFilter = new WhereFilter()
                        .NotIn("metadata_field", new List<string> { "value1", "value2", "value3" });
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"metadata_field":{"$nin":["value1","value2","value3"]}}""", whereAsJson);
        }


        [TestMethod]
        public void TestWhereFilter_In()
        {
            // Example for documentation : "author": {"$in": ["Rowling", "Fitzgerald", "Herbert"]}
            var whereFilter = new WhereFilter()
                        .In("author", new List<string> { "Rowling", "Fitzgerald", "Herbert" });
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"author":{"$in":["Rowling","Fitzgerald","Herbert"]}}""", whereAsJson);
        }


        [TestMethod]
        public void TestWhereFilter_And()
        {
            var whereFilter = new WhereFilter()
                        .Equals("category", "Botanic books")
                        .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"category":"Botanic books"},{"page":{"$gt":10}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_AndFromDoc()
        {
            // "$and": [ {"page": {"$gte": 5 }}, {"page": {"$lte": 10 }} ]
            var whereFilter = new WhereFilter()
                        .GreaterThanOrEqual("page", 5)
                        .LessThanOrEqual("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"page":{"$gte":5}},{"page":{"$lte":10}}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_OrWithOnlyOneItem()
        {
            // Or with one item is like a normal filter, so it should not be wrapped in an $or clause
            var whereFilter = new WhereFilter()
                        .Or()
                        .Equals("color", "blue");
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"color":"blue"}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_Or()
        {
            // Example for documentation : "$or": [ { "color": "red"}, { "color": "blue"} ]
            var whereFilter = new WhereFilter()
                        .Equals("color", "red")
                        .Or()
                        .Equals("color", "blue");
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$or":[{"color":"red"},{"color":"blue"}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_MultipleAnd()
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
        public void TestWhereFilter_All()
        {
            var whereFilter3 = new WhereFilter()
                       .All(
                           new WhereFilter().Equals("category", "Botanic books"),
                           new WhereFilter().Equals("language", "fr"));
            JsonElement whereAsJsonElement = whereFilter3.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"category":"Botanic books"},{"language":"fr"}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_Any()
        {
            var whereFilter3 = new WhereFilter()
                       .Any(
                           new WhereFilter().Equals("language", "en"),
                           new WhereFilter().Equals("language", "fr"));
            JsonElement whereAsJsonElement = whereFilter3.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$or":[{"language":"en"},{"language":"fr"}]}""", whereAsJson);
        }

        [TestMethod]
        public void TestWhereFilter_EqualsAndAny()
        {
            var whereFilter3 = new WhereFilter()
                       .Equals("published", true)
                       .Any(
                           new WhereFilter().Equals("language", "en"),
                           new WhereFilter().Equals("language", "fr"));
            JsonElement whereAsJsonElement = whereFilter3.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            Assert.AreEqual("""{"$and":[{"published":true},{"$or":[{"language":"en"},{"language":"fr"}]}]}""", whereAsJson);
        }
    }
}
