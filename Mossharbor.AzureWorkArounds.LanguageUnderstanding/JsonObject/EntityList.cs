using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class EntityList
    {
        public EntityType[] entities { get; set; }
    }

    public class EntityType
    {
        public string id { get; set; }
        public string name { get; set; }
        public int typeId { get; set; }
        public string readableType { get; set; }
        public string customPrebuiltDomainName { get; set; }
        public string customPrebuiltModelName { get; set; }
    }

}
