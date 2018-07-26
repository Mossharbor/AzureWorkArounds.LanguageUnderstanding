using System;
using System.Collections.Generic;
using System.Text;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class Entitylabel
    {
        public string entityName { get; set; }
        public int startCharIndex { get; set; }
        public int endCharIndex { get; set; }

        public static Entitylabel Create(string intentExample, string entityName, string entityInExample)
        {
            int startIndex = intentExample.IndexOf(entityInExample);
            int endIndex = startIndex += entityInExample.Length;
            return new Entitylabel()
            {
                entityName = entityName,
                startCharIndex = startIndex,
                endCharIndex = endIndex
            };
        }
    }
}
