using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalRSample.Models;

namespace SignalRSample.Controllers
{
    [Route("/[controller]")]
    [EnableCors("Everything")]
    public class SampleController : Controller
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<string, Sample> _dictionary;

        public SampleController(ILoggerFactory loggerfactory)
            : base()
        {
            _logger = loggerfactory.CreateLogger<SampleController>();
            _loggerFactory = loggerfactory;
            _dictionary = new Dictionary<string, Sample>();
            _dictionary["6a506a6a-8248-460e-82e5-41db5fe4af24"] = new Sample { PropertyA = "A1", PropertyB = "B1" };
            _dictionary["1864acac-f2d5-4a12-b3a8-479885a2c868"] = new Sample { PropertyA = "A2", PropertyB = "B2" };
        }

        [HttpPost]
        public IActionResult PostObject(string uid, [FromBody]Sample sample)
        {
            if (!this.ModelState.IsValid)
                return BadRequest();

            _logger.LogInformation("[SignalRSample.SampleController] PostSample: uid was: " + uid);
            string s = JsonConvert.SerializeObject(sample, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _logger.LogInformation("[SignalRSample.SampleController] PostSample", "Sample was: " + s);

            if (!_dictionary.ContainsKey(uid))
                _dictionary[uid] = sample;

            var response = new CreatedAtActionResult("getsample", "sample", new { id = uid }, sample);
            return response;
        }

        [HttpGet]
        public IActionResult GetSample(string id)
        {
            _logger.LogInformation("[SignalRSample.SampleController] GetSample: id was: " + id);
            Response result = null;
            try
            {
                if (_dictionary.ContainsKey(id))
                {
                    result = new Response(Models.Response.OK);
                    string s = JsonConvert.SerializeObject(_dictionary[id]);
                    _logger.LogInformation("[SignalRSample.SampleController] GetSample getting: " + s);
                    result.Result = JObject.Parse(s);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[SignalRSample.SampleController] GetSample");
            }

            if (result == null)
                return NotFound();

            return Json(result);
        }
    }
}
