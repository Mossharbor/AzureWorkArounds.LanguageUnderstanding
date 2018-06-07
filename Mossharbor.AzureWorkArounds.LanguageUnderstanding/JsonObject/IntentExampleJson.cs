using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class IntentExampleJson
    {
        public List<Entitylabel> entityLabelsList = new List<Entitylabel>();

        public IntentExampleJson() { }
        public IntentExampleJson(string example)
        {
            this.text = example;
        }

        public IntentExampleJson(string example, string intentName)
        {
            this.text = example;
            this.intentName = intentName;
        }

        public IntentExampleJson(string example, string intentName, List<KeyValuePair<string,string>> entityToEntityInExample)
        {
            this.text = example;
            this.intentName = intentName;

            foreach(var t in entityToEntityInExample)
            {
                AddEntityLabel(example, t.Key, t.Value);
            }
        }

        public string text { get; set; }
        public string intentName { get; set; }
        public Entitylabel[] entityLabels { get { return entityLabelsList.ToArray(); } set { entityLabelsList = new List<Entitylabel>(value); } }

        internal void AddEntityLabel(string intentExample, string entityName, string entityInExample)
        {
            int startIndex = intentExample.IndexOf(entityInExample);
            int endIndex = startIndex += entityInExample.Length;
            entityLabelsList.Add(new Entitylabel()
            {
                entityName = entityName,
                startCharIndex = startIndex,
                endCharIndex = endIndex
            });
        }
    }

    public class Entitylabel
    {
        public string entityName { get; set; }
        public int startCharIndex { get; set; }
        public int endCharIndex { get; set; }
    }

}
