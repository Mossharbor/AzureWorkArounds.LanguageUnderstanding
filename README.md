# AzureWorkArounds.LanguageUnderstanding
.Net wrapper around Microsofts Language Understanding (LUIS) Rest api

Install the nuget package:  [Install-Package Mossharbor.AzureWorkArounds.LanguageUnderstanding](https://www.nuget.org/packages/Mossharbor.AzureWorkArounds.LanguageUnderstanding)

*Example:*
```cs
using Mossharbor.AzureWorkArounds.LanguageUnderstanding;

// TODO enter your credentials in here!!
LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
var result = model.Query("Can I get an Uber");

// add new answer and questions to your qna knowledgebase
model.Modify()
	.AddIntent(intentName)
	.Update();

var intentNames = model.GetIntents().Keys;

model.Modify()
	.AddSimpleEntity(entityname)
	.Update();

var entityNames = model.GetEntities().Keys;
