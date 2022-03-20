using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Model.DTOs
{
    public class BlogDTO
    {
        public string BlogTitle { get; set; }
        public string BlogText { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> Suggest { get; set; }
    }
}
