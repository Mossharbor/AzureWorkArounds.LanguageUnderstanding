using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class IntentExampleBuilder
    {
        protected Func<IntentExampleJson, IntentExampleJson> fn = null;
        LuisModelBuilder parent;

        private static Func<TA, TC> Compose<TA, TB, TC>(Func<TA, TB> f1, Func<TB, TC> f2)
        {
            return (a) => f2(f1(a));
        }

        public IntentExampleBuilder(LuisModelBuilder parent, string intentExample)
        {
            this.parent = parent;
            this.fn = (ignored) => new IntentExampleJson(intentExample);
        }

        public IntentExampleBuilder(LuisModelBuilder parent,  string intentExample, string intentName)
        {
            this.parent = parent;
            this.fn = (ignored) => new IntentExampleJson(intentExample, intentName);
        }

        public IntentExampleBuilder(LuisModelBuilder parent, string intentExample, string intentName, List<KeyValuePair<string, string>> entityToEntityInExample)
        {
            this.parent = parent;
            this.fn = (ignored) => new IntentExampleJson(intentExample, intentName, entityToEntityInExample);
        }

        public LuisModelBuilder CompleteExample()
        {
            var intent = this.fn(null);
            return this.parent.AddIntentExample(intent);
        }

        public IntentExampleBuilder AddIntentName(string intentName)
        {
            this.fn = Compose(this.fn, (exampleJson) =>
            {
                exampleJson.intentName = intentName;
                return exampleJson;
            });
            return this;
        }

        public IntentExampleBuilder AddEntity(string entityName, string entityInExample)
        {
            this.fn = Compose(this.fn, (exampleJson) =>
            {
                exampleJson.AddEntityLabel(exampleJson.text, entityName, entityInExample);
                return exampleJson;
            });
            return this;
        }
    }
}
