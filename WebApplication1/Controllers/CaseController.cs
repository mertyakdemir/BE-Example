using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaseController : ControllerBase
    {
        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search(string searchKey)
        {
            //var settings = new ConnectionSettings(new Uri("http://host.docker.internal:9200/")).DefaultIndex("esblogdb");
            var settings = new ConnectionSettings(new Uri("http://localhost:9200/")).DefaultIndex("esblogdb");
            var client = new ElasticClient(settings);
            var createIndex = await client.Indices.CreateAsync("esblogdb", c => c.Map<Blog>(m => m.AutoMap()));

            var searchAllResponse = await client.SearchAsync<Blog>(s => s
             .Index("esblogdb")
             .From(0)
             .Query(q => q
             .MultiMatch(m => m
             .Fields(f => f
                .Field(a => a.BlogTitle)
                 .Field(a => a.BlogText)
             )
            .Query(searchKey)
             )));

            var result = searchAllResponse.Documents;

            List<BlogView> model = new List<BlogView>();
            foreach (var item in result)
            {
                var blog = new BlogView()
                {
                    BlogTitle = item.BlogTitle,
                    BlogText = item.BlogText,
                };
                model.Add(blog);
            }

            return Ok(model);
        }

        [HttpPost]
        [Route("PostBlog")]
        public async Task<IActionResult> PostBlog(BlogDTO blog)
        {
            //var settings = new ConnectionSettings(new Uri("http://host.docker.internal:9200/")).DefaultIndex("esblogdb");
            var settings = new ConnectionSettings(new Uri("http://localhost:9200/")).DefaultIndex("esblogdb");
            var client = new ElasticClient(settings);
            var createIndex = await client.Indices.CreateAsync("esblogdb", c => c.Map<Blog>(m => m.AutoMap()));

            if(blog.Suggest.Count == 0 || blog.Suggest == null )
            {
                blog.Suggest.Add(blog.BlogTitle);
                blog.Suggest.Add(blog.BlogText);
            }
            Guid blogId = Guid.NewGuid();
            var model = new Blog
            {
                BlogId = blogId.ToString(),
                BlogText = blog.BlogText,
                BlogTitle = blog.BlogTitle,
                Suggest = new CompletionField
                {
                    Input = blog.Suggest
                },
            };

            var result = await client.IndexDocumentAsync(model);
            if(result.IsValid)
            {
                return Ok("Success");

            }
            return Ok("Error");
        }

        [HttpGet]
        [Route("AutoComplete")]
        public async Task<IActionResult> AutoComplete(string searchKey)
        {
            //var settings = new ConnectionSettings(new Uri("http://host.docker.internal:9200/")).DefaultIndex("esblogdb");
            var settings = new ConnectionSettings(new Uri("http://localhost:9200/")).DefaultIndex("esblogdb");
            var client = new ElasticClient(settings);

            var createIndex = await client.Indices.CreateAsync("esblogdb", c => c.Map<Blog>(m => m.AutoMap().Properties(ps => ps.Completion(c => c.Name(p => p.Suggest)))));

            var searchResponse = await client.SearchAsync<Blog>(s => s
                                     .Index("esblogdb")
                                     .Suggest(su => su
                                          .Completion("suggestions", c => c
                                               .Field(f => f.Suggest)
                                               .Prefix(searchKey)
                                               .Fuzzy(f => f
                                                   .Fuzziness(Fuzziness.Auto)
                                               ))
                                             ));


            var suggests = from suggest in searchResponse.Suggest["suggestions"]
                           from option in suggest.Options
                           select new BlogView
                           {
                               BlogTitle = option.Source.BlogTitle,
                               BlogText = option.Source.BlogText,
                           };
            
            return Ok(suggests);
        }

    }
}


public class Blog
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string BlogId { get; set; }
    public string BlogTitle { get; set; }
    public string BlogText { get; set; }
    public CompletionField Suggest { get; set; }
}

public class BlogDTO
{
    public string BlogTitle { get; set; }
    public string BlogText { get; set; }
    public List<string> Suggest { get; set; }
}

public class BlogView
{
    public string BlogTitle { get; set; }
    public string BlogText { get; set; }
}