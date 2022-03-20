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
using WebApplication1.Model;
using WebApplication1.Model.DTOs;
using WebApplication1.Model.ViewModel;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaseController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;

        public CaseController(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search(string searchKey)
        {
            var searchAllResponse = await _elasticClient.SearchAsync<Blog>(s => s
             .Index("esblogdb9")
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

            List<BlogViewModel> model = new List<BlogViewModel>();
            foreach (var item in result)
            {
                var blog = new BlogViewModel()
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
      
            
            Guid blogId = Guid.NewGuid();
            var model = new Blog
            {
                BlogId = blogId.ToString(),
                BlogText = blog.BlogText,
                BlogTitle = blog.BlogTitle,
                Suggest = new CompletionField
                {
                    Input = blog.BlogText.Split(" ").ToList()
                },
            };

            var result = await _elasticClient.IndexDocumentAsync(model);
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
            
            var searchResponse = await _elasticClient.SearchAsync<Blog>(s => s
                                     .Index("esblogdb9")
                                     .Suggest(su => su
                                          .Completion("suggestions", c => c
                                               .Field(f => f.Suggest)
                                               .Prefix(searchKey)
                                               .Size(5)
                                               .Fuzzy(f => f
                                                  .Fuzziness(Fuzziness.Auto)
                                                ))
                                             ));

            var suggests = from suggest in searchResponse.Suggest["suggestions"]
                           from option in suggest.Options
                           select new
                           {
                                option.Text
                           };
                           
            return Ok(suggests);
        }

    }
}



