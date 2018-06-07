using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{

    public class QueryResponse
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Entity[] entities { get; set; }
        public Intent[] intents { get; set; }
        public Sentimentanalysis sentimentAnalysis { get; set; }
    }

    public class Topscoringintent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Sentimentanalysis
    {
        public string label { get; set; }
        public float score { get; set; }
    }
    
    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }
    
    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public Resolution resolution { get; set; }
        public float score { get; set; }
    }

    public class Resolution
    {
        public string value { get; set; }
        public string unit { get; set; }
        public string[] values { get; set; }
    }

}
