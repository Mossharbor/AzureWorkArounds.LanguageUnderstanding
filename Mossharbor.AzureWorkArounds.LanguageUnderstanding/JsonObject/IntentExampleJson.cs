using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class IntentExampleJson
    {
        [JsonIgnore]
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

        public IntentExampleJson(string example, string intentName, IList<Entitylabel> entityToEntityInExample)
        {
            this.text = example;
            this.intentName = intentName;

            foreach(var t in entityToEntityInExample)
            {
                AddEntityLabel(example, t);
            }
        }

        public string text { get; set; }
        public string intentName { get; set; }
        public Entitylabel[] entityLabels { get { return entityLabelsList.ToArray(); } set { entityLabelsList = new List<Entitylabel>(value); } }

        internal void AddEntityLabel(string intentExample, Entitylabel label)
        {
            entityLabelsList.Add(label);
        }
    }

}
