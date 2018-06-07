using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class ModelData
    {
        public string version { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public object lastTrainedDateTime { get; set; }
        public DateTime lastPublishedDateTime { get; set; }
        public object endpointUrl { get; set; }
        public object assignedEndpointKey { get; set; }
        public object externalApiKeys { get; set; }
        public int intentsCount { get; set; }
        public int entitiesCount { get; set; }
        public int endpointHitsCount { get; set; }
        public string trainingStatus { get; set; }
    }

}
