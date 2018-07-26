using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class LuisModelBuilder
    {
        LuisModel luis;

        /// <summary>
        /// This is the function that modifies the Activity we are trying to build
        /// </summary>
        protected Func<LuisModelBuilder, LuisModelBuilder> fn = null;
        /// <summary>
        /// This function composes the builder function
        /// </summary>
        /// <typeparam name="TA">input type to f1</typeparam>
        /// <typeparam name="TB">output type to f1 and input type to f2</typeparam>
        /// <typeparam name="TC">output type of f2</typeparam>
        /// <param name="f1">first function in the chain to call</param>
        /// <param name="f2">second function in the chain to call</param>
        /// <returns>The function chain</returns>
        private static Func<TA, TC> Compose<TA, TB, TC>(Func<TA, TB> f1, Func<TB, TC> f2)
        {
            return (a) => f2(f1(a));
        }
        
        internal LuisModelBuilder(LuisModel model)
        {
            this.luis = model;
            this.fn = (ignored) => new LuisModelBuilder(model);
        }
        
        internal virtual LuisModelBuilder AddIntentExample(IntentExampleJson intent)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = luis.UriRoot+ "examples";
                string requestBody = JsonConvert.SerializeObject(intent);

                var response = luis.SendPost(uri, requestBody, true).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                return modelBuilder;
            });
            return this;
        }

        public void Update()
        {
            this.fn(null);
        }

        public virtual LuisModelBuilder AddIntent(string intentName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = luis.UriRoot + "intents";
                string jsonBody = "{ \"name\": \"" + intentName + "\"}";
                luis.SendPost(uri, jsonBody, true).Wait();

                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddIntent(string intentName, string intentExample)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = luis.UriRoot + "intents";
                string jsonBody = "{ \"name\": \"" + intentName + "\"}";
                luis.SendPost(uri, jsonBody, true).Wait();

                var examplejson = new IntentExampleJson(intentExample, intentName);
                modelBuilder.AddIntentExample(examplejson);

                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddIntent(string intentName, string intentExample, Entitylabel entityInExample)
        {
            return AddIntent(intentName, intentExample, new List<Entitylabel>() { entityInExample });
        }

        public virtual LuisModelBuilder AddIntent(string intentName, string intentExample, IList<Entitylabel> entitiesInExample)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = luis.UriRoot + "intents";
                string jsonBody = "{ \"name\": \"" + intentName + "\"}";
                luis.SendPost(uri, jsonBody, true).Wait();

                var examplejson = new IntentExampleJson(intentExample, intentName, entitiesInExample);
                modelBuilder.AddIntentExample(examplejson);

                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder RenameIntent(string existingName, string newName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                Guid intentID = luis.GetIntents()[existingName];
                return RenameIntent(intentID, newName);
            });
            return this;
        }

        public virtual LuisModelBuilder RenameIntent(Guid intentID, string newName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = string.Format("{0}intents/{1}", luis.UriRoot, intentID.ToString());
                string jsonBody = "{ \"name\": \"" + newName + "\"}";
                luis.SendPut(uri, jsonBody, true).Wait();

                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder DeleteIntent(string exstingIntent)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                Guid intentToDelete = luis.GetIntents()[exstingIntent];
                return DeleteIntent(intentToDelete);
            });
            return this;
        }

        public virtual LuisModelBuilder DeleteIntent(Guid intentToDelete)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = string.Format("{0}{1}intents/{1}", luis.UriRoot, intentToDelete.ToString());
                luis.SendDelete(uri, true).Wait();
                return modelBuilder;
            });
            return this;
        }
        
        public virtual LuisModelBuilder DeleteEntity(Guid entityToDelete)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                List<Task<HttpResponseMessage>> tasksToWaitOn = new List<Task<HttpResponseMessage>>();
                string uri = string.Format("{0}{1}entities/{1}", luis.UriRoot, entityToDelete.ToString());
                tasksToWaitOn.Add(luis.Retry(luis.SendDelete, uri));

                uri = string.Format("{0}{1}closedlists/{1}", luis.UriRoot, entityToDelete.ToString());
                tasksToWaitOn.Add(luis.Retry(luis.SendDelete, uri));

                uri = string.Format("{0}{1}compositeentities/{1}", luis.UriRoot, entityToDelete.ToString());
                tasksToWaitOn.Add(luis.Retry(luis.SendDelete, uri));

                uri = string.Format("{0}{1}hierarchicalentities/{1}", luis.UriRoot, entityToDelete.ToString());
                tasksToWaitOn.Add(luis.Retry(luis.SendDelete, uri));

                Task.WhenAll(tasksToWaitOn).Wait();
                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder RenameEntity(string existingEntity, string newName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                Guid entityID = luis.GetEntities()[existingEntity];
                return RenameEntity(entityID, newName);
            });
            return this;
        }

        public virtual LuisModelBuilder RenameEntity(Guid entityID, string newName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                string uri = string.Format("{0}{1}entities/{1}", luis.UriRoot, entityID.ToString());
                string jsonBody = "{ \"name\": \"" + newName + "\"}";
                luis.Retry(luis.SendPut, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddSimpleEntity(string entityName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                //A simple entity is a generic entity that describes a single concept
                string uri = luis.UriRoot + "entities";
                string jsonBody = "{ \"name\": \"" + entityName + "\"}";
                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;

        }

        public virtual LuisModelBuilder AddCompositeEntity(string entityName, IEnumerable<string> childEntities)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                //A composite entity is made up of other entities that form parts of a whole.
                //For example, a composite entity named PlaneTicketOrder may have child entities Airline, Destination, DepartureCity, DepartureDate, and PlaneTicketClass. 
                //You build a composite entity from pre - existing simple entities, children of hierarchical entities or prebuilt entities.

                string uri = string.Format("{0}compositeentities", luis.UriRoot);
                StringBuilder jsonBody = new StringBuilder("{");
                jsonBody.Append(string.Format("\"name\":\"{0}\", \"children\":", entityName));
                jsonBody.Append("[\"" + string.Join("\",\"", childEntities) + "\"] }");

                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddCompositeChildEntity(string parentCompositeEntity, string entityName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                var entity = luis.GetEntities("compositeentities").FirstOrDefault(p => p.Equals(parentCompositeEntity));
                Guid parentCompositeEntityID = entity.Value;
                return AddCompositeChildEntity(parentCompositeEntityID, entityName);
            });
            return this;
        }

        public virtual LuisModelBuilder AddCompositeChildEntity(Guid parentCompositeEntityID, string entityName)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                //A composite entity is made up of other entities that form parts of a whole.
                //For example, a composite entity named PlaneTicketOrder may have child entities Airline, Destination, DepartureCity, DepartureDate, and PlaneTicketClass. 
                //You build a composite entity from pre - existing simple entities, children of hierarchical entities or prebuilt entities.

                string uri = string.Format("{0}compositeentities/{1}/children", luis.UriRoot, parentCompositeEntityID.ToString());
                string jsonBody = "{ \"name\": \"" + entityName + "\"}";

                throw new NotImplementedException(); // we need to return a composite entiy builder from composit entity somehow with this function in it so the guid is preserved.
                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;

        }

        public virtual LuisModelBuilder AddClosedListEntity(string entityName, IDictionary<string, IEnumerable<string>> canonicalFormToListMap)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {

                // List entities represent a fixed set of related words in your system. Each list entity may have one or more forms.They aren't machine learned, 
                // and are best used for a known set of variations on ways to represent the same concept. List entities don't have to be labeled in utterances or trained by the system. 
                // A list entity is an explicitly specified list of values. Unlike other entity types, LUIS does not discover additional values for 
                // list entities during training.Therefore, each list entity forms a closed set.
                // https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c14
                string uri = string.Format("{0}closedlists", luis.UriRoot);
                StringBuilder jsonBody = new StringBuilder("{");
                jsonBody.Append(string.Format("\"name\":\"{0}\", \"sublists\":[", entityName));
                foreach (string canonicalForm in canonicalFormToListMap.Keys)
                {
                    jsonBody.Append("{\"canonicalForm\": \"" + canonicalForm + "\",");
                    jsonBody.Append("\"list\": [");
                    foreach (var t in canonicalFormToListMap[canonicalForm])
                    {
                        jsonBody.Append(string.Format("\"{0}\",", t));
                    }
                    jsonBody.Remove(jsonBody.Length - 1, 1); // remove ,
                    jsonBody.Append("]}");
                }
                jsonBody.Append("]}");
                // A Closed List Entity is an entity that can only be from a certain list (includign aliases);
                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddClosedListSubListEntity(string parentClosedListEntity, string canonicalForm, IEnumerable<string> variants)
        {
            var entity = luis.GetEntities("closedlists").FirstOrDefault(p => p.Equals(parentClosedListEntity));
            Guid parentClosedListEntityID = entity.Value;

            return AddClosedListSubListEntity(parentClosedListEntityID, canonicalForm, variants);
        }

        public virtual LuisModelBuilder AddClosedListSubListEntity(Guid parentClosedListEntityID, string canonicalForm, IEnumerable<string> variants)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                // Adds a list to an existing closed list
                // https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/590c5dc65aca2f04a0d3d684
                string uri = string.Format("{0}closedlists/{1}/sublists", luis.UriRoot, parentClosedListEntityID.ToString());
                StringBuilder jsonBody = new StringBuilder("{");
                jsonBody.Append("\"canonicalForm\": \"" + canonicalForm + "\",");
                jsonBody.Append("\"list\": [");
                foreach (var t in variants)
                {
                    jsonBody.Append(string.Format("\"{0}\",", t));
                }
                jsonBody.Remove(jsonBody.Length - 1, 1); // remove ,
                jsonBody.Append("]}");
                throw new NotImplementedException(); // create closed list usb entity needs to return a closedlist sub entity builder with this corrrect guid so it does not have to be passed in.
                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;
        }

        public virtual LuisModelBuilder AddHierarchicalEntity(string entityName, IEnumerable<string> childEntities)
        {
            this.fn = Compose(this.fn, (modelBuilder) =>
            {
                //A hierarchical entity defines a category and its members.It is made up of child entities that form the members of the category. 
                //You can use hierarchical entities to define hierarchical or inheritance relationships between entities, in which children are subtypes of the parent entity. 
                //For example, in a travel agent app, you could add hierarchical entities like these:
                //      $Location, including $FromLocation and $ToLocation as child entities that represent origin and destination locations.
                //      $TravelClass, including $First, $Business, and $Economy as child entities that represent the travel class.
                string uri = string.Format("{0}hierarchicalentities", luis.UriRoot);
                StringBuilder jsonBody = new StringBuilder("{");
                jsonBody.Append(string.Format("\"name\":\"{0}\", \"children\":", entityName));
                jsonBody.Append("[\"" + string.Join("\",\"", childEntities) + "\"] }");
                luis.Retry(luis.SendPost, uri, jsonBody.ToString()).Wait();
                return modelBuilder;
            });
            return this;
        }
    }
}
