using BookStore_API.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILoggerService _logger;

        public HomeController(ILoggerService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all values.
        /// </summary>
        /// <returns></returns>
        // GET: api/<HemoController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInfo("Accessed Home Controller");
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Get a value by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<HemoController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            _logger.LogDebug("Get a value");
            return "value";
        }

        // POST api/<HemoController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            _logger.LogError("Error log test");
        }

        // PUT api/<HemoController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HemoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _logger.LogWarn("This is a warning");
        }
    }
}
