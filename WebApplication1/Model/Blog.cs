using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Model
{
    public class Blog
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogText { get; set; }
        public CompletionField Suggest { get; set; }
    }
}
