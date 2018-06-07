using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mossharbor.AzureWorkArounds.LanguageUnderstanding;

namespace UnitTests
{
    [TestClass]
    public class LuisModelBuilderTests
    {
        // NOTE: Replace this example LUIS application ID with the ID of your LUIS application.
        // You can create you own app with the steps here:   https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-get-started-create-app
        string luisAppID = "";
        // NOTE: Replace this example LUIS programmatic key with a valid key.
        string ocpAcimSubscriptionKey = "";

        [TestMethod]
        public void TestGettingIntents()
        {
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            var intentNames = model.GetIntents().Keys;
        }

        [TestMethod]
        public void TestGettingEntities()
        {
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            var intentNames = model.GetEntities().Keys;
        }

        [TestMethod]
        public void TestGettingTrainingStatus()
        {
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            model.ModelNeedsTraining();
            model.Train();
        }

        [TestMethod]
        public void TestAddingIntent()
        {
            string intentName = "TestAddingIntent";
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            try
            {
                model.Modify()
                    .AddIntent(intentName)
                    .Update();

                var intentNames = model.GetIntents().Keys;
                Assert.IsTrue(intentNames.Contains(intentName));
            }
            finally
            {
                model.DeleteIntent(intentName);
            }
        }

        [TestMethod]
        public void TestAddSimpleEntity()
        {
            string entityname = "TestAddSimpleEntity";
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            try
            {
                model.Modify()
                    .AddSimpleEntity(entityname)
                    .Update();

                var entityNames = model.GetEntities().Keys;
                Assert.IsTrue(entityNames.Contains(entityname));
            }
            finally
            {
                model.DeleteEntity(entityname);
            }
        }

        [TestMethod]
        public void TestAddCompositeEntity()
        {
            string entityname = "TestAddCompositeEntity";
            string childEntity = "ChildEntity";
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            try
            {
                model.Modify()
                    .AddSimpleEntity(childEntity)
                    .AddCompositeEntity(entityname, new string[]{ childEntity })
                    .Update();

                var entityNames = model.GetEntities().Keys;
                Assert.IsTrue(entityNames.Contains(entityname));
            }
            finally
            {
                model.DeleteEntity(entityname);
                model.DeleteEntity(childEntity);
            }
        }

        [TestMethod]
        public void TestAddClosedListEntity()
        {
            string entityname = "TestAddClosedListEntity";
            var washington = new Dictionary<string, IEnumerable<string>> { { "Washington", new List<string>() { "WA", "Washington" } } };
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            try
            {
                model.Modify()
                    .AddClosedListEntity(entityname, washington)
                    .Update();

                var entityNames = model.GetEntities().Keys;
                Assert.IsTrue(entityNames.Contains(entityname));
            }
            finally
            {
                model.DeleteEntity(entityname);
            }
        }

        [TestMethod]
        public void TestAddHierarchicalEntity()
        {
            string entityname = "TestAddHierarchicalEntity";
            var childEntities = new List<string>() { "ChildEntity3", "ChildEntity4" };
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            try
            {
                model.Modify()
                    .AddHierarchicalEntity(entityname, childEntities)
                    .Update();

                var entityNames = model.GetEntities().Keys;
                Assert.IsTrue(entityNames.Contains(entityname));
                foreach (var entity in childEntities)
                    Assert.IsFalse(entityNames.Contains(entity));
            }
            finally
            {
                model.DeleteEntity(entityname);
            }
        }

        [TestMethod]
        public void TestQuery()
        {
            LuisModel model = new LuisModel(luisAppID, ocpAcimSubscriptionKey);
            var result = model.Query("Can I get an Uber");
            Assert.IsTrue(null != result);
        }
    }
}
