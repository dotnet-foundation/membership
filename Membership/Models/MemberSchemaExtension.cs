using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Membership.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MemberSchemaExtension
    {
        [JsonProperty("twitterId", NullValueHandling = NullValueHandling.Include)]
        public string TwitterId { get; set; }

        [JsonProperty("blogUrl", NullValueHandling = NullValueHandling.Include)]
        public string BlogUrl { get; set; }

        [JsonProperty("githubId", NullValueHandling = NullValueHandling.Include)]
        public string GitHubId { get; set; }

        [JsonProperty("expirationDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ExpirationDateTime { get; set; }

        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }
    }
}
