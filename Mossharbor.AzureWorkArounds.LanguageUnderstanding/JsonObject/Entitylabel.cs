using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class Entitylabel
    {
        private static string lastExample;

        [JsonIgnore]
        public string IntentExample { get; set; }
        public string entityName { get; set; }
        public int startCharIndex { get; set; }
        public int endCharIndex { get; set; }

        public static Entitylabel Create(string entityName, string entityInExample)
        {
            return Create(lastExample, entityName, entityInExample);
        }

        public static Entitylabel Create(string intentExample, string entityName, string entityInExample)
        {
            lastExample = intentExample;
            int startIndex = intentExample.IndexOf(entityInExample);
            int endIndex = startIndex += entityInExample.Length;
            return new Entitylabel()
            {
                IntentExample = intentExample,
                entityName = entityName,
                startCharIndex = startIndex,
                endCharIndex = endIndex
            };
        }
    }
}
