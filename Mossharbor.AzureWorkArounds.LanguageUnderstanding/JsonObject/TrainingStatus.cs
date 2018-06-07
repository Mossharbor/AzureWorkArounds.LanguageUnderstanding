using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class TrainingStatus
    {
        public ModelInfo[] models { get; set; }
    }

    public class ModelInfo
    {
        public string modelId { get; set; }
        public Details details { get; set; }
    }

    public class Details
    {
        public int statusId { get; set; }
        public string status { get; set; }
        public int exampleCount { get; set; }
        public DateTime trainingDateTime { get; set; }
        public string failureReason { get; set; }
    }

}
