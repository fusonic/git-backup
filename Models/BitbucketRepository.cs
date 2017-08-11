using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fusonic.GitBackup.Models
{
    public class BitbucketRepository
    {
        [JsonProperty("values")]
        public List<ValuesProperty> Values { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; } 
        
        public class ValuesProperty
        {
            [JsonProperty("links")]
            public LinksProperty Links { get; set; }

            [JsonProperty("full_name")]
            public string Name { get; set; }
        }

        public class LinksProperty
        {
            [JsonProperty("clone")]
            public List<CloneProperty> Clone { get; set; }
        }

        public class CloneProperty
        {
            [JsonProperty("href")]
            public string Href { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}