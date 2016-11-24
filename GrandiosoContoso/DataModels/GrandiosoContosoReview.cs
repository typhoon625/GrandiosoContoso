using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;

namespace GrandiosoContoso.DataModels
{
    public class GrandiosoContosoReview
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime createdDate { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public DateTime updatedDate { get; set; }

        [JsonProperty(PropertyName = "SkypeID")]
        public string SkypeID { get; set; }

        [JsonProperty(PropertyName = "Rating")]
        public string Rating { get; set; }
    }
}