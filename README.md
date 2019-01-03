# AzureWorkArounds.LanguageUnderstanding
.Net wrapper around Microsofts Language Understanding (LUIS) Rest api

Install the nuget package:  [Install-Package Mossharbor.AzureWorkArounds.LanguageUnderstanding](https://www.nuget.org/packages/Mossharbor.AzureWorkArounds.LanguageUnderstanding)

*Example:*
```cs
using Mossharbor.AzureWorkArounds.LanguageUnderstanding;

// Query
LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey); // TODO enter your credentials in here!!
var result = model.Query("Can I get an Uber");

var intentNames = model.GetIntents().Keys;
var entityNames = model.GetEntities().Keys;

// add/Modify your model
model.Modify()
	.AddIntent(intentName)
	.Update();

var washington = new Dictionary<string, IEnumerable<string>> { { "Washington", new List<string>() { "WA", "Washington" } } };
var childEntities = new List<string>() { "childEntity", "ClosedListEntity" };
			
model.Modify()
    .AddSimpleEntity("childEntity")
    .AddCompositeEntity("CompositeEntity", new string[]{ "childEntity" })
    .AddClosedListEntity("ClosedListEntity", washington)
	.AddHierarchicalEntity(entityname, childEntities)
	.Update();

//  Add intents with examples
string intentName = "TestAddingIntentExample";
string intentExample = "Rotate object 30 degrees to the left";
string entityName = "amount";
string entityDirection = "direction";
string entityNameExample = "30 degrees";
string entityDirectionExample = "left";
			
model.Modify()
     .AddIntent(intentName, new List<Entitylabel>()
              {
                Entitylabel.Create(intentExample, entityName, entityNameExample),
                Entitylabel.Create(intentExample, entityDirection, entityDirectionExample),
              })
	.Update()
