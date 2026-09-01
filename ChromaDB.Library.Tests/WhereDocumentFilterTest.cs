using Chroma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ChromaDB.Library.Tests
{
    [TestClass]
    public sealed class WhereDocumentFilterTest
    {
        [TestMethod]
        public void TestWhereDocument_Contains()
        {
            // all records whose document contains a search string
            var whereDocumentFilter1 = new WhereDocumentFilter()
                .Contains("search string");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter1.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            // {"$and":[{"$contains":"search string"}]}
           Assert.AreEqual("""{"$contains":"search string"}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_NotContains()
        {
            // all records whose document contains a search string
            var whereDocumentFilter1 = new WhereDocumentFilter()
                .NotContains("search string");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter1.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            // {"$and":[{"$contains":"search string"}]}
            Assert.AreEqual("""{"$not_contains":"search string"}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_Regex()
        {
            // records whose documents match the regex pattern for an email address
            var whereDocumentFilter2 = new WhereDocumentFilter()
                .Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter2.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            //Assert.AreEqual("""{"$regex":"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"}""", whereDocumentAsJson);
            Assert.AreEqual("""{"$regex":"^[a-zA-Z0-9._%\u002B-]\u002B@[a-zA-Z0-9.-]\u002B\\.[a-zA-Z]{2,}$"}""", whereDocumentAsJson); 
        }

        [TestMethod]
        public void TestWhereDocument_NotRegex()
        {
            // records whose documents match the regex pattern for an email address
            var whereDocumentFilter2 = new WhereDocumentFilter()
                .NotRegex("[0-9]+");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter2.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            Assert.AreEqual("""{"$not_regex":"[0-9]\u002B"}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_All()
        {
            // An $and operator will return results that match all the filters in the list
            var whereDocumentFilter3 = new WhereDocumentFilter()
                .Or()
                .NotContains("search_string_2")
                .NotRegex("[0-9]+")
                .All(
                    new WhereDocumentFilter().Contains("search_string_1"),
                    new WhereDocumentFilter().Regex("[a-z]+")
                );
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter3.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");
            Assert.AreEqual("""{"$or":[{"$not_contains":"search_string_2"},{"$not_regex":"[0-9]\u002B"},{"$and":[{"$contains":"search_string_1"},{"$regex":"[a-z]\u002B"}]}]}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_Any()
        {
            // An $or operator will return results that match any of the filters in the list
            var whereDocumentFilter4 = new WhereDocumentFilter()
                .Regex("[a-z]+")
                .NotRegex("[0-9]+")
                .Any(
                    new WhereDocumentFilter().Contains("search_string_1"),
                    new WhereDocumentFilter().NotContains("search_string_2")
                );
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter4.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            Assert.AreEqual("""{"$and":[{"$regex":"[a-z]\u002B"},{"$not_regex":"[0-9]\u002B"},{"$or":[{"$contains":"search_string_1"},{"$not_contains":"search_string_2"}]}]}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_Or()
        {
            var whereDocumentFilter5 = new WhereDocumentFilter()
                .Contains("search_string_1")
                .Or()
                .NotContains("search_string_2");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter5.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            // {"$or":[{"$contains":"search_string_1"},{"$not_contains":"search_string_2"}]}
            Assert.AreEqual("""{"$or":[{"$contains":"search_string_1"},{"$not_contains":"search_string_2"}]}""", whereDocumentAsJson);
        }

        [TestMethod]
        public void TestWhereDocument_And()
        {
            var whereDocumentFilter6 = new WhereDocumentFilter()
                .Contains("search_string_1")
                .Regex("[a-z]+");
            JsonElement whereDocumentAsJsonElement = whereDocumentFilter6.ToJsonElement();
            string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
            Assert.AreEqual("""{"$and":[{"$contains":"search_string_1"},{"$regex":"[a-z]\u002B"}]}""", whereDocumentAsJson);
        }
    }
}
