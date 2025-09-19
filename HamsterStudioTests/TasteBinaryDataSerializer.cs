using HamsterStudio.Barefeet.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudioTests
{

    class Record
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonIgnore]
        public string RuntimeTag { get; set; }
    }

    [TestClass]
    public class TasteBinaryDataSerializer
    {

        [TestMethod]
        public void MyTestMethod()
        {
            var list = new List<Record>();
            list.Add(new Record { Name = "Alice", Description = "/", Age = 99, RuntimeTag = "NA" });
            list.Add(new Record { Name = "Bob", Description = "/", Age = 98, RuntimeTag = "NA" });
            list.Add(new Record { Name = "Right", Description = "Damn, this is right.", Age = 97, RuntimeTag = "NA" });
            list.Add(new Record { Name = "Damn", Description = "/", Age = 99, RuntimeTag = "NA" });

            var data = BinaryDataSerializer.Serialize(list);
            var ddata = BinaryDataSerializer.Deserialize<List<Record>>(data);

            foreach (var dat in ddata)
            {
                Console.WriteLine(dat.Name + " " + dat.Description);
            }

        }

    }
}
